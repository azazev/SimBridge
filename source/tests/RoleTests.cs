using System.Reflection;
using AnoBossBridge;

void Check(bool ok, string name) { if (!ok) throw new Exception(name); }
bool Refuses(Action action) { try { action(); return false; } catch (InvalidOperationException) { return true; } }
ulong[] ids = [1, 2, 3, 4, 5, 6, 7, 8];
var slots = RoleSync.MapSlots(ids, [6, 1, 2, 3, 4, 5, 7, 8]);
Check(slots.SequenceEqual(new[] { 1, 2, 3, 4, 5, 0, 6, 7 }) && Refuses(() => RoleSync.MapSlots(ids, [1, 2, 3, 4, 5, 6, 7, 7]))
    && Refuses(() => RoleSync.MapSlots(ids, [1, 2, 3, 4, 5, 6, 7, 9])), "ambiguous or missing actors are rejected");
Check(RoleSync.ResolveGroups([0, 1, 2, 3, 0, 1, 2, 3]).SequenceEqual(Enumerable.Range(0, 8)) && RoleSync.ResolveGroups([0, 0, 1, 1, 2, 2, 3, 3]).SequenceEqual(new[] { 4, 1, 6, 3, 0, 5, 2, 7 }) && Refuses(() => RoleSync.ResolveGroups([0, 0, 0, 1, 2, 2, 3, 3])), "incomplete status data is rejected");
var water = new P3ApocalypseDarkWater();
var original = water.Config;
int[] orders = [0, 0, 1, 1, 2, 2, 3, 3];
for (var r = 0; r < 8; ++r) water.States[slots[r]].Order = orders[r];
var sync = new RoleSync(slots, ids, 5);
var fake = new FakeModule(water);
sync.Sync(fake);
var waited = water.States[0].InitialGroup == 0;
water.NumStatuses = 6;
sync.Sync(fake);
Check(waited && water.States[0].InitialGroup == 2 && water.States[0].InitialPosition == 1
    && water.States[slots[0]].AssignedGroup == 2 && water.States[slots[4]].AssignedGroup == 1
    && !ReferenceEquals(original, water.Config) && !original.P3ApocalypseStaticSpreads
    && water.Config.P3ApocalypseStaticSpreads && water.Config.P3DarkestDanceOTBait,
    "component sync waits for statuses, writes structs and isolates configuration");
var preset = new object();
var forced = new object();
var rotation = new FakeRotation { Preset = forced };
var manager = new FakeManager(fake);
using var activation = new Activation(_ => null, _ => { });
void Set(string name, object value) => typeof(Activation).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(activation, value);
Set("module", fake); Set("manager", manager); Set("rotation", rotation); Set("savedPreset", preset); Set("forceDisable", forced); Set("registered", true); Set("roles", sync);
activation.Dispose(); activation.Dispose();
Check(fake.Disposals == 1 && manager.LoadedModules.Count == 0 && ReferenceEquals(rotation.Preset, preset)
    && activation.RoleDiagnostics == null && !original.P3ApocalypseStaticSpreads, "cleanup removes mapping and restores rotation exactly once");

var permutations = 0;
for (var tank = 0; tank < 2; ++tank)
for (var healer = 2; healer < 4; ++healer)
for (var first = 0; first < 4; ++first)
for (var second = first + 1; second < 4; ++second)
for (var shape = 0; shape < 3; ++shape)
for (var cw = 0; cw < 4; ++cw)
for (var fw = 0; fw < 4; ++fw)
{
    int[] priority = [6, 7, 4, 5];
    int a = priority[first], b = priority[second];
    int[] cycle = shape switch { 0 => [healer, b, tank, a], 1 => [healer, tank, b, a], _ => [healer, tank, a, b] };
    var links = Enumerable.Range(0, 4).Select(i => (cycle[i], cycle[(i + 1) % 4])).ToArray();
    var free = new[] { 7, 6, 4, 5 }.Where(r => r != a && r != b).ToArray();
    int[] expected = [healer, tank, b, a, 5 - healer, 1 - tank, free[1], free[0]];
    if (shape == 1) (expected[1], expected[2]) = (expected[2], expected[1]);
    if (shape == 2) (expected[1], expected[3]) = (expected[3], expected[1]);
    int[] waterRoles = [expected[cw], expected[4 + fw]];
    if ((cw < 2) == (fw < 2)) (expected[4 + fw], expected[7 - fw]) = (expected[7 - fw], expected[4 + fw]);
    var result = DarklitSync.Resolve(links, waterRoles);
    if (!result.SequenceEqual(Enumerable.Range(0, 8).Select(r => Array.IndexOf(expected, r) % 4)))
        throw new Exception("Darklit NA assignment mismatch");
    // Link order and direction do not carry strategy information.
    if (!result.SequenceEqual(DarklitSync.Resolve(links.Reverse().Select(l => (l.Item2, l.Item1)).ToArray(), waterRoles.Reverse().ToArray())))
        throw new Exception("Darklit event ordering mismatch");
    ++permutations;
}
var darklitValid = permutations == 1152 && Refuses(() => DarklitSync.Resolve([(2, 5), (5, 0), (0, 4)], [2, 3]))
    && Refuses(() => DarklitSync.Resolve([(2, 5), (5, 0), (0, 4), (4, 2)], [2, 0]))
    && Refuses(() => DarklitSync.Resolve([(2, 5), (5, 0), (0, 4), (2, 5)], [2, 3]));
