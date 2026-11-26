using AnoBossBridge;

internal static class SimulationGateTests
{
    private static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
    internal static void Run()
    {
        var sistersGame = new Game();
        var sistersWorld = new World();
        sistersGame.World.Children.Add(new SimEnemy(10, 0x2213));
        sistersGame.World.Children.Add(new SimEnemy(11, 0x2213));
        sistersWorld.Actors.Add(new Actor(11, 0x2213));
        if (Activation.SimActorsReady(sistersGame, sistersWorld, 0x2213, 2)) throw new Exception("Missing sister accepted.");
        sistersWorld.Actors.Add(new Actor(10, 0x2213));
        if (!Activation.SimActorsReady(sistersGame, sistersWorld, 0x2213, 2)) throw new Exception("Reversed sister order rejected.");
        sistersWorld.Actors.Add(new Actor(12, 0x2213));
        if (Activation.SimActorsReady(sistersGame, sistersWorld, 0x2213, 2)) throw new Exception("Unrelated sister accepted.");

        var lanes = new ExaflareScenario();
        var arena = new World();
        arena.Actors.Add(new Actor(7, 0x18D6)); // extra shared-OID helper observed in the live recording
        for (uint i = 1; i <= 6; ++i)
        {
            lanes.laneHelpers.Add(new SimEnemy(i, 0x18D6));
            arena.Actors.Add(new Actor(i, 0x18D6));
        }
        Check(Activation.ExaflareLanesReady(lanes, arena), "Six identified lanes plus a seventh helper accepted");
        arena.Actors.RemoveAt(6);
        Check(!Activation.ExaflareLanesReady(lanes, arena), "Unrelated helper cannot replace a missing lane");
        arena.Actors.Add(new Actor(6, 0x2213));
        Check(!Activation.ExaflareLanesReady(lanes, arena), "Correct instance ID with wrong OID rejected");
        lanes.laneHelpers[5] = lanes.laneHelpers[0];
        Check(!Activation.ExaflareLanesReady(lanes, arena), "Duplicate lane identity rejected");
        lanes.laneHelpers[5] = null;
        Check(!Activation.ExaflareLanesReady(lanes, arena), "Unspawned lane rejected");
        string[] allowed = [Activation.Scenario, Activation.DarklitScenario, Activation.RelativityScenario,
            Activation.CrystallizeScenario, Activation.DiamondScenario, Activation.FulgentScenario,
            Activation.LightRampantScenario, Activation.ParadiseScenario, Activation.ExaflaresScenario];
        Check(allowed.All(Activation.IsSupported) && !Activation.IsSupported(null)
            && !Activation.IsSupported("AnoMech.Scenarios.Fru.P1UtopianSky.FruP1UtopianSkyScenario")
            && !Activation.IsSupported("AnoMech.Scenarios.Umad.P1TeleTrouncing.UmadP1TeleTrouncingScenario"), "Exact scenario allowlist, not encounter names");

        var game = new Game();
        var world = new World();
        var manager = new Manager();
        object? owned = null;
        using var gate = new SimulationGate(() => game, () => owned);
        gate.Bind(world, manager);
        var realActor = new Actor(10, 19504);
        world.Actors.Add(realActor);
        var real = new Module(realActor);
        manager.LoadedModules.Add(real); manager.ActiveModule = real;
        world.Modified.Fire(new OpFrameStart());
        Check(real.Disposals == 0 && ReferenceEquals(manager.ActiveModule, real), "Offline real fight is untouched");

        game.Status = "Running"; game.ActiveScenario = new object();
        var simActor = new Actor(20, 19504);
        game.World.Children.Add(new SimEnemy(20, 19504));
        world.Actors.Add(simActor);
        var simModule = new Module(simActor);
        // Native discovery completes BEFORE Modified(OpCreate), then manager Update/draw.
        manager.LoadedModules.Add(simModule); manager.ActiveModule = simModule;
        world.Modified.Fire(new OpCreate());
        Check(simModule.Disposals == 1 && !manager.LoadedModules.Contains(simModule) && manager.ActiveModule == null
            && manager.Deactivations == 1 && real.Disposals == 0, "Unsupported discovered module removed before drawing, real same-OID module preserved");

        var pending = new Module(simActor);
        manager.PendingModules.Add(pending);
        world.Modified.Fire(new OpFrameStart());
        Check(pending.Disposals == 1 && manager.PendingModules.Count == 0, "Pending module removed before promotion");
        for (var i = 0; i < 3; ++i)
        {
            var retry = new Module(simActor); manager.LoadedModules.Add(retry);
            world.Modified.Fire(new OpCreate());
            Check(retry.Disposals == 1, "Repeated actor discovery is suppressed");
        }

        var bridgeModule = new Module(simActor); owned = bridgeModule;
        manager.LoadedModules.Add(bridgeModule);
        world.Modified.Fire(new OpFrameStart());
        Check(bridgeModule.Disposals == 0, "Owned supported module retains native guidance");
        var duplicate = new Module(simActor); manager.LoadedModules.Add(duplicate);
        world.Modified.Fire(new OpCreate());
        Check(duplicate.Disposals == 1 && bridgeModule.Disposals == 0, "Only correctly initialized owned module remains");

        // Retain the exact module/actor pair while observed. No OID-only cleanup after exit.
        owned = null; game.Status = "Offline"; game.ActiveScenario = null;
        game.World.Children.Clear(); world.Actors.Remove(simActor);
        world.Modified.Fire(new OpFrameStart());
        Check(bridgeModule.Disposals == 1 && real.Disposals == 0, "Previously observed simulation module cleaned after all sim actors disappear");

        game.Status = "Frozen"; game.ActiveScenario = new object();
        game.World.Children.Add(new SimEnemy(20, 19504)); world.Actors.Add(simActor);
        var frozen = new Module(simActor); manager.PendingModules.Add(frozen);
        world.Modified.Fire(new OpFrameStart());
        Check(frozen.Disposals == 1, "Frozen/failed simulation cannot retain an unsupported module");

        var reused = new Module(simActor); manager.LoadedModules.Add(reused); owned = reused;
        world.Modified.Fire(new OpFrameStart());
        var replacementActor = new Actor(20, 19504);
        reused.PrimaryActor = replacementActor; world.Actors.Remove(simActor); world.Actors.Add(replacementActor);
        game.Status = "Offline"; game.ActiveScenario = null; game.World.Children.Clear(); owned = null;
        world.Modified.Fire(new OpFrameStart());
        Check(reused.Disposals == 0, "Reused IDs with a different actor object never authorize real-world cleanup");

        // A coincident ID with mismatched OID is not a simulated actor.
        game.Status = "Running"; game.ActiveScenario = new object(); game.World.Children.Add(new SimEnemy(20, 999));
        world.Modified.Fire(new OpFrameStart());
        Check(reused.Disposals == 0, "Both instance ID and OID must match");
        gate.Dispose(); gate.Dispose();
        Check(world.Modified.Subscribers == 0, "Disable/unload detaches event callback exactly once");
        game.World.Children.Clear(); game.World.Children.Add(new SimEnemy(20, 19504));
        world.Modified.Fire(new OpFrameStart());
        Check(reused.Disposals == 0, "Disabled lock does not intercept native BossMod");

        using var reload = new SimulationGate(() => game, () => null);
        reload.Bind(world, manager);
        Check(reused.Disposals == 1 && world.Modified.Subscribers == 1, "Re-enable reconciles an active simulation");
        var world2 = new World(); var manager2 = new Manager();
        reload.Bind(world2, manager2);
        Check(world.Modified.Subscribers == 0 && world2.Modified.Subscribers == 1 && real.Disposals == 0, "BossMod reload replaces subscriptions without removing unrelated modules");

        var fail = false;
        using var fault = new SimulationGate(() => fail ? throw new InvalidOperationException("test fault") : game, () => null);
        var faultWorld = new World(); var faultManager = new Manager(); faultManager.LoadedModules.Add(new Module(realActor));
        fault.Bind(faultWorld, faultManager); fail = true;
        faultWorld.Modified.Fire(new OpFrameStart());
        Check(fault.Error?.Contains("test fault") == true && faultWorld.Modified.Subscribers == 0, "Callback failure detaches and never throws into BossMod");
        var rejected = false;
        try { fault.Bind(faultWorld, faultManager); } catch (InvalidOperationException) { rejected = true; }
        Check(rejected, "Deferred fault reaches bridge failure handler on next tick");
        Console.WriteLine("PASS 7/10: simulation-only allowlist lifecycle, pre-draw loaded/pending removal, retries, exit, identity reuse, disable/reload and fault isolation");
    }

