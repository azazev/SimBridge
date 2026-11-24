using System.Collections;
using static AnoBossBridge.Probe;

namespace AnoBossBridge;

internal sealed class RelativitySync(int[] partySlots, ulong[] entityIds, int playerRole)
{
    private int[]? clocks, fires, lasers;
    private object? mapped;
    internal object Snapshot() => new
    {
        PlayerRole = RoleSync.Names[playerRole], Strategy = "NA", AssignmentsApplied = clocks != null,
        Members = partySlots.Select((slot, role) => new
        {
            Role = RoleSync.Names[role], PartySlot = slot, EntityId = entityIds[role],
            RelativeClock = clocks?[role], FireOrder = fires?[role], LaserOrder = lasers?[role]
        }).ToArray()
    };

    // Clock 0 is the observed relative north; clocks increase clockwise. Ice replaces one fire.
    internal static int[] Resolve(int[] fire, bool[] ice)
    {
        if (fire.Length != 8 || ice.Length != 8 || ice.Count(x => x) != 1
            || Enumerable.Range(0, 8).Any(r => ice[r] ? fire[r] != 0 : fire[r] is < 1 or > 3))
            throw new InvalidOperationException("Relativity requires seven fires and one ice assignment.");
        var orders = fire.Select((f, r) => ice[r] ? (r < 4 ? 1 : 3) : f).ToArray();
        if (!orders.Take(4).Order().SequenceEqual(new[] { 1, 2, 3, 3 })
            || !orders.Skip(4).Order().SequenceEqual(new[] { 1, 1, 2, 3 }))
            throw new InvalidOperationException("Relativity debuffs do not match the simulated support/DPS seats.");
        var westSupport = new[] { 3, 2, 0, 1 }.First(r => orders[r] == 3);
        var westDps = new[] { 7, 6, 4, 5 }.First(r => orders[r] == 1);
        return Enumerable.Range(0, 8).Select(r => r < 4
            ? orders[r] switch { 1 => 0, 2 => 6, _ => r == westSupport ? 7 : 1 }
            : orders[r] switch { 1 => r == westDps ? 5 : 3, 2 => 2, _ => 4 }).ToArray();
    }

    internal static (float X, float Z) Direction(float northX, float northZ, int clock)
    {
        var length = MathF.Sqrt(northX * northX + northZ * northZ);
        if (length < 0.001f) throw new InvalidOperationException("Relativity relative north is unavailable.");
        var angle = clock * MathF.PI / 4;
        return ((northX * MathF.Cos(angle) - northZ * MathF.Sin(angle)) / length,
            (northX * MathF.Sin(angle) + northZ * MathF.Cos(angle)) / length);
    }

    internal void Sync(object module)
    {
        var core = ((IEnumerable)Read(module, "Components")!).Cast<object>()
            .FirstOrDefault(c => c.GetType().Name == "P3UltimateRelativity");
        if (core == null || ReferenceEquals(core, mapped) || Convert.ToInt32(Read(core, "_numYellowTethers")) < 3) return;
        var states = (Array)Read(core, "States")!;
        var fire = partySlots.Select(s => Convert.ToInt32(Read(states.GetValue(s), "FireOrder"))).ToArray();
        var ice = partySlots.Select(s => Read(states.GetValue(s), "HaveDarkBlizzard") is true).ToArray();
        if (Enumerable.Range(0, 8).Any(r => fire[r] == 0 && !ice[r])
            || partySlots.Any(s => Convert.ToInt32(Read(states.GetValue(s), "RewindOrder")) == 0)) return;
        var assigned = Resolve(fire, ice);
        var north = Read(core, "_relNorth");
        var laser = new int[8];
        for (var r = 0; r < 8; ++r)
        {
            var state = states.GetValue(partySlots[r])!;
            laser[r] = ice[r] ? (r < 4 ? 2 : 1) : fire[r] switch { 3 => 1, 1 => 2, _ => 3 };
            var dir = Read(state, "AssignedDir")!;
            var (x, z) = Direction(Convert.ToSingle(Read(north, "X")), Convert.ToSingle(Read(north, "Z")), assigned[r]);
            Activation.Write(dir, "X", x); Activation.Write(dir, "Z", z);
            Activation.Write(state, "AssignedDir", dir);
            Activation.Write(state, "LaserOrder", laser[r]);
            states.SetValue(state, partySlots[r]);
        }
        clocks = assigned; fires = fire; lasers = laser; mapped = core;
    }
}