var core = new P4DarklitDragonsong();
var towers = new P4DarklitDragonsongBrightHunger();
var dance = new P4SomberDance();
var darkModule = new FakeModule(core) { Components = [core, towers, dance] };
var darkSync = new RoleSync(slots, ids, 5, true);
darkSync.Sync(darkModule);
var waitsForChains = core.AssignS.Raw == 0;
(int, int)[] chain = [(2, 5), (5, 0), (0, 4), (4, 2)];
core._tethers.AddRange(chain.Select(l => (new Actor(ids[l.Item1]), new Actor(ids[l.Item2]))));
core.Stacks.Raw = (1UL << slots[2]) | (1UL << slots[3]);
darkSync.Sync(darkModule);
ulong Bits(params int[] roles) => roles.Aggregate(0UL, (m, r) => m | (1UL << slots[r]));
var initialDance = dance.ForbiddenPlayers.Raw;
dance.NumCasts = 1; dance.ForbiddenPlayers.Raw |= Bits(0);
darkSync.Sync(darkModule);
Check(darklitValid && waitsForChains && core.AssignS.Raw == Bits(4, 5, 3, 6) && core.AssignE.Raw == Bits(0, 5, 1, 6)
    && towers.Towers[0].ForbiddenSoakers.Raw == (255UL ^ Bits(2, 0))
    && towers.Towers[1].ForbiddenSoakers.Raw == (255UL ^ Bits(4, 5))
    && initialDance == (255UL ^ Bits(0, 1)) && dance.ForbiddenPlayers.Raw == (initialDance | Bits(0)),
    "Darklit sync waits for events, maps M2, repairs tower structs and preserves the first tank hit");

IEnumerable<int[]> Permute(int[] values)
{
    if (values.Length == 0) { yield return []; yield break; }
    foreach (var value in values.Distinct())
    {
        var index = Array.IndexOf(values, value);
        foreach (var rest in Permute(values.Where((_, i) => i != index).ToArray())) yield return new[] { value }.Concat(rest).ToArray();
    }
}
bool Close(float a, float b) => MathF.Abs(a - b) < 0.0001f;
var urCount = 0;
foreach (var supports in Permute([1, 2, 3, 3]))
foreach (var dps in Permute([1, 1, 2, 3]))
foreach (var dpsIce in new[] { false, true })
{
    var fire = supports.Concat(dps).ToArray();
    var ice = new bool[8];
    var iceRole = dpsIce ? 4 + Array.IndexOf(dps, 3) : Array.IndexOf(supports, 1);
    ice[iceRole] = true; fire[iceRole] = 0;
    var clocks = RelativitySync.Resolve(fire, ice);
    var normalized = supports.Concat(dps).ToArray();
    int[] expected = new int[8];
    expected[Array.IndexOf(supports, 1)] = 0; expected[Array.IndexOf(supports, 2)] = 6;
    expected[4 + Array.IndexOf(dps, 2)] = 2; expected[4 + Array.IndexOf(dps, 3)] = 4;
    var longSupport = new[] { 3, 2, 0, 1 }.Where(r => normalized[r] == 3).ToArray();
    expected[longSupport[0]] = 7; expected[longSupport[1]] = 1;
    var shortDps = new[] { 7, 6, 4, 5 }.Where(r => normalized[r] == 1).ToArray();
    expected[shortDps[0]] = 5; expected[shortDps[1]] = 3;
    if (!clocks.SequenceEqual(expected) || clocks.Distinct().Count() != 8) throw new Exception("Relativity clock mismatch");
    for (var north = 0; north < 8; ++north)
    foreach (var clock in clocks)
    {
        var angle = north * MathF.PI / 4;
        var dir = RelativitySync.Direction(3 * MathF.Sin(angle), -3 * MathF.Cos(angle), clock);
        var expectedAngle = (north + clock) * MathF.PI / 4;
        if (!Close(dir.X, MathF.Sin(expectedAngle)) || !Close(dir.Z, -MathF.Cos(expectedAngle))) throw new Exception("Relativity rotation mismatch");
    }
    ++urCount;
}
Check(urCount == 288 && Refuses(() => RelativitySync.Resolve([1,2,3,3,1,1,2,3], new bool[8]))
    && Refuses(() => RelativitySync.Direction(0, 0, 1)), "Relativity: 288 debuff layouts, both ice types and all eight arena rotations");
