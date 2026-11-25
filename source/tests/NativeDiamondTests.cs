using System.Reflection;
using AnoBossBridge;
using B = BossMod;

internal static class NativeDiamondTests
{
    internal static void Run()
    {
        static void Check(bool value, string why) { if (!value) throw new Exception(why); }
        ulong[] ids = [1, 2, 3, 4, 5, 6, 7, 8];
        for (var player = 0; player < 8; ++player)
        {
            var partyIds = new[] { ids[player] }.Concat(ids.Where(i => i != ids[player])).ToArray();
            var slots = RoleSync.MapSlots(ids, partyIds);
            var module = new B.TestModule();
            module.WorldState.Party.Members = partyIds.Select(i => new B.TestMember(i, 0xFF00000000000000UL | i)).ToArray();
            var roleIds = ids.Select(i => 0xFF00000000000000UL | i).ToArray();
            var config = B.GroupAssignment.Config;
            config.Assignments.Clear();
            config.Assignments[999] = B.TestRole.R2; // unrelated real-party assignment
            config.Assignments[roleIds[player]] = B.TestRole.OT; // preserve an existing conflicting player role
            var before = config.Assignments.OrderBy(k => k.Key).ToArray();
            var bait = new B.P2DiamondDustSafespots(module);
            module.Components.Add(bait);
            var sync = new NativeDiamondSync(slots, ids);
            sync.Sync(module); Check(bait.Calls == 0, "wait for native inputs");
            bait._out = true; bait._supportsBaitCones = false; bait._conesAtCardinals = true;
            sync.Sync(module);
            Check(bait.Calls == 1 && bait._safeOffs[0].X == 100 + player, "native initializer receives correct simulated roles for every seat");
            Check(config.Assignments.OrderBy(k => k.Key).SequenceEqual(before), "real and unrelated assignments restored");
            bait._safeOffs[0] = new(987, 654); sync.Sync(module);
            Check(bait.Calls == 1 && bait._safeOffs[0].X == 987, "do not overwrite native post-cast updates");
            var kb = new B.P2HeavenlyStrike(); module.Components.Add(kb); sync.Sync(module);
            Check(kb._safeDirs[0].X == 200 + player, "native knockback calculation receives same roles");
            Check(config.Assignments.OrderBy(k => k.Key).SequenceEqual(before), "restore after knockback calculation");
            var snapshot = System.Text.Json.JsonSerializer.Serialize(sync.Snapshot());
            Check(snapshot.Contains("\"SimulationRolesSupplied\":true") && snapshot.Contains("\"NativeBaitPositionReady\":true") && snapshot.Contains("\"NativeKnockbackPositionReady\":true"), "read-only diagnostics expose native cache readiness");

            // Keep valid native assignments and cached positions exactly as the user supplied them.
            config.Assignments.Clear();
            for (var r = 0; r < 8; ++r) config.Assignments[roleIds[r]] = (B.TestRole)(7 - r);
            var valid = config.Assignments.OrderBy(k => k.Key).ToArray();
            var untouched = new B.P2DiamondDustSafespots(module) { _out = true, _supportsBaitCones = false, _conesAtCardinals = true };
            untouched._safeOffs[0] = new(123, 456); module.Components = [untouched];
            new NativeDiamondSync(slots, ids).Sync(module);
            Check(untouched.Calls == 0 && untouched._safeOffs[0].X == 123 && config.Assignments.OrderBy(k => k.Key).SequenceEqual(valid), "respect valid native configuration");

            config.Assignments.Clear(); config.Assignments[999] = B.TestRole.M1;
            var failing = new B.P2DiamondDustSafespots(module) { _out = true, _supportsBaitCones = true, _conesAtCardinals = true, Throw = true };
            module.Components = [failing];
            var threw = false;
            try { new NativeDiamondSync(slots, ids).Sync(module); } catch (TargetInvocationException) { threw = true; }
            Check(threw && config.Assignments.Count == 1 && config.Assignments[999] == B.TestRole.M1, "restore before propagating native error to disconnect path");
            threw = false;
            try { NativeDiamondSync.WithTemporaryRoles(config.Assignments, [1, 2, 3, 4, 5, 6, 7, 7], typeof(B.TestRole), () => throw new Exception("must not execute")); } catch (InvalidOperationException) { threw = true; }
            Check(threw && config.Assignments.Count == 1, "reject duplicate identities before mutation");
        }
        Console.WriteLine("PASS 3/10: native Diamond initialization, all seats, valid-config preservation and restoration on success/failure");
    }
}

namespace BossMod
{
    internal enum TestRole { MT, OT, H1, H2, M1, M2, R1, R2 }
    internal readonly record struct TestMember(ulong InstanceId, ulong ContentId);
    internal struct TestDir(float x, float z) { public float X = x, Z = z; }
    internal sealed class TestParty { public TestMember[] Members = []; }
    internal sealed class TestWorld { public TestParty Party = new(); }
    internal sealed class TestModule { public TestWorld WorldState = new(); public List<object> Components = []; }
    internal sealed class TestRoleConfig
    {
        public Dictionary<ulong, TestRole> Assignments = [];
        public int[] SlotsPerAssignment(TestParty party)
        {
            var result = Enumerable.Repeat(-1, 8).ToArray();
            for (var i = 0; i < 8; ++i)
            {
                if (!Assignments.TryGetValue(party.Members[i].ContentId, out var role) || result[(int)role] != -1) return [];
                result[(int)role] = i;
            }
            return result;
        }
    }
    internal sealed class GroupAssignment
    {
        private static readonly TestRoleConfig _config = new();
        internal static TestRoleConfig Config => _config;
    }
    internal sealed class P2DiamondDustSafespots(TestModule module)
    {
        internal bool? _out, _supportsBaitCones, _conesAtCardinals;
        internal TestDir[] _safeOffs = new TestDir[8];
        internal int Calls;
        internal bool Throw;
        private void InitIfReady()
        {
            ++Calls;
            var slots = GroupAssignment.Config.SlotsPerAssignment(module.WorldState.Party);
            if (slots.Length != 8) throw new Exception("native assignment failure");
            if (Throw) throw new Exception("native callback failure");
            // A deliberately non-geometric sentinel: bridge must invoke, not duplicate, this native calculation.
            for (var r = 0; r < 8; ++r) _safeOffs[slots[r]] = new(100 + r, 17);
        }
    }
    internal sealed class P2HeavenlyStrike
    {
        internal TestDir[] _safeDirs = new TestDir[8];
        private static TestDir[] BuildSafeDirs(TestModule module)
        {
            var slots = GroupAssignment.Config.SlotsPerAssignment(module.WorldState.Party);
            var result = new TestDir[8];
            for (var r = 0; r < 8; ++r) result[slots[r]] = new(200 + r, 1);
            return result;
        }
    }
}


