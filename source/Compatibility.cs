using System.Reflection;
using System.Text.Json;

namespace AnoBossBridge;

internal sealed record MemberContract(string Name, string[] Shapes, string[]? Bodies);
internal sealed record TypeContract(string Dependency, string Type, MemberContract[] Members);
internal sealed record ContractProfile(Dictionary<string, TypeContract[]> Groups);

internal sealed class Compatibility
{
    private static readonly Lazy<ContractProfile> Profile = new(() => JsonSerializer.Deserialize<ContractProfile>(
        typeof(Compatibility).Assembly.GetManifestResourceStream("SimBridge.Contracts.json")
            ?? throw new InvalidOperationException("Embedded compatibility profile missing"))!);
    internal static readonly string[] Scenarios = [Activation.Scenario, Activation.DarklitScenario, Activation.RelativityScenario,
        Activation.CrystallizeScenario, Activation.DiamondScenario, Activation.FulgentScenario, Activation.LightRampantScenario, Activation.ParadiseScenario, Activation.PredationScenario, Activation.SuppressionScenario, Activation.ExaflaresScenario];
    internal Dictionary<string, string[]> Issues { get; } = [];
    internal string BossVersion { get; }
    internal string AnoVersion { get; }
    internal Guid BossBuild { get; }
    internal Guid AnoBuild { get; }
    internal bool Allows(string group) => Issues.TryGetValue(group, out var errors) && errors.Length == 0;
    internal string Reason(string group) => string.Join("; ", Issues.GetValueOrDefault(group) ?? ["Compatibility was not checked"]);
    internal void Block(string group, string error) => Issues[group] = [error];
    internal object Snapshot() => new { BossVersion, AnoVersion, BossBuild, AnoBuild,
        Validation = "Runtime contracts and guarded methods; live testing is separate", Issues };

    internal Compatibility(Assembly boss, Assembly ano, ContractProfile? profile = null)
    {
        BossVersion = boss.GetName().Version?.ToString() ?? "unknown";
        AnoVersion = ano.GetName().Version?.ToString() ?? "unknown";
        BossBuild = boss.ManifestModule.ModuleVersionId;
        AnoBuild = ano.ManifestModule.ModuleVersionId;
        foreach (var (group, requirements) in (profile ?? Profile.Value).Groups)
        {
            var errors = new List<string>();
            foreach (var contract in requirements)
            {
                try
                {
                    var assembly = contract.Dependency == "BossMod" ? boss : ano;
                    var type = assembly.GetType(contract.Type, true)!;
                    foreach (var expected in contract.Members)
                    {
                        var members = ContractShape.Members(type, expected.Name);
                        if (!members.Select(ContractShape.Shape).Order().SequenceEqual(expected.Shapes.Order()))
                            errors.Add($"{contract.Dependency}: {contract.Type}.{expected.Name} signature changed");
                        else if (expected.Bodies != null && !members.Cast<MethodBase>().Select(ContractShape.Fingerprint).Order().SequenceEqual(expected.Bodies.Order()))
                            errors.Add($"{contract.Dependency}: {contract.Type}.{expected.Name} behavior changed; review required");
                    }
                }
                catch (Exception e) { errors.Add($"{contract.Dependency}: {contract.Type}: {e.GetBaseException().Message}"); }
            }
            Issues[group] = errors.ToArray();
        }
    }
}
