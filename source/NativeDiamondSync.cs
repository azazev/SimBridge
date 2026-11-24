using System.Collections;
using System.Reflection;
using static AnoBossBridge.Probe;

namespace AnoBossBridge;

// Populate native caches with native calculations, supplying only the missing simulated
// party identities. No positions, routes, drawing callbacks or strategy rules live here.
internal sealed class NativeDiamondSync(int[] slots, ulong[] ids)
{
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    private readonly HashSet<object> initialized = new(ReferenceEqualityComparer.Instance);
    private bool rolesSupplied, baitReady, knockbackReady;
    private uint? twinAction;
    private string twinOrientation = "Awaiting first Twin cast";
    internal object Snapshot() => new { Guidance = "BossMod native", SimulationRolesSupplied = rolesSupplied, NativeBaitPositionReady = baitReady, NativeKnockbackPositionReady = knockbackReady, TwinCastAction = twinAction, TwinOrientation = twinOrientation };

    internal void Sync(object module)
    {
        var components = ((IEnumerable)Read(module, "Components")!).Cast<object>().ToArray();
        SyncTwinOrientation(components);
        var bait = components.FirstOrDefault(c => c.GetType().Name == "P2DiamondDustSafespots");
        var knockback = components.FirstOrDefault(c => c.GetType().Name == "P2HeavenlyStrike");
        var baitNeedsInit = bait != null && !initialized.Contains(bait) && Read(bait, "_out") != null
            && Read(bait, "_supportsBaitCones") != null && Read(bait, "_conesAtCardinals") != null;
        var knockbackNeedsInit = knockback != null && !initialized.Contains(knockback);
        if (!baitNeedsInit && !knockbackNeedsInit) return;

        var party = Read(Read(module, "WorldState"), "Party")!;
        var assignmentType = module.GetType().Assembly.GetType("BossMod.GroupAssignment", true)!;
        var config = Read(assignmentType, "_config")!;
        var nativeSlots = (int[])config.GetType().GetMethod("SlotsPerAssignment")!.Invoke(config, [party])!;
        if (nativeSlots.Length == 8)
        {
            // Respect an already valid native assignment. Do not replace user choices.
            if (baitNeedsInit) { initialized.Add(bait!); baitReady = HasPosition(bait!, "_safeOffs"); }
            if (knockbackNeedsInit) { initialized.Add(knockback!); knockbackReady = HasPosition(knockback!, "_safeDirs"); }
            return;
        }

        var members = ((IEnumerable)Read(party, "Members")!).Cast<object>().Take(8).ToArray();
        if (members.Length != 8 || Enumerable.Range(0, 8).Any(r => Convert.ToUInt64(Read(members[slots[r]], "InstanceId")) != ids[r]))
            throw new InvalidOperationException("Simulated party changed during native role initialization.");
        var contentIds = slots.Select(s => Convert.ToUInt64(Read(members[s], "ContentId"))).ToArray();
        var assignments = (IDictionary)Read(config, "Assignments")!;
        var roleType = assignments.GetType().GetGenericArguments()[1];
        WithTemporaryRoles(assignments, contentIds, roleType, () =>
        {
            if (baitNeedsInit)
                bait!.GetType().GetMethod("InitIfReady", Private)!.Invoke(bait, null);
            if (knockbackNeedsInit)
            {
                var calculated = (Array)knockback!.GetType().GetMethod("BuildSafeDirs", Private)!.Invoke(null, [module])!;
                var cache = (Array)Read(knockback, "_safeDirs")!;
                if (calculated.Length != cache.Length) throw new InvalidOperationException("Native knockback cache shape changed.");
                Array.Copy(calculated, cache, cache.Length);
            }
        });
        rolesSupplied = true;
        if (baitNeedsInit) { initialized.Add(bait!); baitReady = HasPosition(bait!, "_safeOffs"); }
        if (knockbackNeedsInit) { initialized.Add(knockback!); knockbackReady = HasPosition(knockback!, "_safeDirs"); }
    }

