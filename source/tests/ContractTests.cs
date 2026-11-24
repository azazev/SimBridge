using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

internal static class ContractTests
{
    internal static void Run()
    {
        var installed = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher", "installedPlugins");
        // The currently installed target is required; older supported versions are verified only if retained by XIVLauncher.
        var availableBossmodVersions = new[] { "7.5.8.6", "7.5.8.7", "7.5.8.8", "7.5.8.9" }
            .Where(version => File.Exists(Path.Combine(installed, "BossModReborn", version, "BossModReborn.dll"))).ToArray();
        if (!availableBossmodVersions.Contains("7.5.8.9"))
            throw new Exception("BossMod Reborn 7.5.8.9 installation not found: update BossMod Reborn in Dalamud first.");
        foreach (var version in availableBossmodVersions)
        CheckAssembly(Path.Combine(installed, "BossModReborn", version, "BossModReborn.dll"), version, md =>
        {
            var manager = Find(md, "BossMod.BossModuleManager");
            Require(md, manager, "LoadedModules", "PendingModules", "ActiveModule", "ModuleDeactivated", "_activeModuleOverridden");
            RequireMethod(md, manager, "UnloadModule", 1);
            RequireMethod(md, manager, "LoadModule", 2);
            Require(md, Find(md, "BossMod.WorldState"), "Modified", "Actors");
            RequireMethod(md, Find(md, "BossMod.Event`1"), "Subscribe", 1);
            RequireMethod(md, Find(md, "BossMod.Event`1"), "Fire", 1);
            Require(md, Find(md, "BossMod.StateMachine"), "ActiveState", "Phases");
            Require(md, Find(md, "BossMod.Actor"), "InstanceID", "OID");
            var fru = Find(md, "BossMod.Dawntrail.Ultimate.FRU.FRU");
            Require(md, fru, "_bossP2", "_bossP5");
            var module = Find(md, "BossMod.BossModule");
            Require(md, module, "StateMachine", "PrimaryActor");
            var dd = Find(md, "BossMod.Dawntrail.Ultimate.FRU.P2DiamondDustSafespots");
            Require(md, dd, "_out", "_supportsBaitCones", "_conesAtCardinals", "_safeOffs");
            RequireMethod(md, dd, "InitIfReady", 0);
            var kb = Find(md, "BossMod.Dawntrail.Ultimate.FRU.P2HeavenlyStrike");
            Require(md, kb, "_safeDirs"); RequireMethod(md, kb, "BuildSafeDirs", 1);
            var twin = Find(md, "BossMod.Dawntrail.Ultimate.FRU.P2TwinStillnessSilence");
            Require(md, twin, "AOEs", "_source"); RequireMethod(md, twin, "OnCastStarted", 2);
            Require(md, Find(md, "BossMod.Components.CastCounter"), "NumCasts");
            Require(md, Find(md, "BossMod.Actor"), "CastInfo", "OID", "Rotation");
            Require(md, Find(md, "BossMod.ActorCastInfo"), "Action", "Rotation");
            var angle = Find(md, "BossMod.Angle"); Require(md, angle, "Rad");
            if (!angle.GetMethods().Select(md.GetMethodDefinition).Any(m => md.GetString(m.Name) == ".ctor" && md.GetBlobBytes(m.Signature).SequenceEqual(new byte[] { 0x20, 1, 1, 0x0c })))
                throw new Exception("Native Angle(float) constructor changed");
            var aoe = md.TypeDefinitions.Select(md.GetTypeDefinition).Single(t => md.GetString(t.Name) == "AOEInstance" && md.GetString(md.GetTypeDefinition(t.GetDeclaringType()).Name) == "GenericAOEs");
            Require(md, aoe, "Activation", "Rotation", "Shape");
            Require(md, Find(md, "BossMod.GroupAssignment"), "_config");
            var roleConfig = Find(md, "BossMod.PartyRolesConfig");
            Require(md, roleConfig, "Assignments"); RequireMethod(md, roleConfig, "SlotsPerAssignment", 1);
            Require(md, Find(md, "BossMod.PartyState"), "Members");
            Find(md, "BossMod.Dawntrail.Ultimate.FRU.P5FulgentBlade");
            var paradiseTowers = Find(md, "BossMod.Dawntrail.Ultimate.FRU.P5ParadiseRegainedTowers");
            RequireMethod(md, paradiseTowers, "OnMapEffect", 2);
            RequireMethod(md, paradiseTowers, "OnEventCast", 2);
            var paradiseBaits = Find(md, "BossMod.Dawntrail.Ultimate.FRU.P5ParadiseRegainedBaits");
            RequireMethod(md, paradiseBaits, "OnCastStarted", 2);
            RequireMethod(md, paradiseBaits, "OnEventCast", 2);
            var polarizing = Find(md, "BossMod.Dawntrail.Ultimate.FRU.P5PolarizingStrikes");
            RequireMethod(md, polarizing, "OnCastStarted", 2);
            RequireMethod(md, polarizing, "OnEventCast", 2);
            RequireMethod(md, polarizing, "OnStatusGain", 2);
            RequireMethod(md, Find(md, "BossMod.Dawntrail.Ultimate.FRU.FRUStates"), "P5ParadiseRegained", 2);
            RequireMethod(md, Find(md, "BossMod.Dawntrail.Ultimate.FRU.FRUStates"), "P5PolarizingStrikes", 2);
            var light = Find(md, "BossMod.Dawntrail.Ultimate.FRU.P2BrightHunger1");
            Require(md, light, "_forbidden", "_config"); RequireMethod(md, light, "RebuildTowers", 0);
            var genericTowers = Find(md, "BossMod.Components.GenericTowers"); Require(md, genericTowers, "Towers");
            var tower = md.TypeDefinitions.Select(md.GetTypeDefinition).Single(t => md.GetString(t.Name) == "Tower" && md.GetString(md.GetTypeDefinition(t.GetDeclaringType()).Name) == "GenericTowers");
            Require(md, tower, "Activation", "ForbiddenSoakers", "Position", "Shape");
            Require(md, Find(md, "BossMod.BitMask"), "Raw");
            RequireMethod(md, Find(md, "BossMod.Dawntrail.Ultimate.FRU.FRUStates"), "P2LightRampant", 2);
            foreach (var name in new[] { "P2LightRampant", "P2LuminousHammer", "P2BrightHunger2", "P2HolyLightBurst", "P2PowerfulLight", "P2LightRampantBanish", "P2HouseOfLightBoss" })
                Find(md, "BossMod.Dawntrail.Ultimate.FRU." + name);
        });
        CheckAssembly(Path.Combine(installed, "AnoMech", "0.5.1.0", "AnoMech.dll"), "0.5.1.0", md =>
        {
            foreach (var part in new[] { "P2DiamondDust", "P5FulgentBlade", "P2LightRampant", "P5ParadiseRegained" })
            {
                var prefix = "AnoMech.Scenarios.Fru." + part + ".Fru" + part;
                Require(md, Find(md, prefix + "Scenario"), "AiStrats");
                Require(md, Find(md, prefix + "Ai"), "Name");
            }
            Require(md, Find(md, "AnoMech.Scenarios.Fru.P2LightRampant.FruP2LightRampantAi"), "Group");
        });
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "AnoBossBridge"));
        if (!Directory.Exists(root)) root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "source"));
        // Direct compiler script and standard dotnet run output layouts.
        var plugin = Path.Combine(root, "bin", "Release", "AnoBossBridge.dll");
        if (!File.Exists(plugin)) plugin = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "bin", "Release", "AnoBossBridge.dll"));
        CheckAssembly(plugin, "0.8.0.0", md =>
        {
            foreach (var h in md.TypeDefinitions)
            {
                var type = md.GetTypeDefinition(h);
                var name = md.GetString(type.Name);
                if (name is "NativeGuide" or "DiamondSync" or "FulgentSync" or "PersonalGuidance") throw new Exception("Custom guidance type remains in release");
                foreach (var mh in type.GetMethods())
                    if (md.GetString(md.GetMethodDefinition(mh).Name) is "DrawArenaForeground" or "DrawArenaBackground" or "AddHints" or "AddAIHints") throw new Exception("Bridge-provided drawing/hint callback remains");
            }
        });
        Console.WriteLine("PASS 6/10: installed BossMod " + string.Join(", ", availableBossmodVersions) + " contracts, simulation lock lifecycle members; release has no custom guidance callbacks");
    }
    private static void CheckAssembly(string path, string version, Action<MetadataReader> check)
    {
        using var stream = File.OpenRead(path); using var pe = new PEReader(stream);
        var md = pe.GetMetadataReader();
        if (md.GetAssemblyDefinition().Version.ToString() != version) throw new Exception("Assembly version mismatch: " + path);
        check(md);
    }
    private static TypeDefinition Find(MetadataReader md, string name) => md.GetTypeDefinition(md.TypeDefinitions.Single(h =>
    {
        var type = md.GetTypeDefinition(h); return md.GetString(type.Namespace) + "." + md.GetString(type.Name) == name;
    }));
    private static void RequireMethod(MetadataReader md, TypeDefinition type, string name, int parameters)
    {
        var found = type.GetMethods().Select(h => md.GetMethodDefinition(h)).Single(m => md.GetString(m.Name) == name);
        if (found.GetParameters().Select(h => md.GetParameter(h)).Count(p => p.SequenceNumber > 0) != parameters) throw new Exception("Native method signature changed: " + name);
    }
    private static void Require(MetadataReader md, TypeDefinition type, params string[] names)
    {
        var available = type.GetFields().Select(h => md.GetString(md.GetFieldDefinition(h).Name)).Concat(type.GetProperties().Select(h => md.GetString(md.GetPropertyDefinition(h).Name))).ToHashSet();
        if (names.Any(n => !available.Contains(n))) throw new Exception("Installed interface contract mismatch");
    }
}



