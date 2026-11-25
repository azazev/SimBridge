using System.Collections;
using static AnoBossBridge.Probe;

namespace AnoBossBridge;

internal sealed class CrystallizeSync(int[] partySlots, ulong[] entityIds, int playerRole)
{
    private static readonly string[] Names = ["None", "Blue Eruption", "Blue Water", "Blue Darkness", "Blue Ice", "Red Aero", "Red Ice"];
    private int[]? mechanics, sides;
    private object? mapped;
    internal object Snapshot() => new
    {
        PlayerRole = RoleSync.Names[playerRole], Strategy = "NA priority / 7-1 Akh Morn (MT solo)",
        AssignmentsApplied = sides != null,
        Members = partySlots.Select((slot, role) => new
        {
            Role = RoleSync.Names[role], PartySlot = slot, EntityId = entityIds[role],
            Debuff = mechanics == null ? null : Names[mechanics[role]],
            ClawSide = sides == null || sides[role] == 0 ? null : sides[role] < 0 ? "West" : "East"
        }).ToArray()
    };

    internal static int[] Resolve(int[] mechanics)
    {
        if (mechanics.Length != 8 || mechanics.Any(m => m is < 1 or > 6)
            || Enumerable.Range(1, 6).Any(m => mechanics.Count(x => x == m) != (m >= 5 ? 2 : 1)))
            throw new InvalidOperationException("Crystallize requires two red aero, two red ice and four distinct blue assignments.");
        var result = new int[8];
        int[] priority = [3, 2, 1, 0, 4, 5, 6, 7];
        foreach (var mechanic in new[] { 5, 6 })
        {
            var pair = priority.Where(r => mechanics[r] == mechanic).ToArray();
            result[pair[0]] = -1; result[pair[1]] = 1;
        }
        return result;
    }

    internal void Sync(object module)
    {
        var core = ((IEnumerable)Read(module, "Components")!).Cast<object>()
            .FirstOrDefault(c => c.GetType().Name == "P4CrystallizeTime");
        if (core == null || ReferenceEquals(core, mapped) || Convert.ToInt32(Read(core, "_numClaws")) < 4) return;
        var source = (Array)Read(core, "PlayerMechanics")!;
        var observed = partySlots.Select(s => Convert.ToInt32(source.GetValue(s))).ToArray();
        if (observed.Contains(0)) return;
        var assigned = Resolve(observed);
        var target = (int[])Read(core, "ClawSides")!;
        for (var r = 0; r < 8; ++r) target[partySlots[r]] = assigned[r];
        mechanics = observed; sides = assigned; mapped = core;
    }
}
