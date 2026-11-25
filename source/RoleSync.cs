using System.Collections;
using System.Reflection;
using static AnoBossBridge.Probe;

namespace AnoBossBridge;

// Scope assignments to the module's own components; never edit global party/config state.
internal sealed class RoleSync(int[] partySlots, ulong[] entityIds, int playerRole, bool isDarklit = false, string strategy = "NA", string? scenarioName = null)
{
    internal static readonly string[] Names = ["MT", "OT", "H1", "H2", "M1", "M2", "R1", "R2"];
    private readonly HashSet<object> configured = new(ReferenceEqualityComparer.Instance);
    private readonly DarklitSync? darklit = isDarklit ? new(partySlots, entityIds, playerRole, strategy) : null;
    private readonly RelativitySync? relativity = scenarioName == Activation.RelativityScenario ? new(partySlots, entityIds, playerRole) : null;
    private readonly CrystallizeSync? crystallize = scenarioName == Activation.CrystallizeScenario ? new(partySlots, entityIds, playerRole) : null;
    private readonly NativeDiamondSync? diamond = scenarioName == Activation.DiamondScenario ? new(partySlots, entityIds) : null;
    private readonly NativeLightRampantSync? lightRampant = scenarioName == Activation.LightRampantScenario ? new(partySlots, entityIds) : null;
    // Keep native guidance; Diamond supplies missing identities only during native initialization.
    private readonly bool nativeOnly = scenarioName is Activation.DiamondScenario or Activation.FulgentScenario or Activation.LightRampantScenario or Activation.ParadiseScenario;
    private object? mappedWater;
    private int[]? adjusted;
    internal object Snapshot() => nativeOnly ? new
    {
        PlayerRole = Names[playerRole], Strategy = strategy, Guidance = "BossMod native", Compatibility = diamond?.Snapshot() ?? lightRampant?.Snapshot(),
        Members = partySlots.Select((slot, role) => new { Role = Names[role], PartySlot = slot, EntityId = entityIds[role] }).ToArray()
    } : relativity?.Snapshot() ?? crystallize?.Snapshot() ?? darklit?.Snapshot() ?? new
    {
        PlayerRole = Names[playerRole], Strategy = "NA priority / static spreads / OT bait",
        AssignmentsApplied = adjusted != null,
        Members = partySlots.Select((slot, role) => new
        {
            Role = Names[role], PartySlot = slot, EntityId = entityIds[role],
            InitialGroup = role / 4 + 1, InitialPosition = role % 4,
            AssignedGroup = adjusted == null ? (int?)null : adjusted[role] / 4 + 1,
            AssignedPosition = adjusted == null ? (int?)null : adjusted[role] % 4
        }).ToArray()
    };

    internal static RoleSync? Create(object game, object world)
    {
        var selected = Read(Read(Read(game, "run"), "run"), "SelectedAi");
        var scenario = Read(game, "ActiveScenario");
        var isDarklit = scenario?.GetType().FullName == Activation.DarklitScenario;
        var strategies = ((IEnumerable)Read(scenario, "AiStrats")!).Cast<object>().ToArray();
        if (selected is not int index || index < 0 || index >= strategies.Length
            || !AcceptsStrategy(scenario!.GetType().FullName!, strategies[index]))
            throw new InvalidOperationException("Select the full-party strategy: Standard for Fulgent Blade, H first / M1-R1 NW / M2-R2 NE; lines T-M-R-H for Paradise Regained, or NA for another supported mechanic. Solo is not supported.");
        var party = Read(Read(game, "World"), "Party")!;
        var ids = ((IEnumerable)Read(party, "slots")!).Cast<object?>()
            .Select(m => Convert.ToUInt64(Read(m, "EntityId"))).ToArray();
        var bossIds = ((IEnumerable)Read(Read(world, "Party"), "Members")!).Cast<object>().Take(8)
            .Select(m => Convert.ToUInt64(Read(m, "InstanceId"))).ToArray();
        if (ids.Length != 8 || bossIds.Length != 8 || ids.Contains(0UL) || bossIds.Contains(0UL)) return null;
        var map = MapSlots(ids, bossIds);
        var playerRole = Convert.ToInt32(Read(party, "PlayerRole"));
        if (playerRole is < 0 or > 7 || map[playerRole] != 0)
            throw new InvalidOperationException("AnoMech player role does not match Bossmod's player slot.");
        return new(map, ids, playerRole, isDarklit, Read(strategies[index], "Name")?.ToString() ?? "NA", scenario!.GetType().FullName);
    }