var rel = new P3UltimateRelativity();
var relModule = new FakeModule(rel);
var relSync = new RoleSync(slots, ids, 5, scenarioName: Activation.RelativityScenario);
int[] relFire = [1,2,3,3,1,0,2,1];
for (var r = 0; r < 8; ++r) { rel.States[slots[r]].FireOrder = relFire[r]; rel.States[slots[r]].RewindOrder = 1; }
rel.States[0].HaveDarkBlizzard = true;
rel.States[0].LaserOrder = 2; // Actual job misclassified the simulated DPS ice seat.
relSync.Sync(relModule);
var waitingForNorth = rel.States[0].AssignedDir.X == 0 && rel.States[0].AssignedDir.Z == 0;
rel._numYellowTethers = 3; relSync.Sync(relModule);
var afterRel = System.Text.Json.JsonSerializer.Serialize(relSync.Snapshot());
Check(waitingForNorth && Close(rel.States[0].AssignedDir.Z, 1) && rel.States[0].LaserOrder == 1
    && rel.States[0].FireOrder == 0 && afterRel.Contains("\"AssignmentsApplied\":true")
    && rel.States[slots[0]].RewindOrder == 1, "Relativity component waits, writes boxed directions and fixes ice laser order without changing statuses");
var ctCount = 0;
foreach (var debuffs in Permute([1,2,3,4,5,5,6,6]))
{
    var sides = CrystallizeSync.Resolve(debuffs);
    int[] priority = [3,2,1,0,4,5,6,7];
    foreach (var red in new[] { 5,6 })
    {
        var pair = Enumerable.Range(0,8).Where(r => debuffs[r] == red).OrderBy(r => Array.IndexOf(priority,r)).ToArray();
        if (sides[pair[0]] != -1 || sides[pair[1]] != 1) throw new Exception("Crystallize side priority mismatch");
    }
    if (Enumerable.Range(0,8).Any(r => debuffs[r] < 5 && sides[r] != 0)) throw new Exception("Blue was assigned a claw side");
    ++ctCount;
}
Check(ctCount == 10080 && Refuses(() => CrystallizeSync.Resolve([1,2,3,4,5,5,6,0]))
    && Refuses(() => CrystallizeSync.Resolve([1,2,3,4,5,5,5,6])), "Crystallize: all 10080 distinct debuff layouts and incomplete/invalid inputs");
var mappedAllSeats = true;
for (var player = 0; player < 8; ++player)
{
    var party = new[] { ids[player] }.Concat(ids.Where(id => id != ids[player])).ToArray();
    var map = RoleSync.MapSlots(ids, party);
    var crystal = new P4CrystallizeTime();
    int[] debuffs = [5,6,1,2,3,4,5,6];
    for (var r = 0; r < 8; ++r) crystal.PlayerMechanics[map[r]] = (P4CrystallizeTime.Mechanic)debuffs[r];
    var module = new FakeModule(crystal);
    var csync = new RoleSync(map, ids, player, scenarioName: Activation.CrystallizeScenario);
    csync.Sync(module); mappedAllSeats &= crystal.ClawSides.All(x => x == 0);
    crystal._numClaws = 4; csync.Sync(module);
    mappedAllSeats &= crystal.ClawSides[map[0]] == -1 && crystal.ClawSides[map[6]] == 1
        && crystal.ClawSides[map[1]] == -1 && crystal.ClawSides[map[7]] == 1;
    mappedAllSeats &= System.Text.Json.JsonSerializer.Serialize(csync.Snapshot()).Contains("\"AssignmentsApplied\":true");
}
Check(mappedAllSeats, "Crystallize component waits for statuses and maps all eight selected player seats");
Check(Activation.Entry(Activation.Scenario).State == 0x02030000
    && Activation.Entry(Activation.DarklitScenario).State == 0x02100020
    && Activation.Entry(Activation.RelativityScenario).State == 0x02010000
    && Activation.Entry(Activation.CrystallizeScenario).State == 0x02130000
    && Activation.Entry(Activation.RelativityScenario).Delay == 1
    && Activation.Entry(Activation.CrystallizeScenario).Delay == 1
    && Refuses(() => Activation.Entry("other")), "All four scenario entries preserve existing timelines and reject unsupported simulations");
