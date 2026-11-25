using System.Reflection;
using System.Reflection.Emit;
using AnoBossBridge;
using static SimulationGateTests;

internal static class ResilienceTests
{
    private static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
    internal static void Run()
    {
        Assembly Make(int version, Type fieldType, int value, bool noise)
        {
            var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("ContractFixture" + version) { Version = new(version, 0, 0, 0) }, AssemblyBuilderAccess.RunAndCollect);
            var module = assembly.DefineDynamicModule("Main");
            if (noise) module.DefineType("UnrelatedAddedType").CreateType();
            var type = module.DefineType("ContractFixture", TypeAttributes.Public);
            type.DefineField("Value", fieldType, FieldAttributes.Public);
            var il = type.DefineMethod("Calculate", MethodAttributes.Public | MethodAttributes.Static, typeof(int), Type.EmptyTypes).GetILGenerator();
            il.Emit(OpCodes.Ldc_I4, value); il.Emit(OpCodes.Ret);
            type.CreateType(); return assembly;
        }
        var baseline = Make(1, typeof(int), 10, false);
        var type = baseline.GetType("ContractFixture")!;
        var requirement = new TypeContract("BossMod", type.FullName!, [new("Value", [ContractShape.Shape(type.GetField("Value")!)], null),
            new("Calculate", [ContractShape.Shape(type.GetMethod("Calculate")!)], [ContractShape.Fingerprint(type.GetMethod("Calculate")!)])]);
        var profile = new ContractProfile(new() { ["SimulationLock"] = [], [Activation.DiamondScenario] = [requirement], [Activation.FulgentScenario] = [] });
        var updated = new Compatibility(Make(999, typeof(int), 10, true), baseline, profile);
        Check(updated.Allows(Activation.DiamondScenario), "Unseen version and unrelated added type are accepted");
        var wrongShape = new Compatibility(Make(2, typeof(string), 10, false), baseline, profile);
        Check(!wrongShape.Allows(Activation.DiamondScenario) && wrongShape.Allows("SimulationLock") && wrongShape.Allows(Activation.FulgentScenario), "Changed field isolates one mechanic");
        var wrongBody = new Compatibility(Make(3, typeof(int), 11, false), baseline, profile);
        Check(wrongBody.Reason(Activation.DiamondScenario).Contains("behavior changed"), "Same signature with changed behavior requires review");
        var anoProfile = new ContractProfile(new() { ["SimulationLock"] = [requirement with { Dependency = "AnoMech" }] });
        Check(!new Compatibility(baseline, Make(4, typeof(long), 10, false), anoProfile).Allows("SimulationLock"), "AnoMech receives the same contract validation");

