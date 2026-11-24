using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace AnoBossBridge;

internal sealed class Probe(Func<string, object?> findPlugin) : IDisposable
{
    private object? observedWorld;
    private IDisposable? subscription;
    private readonly Dictionary<string, int> operations = [];

    internal object Capture(object? bridge = null)
    {
        var ano = findPlugin("/anomech");
        var boss = findPlugin("/bmr");
        var game = ano == null ? null : Read(ano.GetType(), "Game");
        var manager = Read(boss, "_bossmod");
        var world = Read(boss, "_ws");
        Observe(world);

        var actors = Items(Read(world, "Actors")).ToArray();
        var actorIds = actors.Select(a => Number(Read(a, "InstanceID"))).ToHashSet();
        var simulated = Items(Read(Read(game, "World"), "Children"))
            .Where(c => c.GetType().Name == "SimEnemy").Select(c => new
            {
                EntityId = Number(Read(c, "EntityId")),
                OID = Number(Read(c, "BNpcBaseId")),
                Targetable = Read(c, "Targetable"),
                SeenByBossmod = actorIds.Contains(Number(Read(c, "EntityId")))
            }).ToArray();

        var scenario = Read(game, "ActiveScenario");
        return new
        {
            TimestampUtc = DateTimeOffset.UtcNow,
            Bridge = bridge,
            AnoMech = new
            {
                Loaded = ano != null,
                Version = ano?.GetType().Assembly.GetName().Version?.ToString(),
                SimulationAvailable = game != null,
                Status = Read(game, "Status")?.ToString(),
                Scenario = scenario?.GetType().FullName,
                Paused = Read(game, "Paused"),
                Speed = Read(game, "EventTimeScale"),
                Elapsed = Read(Read(game, "Events"), "Elapsed"),
                Enemies = simulated
            },
            BossmodReborn = new
            {
                Loaded = boss != null,
                Version = boss?.GetType().Assembly.GetName().Version?.ToString(),
                WorldAvailable = world != null,
                EventSubscription = subscription != null,
                Zone = Read(world, "CurrentZone"),
                ActorCount = actors.Length,
                PartyCount = Items(Read(Read(world, "Party"), "Members"))
                    .Take(8).Count(m => Number(Read(m, "InstanceId")) != 0),
                Active = Module(Read(manager, "ActiveModule")),
                LoadedModules = Items(Read(manager, "LoadedModules")).Select(Module).ToArray(),
                PendingModules = Items(Read(manager, "PendingModules")).Select(Module).ToArray(),
                Enemies = actors.Where(a => Read(a, "Type")?.ToString() == "Enemy")
                    .Select(a => new
                    {
                        EntityId = Number(Read(a, "InstanceID")),
                        OID = Number(Read(a, "OID")),
                        Targetable = Read(a, "IsTargetable"),
                        InCombat = Read(a, "InCombat"),
                        CastAction = Read(Read(Read(a, "CastInfo"), "Action"), "ID")
                    }).ToArray(),
                OperationsSincePreviousSnapshot = DrainOperations()
            }
        };
    }

    private static object? Module(object? module)
    {
        if (module == null) return null;
        var machine = Read(module, "StateMachine");
        var state = Read(machine, "ActiveState");
        var primary = Read(module, "PrimaryActor");
        return new
        {
            Type = module.GetType().FullName,
            PrimaryOID = Number(Read(primary, "OID")),
            Targetable = Read(primary, "IsTargetable"),
            InCombat = Read(primary, "InCombat"),
            Phase = Read(machine, "ActivePhaseIndex"),
            StateId = Read(state, "ID"),
            StateName = Read(state, "Name")
        };
    }

    private void Observe(object? world)
    {
        if (ReferenceEquals(world, observedWorld)) return;
        Dispose();
        if (world == null) return;
        var ev = Read(world, "Modified") ?? throw new MissingMemberException("WorldState.Modified is null");
        var subscribe = ev.GetType().GetMethod("Subscribe")
            ?? throw new MissingMethodException("WorldState.Modified.Subscribe");
        var delegateType = subscribe.GetParameters().Single().ParameterType;
        var argumentType = delegateType.GetMethod("Invoke")!.GetParameters().Single().ParameterType;
        var argument = Expression.Parameter(argumentType);
        Action<object> count = op =>
        {
            lock (operations)
            {
                var key = op.GetType().Name;
                operations[key] = operations.GetValueOrDefault(key) + 1;
            }
        };
        var callback = Expression.Lambda(delegateType,
            Expression.Invoke(Expression.Constant(count), Expression.Convert(argument, typeof(object))), argument).Compile();
        subscription = subscribe.Invoke(ev, [callback]) as IDisposable
            ?? throw new InvalidOperationException("Bossmod event subscription is not disposable");
        observedWorld = world;
    }

    private Dictionary<string, int> DrainOperations()
    {
        lock (operations)
        {
            var result = new Dictionary<string, int>(operations);
            operations.Clear();
            return result;
        }
    }

    internal static object? Read(object? target, string name)
    {
        if (target == null) return null;
        var type = target as Type ?? target.GetType();
        var instance = target is Type ? null : target;
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy
            | (target is Type ? BindingFlags.Static : BindingFlags.Instance);
        if (type.GetProperty(name, flags) is { } property) return property.GetValue(instance);
        if (type.GetField(name, flags) is { } field) return field.GetValue(instance);
        throw new MissingMemberException(type.FullName, name);
    }

    private static IEnumerable<object> Items(object? value) =>
        value == null ? [] : ((IEnumerable)value).Cast<object>();

    private static ulong Number(object? value) => value == null ? 0 : Convert.ToUInt64(value);

    public void Dispose()
    {
        subscription?.Dispose();
        subscription = null;
        observedWorld = null;
        lock (operations) operations.Clear();
    }
}