    internal static bool AcceptsStrategy(string scenario, object strategy) =>
        strategy.GetType().FullName == scenario[..^"Scenario".Length] + "Ai"
        && (scenario switch
        {
            Activation.FulgentScenario => Read(strategy, "Name")?.ToString() == "Standard",
            Activation.ParadiseScenario => Read(strategy, "Name")?.ToString() == "H first / M1-R1 NW / M2-R2 NE; lines T-M-R-H",
            _ => Read(strategy, "Group")?.ToString() == "NA"
        });

    internal static int[] MapSlots(ulong[] roleIds, ulong[] partyIds)
    {
        if (roleIds.Length != 8 || partyIds.Length != 8 || roleIds.Contains(0UL)
            || roleIds.Distinct().Count() != 8 || partyIds.Distinct().Count() != 8)
            throw new InvalidOperationException("Party mapping requires eight unique nonzero actors.");
        var map = roleIds.Select(id => Array.IndexOf(partyIds, id)).ToArray();
        if (map.Contains(-1)) throw new InvalidOperationException("A simulated party member is missing from Bossmod.");
        return map;
    }

    // Same NA priority rule as AnoMech: pair the first support and DPS for each duplicated duration.
    internal static int[] ResolveGroups(int[] orders)
    {
        if (orders.Length != 8 || Enumerable.Range(0, 4).Any(n => orders.Count(x => x == n) != 2))
            throw new InvalidOperationException("Dark Water assignments require two of each duration (including none).");
        var slots = Enumerable.Range(0, 8).ToArray();
        int[] Adjusters(int start) => Enumerable.Range(0, 4)
            .Where(n => Enumerable.Range(start, 4).Count(i => orders[i] == n) == 2)
            .Select(n => Enumerable.Range(start, 4).First(i => orders[i] == n)).ToArray();
        var supports = Adjusters(0);
        var dps = Adjusters(4);
        for (var i = 0; i < supports.Length; ++i)
            (slots[supports[i]], slots[dps[i]]) = (slots[dps[i]], slots[supports[i]]);
        return slots;
    }

    internal void Sync(object module)
    {
        if (diamond != null) { diamond.Sync(module); return; }
        if (lightRampant != null) { lightRampant.Sync(module); return; }
        if (nativeOnly) return;
        if (relativity != null) { relativity.Sync(module); return; }
        if (crystallize != null) { crystallize.Sync(module); return; }
        if (darklit != null) { darklit.Sync(module); return; }
        foreach (var component in ((IEnumerable)Read(module, "Components")!).Cast<object>())
        {
            if (configured.Add(component))
            {
                var field = component.GetType().GetField("_config", BindingFlags.Instance | BindingFlags.NonPublic);
                if (field?.GetValue(component) is { } config && config.GetType().Name == "FRUConfig")
                {
                    var copy = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(config, null)!;
                    Activation.Write(copy, "P3ApocalypseStaticSpreads", true);
                    Activation.Write(copy, "P3ApocalypseUptime", false);
                    Activation.Write(copy, "P3DarkestDanceOTBait", true);
                    Activation.Write(copy, "P3ApocalypseDarkWater1ReferenceDirection", -90f);
                    field.SetValue(component, copy);
                }
            }
            if (component.GetType().Name != "P3ApocalypseDarkWater" || ReferenceEquals(component, mappedWater)
                || Convert.ToInt32(Read(component, "NumStatuses")) < 6) continue;
            var states = (Array)Read(component, "States")!;
            adjusted = ResolveGroups(partySlots.Select(slot => Convert.ToInt32(Read(states.GetValue(slot), "Order"))).ToArray());
            for (var role = 0; role < 8; ++role)
            {
                var state = states.GetValue(partySlots[role])!; // boxed struct: write it back to the array
                Activation.Write(state, "InitialGroup", role / 4 + 1);
                Activation.Write(state, "InitialPosition", role % 4);
                Activation.Write(state, "AssignedGroup", adjusted[role] / 4 + 1);
                Activation.Write(state, "AssignedPosition", adjusted[role] % 4);
                states.SetValue(state, partySlots[role]);
            }
            var swaps = Enumerable.Range(0, 4).Where(r => adjusted[r] != r)
                .Select(r => $"{Names[r]} <-> {Names[adjusted[r]]}").ToArray();
            Activation.Write(component, "_swaps", "Swaps: " + (swaps.Length == 0 ? "none" : string.Join(", ", swaps)));
            mappedWater = component;
        }
    }
}
