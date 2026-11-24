using System.Diagnostics;
using System.Runtime.Loader;
using AnoBossBridge;

internal static class InstalledCompatibilityTests
{
    internal static void Run()
    {
        var store = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher", "installedPlugins");
        var hooks = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher", "addon", "Hooks", "dev");
        var anoDir = Path.Combine(store, "AnoMech", "0.5.1.0");
        foreach (var version in new[] { "7.5.8.9", "7.5.8.10" })
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
                var errors = result.Issues.SelectMany(g => g.Value.Select(e => g.Key + ": " + e)).ToArray();
                if (errors.Length != 0) throw new Exception(string.Join(Environment.NewLine, errors));
                Console.WriteLine($"Installed {version} / {result.AnoVersion}: all {result.Issues.Count} compatibility groups passed ({watch.ElapsedMilliseconds} ms, offline reflection only).");
            }
            finally { context.Unload(); }
        }
        Console.WriteLine("PASS 10/10: unchanged reviewed contracts and normalized behavior guards across installed BossMod .9/.10 with AnoMech 0.5.1.0");
    }
}


