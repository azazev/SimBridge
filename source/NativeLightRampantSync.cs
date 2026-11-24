using System.Collections;
using System.Numerics;
using System.Reflection;
using static AnoBossBridge.Probe;

namespace AnoBossBridge;

// Supply missing party identities to the original native tower calculation.
// No conga solver, strategy override, position calculation or drawing lives here.
internal sealed class NativeLightRampantSync(int[] slots, ulong[] ids)
{
    private object? initialized;
    private bool rolesSupplied, towersReady;
    private int hammerTargets;
    internal object Snapshot() => new { Guidance = "BossMod native", SimulationRolesSupplied = rolesSupplied, HammerTargetsObserved = hammerTargets, NativeTowerAssignmentsReady = towersReady };

    internal void Sync(object module)
    {
        var tower = ((IEnumerable)Read(module, "Components")!).Cast<object>().FirstOrDefault(c => c.GetType().Name == "P2BrightHunger1");
        if (tower == null || ReferenceEquals(tower, initialized) || Convert.ToInt32(Read(tower, "NumCasts")) != 0) return;
        var forbidden = Convert.ToUInt64(Read(Read(tower, "_forbidden"), "Raw"));
        hammerTargets = BitOperations.PopCount(forbidden & 0xFF);
        var towers = (IList)Read(tower, "Towers")!;
        if (hammerTargets != 2 || towers.Count != 6) return; // wait for both native icon events

        var party = Read(Read(module, "WorldState"), "Party")!;
        var config = Read(module.GetType().Assembly.GetType("BossMod.GroupAssignment", true)!, "_config")!;
        var nativeSlots = (int[])config.GetType().GetMethod("SlotsPerAssignment")!.Invoke(config, [party])!;
        if (nativeSlots.Length != 8)
        {
            var members = ((IEnumerable)Read(party, "Members")!).Cast<object>().Take(8).ToArray();
            if (members.Length != 8 || Enumerable.Range(0, 8).Any(r => Convert.ToUInt64(Read(members[slots[r]], "InstanceId")) != ids[r]))
                throw new InvalidOperationException("Simulated party changed during native Light Rampant initialization.");
            var contentIds = slots.Select(s => Convert.ToUInt64(Read(members[s], "ContentId"))).ToArray();
            var assignments = (IDictionary)Read(config, "Assignments")!;
            var saved = towers.Cast<object>().ToArray();
            try
            {
                NativeDiamondSync.WithTemporaryRoles(assignments, contentIds, assignments.GetType().GetGenericArguments()[1], () =>
                    tower.GetType().GetMethod("RebuildTowers", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(tower, null));
                if (towers.Count != saved.Length) throw new InvalidOperationException("Native Light Rampant tower count changed.");
                for (var i = 0; i < towers.Count; ++i)
                {
                    var value = towers[i]!;
                    // Native rebuild uses current time; retain its original event-based deadlines.
                    Activation.Write(value, "Activation", Read(saved[i], "Activation"));
                    towers[i] = value;
                }
            }
            catch
            {
                towers.Clear();
                foreach (var value in saved) towers.Add(value);
                throw;
            }
            rolesSupplied = true;
        }
        // Observe readiness only; never invent a fallback for native strategy configuration.
        var allowed = towers.Cast<object>().Select(t => ~Convert.ToUInt64(Read(Read(t, "ForbiddenSoakers"), "Raw")) & 0xFF).ToArray();
        towersReady = allowed.All(m => BitOperations.PopCount(m) == 1) && allowed.Distinct().Count() == 6;
        initialized = tower;
    }
}
