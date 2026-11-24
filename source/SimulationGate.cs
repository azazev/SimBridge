using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using static AnoBossBridge.Probe;

namespace AnoBossBridge;

// Only modules whose primary actor is positively identified in AnoMech are in scope.
// Never changes BossMod configuration, actor data, encounter guidance or real modules.
internal sealed class SimulationGate(Func<object?> getGame, Func<object?> getOwnedModule) : IDisposable
{
    private object? world, manager;
    private IDisposable? subscription;
    private readonly Dictionary<object, object> tracked = new(ReferenceEqualityComparer.Instance);
    private bool sweeping;
    internal string? Error { get; private set; }
    internal int Removed { get; private set; }
    internal bool Attached => subscription != null;
    internal object Snapshot() => new { Attached, Removed, Tracked = tracked.Count, Error };

    internal void Bind(object newWorld, object newManager)
    {
        if (Error != null) throw new InvalidOperationException(Error);
        if (!ReferenceEquals(world, newWorld) || !ReferenceEquals(manager, newManager))
        {
            Dispose();
            world = newWorld;
            manager = newManager;
            var ev = Read(world, "Modified")!;
            var subscribe = ev.GetType().GetMethod("Subscribe")!;
            var delegateType = subscribe.GetParameters().Single().ParameterType;
            var argument = Expression.Parameter(delegateType.GetMethod("Invoke")!.GetParameters().Single().ParameterType);
            Action<object> handler = OnOperation;
            var callback = Expression.Lambda(delegateType,
                Expression.Invoke(Expression.Constant(handler), Expression.Convert(argument, typeof(object))), argument).Compile();
            subscription = (IDisposable)subscribe.Invoke(ev, [callback])!;
        }
        Sweep();
    }

    private void OnOperation(object operation)
    {
        // Modified fires AFTER the actor operation finishes, BEFORE manager Update/drawing.
        // In particular, don't modify PendingModules inside ModuleLoaded's promotion loop.
        if (operation.GetType().Name is not ("OpFrameStart" or "OpCreate" or "OpDestroy" or "OpZoneChange")) return;
        try { Sweep(); }
        catch (Exception e)
        {
            Error = "Simulation module lock failed: " + e.GetBaseException().Message;
            subscription?.Dispose();
            subscription = null;
            // Report via Activation on its next tick; never throw into BossMod's event dispatch.
        }
    }

    private void Sweep()
    {
        if (sweeping || manager == null || world == null) return;
        sweeping = true;
        try
        {
            var loaded = List(manager, "LoadedModules");
            var pending = List(manager, "PendingModules");
            if (loaded.Count == 0 && pending.Count == 0) { tracked.Clear(); return; }

            var game = getGame();
            var scenario = Read(game, "ActiveScenario");
            var status = Read(game, "Status")?.ToString();
            var inSimulation = scenario != null && status is "Running" or "Failed" or "Succeeded" or "Frozen";
            if (!inSimulation && tracked.Count == 0) return;
            var identities = new HashSet<(ulong, uint)>();
            if (inSimulation)
                foreach (var sim in Items(Read(Read(game, "World"), "Children")))
                    if (sim.GetType().Name == "SimEnemy")
                        identities.Add((Convert.ToUInt64(Read(sim, "EntityId")), Convert.ToUInt32(Read(sim, "BNpcBaseId"))));
            var actors = new HashSet<object>(ReferenceEqualityComparer.Instance);
            if (identities.Count != 0)
                foreach (var actor in Items(Read(world, "Actors")))
                    if (identities.Contains((Convert.ToUInt64(Read(actor, "InstanceID")), Convert.ToUInt32(Read(actor, "OID")))))
                        actors.Add(actor);

            var all = loaded.Cast<object>().Concat(pending.Cast<object>()).Distinct(ReferenceEqualityComparer.Instance).ToArray();
            foreach (var stale in tracked.Keys.Where(m => !all.Contains(m, ReferenceEqualityComparer.Instance)).ToArray()) tracked.Remove(stale);
            foreach (var module in all)
            {
                var primary = Read(module, "PrimaryActor")!;
                var current = actors.Contains(primary);
                if (current) tracked[module] = primary;
                else if (!tracked.TryGetValue(module, out var previous) || !ReferenceEquals(previous, primary))
                {
                    // A different actor object (even with reused IDs) is not ours to clean up.
                    tracked.Remove(module);
                    continue;
                }
                if (ReferenceEquals(module, getOwnedModule()))
                {
                    // Activation owns its full teardown, including rotation restoration.
                    continue;
                }
                // Only Activation's correctly initialized module is permitted, even for an
                // allowlisted scenario. A natively discovered duplicate may be in the wrong phase.
                Remove(module, loaded, pending);
                tracked.Remove(module);
            }
        }
        finally { sweeping = false; }
    }

    private void Remove(object module, IList loaded, IList pending)
    {
        var index = loaded.IndexOf(module);
        if (index >= 0)
        {
            if (Read(Read(module, "StateMachine"), "ActiveState") != null)
                Invoke(Read(manager, "ModuleDeactivated")!, "Fire", module);
            Invoke(manager!, "UnloadModule", index);
        }
        else if (pending.Contains(module))
        {
            pending.Remove(module);
            ((IDisposable)module).Dispose();
        }
        else return;
        ++Removed;
    }

    private static IEnumerable<object> Items(object? value) => value is IEnumerable items ? items.Cast<object>() : [];
    private static IList List(object target, string name) => (IList)Read(target, name)!;
    private static object? Invoke(object target, string name, params object?[] args) =>
        target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(target, args);

    public void Dispose()
    {
        subscription?.Dispose();
        subscription = null;
        if (manager != null)
        {
            foreach (var pair in tracked.ToArray())
            {
                if (!ReferenceEquals(pair.Key, getOwnedModule()) && ReferenceEquals(Read(pair.Key, "PrimaryActor"), pair.Value))
                    Remove(pair.Key, List(manager, "LoadedModules"), List(manager, "PendingModules"));
                tracked.Remove(pair.Key);
            }
        }
        tracked.Clear(); world = manager = null;
    }
}
