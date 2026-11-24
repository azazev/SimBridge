using AnoBossBridge;

internal static class NewMechanicTests
{
    internal static void Run()
    {
        static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
        Check(Activation.Entry(Activation.DiamondScenario) == (0x01020000u, 2f, "P2 Diamond Dust") && Activation.Boss(Activation.DiamondScenario) == (1, 0x459Fu, "_bossP2"), "Diamond entry");
        Check(Activation.Entry(Activation.FulgentScenario) == (0x03010000u, 1.5f, "P5 Fulgent Blade") && Activation.Boss(Activation.FulgentScenario) == (3, 0x45AFu, "_bossP5"), "Fulgent entry");
        Check(Activation.Entry(Activation.LightRampantScenario) == (0x01050000u, 3f, "P2 Light Rampant") && Activation.Boss(Activation.LightRampantScenario) == (1, 0x459Fu, "_bossP2"), "Light Rampant entry after skipped mirror phase");
        var conga = new AnoMech.Scenarios.Fru.P2LightRampant.FruP2LightRampantAi();
        Check(Activation.Entry(Activation.ParadiseScenario) == (0x03020000u, 1.3f, "P5 Paradise Regained") && Activation.Boss(Activation.ParadiseScenario) == (3, 0x45AFu, "_bossP5"), "Paradise entry after skipped Fulgent sequence");
        var paradise = new AnoMech.Scenarios.Fru.P5ParadiseRegained.FruP5ParadiseRegainedAi();
        Check(RoleSync.AcceptsStrategy(Activation.ParadiseScenario, paradise), "Paradise full-party strategy without NA Group accepted");
        paradise.Name = "Other";
        Check(!RoleSync.AcceptsStrategy(Activation.ParadiseScenario, paradise), "unverified Paradise strategy rejected");
        Check(RoleSync.AcceptsStrategy(Activation.LightRampantScenario, conga), "Light Rampant NA conga accepted");
        conga.Group = "Other"; Check(!RoleSync.AcceptsStrategy(Activation.LightRampantScenario, conga), "unverified Light Rampant strategy rejected");
        var standard = new AnoMech.Scenarios.Fru.P5FulgentBlade.FruP5FulgentBladeAi();
        var na = new AnoMech.Scenarios.Fru.P2DiamondDust.FruP2DiamondDustAi();
        Check(RoleSync.AcceptsStrategy(Activation.FulgentScenario, standard) && RoleSync.AcceptsStrategy(Activation.DiamondScenario, na), "strategy selection");
        Check(!RoleSync.AcceptsStrategy(Activation.ParadiseScenario, standard) && !RoleSync.AcceptsStrategy(Activation.ParadiseScenario, na), "other scenario strategies cannot activate Paradise");
        na.Group = "Other";
        Check(!RoleSync.AcceptsStrategy(Activation.DiamondScenario, na) && !RoleSync.AcceptsStrategy(Activation.DiamondScenario, standard), "unsupported strategy refused");
        ulong[] ids = [1, 2, 3, 4, 5, 6, 7, 8];
        foreach (var scenario in new[] { Activation.FulgentScenario, Activation.ParadiseScenario })
        for (var role = 0; role < 8; ++role)
        {
            var party = new[] { ids[role] }.Concat(ids.Where(id => id != ids[role])).ToArray();
            var map = RoleSync.MapSlots(ids, party);
            var sync = new RoleSync(map, ids, role, strategy: scenario == Activation.FulgentScenario ? "Standard" : "NA", scenarioName: scenario);
            // A native-only adapter must not inspect or mutate native component data at all.
            var module = new UntouchableModule();
            sync.Sync(module); sync.Sync(module);
            var snapshot = System.Text.Json.JsonSerializer.Serialize(sync.Snapshot());
            Check(snapshot.Contains("\"Guidance\":\"BossMod native\"") && !snapshot.Contains("Target") && !snapshot.Contains("AssignmentsApplied"), "diagnostics must not claim custom guidance/assignments");
        }
        Console.WriteLine("PASS 2/10: activation/strategies and untouched native Fulgent/Paradise guidance across all seats");
    }
    private sealed class UntouchableModule
    {
        public object Components => throw new Exception("Native guidance must not be read or modified by RoleSync");
        public object Arena => throw new Exception("Bridge must not draw markers");
        public object WorldState => throw new Exception("Bridge must not compute new destinations");
    }
}

namespace AnoMech.Scenarios.Fru.P5FulgentBlade
{
    public sealed class FruP5FulgentBladeAi { public string Name => "Standard"; }
}
namespace AnoMech.Scenarios.Fru.P2DiamondDust
{
    public sealed class FruP2DiamondDustAi { public string Name => "NA Partner Swap"; public string Group { get; set; } = "NA"; }
}
namespace AnoMech.Scenarios.Fru.P2LightRampant
{
    public sealed class FruP2LightRampantAi { public string Name => "NA Conga"; public string Group { get; set; } = "NA"; }
}
namespace AnoMech.Scenarios.Fru.P5ParadiseRegained
{
    public sealed class FruP5ParadiseRegainedAi { public string Name { get; set; } = "H first / M1-R1 NW / M2-R2 NE; lines T-M-R-H"; }
}


