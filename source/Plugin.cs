using System.Diagnostics;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace AnoBossBridge;

public sealed class Plugin : IDalamudPlugin
{
    private const string Command = "/simbridge";
    private readonly IDalamudPluginInterface pluginInterface;
    private readonly ICommandManager commands;
    private readonly IChatGui chat;
    private readonly Probe probe;
    private readonly Activation activation;
    private readonly Configuration config;
    private readonly Action<string> openFolder;
    private readonly ConcurrentQueue<string> actions = new();
    private StreamWriter? output;
    private long nextSample;
    private long recordingEnds;
    private int samples;
    private bool disposed;
    private bool windowOpen;
    private bool devMode;
    private string? folderError;
    private string? lastPath;

    public Plugin(IDalamudPluginInterface pluginInterface, ICommandManager commands,
        IChatGui chat)
        : this(pluginInterface, commands, chat, path =>
        {
            using var process = Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }) { }

    internal Plugin(IDalamudPluginInterface pluginInterface, ICommandManager commands,
        IChatGui chat, Action<string> openFolder)
    {
        this.pluginInterface = pluginInterface;
        this.commands = commands;
        this.chat = chat;
        this.openFolder = openFolder;
        probe = new Probe(FindPlugin);
        activation = new Activation(FindPlugin, Say);
        try { config = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration(); }
        catch (Exception e)
        {
            config = new Configuration { Enabled = false };
            chat.PrintError($"[SimBridge] Could not load settings; starting disabled: {e.GetBaseException().Message}");
        }
        if (!commands.AddHandler(Command, new CommandInfo(OnCommand)
            { HelpMessage = "Open bridge controls. Commands: enable, disable, status, start, stop." }))
            throw new InvalidOperationException($"{Command} is already registered");
        // Bossmod updates its world and module lists in UiBuilder.Draw too.
        pluginInterface.UiBuilder.DisableAutomaticUiHide = true;
        pluginInterface.UiBuilder.Draw += Update;
        pluginInterface.UiBuilder.OpenMainUi += OpenWindow;
        pluginInterface.UiBuilder.OpenConfigUi += OpenWindow;
        if (config.Enabled) actions.Enqueue("enable");
    }

