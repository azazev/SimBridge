using System.Collections;
using static AnoBossBridge.Probe;

namespace AnoBossBridge;

internal sealed class DarklitSync(int[] partySlots, ulong[] entityIds, int playerRole, string strategy)
{
    private int[]? quadrants;
    private int soakers, waters;
    private object? mappedComponent;
    internal object Snapshot() => new
    {
        PlayerRole = RoleSync.Names[playerRole], Strategy = strategy,
        AssignmentsApplied = quadrants != null,
        Members = partySlots.Select((slot, role) => new
        {
            Role = RoleSync.Names[role], PartySlot = slot, EntityId = entityIds[role],
            Quadrant = quadrants == null ? null : new[] { "NW", "NE", "SE", "SW" }[quadrants[role]],
            TowerSoaker = quadrants == null ? (bool?)null : (soakers & (1 << role)) != 0,
            WaterTarget = quadrants == null ? (bool?)null : (waters & (1 << role)) != 0
        }).ToArray()
    };

    // Reconstruct AnoMech's NA bowtie from observed chains, with no access to future scenario state.
    internal static int[] Resolve((int From, int To)[] links, int[] waterRoles)
    {
        var chained = links.SelectMany(l => new[] { l.From, l.To }).Distinct().ToArray();
        if (links.Length != 4 || chained.Length != 4 || chained.Any(r => r is < 0 or > 7)
            || links.Any(l => l.From == l.To)
            || links.Select(l => (Math.Min(l.From, l.To), Math.Max(l.From, l.To))).Distinct().Count() != 4
            || chained.Any(r => links.Count(l => l.From == r || l.To == r) != 2)
            || chained.Count(r => r < 2) != 1 || chained.Count(r => r is 2 or 3) != 1
            || waterRoles.Length != 2 || waterRoles.Distinct().Count() != 2 || waterRoles.Any(r => r is < 0 or > 7)
            || waterRoles.Count(chained.Contains) != 1)
            throw new InvalidOperationException("Darklit requires a four-player chain loop and one chained/free water target.");
        var healer = chained.Single(r => r is 2 or 3);
        var tank = chained.Single(r => r < 2);
        var pair = new[] { 6, 7, 4, 5 }.Where(chained.Contains).ToArray();
        var neighbors = links.Where(l => l.From == healer || l.To == healer)
            .Select(l => l.From == healer ? l.To : l.From).ToArray();
        var opposite = chained.Single(r => r != healer && !neighbors.Contains(r));
        int Rank(int role) => role == pair[1] ? 2 : role == tank ? 1 : 0;
        var east = neighbors.MaxBy(Rank);
        var west = neighbors.Single(r => r != east);
        var freeDps = new[] { 7, 6, 4, 5 }.Where(r => !chained.Contains(r)).ToArray();
        int[] order = [healer, opposite, east, west, 5 - healer, 1 - tank, freeDps[1], freeDps[0]];
        var tetherWater = Array.IndexOf(order, waterRoles.Single(chained.Contains));
        var freeWater = Array.IndexOf(order, waterRoles.Single(r => !chained.Contains(r))) - 4;
        if ((tetherWater < 2) == (freeWater < 2))
            (order[4 + freeWater], order[4 + 3 - freeWater]) = (order[4 + 3 - freeWater], order[4 + freeWater]);
        return Enumerable.Range(0, 8).Select(r => Array.IndexOf(order, r) % 4).ToArray();
    }

    internal void Sync(object module)
    {
        var components = ((IEnumerable)Read(module, "Components")!).Cast<object>().ToArray();
        var core = components.FirstOrDefault(c => c.GetType().Name == "P4DarklitDragonsong");
        if (core != null && !ReferenceEquals(core, mappedComponent))
        {
            var links = ((IEnumerable)Read(core, "_tethers")!).Cast<object>().ToArray();
            var waterMask = Convert.ToUInt64(Read(Read(core, "Stacks"), "Raw"));
            var waterRoles = Enumerable.Range(0, 8).Where(r => (waterMask & (1UL << partySlots[r])) != 0).ToArray();
            if (links.Length == 4 && waterRoles.Length == 2)
            {
                int Role(object actor) => Array.IndexOf(entityIds, Convert.ToUInt64(Read(actor, "InstanceID")));
                var edges = links.Select(l => (Role(Read(l, "Item1")!), Role(Read(l, "Item2")!))).ToArray();
                quadrants = Resolve(edges, waterRoles);
                soakers = edges.SelectMany(l => new[] { l.Item1, l.Item2 }).Distinct().Aggregate(0, (m, r) => m | (1 << r));
                waters = waterRoles.Aggregate(0, (m, r) => m | (1 << r));
                WriteMask(core, "AssignS", Mask(r => quadrants[r] >= 2));
                WriteMask(core, "AssignE", Mask(r => quadrants[r] is 1 or 2));
                mappedComponent = core;
            }
        }
        if (quadrants == null) return;
        foreach (var c in components)
        {
            if (c.GetType().Name == "P4DarklitDragonsongBrightHunger")
            {
                var towers = (IList)Read(c, "Towers")!;
                for (var i = 0; i < towers.Count; ++i)
                {
                    var tower = towers[i]!;
                    var south = Convert.ToSingle(Read(Read(tower, "Position"), "Z")) > Convert.ToSingle(Read(Read(module, "Center"), "Z"));
                    WriteMask(tower, "ForbiddenSoakers", 0xFFUL ^ Mask(r => (soakers & (1 << r)) != 0 && (quadrants[r] >= 2) == south));
                    towers[i] = tower;
                }
            }
            // Use the simulation's tank seats even when the real player's job differs.
            if (c.GetType().Name == "P4SomberDance" && Convert.ToInt32(Read(c, "NumCasts")) == 0)
                WriteMask(c, "ForbiddenPlayers", 0xFFUL ^ Mask(r => r < 2));
        }
    }

    private ulong Mask(Func<int, bool> include) => Enumerable.Range(0, 8).Where(include).Aggregate(0UL, (m, r) => m | (1UL << partySlots[r]));
    private static void WriteMask(object target, string member, ulong value)
    {
        var mask = Read(target, member)!;
        Activation.Write(mask, "Raw", value);
        Activation.Write(target, member, mask);
    }
}
