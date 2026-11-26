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
    internal const int PredationPhase = 3, SuppressionPhase = 4;
    internal const string ExaflaresScenario = "AnoMech.Scenarios.Ucob.P5Exaflares.UcobP5ExaflaresScenario";
    internal const string UcobNamespace = "BossMod.Stormblood.Ultimate.UCOB.";
    internal const int ExaflaresPhase = 5;
    internal const uint BahamutOid = 0x1FE8;
    internal const string SuppressionScenario = "AnoMech.Scenarios.Uwu.UltimateSuppression.UltimateSuppressionScenario";
    internal const string PredationScenario = "AnoMech.Scenarios.Uwu.UltimatePredation.UltimatePredationScenario";
    internal static bool IsSupported(string? scenario) => scenario is Scenario or DarklitScenario or RelativityScenario
        or CrystallizeScenario or DiamondScenario or FulgentScenario or LightRampantScenario or ParadiseScenario or PredationScenario or SuppressionScenario or ExaflaresScenario;
    internal static (uint State, float Delay, string Label) Entry(string scenario) => scenario switch
    {
        ExaflaresScenario => (0x05030000u, 3f, "P5 Exaflares"),
        SuppressionScenario => (0x04000000u, 2.5f, "Ultimate Suppression"),
        PredationScenario => (0x03020000u, 2.5f, "Ultimate Predation"),
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
    internal object? ExaflaresDiagnostics
    {
        get
        {
            if (module?.GetType().FullName != UcobNamespace + "UCOB") return null;
            var component = Items(Read(module, "Components")).FirstOrDefault(c => c.GetType().FullName == UcobNamespace + "P5Exaflare");
            return component == null ? null : new { NumCasts = Read(component, "NumCasts"), ActiveLines = (Read(component, "Lines") as ICollection)?.Count };
        }
    }
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
        Report("Armed for supported simulations. Start a fresh simulation at normal speed.");
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
            Report(running ? "Unsupported simulation; BossMod simulation modules blocked." : "Armed; waiting for a supported simulation.");
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
        var world = Read(boss, "_ws") ?? throw new InvalidOperationException("Bossmod world unavailable");
        var isUwu = scenarioName is PredationScenario or SuppressionScenario;
        object? FindActor(uint oid)
        {
            var matches = Items(Read(Read(game, "World"), "Children")).Where(a =>
                a.GetType().Name == "SimEnemy" && Convert.ToUInt32(Read(a, "BNpcBaseId")) == oid);
            var sim = isUwu || scenarioName == ExaflaresScenario ? matches.SingleOrDefault() : matches.FirstOrDefault();
            if (sim == null) return null;
            var id = Convert.ToUInt64(Read(sim, "EntityId"));
            return id == 0 ? null : Items(Read(world, "Actors")).FirstOrDefault(a =>
                Convert.ToUInt64(Read(a, "InstanceID")) == id && Convert.ToUInt32(Read(a, "OID")) == oid);
        }

        if (scenarioName == ExaflaresScenario)
        {
            var bahamut = FindActor(BahamutOid);
            if (bahamut == null || !ExaflareLanesReady(scenario!, world)) return;
            attempted = true;
            Attach(boss, world, bahamut, null, elapsed, scenarioName);
            return;
        }
        if (isUwu)
        {
            var garuda = FindActor(0x2212);
            var ifrit = FindActor(0x221A);
            var titan = FindActor(0x2217);
            var ultima = FindActor(0x221E);
            if (garuda == null || ifrit == null || titan == null || ultima == null) return;
            if (scenarioName == SuppressionScenario && !SimActorsReady(game, world, 0x2213, 2)) return;
            attempted = true;
            Attach(boss, world, garuda, null, elapsed, scenarioName, new(ifrit, titan, ultima));
            return;
        }

        var actor = FindActor(Boss(scenarioName).OID);
        if (actor == null) return;
        var phase4 = scenarioName is DarklitScenario or CrystallizeScenario;
        var oracle = phase4 ? FindActor(0x45AB) : null;
        if (phase4 && oracle == null) return;
        roles = RoleSync.Create(game, world);
        if (roles == null) return;
        attempted = true;
        Attach(boss, world, actor, oracle, elapsed, scenarioName);
    }

    internal static bool ExaflareLanesReady(object scenario, object world)
    {
        // The simulation can also spawn a general-purpose helper with this OID.
        // Only the scenario's six assigned lane actors have to be present.
        const uint helperOid = 0x18D6;
        var lanes = Items(Read(scenario, "laneHelpers")).ToArray();
        if (lanes.Length != 6) return false;
        var expected = new HashSet<ulong>();
        foreach (var lane in lanes)
        {
            if (lane == null || Convert.ToUInt32(Read(lane, "BNpcBaseId")) != helperOid) return false;
            var id = Convert.ToUInt64(Read(lane, "EntityId"));
            if (id == 0 || !expected.Add(id)) return false;
        }
        var observed = Items(Read(world, "Actors")).Where(a => Convert.ToUInt32(Read(a, "OID")) == helperOid)
            .Select(a => Convert.ToUInt64(Read(a, "InstanceID")));
        return expected.IsSubsetOf(observed);
    }

    // Match every helper by its AnoMech identity, including multiple enemies with a shared OID.
    internal static bool SimActorsReady(object game, object world, uint oid, int required)
    {
        var simulated = Items(Read(Read(game, "World"), "Children"))
            .Where(a => a.GetType().Name == "SimEnemy" && Convert.ToUInt32(Read(a, "BNpcBaseId")) == oid)
            .Select(a => Convert.ToUInt64(Read(a, "EntityId"))).ToArray();
        if (simulated.Length != required || simulated.Contains(0UL) || simulated.Distinct().Count() != required) return false;
        var observed = Items(Read(world, "Actors")).Where(a => Convert.ToUInt32(Read(a, "OID")) == oid)
            .Select(a => Convert.ToUInt64(Read(a, "InstanceID"))).ToHashSet();
        return observed.SetEquals(simulated);
    }

    internal sealed record UwuActors(object Ifrit, object Titan, object Ultima)
    {
        internal void Bind(object encounter, object garuda)
        {
            // In the full encounter BossMod remembers Ifrit during P2.
            Write(encounter, "_mainIfrit", Ifrit);
            if (!ReferenceEquals(Invoke(encounter, "Garuda"), garuda)
                || !ReferenceEquals(Invoke(encounter, "Ifrit"), Ifrit)
                || !ReferenceEquals(Invoke(encounter, "Titan"), Titan)
                || !ReferenceEquals(Invoke(encounter, "Ultima"), Ultima))
                throw new InvalidOperationException("UWU actor identities do not match this simulation.");
        }
    }

    internal static object ConstructorSeed(object actor, uint oid)
    {
        var seed = typeof(object).GetMethod("MemberwiseClone", Flags)!.Invoke(actor, null)!;
        Write(seed, "OID", oid);
        return seed;
    }

    internal static object ExaflaresSeed(object bahamut)
    {
        // Native UCOB uses Twintania as its primary throughout the encounter. In P5
        // she is gone. This managed constructor argument never enters either world.
        var seed = ConstructorSeed(bahamut, 0x1FDF);
        Write(seed, "IsDestroyed", true);
        Write(seed, "IsTargetable", false);
        return seed;
    }

    internal static void StartExaflares(object machine, DateTime now)
    {
        // Start() enters P1 and installs Twintania components that can span phases.
        // Initialize the two native Start clocks and enter P5 directly.
        Write(machine, "_activation", now);
        Write(machine, "_curTime", now);
        Invoke(machine, "TransitionToPhase", ExaflaresPhase);
    }

    private void Attach(object boss, object world, object actor, object? oracle, double elapsed,
        string scenarioName, UwuActors? uwuActors = null)
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
            int phaseIndex;
            bool isExaflares = scenarioName == ExaflaresScenario;
            if (isExaflares)
            {
                var type = boss.GetType().Assembly.GetType(UcobNamespace + "UCOB", true)!;
                module = Activator.CreateInstance(type, [world, ExaflaresSeed(actor)])!;
                Write(module, "_bahamutPrime", actor);
                if (!ReferenceEquals(Invoke(module, "BahamutPrime"), actor) || Invoke(module, "Twintania") != null)
                    throw new InvalidOperationException("UCoB P5 actor binding failed.");
                phaseIndex = ExaflaresPhase;
            }
            else if (uwuActors != null)
            {
                var type = boss.GetType().Assembly.GetType("BossMod.Stormblood.Ultimate.UWU.UWU", true)!;
                module = Activator.CreateInstance(type, [world, actor])!;
                uwuActors.Bind(module, actor);
                phaseIndex = scenarioName == SuppressionScenario ? SuppressionPhase : PredationPhase;
            }
            else
            {
                // The constructor selects its state machine using P1's OID. Clone only the managed
                // actor for construction; never change the real actor or insert a synthetic actor.
                var seed = ConstructorSeed(actor, 0x459Bu);
                var type = boss.GetType().Assembly.GetType("BossMod.Dawntrail.Ultimate.FRU.FRU", true)!;
                module = Activator.CreateInstance(type, [world, seed])!;
                Write(module, "PrimaryActor", actor);
                var encounter = Boss(scenarioName);
                Write(module, encounter.BossField, actor);
                if (oracle != null) Write(module, "_bossP4Oracle", oracle);
                phaseIndex = encounter.Phase;
            }
            var machine = Read(module, "StateMachine")!;
            var phases = List(machine, "Phases");
            if (phaseIndex >= phases.Count) throw new InvalidOperationException("Expected encounter phase is missing; review required.");
            var phase = phases[phaseIndex]!;
            var setup = Entry(scenarioName);
            var entry = FindState(Read(phase, "InitialState")!, setup.State);
            Write(entry, "Duration", (float)Math.Max(0, setup.Delay - elapsed));
            Write(phase, "InitialState", entry);
            // Retail's phase completion condition expects the absent P1 actor to be dead.
            if (!isExaflares && uwuActors == null) Write(phase, "Update", null);
            var now = (DateTime)Read(world, "CurrentTime")!;
            if (isExaflares) StartExaflares(machine, now);
            else
            {
                Invoke(machine, "Start", now);
                Invoke(machine, "TransitionToPhase", phaseIndex);
            }
            if (oracle != null)
                foreach (var name in scenarioName == DarklitScenario ? new[] { "P4AkhRhai", "P4FragmentOfFate" } : new[] { "P4FragmentOfFate" })
                    module.GetType().GetMethod("ActivateComponent")!.MakeGenericMethod(
                        module.GetType().Assembly.GetType("BossMod.Dawntrail.Ultimate.FRU." + name, true)!).Invoke(module, null);
            Invoke(mgr, "LoadModule", module, false);
            registered = true;
            if (!List(mgr, "LoadedModules").Contains(module))
                throw new InvalidOperationException("Encounter module was not loaded. Enable it in Bossmod's supported fights settings.");
            Write(mgr, "ActiveModule", module);
            Invoke(Read(mgr, "ModuleActivated")!, "Fire", module);
            var encounterName = isExaflares ? "UCoB" : uwuActors == null ? "FRU" : "UWU";
            Report($"Connected: {encounterName} {setup.Label}. Bossmod rotation is temporarily disabled.");
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