        var all = new ContractProfile(new[] { "Discovery", "SimulationLock", "Activation" }.Concat(Compatibility.Scenarios).ToDictionary(s => s, _ => Array.Empty<TypeContract>()));
        var validations = 0;
        Compatibility Validate(Assembly b, Assembly a) { ++validations; return new(b, a, all); }
        Ano.Game = new Game { Status = "Running", ActiveScenario = new AnoMech.Scenarios.Fru.P2DiamondDust.FruP2DiamondDustScenario() };
        object? ano = new Ano(); object? boss = new Boss();
        using var activation = new Activation(command => command == "/bmr" ? boss : ano, _ => { }, Validate);
        activation.Enable(); activation.Tick();
        var old = (Boss)boss;
        Check(old._ws.Modified.Subscribers == 1 && validations == 1, "Validated once and subscribed");
        for (var i = 0; i < 20; ++i) activation.Tick();
        Check(validations == 1, "No repeated compatibility scanning on frames");
        var faultActor = new SimulationGateTests.Actor(25, 0x459F);
        var faultOwned = new SimulationGateTests.Module(faultActor); old._bossmod.LoadedModules.Add(faultOwned);
        Set(activation, "module", faultOwned); Set(activation, "manager", old._bossmod); Set(activation, "owner", old); Set(activation, "registered", true);
        // Fixture intentionally lacks _ai; this exercises the actual exception path, not a status injection.
        activation.Tick();
        Check(faultOwned.Disposals == 1 && Get(activation, "runError") != null && old._ws.Modified.Subscribers == 1,
            "Actual mechanic exception tears down ownership without dropping the gate");
        Ano.Game.World.Children.Add(new SimEnemy(20, 19504)); var actor = new SimulationGateTests.Actor(20, 19504); old._ws.Actors.Add(actor);
        var unsupported = new SimulationGateTests.Module(actor); old._bossmod.PendingModules.Add(unsupported);
        old._ws.Modified.Fire(new OpFrameStart());
        Check(unsupported.Disposals == 1 && activation.Enabled, "Mechanic failure leaves unsupported lock running");
        Ano.Game.Events.Elapsed = 12;
        boss = new Boss(); activation.Tick();
        Check(old._ws.Modified.Subscribers == 0 && ((Boss)boss)._ws.Modified.Subscribers == 1 && validations == 2 && activation.Status.Contains("fresh simulation"), "Reload detaches stale callbacks and fences an active run");
        Ano.Game.Events.Elapsed = 0; activation.Tick();
        Check(!(bool)Get(activation, "waitForFresh")!, "Restart unlocks reconnect");
        var previousWorld = ((Boss)boss)._ws;
        ano = null; activation.Tick();
        Check(previousWorld.Modified.Subscribers == 0 && activation.Enabled && activation.Status.Contains("Waiting"), "Dependency disappearance waits while enabled");
        ano = new Ano(); activation.Tick();
        Check(previousWorld.Modified.Subscribers == 1 && validations == 3, "Dependency return is revalidated");
        activation.Dispose(); Check(previousWorld.Modified.Subscribers == 0, "Disable detaches protection");

        // Shared mechanic failure must not disable the independently verified gate.
        var failedCore = all with { Groups = new(all.Groups) { ["Activation"] = [requirement] } };
        using var unavailable = new Activation(command => command == "/bmr" ? boss : ano, _ => { }, (b, a) => new(b, a, failedCore));
        unavailable.Enable(); unavailable.Tick();
        Check(previousWorld.Modified.Subscribers == 1 && unavailable.Status.Contains("activation unavailable"), "Core activation contract can fail with lock intact");
        unavailable.Dispose();

        var failedLock = all with { Groups = new(all.Groups) { ["SimulationLock"] = [requirement] } };
        using var lockUnavailable = new Activation(command => command == "/bmr" ? boss : ano, _ => { }, (b, a) => new(b, a, failedLock));
        lockUnavailable.Enable(); lockUnavailable.Tick();
        Check(previousWorld.Modified.Subscribers == 0 && lockUnavailable.Status.Contains("Integration paused"), "Failed lock contract never advertises protection or activates a mechanic");

        // Failed unload keeps ownership so a later cleanup attempt can finish.
        var manager = new Manager { ThrowUnload = true }; var owned = new SimulationGateTests.Module(actor); manager.LoadedModules.Add(owned);
        var saved = new object(); var forced = new object(); var rotation = new FakeRotation { Preset = forced };
        using var cleanup = new Activation(_ => null, _ => { });
        Set(cleanup, "module", owned); Set(cleanup, "manager", manager); Set(cleanup, "rotation", rotation);
        Set(cleanup, "savedPreset", saved); Set(cleanup, "forceDisable", forced); Set(cleanup, "registered", true);
        try { cleanup.Dispose(); } catch (InvalidOperationException) { }
        Check(ReferenceEquals(Get(cleanup, "module"), owned) && ReferenceEquals(rotation.Preset, saved), "Failed unload preserves ownership and still restores rotation");
        manager.ThrowUnload = false; cleanup.Dispose();
        Check(owned.Disposals == 1 && Get(cleanup, "module") == null, "Cleanup retry succeeds once");
        Console.WriteLine("PASS 9/10: future versions, changed signatures/bodies, AnoMech checks, feature isolation, validation caching, reload/restart fencing and cleanup retry");
    }
    private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(target, value);
    private static object? Get(object target, string field) => target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(target);
    private sealed class Ano { public static Game Game { get; set; } = new(); }
    private sealed class Boss { public World _ws = new(); public Manager _bossmod = new(); }
}
namespace AnoMech.Scenarios.Fru.P2DiamondDust { internal sealed class FruP2DiamondDustScenario; }



