using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using AnoBossBridge;

// Maintainer-only: run against a reviewed baseline. Never refresh guards automatically during a release build.
var pluginStore = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher", "installedPlugins");
var hooks = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher", "addon", "Hooks", "dev");
var bossDir = Path.Combine(pluginStore, "BossModReborn", args[0]);
var anoDir = Path.Combine(pluginStore, "AnoMech", "0.5.1.0");
AssemblyLoadContext.Default.Resolving += (_, name) =>
{
    foreach (var dir in new[] { bossDir, anoDir, hooks })
        if (File.Exists(Path.Combine(dir, name.Name + ".dll"))) return AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(dir, name.Name + ".dll"));
    return null;
};
var boss = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(bossDir, "BossModReborn.dll"));
var ano = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(anoDir, "AnoMech.dll"));
var groups = new Dictionary<string, Dictionary<(string, string), Dictionary<string, MemberContract>>>();
Type B(string name) => boss.GetType(name.StartsWith("BossMod.") ? name : "BossMod.Dawntrail.Ultimate.FRU." + name, true)!;
Type A(string name) => ano.GetType(name, true)!;
void Add(string group, Type type, string names = "", bool body = false)
{
    if (!groups.TryGetValue(group, out var types)) groups[group] = types = [];
    var dependency = type.Assembly == boss ? "BossMod" : "AnoMech";
    var key = (dependency, type.FullName!);
    if (!types.TryGetValue(key, out var entries)) types[key] = entries = [];
    foreach (var name in names.Split(' ', StringSplitOptions.RemoveEmptyEntries))
    {
        var members = ContractShape.Members(type, name);
        if (members.Length == 0) throw new Exception($"Missing baseline member {type.FullName}.{name}");
        entries[name] = new(name, members.Select(ContractShape.Shape).Order().ToArray(), body ? members.Cast<MethodBase>().Select(ContractShape.Fingerprint).Order().ToArray() : null);
    }
}
Type PathType(string group, Type type, string path)
{
    foreach (var name in path.Split('.'))
    {
        Add(group, type, name);
        var member = ContractShape.Members(type, name).Single();
        type = member is PropertyInfo p ? p.PropertyType : ((FieldInfo)member).FieldType;
    }
    return type;
}
void EnumShape(string group, Type type) => Add(group, type, string.Join(' ', type.GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => f.Name)));
void ObjectID(string group, uint id)
{
    var fields = B("OID").GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => Convert.ToUInt32(f.GetRawConstantValue()) == id).ToArray();
    if (fields.Length == 0) throw new Exception("Missing reviewed OID " + id);
    Add(group, B("OID"), string.Join(' ', fields.Select(f => f.Name)));
}
const string discovery = "Discovery", gate = "SimulationLock", core = "Activation";
var game = PathType(discovery, A("AnoMech.Plugin"), "Game");
PathType(discovery, B("BossMod.Plugin"), "_bossmod"); PathType(discovery, B("BossMod.Plugin"), "_ws");
Add(gate, game, "ActiveScenario Status World");
Add(gate, game, "get_ActiveScenario get_Status", true);
EnumShape(gate, PathType(gate, game, "Status"));
PathType(gate, game, "World.Children");
Add(gate, A("AnoMech.Core.SimObjects.SimEnemy"), "EntityId BNpcBaseId");
Add(gate, A("AnoMech.Core.SimObjects.SimEnemy"), "get_EntityId get_BNpcBaseId", true);
Add(gate, B("BossMod.WorldState"), "Modified Actors");
Add(gate, B("BossMod.WorldState"), "Execute", true);
Add(gate, B("BossMod.BossModuleManager"), "LoadedModules PendingModules ModuleDeactivated ActiveModule");
Add(gate, B("BossMod.BossModuleManager"), "UnloadModule", true);
Add(gate, B("BossMod.BossModule"), "PrimaryActor StateMachine Dispose");
Add(gate, B("BossMod.StateMachine"), "ActiveState");
Add(gate, B("BossMod.Actor"), "InstanceID OID");
Add(gate, B("BossMod.Event`1"), "Subscribe Fire");
foreach (var name in new[] { "BossMod.WorldState+OpFrameStart", "BossMod.WorldState+OpZoneChange", "BossMod.ActorState+OpCreate", "BossMod.ActorState+OpDestroy" }) Add(gate, B(name));

Add(core, game, "ActiveScenario Paused EventTimeScale");
PathType(core, game, "Events.Elapsed"); PathType(core, game, "run.run.SelectedAi");
Add(core, PathType(core, game, "Events"), "get_Elapsed", true);
var slots = PathType(core, game, "World.Party.slots");
Add(core, slots.GetElementType()!, "EntityId");
EnumShape(core, PathType(core, game, "World.Party.PlayerRole"));
PathType(core, B("BossMod.Plugin"), "_ai.Beh");
var rotation = PathType(core, B("BossMod.Plugin"), "_rotation"); Add(core, rotation, "Preset ForceDisable");
Add(core, B("BossMod.BossModuleManager"), "_activeModuleOverridden ActiveModule ModuleActivated");
Add(core, B("BossMod.BossModuleManager"), "LoadModule", true);
Add(core, B("BossMod.WorldState"), "Party CurrentTime");
Add(core, B("BossMod.PartyState"), "Members"); Add(core, B("BossMod.PartyState+Member"), "InstanceId ContentId");
Add(core, B("FRU"), ".ctor");
ObjectID(core, 0x459B);
Add(core, B("BossMod.BossModule"), "PrimaryActor StateMachine Components WorldState Center ActivateComponent");
Add(core, B("BossMod.StateMachine"), "Phases");
Add(core, B("BossMod.StateMachine"), "Start TransitionToPhase", true);
Add(core, B("BossMod.StateMachine+Phase"), "InitialState Update");
Add(core, B("BossMod.StateMachine+State"), "ID NextStates Duration");

