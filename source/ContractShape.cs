using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;

namespace AnoBossBridge;

// Names and shapes deliberately exclude assembly versions and metadata token numbers.
internal static class ContractShape
{
    internal const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    internal static string TypeName(Type t) => t.IsByRef ? TypeName(t.GetElementType()!) + "&"
        : t.IsPointer ? TypeName(t.GetElementType()!) + "*"
        : t.IsArray ? TypeName(t.GetElementType()!) + "[" + new string(',', t.GetArrayRank() - 1) + "]"
        : t.IsGenericParameter ? (t.DeclaringMethod == null ? "!" : "!!") + t.GenericParameterPosition
        : t.IsGenericType ? t.GetGenericTypeDefinition().FullName + "<" + string.Join(",", t.GetGenericArguments().Select(TypeName)) + ">"
        : t.FullName ?? t.Name;

    internal static MemberInfo[] Members(Type t, string name) => name == ".ctor"
        ? t.GetConstructors(Flags).Cast<MemberInfo>().ToArray()
        : t.GetMember(name, Flags).Where(m => m is FieldInfo or PropertyInfo or MethodInfo).ToArray();

    internal static string Shape(MemberInfo m) => m switch
    {
        FieldInfo f => $"F:{TypeName(f.FieldType)}:{f.Attributes}:" + (f.IsLiteral ? Convert.ToString(f.GetRawConstantValue(), System.Globalization.CultureInfo.InvariantCulture) : ""),
        PropertyInfo p => $"P:{TypeName(p.PropertyType)}:{p.GetMethod?.Attributes}:{p.SetMethod?.Attributes}:" + string.Join(",", p.GetIndexParameters().Select(x => TypeName(x.ParameterType))),
        MethodBase method => $"M:{method.Attributes}:{(method.IsGenericMethod ? method.GetGenericArguments().Length : 0)}:" + (method is MethodInfo mi ? TypeName(mi.ReturnType) : "void") + "(" + string.Join(",", method.GetParameters().Select(p => TypeName(p.ParameterType))) + ")",
        Type t => TypeName(t),
        _ => throw new NotSupportedException(m.ToString())
    };

    private static readonly Dictionary<short, OpCode> Codes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)!).ToDictionary(c => c.Value);

    // Guard only explicitly selected lifecycle/adapter boundaries, never a whole DLL.
    // Resolve IL tokens to names/signatures so unrelated added types do not invalidate a guard.
    internal static string Fingerprint(MethodBase method)
    {
        var body = method.GetMethodBody() ?? throw new InvalidOperationException("No managed method body: " + method.Name);
        var il = body.GetILAsByteArray()!;
        var result = new StringBuilder().Append(body.InitLocals).Append('|');
        foreach (var local in body.LocalVariables) result.Append(TypeName(local.LocalType)).Append(':').Append(local.IsPinned).Append('|');
        foreach (var clause in body.ExceptionHandlingClauses)
        {
            result.Append(clause.Flags).Append(':').Append(clause.TryOffset).Append(':').Append(clause.TryLength).Append(':').Append(clause.HandlerOffset).Append(':').Append(clause.HandlerLength);
            if (clause.Flags == ExceptionHandlingClauseOptions.Clause) result.Append(':').Append(TypeName(clause.CatchType!));
            if (clause.Flags == ExceptionHandlingClauseOptions.Filter) result.Append(':').Append(clause.FilterOffset);
            result.Append('|');
        }
        var typeArgs = method.DeclaringType?.GetGenericArguments();
        var methodArgs = method.IsGenericMethod ? method.GetGenericArguments() : null;
        for (var pos = 0; pos < il.Length;)
        {
            short value = il[pos++];
            if (value == 0xFE) value = unchecked((short)(0xFE00 | il[pos++]));
            var code = Codes[value];
            result.Append(code.Name).Append(':');
            var size = code.OperandType switch
            {
                OperandType.InlineNone => 0,
                OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                OperandType.InlineVar => 2,
                OperandType.InlineI8 or OperandType.InlineR => 8,
                OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, pos),
                _ => 4
            };
            if (code.OperandType == OperandType.InlineString)
                result.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(method.Module.ResolveString(BitConverter.ToInt32(il, pos)))));
            else if (code.OperandType is OperandType.InlineField or OperandType.InlineMethod or OperandType.InlineType or OperandType.InlineTok)
            {
                var member = method.Module.ResolveMember(BitConverter.ToInt32(il, pos), typeArgs, methodArgs)!;
                result.Append(member.DeclaringType == null ? "" : TypeName(member.DeclaringType)).Append('.').Append(member.Name).Append(':').Append(Shape(member));
                if (member is MethodInfo generic && generic.IsGenericMethod) result.Append('<').AppendJoin(',', generic.GetGenericArguments().Select(TypeName)).Append('>');
                if (member is MethodBase generated && generated.Name.StartsWith("<" + method.Name + ">", StringComparison.Ordinal))
                    result.Append(':').Append(Fingerprint(generated));
            }
            else if (code.OperandType == OperandType.InlineSig)
                throw new NotSupportedException("Indirect-call signature needs review: " + method.Name);
            else result.Append(Convert.ToHexString(il.AsSpan(pos, size)));
            pos += size;
            result.Append('|');
        }
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(result.ToString())));
    }
}
