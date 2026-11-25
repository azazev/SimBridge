using AnoBossBridge;
using B = BossMod;

internal static class NativeTwinTests
{
    internal static void Run()
    {
        static void Check(bool ok, string why) { if (!ok) throw new Exception(why); }
        static bool SameAngle(float a, float b) => MathF.Abs(MathF.IEEERemainder(a - b, MathF.Tau)) < 0.0001f;
        foreach (var action in new uint[] { 40193, 40194 })
        for (var direction = 0; direction < 8; ++direction)
        {
            var facing = direction * MathF.PI / 4;
            var actor = new B.TwinActor { Rotation = new(facing), CastInfo = new() { Action = new(action), Rotation = new(facing) } };
            var twin = new B.P2TwinStillnessSilence(actor);
            var module = new B.TestModule { Components = [twin] };
            var sync = new NativeDiamondSync([], []);
            sync.Sync(module); Check(twin.Calls == 0, "wait for native cast handler");
            twin.OnCastStarted(actor, actor.CastInfo);
            var original = twin.AOEs.ToArray(); var liveCast = actor.CastInfo;
            sync.Sync(module);
            Check(SameAngle(twin.AOEs[0].Rotation.Rad, facing + (action == 40194 ? MathF.PI : 0)), "first cleave follows AnoMech damage area direction");
            Check(SameAngle(twin.AOEs[1].Rotation.Rad, facing + (action == 40193 ? MathF.PI : 0)), "second cleave follows opposite damage area direction");
            Check(twin.AOEs[0].Width == (action == 40194 ? 90 : 270) && twin.AOEs[1].Width == (action == 40194 ? 270 : 90), "native cone widths and sequence retained");
            Check(twin.AOEs.Select(x => x.Activation).SequenceEqual(original.Select(x => x.Activation)), "original deadlines preserved exactly");
            Check(ReferenceEquals(liveCast, actor.CastInfo) && actor.CastInfo.Rotation.Rad == facing && actor.Rotation.Rad == facing, "live actor and cast unchanged");
            Check(twin.Calls == (action == 40194 ? 2 : 1), "only Silence invokes native reconstruction");
            sync.Sync(module); Check(twin.Calls == (action == 40194 ? 2 : 1), "normalization occurs once");
            twin.NumCasts = 1; twin.AOEs.RemoveAt(0); sync.Sync(module);
            Check(twin.AOEs.Count == 1, "resolved sequence is not reintroduced");
        }
        foreach (var mode in new[] { "already-native", "resolved", "unexpected", "throws" })
        {
            var actor = new B.TwinActor { CastInfo = new() { Action = new(40194), Rotation = new(mode == "already-native" ? MathF.PI : mode == "unexpected" ? 1 : 0) } };
            var twin = new B.P2TwinStillnessSilence(actor);
            twin.OnCastStarted(actor, actor.CastInfo);
            var before = twin.AOEs.ToArray();
            twin.NumCasts = mode == "resolved" ? 1 : 0; twin.Throw = mode == "throws";
            var failed = false;
            try { new NativeDiamondSync([], []).Sync(new B.TestModule { Components = [twin] }); }
            catch (Exception) { failed = true; }
            Check(failed == (mode is "unexpected" or "throws"), "unexpected directions and native errors propagate to disconnect");
            Check(twin.AOEs.SequenceEqual(before), "leave native directions intact or restore AOEs after failure");
        }
        Console.WriteLine("PASS 4/10: both Twin sequences at eight angles, native timing, once-only normalization and failure restoration");
    }
}

namespace BossMod
{
    internal struct TwinAngle(float rad) { public float Rad = rad; }
    internal record struct TwinAction(uint ID);
    internal sealed class TwinCast { public TwinAction Action; public TwinAngle Rotation; }
    internal sealed class TwinActor { public uint OID = 0x45A0; public TwinAngle Rotation; public TwinCast CastInfo = new(); }
    internal record struct TwinAOE(int Width, TwinAngle Rotation, DateTime Activation);
    // Test double of the pinned native handler: the bridge supplies only a cast copy.
    internal sealed class P2TwinStillnessSilence(TwinActor source)
    {
        private readonly TwinActor _source = source;
        public int NumCasts = 0, Calls = 0;
        public bool Throw = false;
        public List<TwinAOE> AOEs = [];
        public void OnCastStarted(TwinActor caster, TwinCast cast)
        {
            ++Calls;
            var silence = cast.Action.ID == 40194;
            AOEs.Add(new(silence ? 90 : 270, cast.Rotation, new DateTime(2026, 11, 17).AddSeconds(Calls)));
            if (Throw) throw new InvalidOperationException("native handler failure");
            AOEs.Add(new(silence ? 270 : 90, new(cast.Rotation.Rad + MathF.PI), new DateTime(2026, 11, 17).AddSeconds(Calls + 2.1)));
        }
    }
}