Console.WriteLine("PASS 1/10: v0.5 regression assignments, scenarios and cleanup (all prior assertions)");
NewMechanicTests.Run();
NativeDiamondTests.Run();
NativeTwinTests.Run();
NativeLightRampantTests.Run();
var selectedBossModVersions = InstalledCompatibilityTests.ResolveTargets(args);
ContractTests.Run(selectedBossModVersions);
SimulationGateTests.Run();
ResilienceTests.Run();
InstalledCompatibilityTests.Run(selectedBossModVersions);
Console.WriteLine("Core regression and resilience groups passed.");



sealed class FRUConfig
{
    public bool P3ApocalypseStaticSpreads = false;
    public bool P3ApocalypseUptime = true;
    public bool P3DarkestDanceOTBait = false;
    public float P3ApocalypseDarkWater1ReferenceDirection = 0;
}
sealed class P3ApocalypseDarkWater
{
    public struct State
    {
        public int Order;
        public int InitialGroup, InitialPosition, AssignedGroup, AssignedPosition;
        public State() { Order = InitialGroup = InitialPosition = AssignedGroup = AssignedPosition = 0; }
    }
    public State[] States = new State[8];
    public int NumStatuses = 0;
    private readonly FRUConfig _config = new();
    private string _swaps = "";
    public FRUConfig Config => _config;
    public string Swaps => _swaps;
}
sealed class FakeModule(object water) : IDisposable
{
    public object[] Components = [water];
    public Point Center = new(100);
    public int Disposals;
    public void Dispose() => ++Disposals;
}
sealed class FakeRotation { public object? Preset; }
sealed class Signal { public void Fire(object module) { } }
sealed class FakeManager(FakeModule module)
{
    public List<FakeModule> LoadedModules = [module];
    public List<FakeModule> PendingModules = [];
    public object? ActiveModule = module;
    public bool _activeModuleOverridden = true;
    public Signal ModuleDeactivated = new();
    private void UnloadModule(int index) { LoadedModules[index].Dispose(); LoadedModules.RemoveAt(index); ActiveModule = null; }
}

sealed class Actor(ulong id) { public ulong InstanceID = id; }
struct Mask { public ulong Raw; public Mask() { Raw = 0; } }
struct Point(float z) { public float Z = z; }
sealed class P4DarklitDragonsong
{
    public List<(Actor, Actor)> _tethers = [];
    public Mask Stacks = new(), AssignS = new(), AssignE = new();
}
sealed class P4DarklitDragonsongBrightHunger
{
    public struct Tower(float z) { public Point Position = new(z); public Mask ForbiddenSoakers = new(); }
    public List<Tower> Towers = [new(92), new(108)];
}
sealed class P4SomberDance { public int NumCasts = 0; public Mask ForbiddenPlayers = new(); }


struct Direction(float x, float z) { public float X = x, Z = z; }
sealed class P3UltimateRelativity
{
    public struct PlayerState
    {
        public int FireOrder, RewindOrder, LaserOrder;
        public bool HaveDarkBlizzard;
        public Direction AssignedDir;
        public PlayerState() { FireOrder = RewindOrder = LaserOrder = 0; HaveDarkBlizzard = false; AssignedDir = new(0,0); }
    }
    public PlayerState[] States = new PlayerState[8];
    public int _numYellowTethers = 0;
    public Direction _relNorth = new(0,-1);
}
sealed class P4CrystallizeTime
{
    public enum Mechanic { None, FangEruption, FangWater, FangDarkness, FangBlizzard, ClawAir, ClawBlizzard }
    public Mechanic[] PlayerMechanics = new Mechanic[8];
    public int[] ClawSides = new int[8];
    public int _numClaws = 0;
}



