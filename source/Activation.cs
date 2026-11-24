using System.Collections;
using System.Reflection;
using static AnoBossBridge.Probe;

namespace AnoBossBridge;

// Contract-checked adapter. SimulationGate only owns positively identified simulation modules.
internal sealed class Activation(Func<string, object?> findPlugin, Action<string> say,
    Func<Assembly, Assembly, Compatibility>? validate = null) : IDisposable
{
    internal const string Scenario = "AnoMech.Scenarios.Fru.P3Apocalypse.FruP3ApocalypseScenario";
    internal const string DarklitScenario = "AnoMech.Scenarios.Fru.P4DarklitDragonsong.FruP4DarklitDragonsongScenario";
    internal const string RelativityScenario = "AnoMech.Scenarios.Fru.P3UltimateRelativity.FruP3UltimateRelativityScenario";
    internal const string CrystallizeScenario = "AnoMech.Scenarios.Fru.P4CrystallizeTime.FruP4CrystallizeTimeScenario";
    internal const string DiamondScenario = "AnoMech.Scenarios.Fru.P2DiamondDust.FruP2DiamondDustScenario";
    internal const string FulgentScenario = "AnoMech.Scenarios.Fru.P5FulgentBlade.FruP5FulgentBladeScenario";
    internal const string LightRampantScenario = "AnoMech.Scenarios.Fru.P2LightRampant.FruP2LightRampantScenario";
    internal const string ParadiseScenario = "AnoMech.Scenarios.Fru.P5ParadiseRegained.FruP5ParadiseRegainedScenario";
    internal static bool IsSupported(string? scenario) => scenario is Scenario or DarklitScenario or RelativityScenario
        or CrystallizeScenario or DiamondScenario or FulgentScenario or LightRampantScenario or ParadiseScenario;
    internal static (uint State, float Delay, string Label) Entry(string scenario) => scenario switch
    {
        Scenario => (0x02030000u, 2.1f, "P3 Apocalypse"),
        DarklitScenario => (0x02100020u, 6.6f, "P4 Darklit Dragonsong"),
        RelativityScenario => (0x02010000u, 1f, "P3 Ultimate Relativity"),
        CrystallizeScenario => (0x02130000u, 1f, "P4 Crystallize Time"),
        DiamondScenario => (0x01020000u, 2f, "P2 Diamond Dust"),
        FulgentScenario => (0x03010000u, 1.5f, "P5 Fulgent Blade"),
        LightRampantScenario => (0x01050000u, 3f, "P2 Light Rampant"),
        ParadiseScenario => (0x03020000u, 1.3f, "P5 Paradise Regained"),
        _ => throw new InvalidOperationException("Unsupported scenario")
    };
    internal static (int Phase, uint OID, string BossField) Boss(string scenario) => scenario switch
    {
        DiamondScenario or LightRampantScenario => (1, 0x459Fu, "_bossP2"),
        FulgentScenario or ParadiseScenario => (3, 0x45AFu, "_bossP5"),
        DarklitScenario or CrystallizeScenario => (2, 0x45A9u, "_bossP4Usurper"),
        Scenario or RelativityScenario => (2, 0x45A7u, "_bossP3"),
        _ => throw new InvalidOperationException("Unsupported scenario")
    };
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private object? owner, manager, module, rotation, savedPreset, forceDisable, lastScenario;
    private double lastElapsed = double.MaxValue;
    private bool attempted, savedOverride;
    private RoleSync? roles;
    private SimulationGate? gate;
    private Compatibility? compatibility;
    private object? boundBoss, boundAno, boundGame, boundWorld, boundManager;
    private bool wasBound, waitForFresh;
    private string? runError, cleanupError;
    internal object? CompatibilityDiagnostics => compatibility?.Snapshot();
    internal object? RoleDiagnostics => roles?.Snapshot();
    internal object? LockDiagnostics => gate?.Snapshot();
    internal bool Enabled { get; private set; }
    internal string Status { get; private set; } = "Disabled";

    internal void Enable()
    {
        if (Enabled) return;
        if (cleanupError != null) { ResetSession(); cleanupError = null; }
        Enabled = true;
        boundBoss = boundAno = boundGame = boundWorld = boundManager = null;
        compatibility = null;
        wasBound = waitForFresh = false;
        runError = null;
        attempted = false;
        lastElapsed = double.MaxValue;
        Report("Armed for supported FRU simulations. Start a fresh simulation at normal speed.");
    }

    internal void Tick()
    {
        if (!Enabled) return;
        if (cleanupError != null) { Report("Cleanup needs attention: " + cleanupError + ". Disable/re-enable to retry."); return; }
        try { TickDependencies(); }
        catch (Exception e)
        {
            var error = e.GetBaseException().Message;
            try { ResetSession(); }
            catch (Exception cleanup) { cleanupError = cleanup.GetBaseException().Message; }
            compatibility?.Block("Discovery", error);
            Report("Integration unavailable: " + error + ". Simulation lock unavailable.");
        }
    }

    private void TickDependencies()
    {
        var ano = findPlugin("/anomech");
        var boss = findPlugin("/bmr");
        if (!ReferenceEquals(ano, boundAno) || !ReferenceEquals(boss, boundBoss))
        {
            ResetSession();
            boundAno = ano; boundBoss = boss;
            boundGame = boundWorld = boundManager = null;
            compatibility = boss != null && ano != null ? (validate?.Invoke(boss.GetType().Assembly, ano.GetType().Assembly)
                ?? new Compatibility(boss.GetType().Assembly, ano.GetType().Assembly)) : null;
        }
        if (boss == null || ano == null)
        {
            Report("Waiting for AnoMech and BossMod to load. Simulation lock unavailable.");
            return;
        }
        if (compatibility == null || !compatibility.Allows("Discovery"))
        {
            Report("Dependency interface unavailable: " + compatibility?.Reason("Discovery") + ". Simulation lock unavailable.");
            return;
        }
        var game = Read(ano.GetType(), "Game");
        var currentManager = Read(boss, "_bossmod");
        var currentWorld = Read(boss, "_ws");
        if (!ReferenceEquals(game, boundGame) || !ReferenceEquals(currentWorld, boundWorld) || !ReferenceEquals(currentManager, boundManager))
        {
            ResetSession();
            boundGame = game; boundWorld = currentWorld; boundManager = currentManager;
            waitForFresh = wasBound;
            if (game != null && currentWorld != null && currentManager != null)
            {
                wasBound = true;
                if (compatibility.Allows("SimulationLock") && compatibility.Allows("Activation"))
                {
                    lastScenario = Read(game, "ActiveScenario");
                    lastElapsed = Convert.ToDouble(Read(Read(game, "Events"), "Elapsed"));
                }
            }
        }
        if (game == null || currentManager == null || currentWorld == null)
        {
            Report("Waiting for dependency initialization. Simulation lock unavailable.");
            return;
        }
        if (!compatibility.Allows("SimulationLock"))
        {
            Report("Simulation lock unavailable: " + compatibility.Reason("SimulationLock") + ". Integration paused.");
            return;
        }
        try
        {
            gate ??= new SimulationGate(() => boundGame, () => module);
            gate.Bind(currentWorld, currentManager);
        }
        catch (Exception e)
        {
            compatibility.Block("SimulationLock", e.GetBaseException().Message);
            ResetSession();
            Report("Simulation lock unavailable: " + compatibility.Reason("SimulationLock") + ". Integration paused.");
            return;
        }
        if (!compatibility.Allows("Activation"))
        {
            Report("Mechanic activation unavailable: " + compatibility.Reason("Activation") + ". Simulation lock active.");
            return;
        }
        try { TickScenario(game, boss); }
        catch (Exception e)
        {
            runError = e.GetBaseException().Message;
            attempted = true;
            try { Release(); }
            catch (Exception cleanup) { cleanupError = cleanup.GetBaseException().Message; }
            Report("This simulation disconnected: " + runError + ". Simulation lock active; start a fresh run after resolving the issue.");
        }
    }

    private void ResetSession()
    {
        try { gate?.Dispose(); gate = null; }
        finally { Release(); }
        attempted = false;
        runError = null;
    }

    private void TickScenario(object game, object boss)
    {
        var scenario = Read(game, "ActiveScenario");
        var elapsed = Convert.ToDouble(Read(Read(game, "Events"), "Elapsed"));
        var running = Read(game, "Status")?.ToString() == "Running";
        var fresh = !ReferenceEquals(lastScenario, scenario) || elapsed < lastElapsed - 0.01;
        lastScenario = scenario;
        lastElapsed = elapsed;
        if (fresh || !running)
        {
            Release();
            attempted = false;
            runError = null;
            waitForFresh = false;
        }
        if (module != null && (!ReferenceEquals(owner, boss) || !ReferenceEquals(manager, Read(boss, "_bossmod")))) Release();
        if (!running || !IsSupported(scenario?.GetType().FullName) || boss == null)
        {
            Release();
            Report(running ? "Unsupported simulation; BossMod simulation modules blocked." : "Armed; waiting for a supported FRU simulation.");
            return;
        }
        if (waitForFresh) { Report("Dependencies reconnected. Start a fresh simulation. Simulation lock active."); return; }
        var currentScenarioName = scenario!.GetType().FullName!;
        if (!compatibility!.Allows(currentScenarioName))
        {
            Report(Entry(currentScenarioName).Label + " unavailable: " + compatibility.Reason(currentScenarioName) + ". Simulation lock active.");
            return;
        }
        if (runError != null) return;
        if (Read(game, "Paused") is true || Convert.ToDouble(Read(game, "EventTimeScale")) != 1)
        {
            Release();
            attempted = true;
            Report("Simulation paused or speed changed. Restart at normal speed to reconnect.");
            return;
        }
        if (module != null)
        {
            if (!List(manager!, "LoadedModules").Contains(module))
            {
                Release();
                Report("Bossmod unloaded the bridge module. Restart the simulation to retry.");
            }
            else if (Read(Read(boss, "_ai"), "Beh") != null || !ReferenceEquals(Read(rotation, "Preset"), forceDisable))
                throw new InvalidOperationException("Bossmod AI or rotation changed during the test; bridge disconnected.");
            else roles?.Sync(module);
            return;
        }
        if (attempted) return;
        if (elapsed > 1.5)
        {
            attempted = true;
            Report("Start was missed. Restart the simulation with the bridge enabled.");
            return;
        }
        var scenarioName = scenario!.GetType().FullName!;
        var phase4 = scenarioName is DarklitScenario or CrystallizeScenario;
        var primaryOID = Boss(scenarioName).OID;
        var world = Read(boss, "_ws") ?? throw new InvalidOperationException("Bossmod world unavailable");
        var sim = Items(Read(Read(game, "World"), "Children"))
            .FirstOrDefault(a => a.GetType().Name == "SimEnemy" && Convert.ToUInt32(Read(a, "BNpcBaseId")) == primaryOID);
        if (sim == null) return;
        var actor = Items(Read(world, "Actors")).FirstOrDefault(a =>
            Convert.ToUInt64(Read(a, "InstanceID")) == Convert.ToUInt64(Read(sim, "EntityId"))
            && Convert.ToUInt32(Read(a, "OID")) == primaryOID);
        if (actor == null) return;
        var simOracle = phase4 ? Items(Read(Read(game, "World"), "Children")).FirstOrDefault(a =>
            a.GetType().Name == "SimEnemy" && Convert.ToUInt32(Read(a, "BNpcBaseId")) == 0x45AB) : null;
        var oracle = simOracle == null ? null : Items(Read(world, "Actors")).FirstOrDefault(a =>
            Convert.ToUInt32(Read(a, "OID")) == 0x45AB && Convert.ToUInt64(Read(a, "InstanceID")) == Convert.ToUInt64(Read(simOracle, "EntityId")));
        if (phase4 && oracle == null) return;
        roles = RoleSync.Create(game!, world);
        if (roles == null) return;
        attempted = true;
        Attach(boss, world, actor, oracle, elapsed, scenarioName);
    }

    private void Attach(object boss, object world, object actor, object? oracle, double elapsed, string scenarioName)
    {
        var mgr = Read(boss, "_bossmod")!;
        if (List(mgr, "LoadedModules").Count != 0 || List(mgr, "PendingModules").Count != 0)
            throw new InvalidOperationException("Another Bossmod module is loaded. Close the demo/encounter before testing.");
        if (Read(Read(boss, "_ai"), "Beh") != null)
            throw new InvalidOperationException("Turn Bossmod AI off before enabling the bridge.");

        owner = boss;
        manager = mgr;
        savedOverride = (bool)Read(mgr, "_activeModuleOverridden")!;
        rotation = Read(boss, "_rotation")!;
        savedPreset = Read(rotation, "Preset");
        forceDisable = Read(rotation.GetType(), "ForceDisable")!;
        Write(rotation, "Preset", forceDisable);
        try
        {
            // The constructor selects its state machine using P1's OID. Clone only the managed
            // actor for construction; never change the real actor or insert a synthetic actor.
            var seed = typeof(object).GetMethod("MemberwiseClone", Flags)!.Invoke(actor, null)!;
            Write(seed, "OID", 0x459Bu);
            var type = boss.GetType().Assembly.GetType("BossMod.Dawntrail.Ultimate.FRU.FRU", true)!;
            module = Activator.CreateInstance(type, [world, seed])!;
            Write(module, "PrimaryActor", actor);
            var encounter = Boss(scenarioName);
            Write(module, encounter.BossField, actor);
            if (oracle != null) Write(module, "_bossP4Oracle", oracle);
            var machine = Read(module, "StateMachine")!;
            var phases = List(machine, "Phases");
            if (encounter.Phase >= phases.Count) throw new InvalidOperationException("Expected FRU phase is missing; review required.");
            var phase = phases[encounter.Phase]!;
            var setup = Entry(scenarioName);
            var entry = FindState(Read(phase, "InitialState")!, setup.State);
            Write(entry, "Duration", (float)Math.Max(0, setup.Delay - elapsed));
            Write(phase, "InitialState", entry);
            // Retail's phase completion condition expects the absent P1 actor to be dead.
            Write(phase, "Update", null);
            Invoke(machine, "Start", Read(world, "CurrentTime"));
            Invoke(machine, "TransitionToPhase", encounter.Phase);
            if (oracle != null)
                foreach (var name in scenarioName == DarklitScenario ? new[] { "P4AkhRhai", "P4FragmentOfFate" } : new[] { "P4FragmentOfFate" })
                    module.GetType().GetMethod("ActivateComponent")!.MakeGenericMethod(
                        type.Assembly.GetType("BossMod.Dawntrail.Ultimate.FRU." + name, true)!).Invoke(module, null);
            Invoke(mgr, "LoadModule", module, false);
            registered = true;
            if (!List(mgr, "LoadedModules").Contains(module))
                throw new InvalidOperationException("FRU was not loaded. Enable it in Bossmod's supported fights settings.");
            Write(mgr, "ActiveModule", module);
            Invoke(Read(mgr, "ModuleActivated")!, "Fire", module);
            Report($"Connected: FRU {setup.Label}. Bossmod rotation is temporarily disabled.");
        }
        catch { Release(); throw; }
    }

    internal static object FindState(object initial, uint id)
    {
        var pending = new Stack<object>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        pending.Push(initial);
        while (pending.TryPop(out var state))
        {
            if (!visited.Add(state)) continue;
            if (Convert.ToUInt32(Read(state, "ID")) == id) return state;
            foreach (var next in Items(Read(state, "NextStates"))) pending.Push(next);
        }
        throw new InvalidOperationException($"Expected Bossmod state {id:X8} is missing");
    }

    private void Release()
    {
        var owned = module;
        Exception? failure = null;
        try
        {
            if (owned != null && manager != null)
            {
                var loaded = List(manager, "LoadedModules");
                var pending = List(manager, "PendingModules");
                var index = loaded.IndexOf(owned);
                if (index >= 0)
                {
                    Invoke(Read(manager, "ModuleDeactivated")!, "Fire", owned);
                    Invoke(manager, "UnloadModule", index);
                }
                else if (pending.Contains(owned))
                {
                    pending.Remove(owned);
                    ((IDisposable)owned).Dispose();
                }
                // A failed construction/setup has not handed the instance to the manager yet.
                else if (!registered) ((IDisposable)owned).Dispose();
                if (Read(manager, "ActiveModule") == null)
                    Write(manager, "_activeModuleOverridden", savedOverride);
            }
        }
        catch (Exception e) { failure = e; }
        try
        {
            if (rotation != null && ReferenceEquals(Read(rotation, "Preset"), forceDisable))
                Write(rotation, "Preset", savedPreset);
        }
        catch (Exception e) { failure = failure == null ? e : new AggregateException(failure, e); }
        // Preserve ownership after a failed cleanup so disable/re-enable can retry it.
        if (failure != null) throw new InvalidOperationException("Owned module cleanup failed: " + failure.GetBaseException().Message, failure);
        module = owner = manager = rotation = savedPreset = forceDisable = null;
        roles = null;
        registered = false;
    }

    private bool registered;
    private void Report(string message) { if (Status != message) { Status = message; say(message); } }
    private static IEnumerable<object> Items(object? value) => value is IEnumerable items ? items.Cast<object>() : [];
    private static IList List(object value, string name) => (IList)Read(value, name)!;
    private static object? Invoke(object target, string name, params object?[] args) =>
        (target.GetType().GetMethod(name, Flags) ?? throw new MissingMethodException(target.GetType().FullName, name)).Invoke(target, args);
    internal static void Write(object target, string name, object? value)
    {
        var type = target.GetType();
        if (type.GetProperty(name, Flags) is { } property) property.SetValue(target, value);
        else if (type.GetField(name, Flags) is { } field) field.SetValue(target, value);
        else throw new MissingMemberException(type.FullName, name);
    }

    public void Dispose()
    {
        Enabled = false;
        try { ResetSession(); cleanupError = null; }
        catch (Exception e) { cleanupError = e.GetBaseException().Message; throw; }
        boundBoss = boundAno = boundGame = boundWorld = boundManager = null;
        compatibility = null;
        Status = "Disabled";
    }
}
