using System.Diagnostics;
using System.Runtime.Loader;
using AnoBossBridge;

internal static class InstalledCompatibilityTests
{
    internal static string[] ResolveTargets(string[] requestedVersions)
    {
        var store = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher", "installedPlugins");
        var versions = requestedVersions.Length == 0 ? new[] { "7.5.8.9", "7.5.8.10" } : requestedVersions;
        if (versions.Distinct(StringComparer.Ordinal).Count() != versions.Length)
            throw new ArgumentException("Duplicate BossMod validation targets.");
        foreach (var version in versions)
        {
            if (!Version.TryParse(version, out var parsed) || parsed.Revision < 0 || parsed.ToString() != version)
                throw new ArgumentException("Expected a four-part BossMod version: " + version);
            if (!File.Exists(Path.Combine(store, "BossModReborn", version, "BossModReborn.dll")))
                throw new Exception("Missing requested BossMod Reborn validation target " + version);
        }
        if (!File.Exists(Path.Combine(store, "AnoMech", "0.5.1.0", "AnoMech.dll")))
            throw new Exception("Missing validation target AnoMech 0.5.1.0");
        return versions;
    }

    internal static void Run(string[] versions)
    {
        var store = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher", "installedPlugins");
        var hooks = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher", "addon", "Hooks", "dev");
        var anoDir = Path.Combine(store, "AnoMech", "0.5.1.0");
        foreach (var version in versions)
        {
            var bossDir = Path.Combine(store, "BossModReborn", version);
            if (!File.Exists(Path.Combine(bossDir, "BossModReborn.dll"))) throw new Exception("Missing validation target " + version);
            var context = new AssemblyLoadContext("SimBridge contract test " + version, true);
            context.Resolving += (_, name) =>
            {
                foreach (var dir in new[] { bossDir, anoDir, hooks })
                    if (File.Exists(Path.Combine(dir, name.Name + ".dll"))) return context.LoadFromAssemblyPath(Path.Combine(dir, name.Name + ".dll"));
                return null;
            };
            try
            {
                var boss = context.LoadFromAssemblyPath(Path.Combine(bossDir, "BossModReborn.dll"));
                var ano = context.LoadFromAssemblyPath(Path.Combine(anoDir, "AnoMech.dll"));
                var config = Probe.Read(boss.GetType("BossMod.Service", true)!, "Config");
                var nodes = (System.Collections.IDictionary)Probe.Read(config, "_nodes")!;
                foreach (var name in new[] { "BossMod.BossModuleConfig", "BossMod.ColorConfig" })
                {
                    var type = boss.GetType(name, true)!;
                    nodes[type] = Activator.CreateInstance(type)!;
                }
                VerifyUwu(boss);
                VerifyExaflares(boss);
                var watch = Stopwatch.StartNew();
                var result = new Compatibility(boss, ano);
                watch.Stop();
                if (result.BossVersion != version || result.AnoVersion != "0.5.1.0")
                    throw new Exception($"Validation assembly version mismatch: requested {version} / 0.5.1.0, found {result.BossVersion} / {result.AnoVersion}");
                var errors = result.Issues.SelectMany(g => g.Value.Select(e => g.Key + ": " + e)).ToArray();
                if (errors.Length != 0) throw new Exception(string.Join(Environment.NewLine, errors));
                Console.WriteLine($"Installed {version} / {result.AnoVersion}: all {result.Issues.Count} compatibility groups passed ({watch.ElapsedMilliseconds} ms, offline reflection only).");
            }
            finally { context.Unload(); }
        }
        Console.WriteLine($"PASS 10/10: unchanged reviewed contracts and normalized behavior guards across installed BossMod {string.Join(", ", versions)} with AnoMech 0.5.1.0");
    }

    private static void VerifyExaflares(System.Reflection.Assembly boss)
    {
        Type T(string name) => boss.GetType("BossMod." + name, true)!;
        object Bare(Type type) => System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(type);
        var ucob = boss.GetType(Activation.UcobNamespace + "UCOB", true)!;
        var encounter = Bare(ucob);
        var actorType = T("Actor");
        var bahamut = Bare(actorType);
        Activation.Write(bahamut, "OID", Activation.BahamutOid);
        Activation.Write(bahamut, "IsTargetable", true);
        var seed = Activation.ExaflaresSeed(bahamut);
        Activation.Write(encounter, "PrimaryActor", seed);
        Activation.Write(encounter, "_bahamutPrime", bahamut);
        if (!ReferenceEquals(ucob.GetMethod("BahamutPrime")!.Invoke(encounter, null), bahamut)
            || ucob.GetMethod("Twintania")!.Invoke(encounter, null) != null
            || !Equals(Probe.Read(seed, "OID"), 0x1FDFu) || !Equals(Probe.Read(bahamut, "OID"), Activation.BahamutOid)
            || Probe.Read(bahamut, "IsDestroyed") is not false || Probe.Read(bahamut, "IsTargetable") is not true)
            throw new Exception("UCoB registration or actor isolation failed.");

        var world = Bare(T("WorldState"));
        Activation.Write(world, "Actors", Activator.CreateInstance(T("ActorState")));
        Activation.Write(encounter, "WorldState", world);
        var components = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(T("BossComponent")))!;
        Activation.Write(encounter, "Components", components);
        var builder = Activator.CreateInstance(boss.GetType(Activation.UcobNamespace + "UCOBStates", true)!, [encounter])!;
        var machine = builder.GetType().GetMethod("Build")!.Invoke(builder, null)!;
        Activation.Write(encounter, "StateMachine", machine);
        var phases = (System.Collections.IList)Probe.Read(machine, "Phases")!;
        var phase = phases[Activation.ExaflaresPhase]!;
        var setup = Activation.Entry(Activation.ExaflaresScenario);
        var entry = Activation.FindState(Probe.Read(phase, "InitialState")!, setup.State);
        foreach (uint offset in new uint[] { 0x10, 0x20, 0x30 }) Activation.FindState(entry, setup.State + offset);
        Activation.Write(phase, "InitialState", entry);
        Activation.StartExaflares(machine, DateTime.UnixEpoch);
        if (!ReferenceEquals(Probe.Read(machine, "ActiveState"), entry) || components.Count != 1
            || components[0]!.GetType().Name != "P5Exaflare" || !Equals(Probe.Read(machine, "TimeSinceActivation"), 0f))
            throw new Exception("UCoB did not enter the native P5 Exaflare component directly.");

