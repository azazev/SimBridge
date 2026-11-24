using System.Reflection;
using AnoBossBridge;
using B = BossMod;

internal static class NativeLightRampantTests
{
    internal static void Run()
    {
        static void Check(bool ok, string why) { if (!ok) throw new Exception(why); }
        ulong[] ids = [1, 2, 3, 4, 5, 6, 7, 8];
        for (var player = 0; player < 8; ++player)
        for (var first = 0; first < 8; ++first)
        for (var second = first + 1; second < 8; ++second)
        {
            var partyIds = new[] { ids[player] }.Concat(ids.Where(i => i != ids[player])).ToArray();
            var slots = RoleSync.MapSlots(ids, partyIds);
            var module = new B.TestModule();
            module.WorldState.Party.Members = partyIds.Select(i => new B.TestMember(i, 1000 + i)).ToArray();
            var config = B.GroupAssignment.Config;
            config.Assignments.Clear(); config.Assignments[999] = B.TestRole.R2; config.Assignments[1001] = B.TestRole.OT;
            var before = config.Assignments.OrderBy(k => k.Key).ToArray();
            var tower = new B.P2BrightHunger1(module); module.Components = [tower];
            tower._forbidden = new(1UL << slots[first]);
            var sync = new NativeLightRampantSync(slots, ids);
            sync.Sync(module); Check(tower.Calls == 0, "wait for both hammer icons");
            tower._forbidden = new((1UL << slots[first]) | (1UL << slots[second]));
            sync.Sync(module); Check(tower.Calls == 0, "wait for native towers");
            tower.Seed(); var initial = tower.Towers.ToArray();
            sync.Sync(module);
            Check(tower.Calls == 1 && tower.Towers.Count == 6, "invoke original native rebuild once");
            Check(config.Assignments.OrderBy(k => k.Key).SequenceEqual(before), "restore existing and unrelated roles");
            Check(tower.Towers.Select(t => t.Activation).SequenceEqual(initial.Select(t => t.Activation)), "preserve all original deadlines");
            var expected = Enumerable.Range(0,8).Where(r => r != first && r != second).Reverse().ToArray();
            for (var i = 0; i < 6; ++i)
            {
                Check(tower.Towers[i].ForbiddenSoakers.Raw == (0xFFUL ^ (1UL << slots[expected[i]])), "simulated role input reaches native calculation for every seat and bait pair");
                Check(tower.Towers[i].Position == 700 + i && ReferenceEquals(tower.Towers[i].Shape, tower.NativeShape), "native position and shape untouched");
            }
            var snapshot = System.Text.Json.JsonSerializer.Serialize(sync.Snapshot());
            Check(snapshot.Contains("\"NativeTowerAssignmentsReady\":true") && snapshot.Contains("\"HammerTargetsObserved\":2") && snapshot.Contains("\"SimulationRolesSupplied\":true"), "readiness diagnostics");
            tower.NumCasts = 1; tower.Towers.RemoveAt(0); sync.Sync(module);
            Check(tower.Calls == 1 && tower.Towers.Count == 5, "do not rebuild resolved or initialized towers");
        }
        foreach (var mode in new[] { "valid", "failure", "changed-count", "changed-party", "resolved", "native-fallback" })
        {
            var module = new B.TestModule();
            module.WorldState.Party.Members = ids.Select(i => new B.TestMember(i, 1000+i)).ToArray();
            var config = B.GroupAssignment.Config; config.Assignments.Clear(); config.Assignments[999] = B.TestRole.M1;
            if (mode == "valid") for (var r = 0; r < 8; ++r) config.Assignments[1001UL+(ulong)r] = (B.TestRole)(7-r);
            var before = config.Assignments.OrderBy(k => k.Key).ToArray();
            var tower = new B.P2BrightHunger1(module) { _forbidden = new(3), Throw = mode == "failure", BadCount = mode == "changed-count", Fallback = mode == "native-fallback", NumCasts = mode == "resolved" ? 1 : 0 };
            module.Components = [tower]; tower.Seed(); var initial = tower.Towers.ToArray();
            if (mode == "changed-party") module.WorldState.Party.Members[0] = new(90,1001);
            var sync = new NativeLightRampantSync(Enumerable.Range(0,8).ToArray(), ids);
            var threw = false;
            try { sync.Sync(module); } catch (Exception e) when (e is InvalidOperationException or TargetInvocationException) { threw = true; }
            Check(threw == (mode is "failure" or "changed-count" or "changed-party"), "native errors and changed party propagate to disconnect");
            Check(config.Assignments.OrderBy(k => k.Key).SequenceEqual(before), "config always restored");
            if (mode != "native-fallback") Check(tower.Towers.SequenceEqual(initial), "valid maps and resolved data untouched; partial native errors rolled back");
            if (mode is "valid" or "resolved" or "changed-party") Check(tower.Calls == 0, "do not overwrite valid native configuration or run with stale party");
            if (mode == "native-fallback") Check(System.Text.Json.JsonSerializer.Serialize(sync.Snapshot()).Contains("\"NativeTowerAssignmentsReady\":false"), "native fallback preserved without invented assignments");
        }
        Console.WriteLine("PASS 5/10: native Light Rampant delegation, 28 bait pairs/all seats, timing, config preservation and failure rollback");
    }
}

namespace BossMod
{
    internal readonly record struct LightMask(ulong Raw);
    internal record struct LightTower(int Position, object Shape, LightMask ForbiddenSoakers, DateTime Activation);
    internal sealed class P2BrightHunger1(TestModule module)
    {
        public LightMask _forbidden;
        public int NumCasts = 0, Calls = 0;
        public bool Throw = false, BadCount = false, Fallback = false;
        public readonly object NativeShape = new();
        public List<LightTower> Towers = [];
        public void Seed()
        {
            for (var i = 0; i < 6; ++i) Towers.Add(new(10+i, new object(), _forbidden, new DateTime(2026,11,17).AddSeconds(i)));
        }
        private void RebuildTowers()
        {
            ++Calls;
            var slots = GroupAssignment.Config.SlotsPerAssignment(module.WorldState.Party);
            if (slots.Length != 8) throw new Exception("temporary roles missing");
            // Non-geometric sentinels intentionally differ from the real conga algorithm.
            // The adapter must accept this native result rather than compute its own.
            var assigned = slots.Where(s => (_forbidden.Raw & (1UL << s)) == 0).Reverse().ToArray();
            Towers.Clear();
            for (var i = 0; i < (BadCount ? 5 : 6); ++i)
            {
                Towers.Add(new(700+i, NativeShape, Fallback ? _forbidden : new(0xFFUL ^ (1UL << assigned[i])), new DateTime(2026,11,18)));
                if (Throw) throw new InvalidOperationException("partial native rebuild failure");
            }
        }
    }
}