    internal sealed class Events { public double Elapsed = 0; }
    private sealed class ExaflareScenario { public List<SimEnemy?> laneHelpers = []; }
    internal sealed class Game { public object? ActiveScenario; public string Status = "Offline"; public SimWorld World = new(); public Events Events = new(); public bool Paused = false; public double EventTimeScale = 1; }
    internal sealed class SimWorld { public List<SimEnemy> Children = []; }
    internal sealed class SimEnemy(ulong id, uint oid) { public ulong EntityId = id; public uint BNpcBaseId = oid; }
    internal sealed class Actor(ulong id, uint oid) { public ulong InstanceID = id; public uint OID = oid; }
    internal sealed class World { public List<Actor> Actors = []; public Signal Modified = new(); }
    internal sealed class OpFrameStart;
    internal sealed class OpCreate;
    internal sealed class Signal
    {
        private event Action<object>? callbacks;
        public int Subscribers;
        public IDisposable Subscribe(Action<object> action) { callbacks += action; ++Subscribers; return new Subscription(() => { callbacks -= action; --Subscribers; }); }
        public void Fire(object value) => callbacks?.Invoke(value);
    }
    internal sealed class Subscription(Action dispose) : IDisposable { public void Dispose() => dispose(); }
    internal sealed class Machine { public object? ActiveState = new(); }
    internal sealed class Module(Actor actor) : IDisposable
    {
        public Actor PrimaryActor = actor;
        public Machine StateMachine = new();
        public int Disposals;
        public void Dispose() { if (++Disposals != 1) throw new Exception("Double dispose"); }
    }
    internal sealed class Manager
    {
        public List<Module> LoadedModules = [], PendingModules = [];
        public Module? ActiveModule;
        public Signal ModuleDeactivated = new();
        public int Deactivations; public bool ThrowUnload = false; public bool _activeModuleOverridden = false;
        public Manager() => ModuleDeactivated.Subscribe(_ => ++Deactivations);
        private void UnloadModule(int index)
        {
            if (ThrowUnload) throw new InvalidOperationException("Injected unload failure");
            var module = LoadedModules[index];
            if (ReferenceEquals(ActiveModule, module)) ActiveModule = null;
            module.Dispose(); LoadedModules.RemoveAt(index);
        }
    }
}