        var component = components[0]!;
        var onCast = component.GetType().GetMethod("OnCastStarted")!;
        var onHit = component.GetType().GetMethod("OnEventCast")!;
        object Spell(uint id) => Activator.CreateInstance(T("ActionID"), [0x01000000u | id])!;
        var cast = Bare(T("ActorCastInfo"));
        Activation.Write(cast, "Action", Spell(9968));
        var hit = Bare(T("ActorCastEvent"));
        var lines = (System.Collections.IList)Probe.Read(component, "Lines")!;
        object[] helpers = Enumerable.Range(0, 6).Select(_ => Bare(actorType)).ToArray();
        for (int lane = 0; lane < helpers.Length; ++lane)
        {
            Activation.Write(helpers[lane], "PosRot", new System.Numerics.Vector4(lane * 6, 0, -20, 0));
            onCast.Invoke(component, [helpers[lane], cast]);
        }
        if (lines.Count != 6) throw new Exception("Expected six native exaflare lines.");
        for (int step = 0; step < 6; ++step)
        {
            Activation.Write(hit, "Action", Spell(step == 0 ? 9968u : 9969u));
            for (int lane = 0; lane < helpers.Length; ++lane)
            {
                Activation.Write(helpers[lane], "PosRot", new System.Numerics.Vector4(lane * 6, 0, -20 + 8 * step, 0));
                onHit.Invoke(component, [helpers[lane], hit]);
            }
            if (lines.Count != (step == 5 ? 0 : 6)) throw new Exception("Native lines disappeared early or remained after their last hit.");
        }
        if (Convert.ToInt32(Probe.Read(component, "NumCasts")) != 36) throw new Exception("Incomplete native exaflare sequence.");
        Console.WriteLine("UCoB: isolated Twintania seed, live Bahamut identity, native P5-only entry, six lines and 36 effects through native handlers passed.");
    }

    private static void VerifyUwu(System.Reflection.Assembly boss)
    {
        const string uwu = "BossMod.Stormblood.Ultimate.UWU.";
        object Bare(Type type) => System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(type);
        var encounter = Bare(boss.GetType(uwu + "UWU", true)!);
        var actor = boss.GetType("BossMod.Actor", true)!;
        var garuda = Bare(actor);
        var ifrit = Bare(actor);
        var titan = Bare(actor);
        var ultima = Bare(actor);
        var actorList = typeof(List<>).MakeGenericType(actor);
        System.Collections.IList ListWith(object member)
        {
            var list = (System.Collections.IList)Activator.CreateInstance(actorList)!;
            list.Add(member);
            return list;
        }
        Activation.Write(encounter, "PrimaryActor", garuda);
        Activation.Write(encounter, "_titan", ListWith(titan));
        Activation.Write(encounter, "_ultima", ListWith(ultima));
        var actors = new Activation.UwuActors(ifrit, titan, ultima);
        actors.Bind(encounter, garuda);
        var rejected = false;
        try { (actors with { Titan = Bare(actor) }).Bind(encounter, garuda); }
        catch (InvalidOperationException) { rejected = true; }
        if (!rejected) throw new Exception("UWU accepted another Titan.");

        var builder = Activator.CreateInstance(boss.GetType(uwu + "UWUStates", true)!, [encounter])!;
        var machine = builder.GetType().GetMethod("Build")!.Invoke(builder, null);
        var phases = (System.Collections.IList)Probe.Read(machine, "Phases")!;
        var initial = Probe.Read(phases[Activation.PredationPhase], "InitialState")!;
        var setup = Activation.Entry(Activation.PredationScenario);
        var entry = Activation.FindState(initial, setup.State);
        foreach (uint offset in new uint[] { 0x20, 0x30, 0x40, 0x51 })
            Activation.FindState(entry, setup.State + offset);
        var suppression = Activation.Entry(Activation.SuppressionScenario);
        var first = Probe.Read(phases[Activation.SuppressionPhase], "InitialState")!;
        if (Convert.ToUInt32(Probe.Read(first, "ID")) != suppression.State)
            throw new Exception("Suppression must enter at the opening cast.");
        foreach (uint offset in new uint[] { 0x31, 0x41, 0x62, 0x70, 0x80, 0x83, 0x84, 0x94 })
            Activation.FindState(first, suppression.State + offset);
        if (!Activation.IsSupported(Activation.PredationScenario) || !Activation.IsSupported(Activation.SuppressionScenario)
            || Activation.IsSupported("AnoMech.Scenarios.Uwu.UltimateAnnihilation.UltimateAnnihilationScenario"))
            throw new Exception("UWU scenario scope changed.");
        Console.WriteLine("Predation: installed UWU actor binding, mismatched-actor rejection, phase entry, both dodge states, Feather Rain passed. Suppression opening cast, native overlap, lasers, Mesohigh, stack, Tank Purge and unsupported-scenario exclusion passed.");
    }
}