foreach (var scenario in Compatibility.Scenarios)
{
    var part = scenario.Split('.')[^2];
    Add(scenario, A(scenario), "AiStrats");
    Add(scenario, A(scenario[..^8] + "Ai"), scenario is Activation.FulgentScenario or Activation.ParadiseScenario ? "Name" : "Name Group");
    Add(scenario, A(scenario[..^8] + "Ai"), scenario is Activation.FulgentScenario or Activation.ParadiseScenario ? "get_Name" : "get_Name get_Group", true);
    Add(scenario, B("FRU"), Activation.Boss(scenario).BossField);
    ObjectID(scenario, Activation.Boss(scenario).OID);
    Add(scenario, B("FRUStates"), part, true);
    if (scenario is Activation.DarklitScenario or Activation.CrystallizeScenario)
    {
        Add(scenario, B("FRU"), "_bossP4Oracle"); Add(scenario, B("P4FragmentOfFate"));
        ObjectID(scenario, 0x45AB);
    }
}
void Roles(string group)
{
    Add(group, B("BossMod.GroupAssignment"), "_config");
    Add(group, B("BossMod.PartyRolesConfig"), "Assignments");
    Add(group, B("BossMod.PartyRolesConfig"), "SlotsPerAssignment", true);
    EnumShape(group, B("BossMod.PartyRolesConfig+Assignment"));
}
var dd = Activation.DiamondScenario;
ObjectID(dd, 0x45A0);
Roles(dd);
Add(dd, B("P2DiamondDustSafespots"), "_out _supportsBaitCones _conesAtCardinals _safeOffs");
Add(dd, B("P2DiamondDustSafespots"), "InitIfReady", true);
Add(dd, B("P2HeavenlyStrike"), "_safeDirs"); Add(dd, B("P2HeavenlyStrike"), "BuildSafeDirs", true);
Add(dd, B("P2TwinStillnessSilence"), "AOEs _source NumCasts"); Add(dd, B("P2TwinStillnessSilence"), "OnCastStarted", true);
Add(dd, B("BossMod.Actor"), "CastInfo Rotation"); Add(dd, B("BossMod.ActorCastInfo"), "Action Rotation"); Add(dd, B("BossMod.ActionID"), "ID");
Add(dd, B("BossMod.Angle"), ".ctor Rad"); Add(dd, B("BossMod.WDir"), "X Z");
Add(dd, B("BossMod.Components.GenericAOEs+AOEInstance"), "Activation");
// Values used by the existing Twin cast normalization.
var twinActions = B("AID").GetFields(BindingFlags.Static | BindingFlags.Public).Where(f => Convert.ToUInt32(f.GetRawConstantValue()) is 40193 or 40194).ToArray();
if (twinActions.Length != 2) throw new Exception("Twin action IDs require review");
Add(dd, B("AID"), string.Join(' ', twinActions.Select(f => f.Name)));
var lr = Activation.LightRampantScenario; Roles(lr);
Add(lr, B("P2BrightHunger1"), "_forbidden Towers NumCasts"); Add(lr, B("P2BrightHunger1"), "RebuildTowers", true);
Add(lr, B("BossMod.Components.GenericTowers+Tower"), "Activation ForbiddenSoakers"); Add(lr, B("BossMod.BitMask"), "Raw");
var ap = Activation.Scenario;
Add(ap, B("P3ApocalypseDarkWater"), "_config States NumStatuses _swaps");
Add(ap, B("P3ApocalypseDarkWater+State"), "Order InitialGroup InitialPosition AssignedGroup AssignedPosition");
Add(ap, B("FRUConfig"), "P3ApocalypseStaticSpreads P3ApocalypseUptime P3DarkestDanceOTBait P3ApocalypseDarkWater1ReferenceDirection");
var ur = Activation.RelativityScenario;
Add(ur, B("P3UltimateRelativity"), "_numYellowTethers States _relNorth");
Add(ur, B("P3UltimateRelativity+PlayerState"), "FireOrder HaveDarkBlizzard RewindOrder AssignedDir LaserOrder"); Add(ur, B("BossMod.WDir"), "X Z");
var ct = Activation.CrystallizeScenario;
Add(ct, B("P4CrystallizeTime"), "_numClaws PlayerMechanics ClawSides"); EnumShape(ct, B("P4CrystallizeTime+Mechanic"));
var dark = Activation.DarklitScenario;
Add(dark, B("P4AkhRhai")); Add(dark, B("P4DarklitDragonsong"), "_tethers Stacks AssignS AssignE");
Add(dark, B("P4DarklitDragonsongBrightHunger"), "Towers"); Add(dark, B("P4SomberDance"), "NumCasts ForbiddenPlayers");
Add(dark, B("BossMod.Components.GenericTowers+Tower"), "Position ForbiddenSoakers"); Add(dark, B("BossMod.BitMask"), "Raw"); Add(dark, B("BossMod.WPos"), "Z");
Add(Activation.ParadiseScenario, B("FRUStates"), "P5PolarizingStrikes", true);
var profile = new ContractProfile(groups.ToDictionary(g => g.Key, g => g.Value.Select(t => new TypeContract(t.Key.Item1, t.Key.Item2, t.Value.Values.ToArray())).ToArray()));
File.WriteAllText(args[1], JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Reviewed baseline: BossMod {boss.GetName().Version}, AnoMech {ano.GetName().Version}; {groups.Count} groups, {groups.Values.Sum(g => g.Values.Sum(m => m.Count))} member contracts.");
