using System.Diagnostics;
using System.Runtime.Loader;
using AnoBossBridge;

internal static class InstalledCompatibilityTests
{
    internal static void Run(string[] requestedVersions)
    {
        var store = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher", "installedPlugins");
        var hooks = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher", "addon", "Hooks", "dev");
        var anoDir = Path.Combine(store, "AnoMech", "0.5.1.0");
        var versions = requestedVersions.Length == 0 ? new[] { "7.5.8.9", "7.5.8.10" } : requestedVersions;
        if (versions.Distinct(StringComparer.Ordinal).Count() != versions.Length)
            throw new ArgumentException("Duplicate BossMod validation targets.");
        foreach (var version in versions)
            if (!Version.TryParse(version, out var parsed) || parsed.Revision < 0 || parsed.ToString() != version)
                throw new ArgumentException("Expected a four-part BossMod version: " + version);
        if (!File.Exists(Path.Combine(anoDir, "AnoMech.dll")))
            throw new Exception("Missing validation target AnoMech 0.5.1.0");
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
}


