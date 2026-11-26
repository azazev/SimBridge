// Offline host/ImGui doubles exercise the actual Plugin controller, not native rendering.
using System.Reflection;
using System.Text.Json;
using AnoBossBridge;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

internal static class UiTests
{
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    private static T Field<T>(Plugin p, string name) => (T)typeof(Plugin).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(p)!;
    private static void Set(Plugin p, string name, object value) => typeof(Plugin).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(p, value);
    public static void Main()
    {
        var host = new Host();
        var commands = new FakeCommands();
        var chat = new Chat();
        var p = new Plugin(host, commands, chat, host.OpenFolder);
        host.UiBuilder.Frame();
        var activation = Field<Activation>(p, "activation");
        Check(activation.Enabled && !Field<bool>(p, "windowOpen") && !Directory.Exists(Path.Combine(host.ConfigDirectory.FullName, "diagnostics")), "Default startup is armed, window closed, recording off");
        Check(host.UiBuilder.Subscribers == 3, "Both Dalamud callbacks and Draw registered");
        Check(commands.Commands.ContainsKey("/simbridge") && !commands.Commands.ContainsKey("/anobridge"), "Only renamed command registered");
        host.UiBuilder.Main(); host.UiBuilder.Frame();
        Check(Field<bool>(p, "windowOpen"), "Main callback opens window");
        Check(ImGui.Title == "SimBridge v0.9.0###AnoBossBridge" && !Field<bool>(p, "devMode") && ImGui.Buttons.Count == 0, "New title and diagnostics hidden by default");
        ImGui.Close = true; host.UiBuilder.Frame();
        Check(!Field<bool>(p, "windowOpen") && activation.Enabled, "Closing window keeps bridge armed");
        host.UiBuilder.Config(); host.UiBuilder.Frame();
        Check(Field<bool>(p, "windowOpen"), "Config callback opens same window");
        ImGui.Toggle = "Enable bridge"; host.UiBuilder.Frame(); host.UiBuilder.Frame();
        Check(!activation.Enabled && host.Saved?.Enabled == false, "Toggle disables and persists");
        p.Dispose();
        Check(host.UiBuilder.Subscribers == 0 && commands.Commands.Count == 0, "Dispose unregisters all callbacks and command");
        p = new Plugin(host, commands, chat, host.OpenFolder); host.UiBuilder.Frame();
        activation = Field<Activation>(p, "activation");
        Check(!activation.Enabled, "Reload respects disabled preference");
        commands.Send(""); host.UiBuilder.Frame();
        Check(Field<bool>(p, "windowOpen") && !Directory.Exists(Path.Combine(host.ConfigDirectory.FullName, "diagnostics")), "Bare command opens UI without creating a snapshot");
        commands.Send("enable"); host.UiBuilder.Frame();
        Check(activation.Enabled && host.Saved?.Enabled == true, "Command enable persists and updates same state");
        ImGui.Toggle = "Dev mode"; host.UiBuilder.Frame(); host.UiBuilder.Frame();
        Check(Field<bool>(p, "devMode") && ImGui.Buttons.Contains("Start diagnostics") && ImGui.Buttons.Contains("Open diagnostics folder"), "Dev mode reveals both controls");
        ImGui.Press = "Open diagnostics folder"; host.UiBuilder.Frame(); host.UiBuilder.Frame();
        Check(host.OpenedFolder == Path.Combine(host.ConfigDirectory.FullName, "diagnostics") && Directory.Exists(host.OpenedFolder), "Folder button creates and opens exact diagnostics directory before first log");
        Check(Field<StreamWriter?>(p, "output") == null && activation.Enabled, "Opening folder does not start diagnostics or disable bridge");
        ImGui.Press = "Start diagnostics"; host.UiBuilder.Frame(); host.UiBuilder.Frame();
        var path = Field<string>(p, "lastPath");
        Check(File.Exists(path) && Field<int>(p, "samples") == 1, "Diagnostics button starts a recording");
        ImGui.Press = "Stop diagnostics"; host.UiBuilder.Frame(); host.UiBuilder.Frame();
        Check(Field<StreamWriter?>(p, "output") == null && activation.Enabled, "Stopping diagnostics leaves bridge enabled");
        using (var json = JsonDocument.Parse(File.ReadLines(path).First()))
            Check(json.RootElement.GetProperty("Bridge").GetProperty("Version").GetString() == "0.9.0", "Recorded version updated");
        using (File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
        commands.Send("start"); host.UiBuilder.Frame();
        path = Field<string>(p, "lastPath");
        ImGui.Toggle = "Dev mode"; host.UiBuilder.Frame(); host.UiBuilder.Frame();
        Check(!Field<bool>(p, "devMode") && Field<StreamWriter?>(p, "output") == null && ImGui.Buttons.Count == 0 && activation.Enabled, "Dev mode off stops recording, hides controls and leaves bridge enabled");
        using (File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
        commands.Send("start"); host.UiBuilder.Frame();
        Check(Field<bool>(p, "devMode") && Field<bool>(p, "windowOpen"), "Explicit start command reveals diagnostics");
        host.ThrowOpen = true;
        ImGui.Press = "Open diagnostics folder"; host.UiBuilder.Frame(); host.UiBuilder.Frame();
        Check(activation.Enabled && Field<StreamWriter?>(p, "output") != null && Field<string?>(p, "folderError") != null && chat.Errors.Count == 1, "Explorer failure reports error without stopping bridge or recording");
        host.ThrowOpen = false;
        ImGui.Press = "Open diagnostics folder"; host.UiBuilder.Frame(); host.UiBuilder.Frame();
        Check(Field<string?>(p, "folderError") == null, "Folder retry clears error");
        chat.Errors.Clear(); commands.Send("stop"); host.UiBuilder.Frame();
        p.Dispose(); p = new Plugin(host, commands, chat, host.OpenFolder); host.UiBuilder.Frame();
        activation = Field<Activation>(p, "activation");
        Check(activation.Enabled && !Field<bool>(p, "devMode") && Field<StreamWriter?>(p, "output") == null, "Reload preserves enabled preference but resets dev mode and recording");
        commands.Send("status"); host.UiBuilder.Frame();
        Check(Directory.GetFiles(Path.GetDirectoryName(path)!, "*.json").Length == 1, "Explicit status still produces JSON");
        commands.Send("start"); host.UiBuilder.Frame();
        Set(p, "nextSample", 0L); Set(p, "recordingEnds", 0L); host.UiBuilder.Frame();
        Check(Field<StreamWriter?>(p, "output") == null && Field<bool>(p, "windowOpen"), "Time limit stops recording while UI stays usable");
        commands.Send("start"); host.UiBuilder.Frame();
        Set(p, "samples", 599); Set(p, "nextSample", 0L); host.UiBuilder.Frame();
        Check(Field<StreamWriter?>(p, "output") == null, "600-sample limit retained");
        commands.Send("start"); host.UiBuilder.Frame();
        Probe.ThrowCapture = true;
        Set(p, "nextSample", 0L); host.UiBuilder.Frame();
        var degradedPath = Field<string>(p, "lastPath");
        using (var degraded = new StreamReader(new FileStream(degradedPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            Check(activation.Enabled && Field<StreamWriter?>(p, "output") != null && degraded.ReadToEnd().Contains("DiagnosticError"),
                "Diagnostic interface failure preserves bridge status and recording");
        Probe.ThrowCapture = false;
        activation.ThrowOnTick = true; host.UiBuilder.Frame();
        Check(!activation.Enabled && Field<StreamWriter?>(p, "output") == null && chat.Errors.Count == 1, "Runtime error disables bridge and closes recording");
        activation.ThrowOnTick = false;
        p.Dispose();
        host.ThrowLoad = true;
        p = new Plugin(host, commands, chat, host.OpenFolder); host.UiBuilder.Frame();
        Check(!Field<Activation>(p, "activation").Enabled && chat.Errors.Count == 2, "Unreadable config starts disabled with an error");
        p.Dispose(); host.ThrowLoad = false;
        p = new Plugin(host, commands, chat, host.OpenFolder); host.UiBuilder.Frame();
        commands.Send("start"); host.UiBuilder.Frame();
        path = Field<string>(p, "lastPath"); p.Dispose();
        using (File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
        Check(host.UiBuilder.Subscribers == 0 && commands.Commands.Count == 0, "Unload closes recording and detaches UI");
        Check(ImGui.Begins == ImGui.Ends, "ImGui Begin/End balanced including closed/collapsed windows");
        Console.WriteLine("PASS 8/10: SimBridge command/title, saved settings, Dev mode visibility/stop/reload, folder creation/failure recovery, diagnostics limits and cleanup (offline host)");
    }
}

internal sealed class Host : IDalamudPluginInterface
{
    public UiBuilder UiBuilder { get; } = new();
    public DirectoryInfo ConfigDirectory { get; } = new(Path.Combine(Path.GetTempPath(), "AnoBossBridge-ui-tests-" + Guid.NewGuid().ToString("N")));
    public Plugin.Configuration? Saved;
    public bool ThrowLoad;
    public bool ThrowOpen;
    public string? OpenedFolder;
    public void OpenFolder(string path) { if (ThrowOpen) throw new IOException("test Explorer error"); OpenedFolder = path; }
    public object? GetPluginConfig() => ThrowLoad ? throw new IOException("test config error") : Saved == null ? null : JsonSerializer.Deserialize<Plugin.Configuration>(JsonSerializer.Serialize(Saved));
    public void SavePluginConfig(Dalamud.Configuration.IPluginConfiguration config) => Saved = JsonSerializer.Deserialize<Plugin.Configuration>(JsonSerializer.Serialize((Plugin.Configuration)config));
}
internal sealed class FakeCommands : ICommandManager
{
    public Dictionary<string, CommandInfo> Commands { get; } = new();
    public bool AddHandler(string command, CommandInfo info) => Commands.TryAdd(command, info);
    public void RemoveHandler(string command) => Commands.Remove(command);
    public void Send(string args) => Commands["/simbridge"].Handler("/simbridge", args);
}
internal sealed class Chat : IChatGui
{
    public List<string> Errors { get; } = [];
    public void Print(string text) { }
    public void PrintError(string text) => Errors.Add(text);
}
namespace Dalamud.Configuration { public interface IPluginConfiguration { int Version { get; set; } } }
namespace Dalamud.Plugin
{
    public interface IDalamudPlugin : IDisposable { }
    public interface IDalamudPluginInterface
    {
        UiBuilder UiBuilder { get; }
        DirectoryInfo ConfigDirectory { get; }
        object? GetPluginConfig();
        void SavePluginConfig(Dalamud.Configuration.IPluginConfiguration config);
    }
    public sealed class UiBuilder
    {
        public bool DisableAutomaticUiHide { get; set; }
        public event Action? Draw;
        public event Action? OpenMainUi;
        public event Action? OpenConfigUi;
        public int Subscribers => (Draw?.GetInvocationList().Length ?? 0) + (OpenMainUi?.GetInvocationList().Length ?? 0) + (OpenConfigUi?.GetInvocationList().Length ?? 0);
        public void Frame() { ImGui.Buttons.Clear(); Draw?.Invoke(); }
        public void Main() => OpenMainUi?.Invoke();
        public void Config() => OpenConfigUi?.Invoke();
    }
}
namespace Dalamud.Plugin.Services
{
    public interface ICommandManager
    {
        Dictionary<string, CommandInfo> Commands { get; }
        bool AddHandler(string command, CommandInfo info);
        void RemoveHandler(string command);
    }
    public interface IChatGui { void Print(string text); void PrintError(string text); }
}
namespace Dalamud.Game.Command
{
    public sealed class CommandInfo(Action<string, string> handler)
    {
        public Action<string, string> Handler { get; } = handler;
        public string HelpMessage { get; set; } = "";
    }
}
namespace Dalamud.Bindings.ImGui
{
    public enum ImGuiCond { FirstUseEver }
    public static class ImGui
    {
        public static bool Close;
        public static string? Toggle, Title;
        public static string? Press;
        public static List<string> Buttons { get; } = [];
        public static int Begins, Ends;
        public static void SetNextWindowSize(System.Numerics.Vector2 size, ImGuiCond condition) { }
        public static bool Begin(string name, ref bool open) { Title = name; ++Begins; if (Close) { open = false; Close = false; return false; } return true; }
        public static void End() => ++Ends;
        public static bool Checkbox(string label, ref bool value) { if (Toggle != label) return false; Toggle = null; value = !value; return true; }
        public static bool Button(string label) { Buttons.Add(label); if (Press != label) return false; Press = null; return true; }
        public static void TextWrapped(string text) { }
        public static void Separator() { }
    }
}
namespace AnoBossBridge
{
    internal sealed class Activation : IDisposable
    {
        public Activation(Func<string, object?> find, Action<string> say) { }
        public bool Enabled { get; private set; }
        public bool ThrowOnTick;
        public string Status => Enabled ? "Armed" : "Disabled";
        public object? RoleDiagnostics => null; public object? ExaflaresDiagnostics => null;
        public object? LockDiagnostics => null; public object? CompatibilityDiagnostics => null;
        public void Enable() => Enabled = true;
        public void Tick() { if (ThrowOnTick) throw new InvalidOperationException("test runtime error"); }
        public void Dispose() => Enabled = false;
    }
    internal sealed class Probe : IDisposable
    {
        public static bool ThrowCapture;
        public Probe(Func<string, object?> find) { }
        public object Capture(object bridge) => ThrowCapture ? throw new MissingMemberException("Injected diagnostic interface change") : new { Bridge = bridge };
        public void Dispose() { }
    }
}