    [Serializable]
    public sealed class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 1;
        public bool Enabled { get; set; } = true;
    }

    private void OpenWindow() => actions.Enqueue("ui");

    private void SaveEnabled(bool enabled)
    {
        if (config.Enabled == enabled) return;
        var previous = config.Enabled;
        config.Enabled = enabled;
        try { pluginInterface.SavePluginConfig(config); }
        catch { config.Enabled = previous; throw; }
    }

    private object? FindPlugin(string command)
    {
        if (!commands.Commands.TryGetValue(command, out var info)) return null;
        var target = info.Handler.Target;
        var expected = command == "/bmr" ? "BossMod.Plugin" : "AnoMech.Plugin";
        if (target?.GetType().FullName != expected)
            throw new InvalidOperationException($"{command} does not expose the expected plugin instance");
        return target;
    }

    private void OnCommand(string command, string args) =>
        actions.Enqueue(args.Trim().ToLowerInvariant());

    private void Handle(string action)
    {
        if (disposed) return;
        try
        {
            switch (action)
            {
                case "enable":
                    activation.Enable();
                    SaveEnabled(true);
                    break;
                case "disable":
                    activation.Dispose();
                    SaveEnabled(false);
                    Say("Bridge disabled; its encounter module removed.");
                    break;
                case "start":
                    devMode = true;
                    windowOpen = true;
                    if (output != null) { Say("Already recording. Use /simbridge stop when finished."); return; }
                    lastPath = NewPath("jsonl");
                    output = new StreamWriter(lastPath, false) { AutoFlush = true };
                    samples = 0;
                    nextSample = 0;
                    recordingEnds = Stopwatch.GetTimestamp() + 300 * Stopwatch.Frequency;
                    Say($"Recording for up to 5 minutes: {lastPath}");
                    break;
                case "stop":
                    Stop();
                    Say(lastPath == null ? "No recording yet." : $"Saved: {lastPath}");
                    break;
                case "dev-on":
                    devMode = true;
                    break;
                case "dev-off":
                    devMode = false;
                    if (output != null)
                    {
                        Stop();
                        Say($"Dev mode disabled. Saved: {lastPath}");
                    }
                    break;
                case "folder":
                    OpenDiagnosticsFolder();
                    break;
                case "":
                case "ui":
                    windowOpen = true;
                    break;
                case "status":
                    using (var snapshot = new Probe(FindPlugin))
                    {
                        var statusPath = NewPath("json");
                        File.WriteAllText(statusPath, JsonSerializer.Serialize(Capture(snapshot),
                            new JsonSerializerOptions { WriteIndented = true }));
                        Say($"Bridge: {activation.Status}. Snapshot: {statusPath}");
                    }
                    break;
                default:
                    Say("Use /simbridge for controls, or enable, disable, status, start, stop. v0.9.0: FRU Paradise Regained, Light Rampant, Diamond Dust, Apocalypse, Ultimate Relativity, Darklit Dragonsong, Crystallize Time and Fulgent Blade. UWU Ultimate Predation and Ultimate Suppression. UCoB P5 Exaflares.");
                    break;
            }
        }
        catch (Exception e) { Fail(e); }
    }

    private void Update()
    {
        if (disposed) return;
        try
        {
            while (actions.TryDequeue(out var action)) Handle(action);
            activation.Tick();
            RecordSample();
            DrawWindow();
        }
        catch (Exception e) { windowOpen = false; Fail(e); }
    }

    private void RecordSample()
    {
        if (output == null || Stopwatch.GetTimestamp() < nextSample) return;
        if (Stopwatch.GetTimestamp() >= recordingEnds)
        {
            Stop();
            Say($"Recording limit reached. Saved: {lastPath}");
            return;
        }
        output.WriteLine(JsonSerializer.Serialize(Capture(probe)));
        nextSample = Stopwatch.GetTimestamp() + Stopwatch.Frequency / 2;
        if (++samples >= 600)
        {
            Stop();
            Say($"Recording limit reached. Saved: {lastPath}");
        }
    }

    private void DrawWindow()
    {
        if (!windowOpen) return;
        ImGui.SetNextWindowSize(new Vector2(480, 210), ImGuiCond.FirstUseEver);
        var visible = ImGui.Begin("SimBridge v0.9.0###AnoBossBridge", ref windowOpen);
        try
        {
            if (!visible) return;
            var enabled = activation.Enabled;
            if (ImGui.Checkbox("Enable bridge", ref enabled)) actions.Enqueue(enabled ? "enable" : "disable");
            ImGui.TextWrapped("Remembers your choice on restart. Start a fresh AnoMech run after enabling.");
            ImGui.TextWrapped(activation.Status);
            ImGui.Separator();
            var showDiagnostics = devMode;
            if (ImGui.Checkbox("Dev mode", ref showDiagnostics))
                actions.Enqueue(showDiagnostics ? "dev-on" : "dev-off");
            if (!showDiagnostics) return;
            if (ImGui.Button(output == null ? "Start diagnostics" : "Stop diagnostics"))
                actions.Enqueue(output == null ? "start" : "stop");
            ImGui.TextWrapped(output == null ? "Diagnostics stopped." : $"Recording: {samples} samples (up to 5 minutes).");
            if (lastPath != null) ImGui.TextWrapped($"Diagnostics file: {lastPath}");
            if (ImGui.Button("Open diagnostics folder")) actions.Enqueue("folder");
            if (folderError != null) ImGui.TextWrapped(folderError);
        }
        finally { ImGui.End(); }
    }

    private string DiagnosticsDirectory()
    {
        var directory = Path.Combine(pluginInterface.ConfigDirectory.FullName, "diagnostics");
        Directory.CreateDirectory(directory);
        return directory;
    }

    private void OpenDiagnosticsFolder()
    {
        try
        {
            openFolder(DiagnosticsDirectory());
            folderError = null;
        }
        catch (Exception e)
        {
            folderError = $"Could not open diagnostics folder: {e.GetBaseException().Message}";
            chat.PrintError($"[SimBridge] {folderError}");
        }
    }

    private string NewPath(string extension) => Path.Combine(DiagnosticsDirectory(),
        $"bridge-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}-{Guid.NewGuid().ToString("N")[..6]}.{extension}");

    private void Say(string message) => chat.Print($"[SimBridge] {message}");
    private object BridgeStatus() => new { Version = "0.9.0", activation.Enabled, activation.Status, Roles = activation.RoleDiagnostics, Exaflares = activation.ExaflaresDiagnostics, SimulationLock = activation.LockDiagnostics, Compatibility = activation.CompatibilityDiagnostics };

    private object Capture(Probe source)
    {
        try { return source.Capture(BridgeStatus()); }
        catch (Exception e)
        {
            return new { TimestampUtc = DateTimeOffset.UtcNow, Bridge = BridgeStatus(), DiagnosticError = e.GetBaseException().Message };
        }
    }

    private void Fail(Exception e)
    {
        var error = e.GetBaseException().Message;
        try { activation.Dispose(); }
        catch (Exception cleanup) { error += $" Cleanup: {cleanup.GetBaseException().Message}"; }
        try { output?.WriteLine(JsonSerializer.Serialize(new { TimestampUtc = DateTimeOffset.UtcNow, Error = error })); }
        finally { Stop(); }
        chat.PrintError($"[SimBridge] Stopped: {error}");
    }

    private void Stop()
    {
        probe.Dispose();
        var writer = output;
        output = null;
        writer?.Dispose();
    }

    public void Dispose()
    {
        disposed = true;
        pluginInterface.UiBuilder.Draw -= Update;
        pluginInterface.UiBuilder.OpenMainUi -= OpenWindow;
        pluginInterface.UiBuilder.OpenConfigUi -= OpenWindow;
        commands.RemoveHandler(Command);
        try { activation.Dispose(); }
        finally { Stop(); }
    }
}