    private void SyncTwinOrientation(object[] components)
    {
        var twin = components.FirstOrDefault(c => c.GetType().Name == "P2TwinStillnessSilence");
        if (twin == null || initialized.Contains(twin) || Convert.ToInt32(Read(twin, "NumCasts")) != 0) return;
        var caster = Read(twin, "_source");
        var cast = Read(caster, "CastInfo");
        var action = Convert.ToUInt32(Read(Read(cast, "Action"), "ID"));
        if (action is not (40193 or 40194) || Convert.ToUInt32(Read(caster, "OID")) != 0x45A0) return;
        var aoes = (IList)Read(twin, "AOEs")!;
        if (aoes.Count != 2) return; // wait for the native first-cast handler
        twinAction = action;
        if (action == 40193)
        {
            twinOrientation = "Stillness: native orientation";
            initialized.Add(twin);
            return;
        }
        var rotation = Read(cast, "Rotation")!;
        var radians = Convert.ToSingle(Read(rotation, "Rad"));
        var facing = Convert.ToSingle(Read(Read(caster, "Rotation"), "Rad"));
        var difference = MathF.Abs(MathF.IEEERemainder(radians - facing, MathF.Tau));
        if (MathF.Abs(difference - MathF.PI) < 0.03f)
        {
            twinOrientation = "Silence: already native orientation";
            initialized.Add(twin);
            return;
        }
        if (difference >= 0.03f) throw new InvalidOperationException("Unexpected Twin Silence cast direction; bridge disconnected.");

        // AnoMech encodes Silence's rearward angle in its damage area, not its cast
        // packet. Give ONLY the owned native component a normalized cast copy.
        // Never change the live actor/cast, game packets, shapes or drawing code.
        var copy = typeof(object).GetMethod("MemberwiseClone", Private)!.Invoke(cast, null)!;
        var corrected = MathF.IEEERemainder(radians + MathF.PI, MathF.Tau);
        Activation.Write(copy, "Rotation", Activator.CreateInstance(rotation.GetType(), [corrected]));
        var saved = aoes.Cast<object>().ToArray();
        try
        {
            aoes.Clear();
            twin.GetType().GetMethod("OnCastStarted")!.Invoke(twin, [caster, copy]);
            if (aoes.Count != 2) throw new InvalidOperationException("Native Twin cleave sequence changed.");
            for (var i = 0; i < 2; ++i)
            {
                var value = aoes[i]!;
                // Preserve the original deadlines exactly despite initializing on the next frame.
                Activation.Write(value, "Activation", Read(saved[i], "Activation"));
                aoes[i] = value;
            }
        }
        catch
        {
            aoes.Clear();
            foreach (var aoe in saved) aoes.Add(aoe);
            throw;
        }
        twinOrientation = "Silence: cast direction normalized by 180 degrees";
        initialized.Add(twin);
    }

    private static bool HasPosition(object component, string field)
    {
        var value = ((Array)Read(component, field)!).GetValue(0);
        return Convert.ToSingle(Read(value, "X")) != 0 || Convert.ToSingle(Read(value, "Z")) != 0;
    }

    internal static void WithTemporaryRoles(IDictionary assignments, ulong[] contentIds, Type roleType, Action calculate)
    {
        if (contentIds.Length != 8 || contentIds.Contains(0UL) || contentIds.Distinct().Count() != 8)
            throw new InvalidOperationException("Native party resolution requires eight unique nonzero identities.");
        var previous = contentIds.Select(id => (Present: assignments.Contains(id), Value: assignments[id])).ToArray();
        var temporary = Enumerable.Range(0, 8).Select(r => Enum.Parse(roleType, RoleSync.Names[r])).ToArray();
        try
        {
            for (var r = 0; r < 8; ++r) assignments[contentIds[r]] = temporary[r];
            calculate();
        }
        finally
        {
            // No Modified event or save: the real-party dictionary is restored before returning.
            for (var r = 0; r < 8; ++r)
                if (previous[r].Present) assignments[contentIds[r]] = previous[r].Value;
                else assignments.Remove(contentIds[r]);
        }
    }
}
