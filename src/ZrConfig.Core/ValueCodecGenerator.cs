using System.Text;

namespace ZrConfig.Core;
/// <summary>把类型图编译成普通 C# 调用；Unity 热更新程序集不依赖运行时反射/动态泛型。</summary>
internal sealed class ValueCodecGenerator
{
    private readonly IReadOnlyList<ConfigType> _types;
    private readonly Dictionary<ConfigType, int> _ids;
    private readonly ConfigType[] _tags;
    public ValueCodecGenerator(IEnumerable<ConfigType> roots)
    {
        _types = ConfigTypeRegistry.Graph(roots);
        _ids = _types.Select((t, i) => (t, i)).ToDictionary(p => p.t, p => p.i);
        _tags = ConfigTypeRegistry.WireTags(_types);
    }

    public string Write(ConfigType type, string value, string depth = "0") => $"Write{_ids[type]}(writer, {value}, {depth});";
    public string Read(ConfigType type, string depth = "0") => $"Read{_ids[type]}(reader, context, {depth})";
    public void Append(StringBuilder sb)
    {
        sb.AppendLine("        private static void WriteTypes(BinaryWriter writer)")
            .AppendLine("        {")
            .AppendLine($"            writer.Write({_tags.Length});");
        foreach (ConfigType tag in _tags)
        {
            sb.AppendLine($"            BinaryFormat.WriteString(writer, \"{tag.ClrType.FullName}\");");
        }

        sb.AppendLine("        }")
            .AppendLine("        private static ConfigReadContext ReadTypes(BinaryReader reader)")
            .AppendLine("        {")
            .AppendLine($"            return new ConfigReadContext(reader, ResolveType, {_tags.Length});")
            .AppendLine("        }")
            .AppendLine("        private static int ResolveType(string name)")
            .AppendLine("        {")
            .AppendLine("            switch (name)")
            .AppendLine("            {");
        for (int i = 0; i < _tags.Length; i++)
        {
            sb.AppendLine($"                case \"{_tags[i].ClrType.FullName}\": return {i};");
        }

        sb.AppendLine("                default: return -1;").AppendLine("            }").AppendLine("        }");
        foreach (ConfigType type in _types)
        {
            int id = _ids[type];
            if (type.Kind == ConfigTypeKind.Dictionary && type.Key!.Kind == ConfigTypeKind.Enum)
            {
                string underlying = ConfigTypeRegistry.CsName(Enum.GetUnderlyingType(type.Key.ClrType));
                sb.AppendLine($"        private static readonly Comparison<{type.Key.CsName}> s_compare{id} = Compare{id};")
                    .AppendLine($"        private static int Compare{id}({type.Key.CsName} left, {type.Key.CsName} right)")
                    .AppendLine("        {")
                    .AppendLine($"            return (({underlying})left).CompareTo(({underlying})right);")
                    .AppendLine("        }");
            }

            if (type.Kind == ConfigTypeKind.Converter)
            {
                sb.AppendLine($"        private static readonly IConfigConverter<{type.CsName}> s_converter{id} = new {ConfigTypeRegistry.CsName(type.Converter!)}();");
            }

            sb.AppendLine($"        private static void Write{id}(BinaryWriter writer, {type.CsName} value, int depth)")
                .AppendLine("        {");
            WriteBody(sb, type);
            sb.AppendLine("        }")
                .AppendLine($"        private static {type.CsName} Read{id}(BinaryReader reader, ConfigReadContext context, int depth)")
                .AppendLine("        {");
            ReadBody(sb, type);
            sb.AppendLine("        }");
            if (type.Kind == ConfigTypeKind.Object && !type.ClrType.IsAbstract)
            {
                sb.AppendLine($"        private static void WriteBody{id}(BinaryWriter writer, {type.CsName} value, int depth)")
                    .AppendLine("        {")
                    .AppendLine("            ConfigReadContext.CheckDepth(depth);");
                foreach (ConfigMember member in type.Members)
                {
                    sb.AppendLine("            " + Write(member.Type, "value.@" + member.Name, "depth + 1"));
                }

                sb.AppendLine("        }")
                    .AppendLine($"        private static {type.CsName} ReadBody{id}(BinaryReader reader, ConfigReadContext context, int depth)")
                    .AppendLine("        {")
                    .AppendLine("            ConfigReadContext.CheckDepth(depth);")
                    .AppendLine($"            var value = new {type.CsName}();");
                foreach (ConfigMember member in type.Members)
                {
                    sb.AppendLine($"            try{{value.@{member.Name} = {Read(member.Type, "depth + 1")};}}catch(Exception error){{throw global::ZrConfig.Runtime.Server.DecodeGuard.AtField(error,\"{member.Name}\");}}");
                }

                sb.AppendLine("            return value;").AppendLine("        }");
            }
        }
    }

    private void WriteBody(StringBuilder sb, ConfigType type)
    {
        int id = _ids[type];
        switch (type.Kind)
        {
            case ConfigTypeKind.Primitive:
            case ConfigTypeKind.TableReference:
                sb.AppendLine(type.ClrType == typeof(string) ? "            BinaryFormat.WriteString(writer, value);" : "            writer.Write(value);");
                break;
            case ConfigTypeKind.Enum:
                sb.AppendLine($"            writer.Write(({ConfigTypeRegistry.CsName(Enum.GetUnderlyingType(type.ClrType))})value);");
                break;
            case ConfigTypeKind.Array:
                if (type.ClrType.GetArrayRank() > 1)
                {
                    WriteMatrix(sb, type);
                    break;
                }

                sb.AppendLine("            ConfigReadContext.CheckDepth(depth);")
                    .AppendLine("            int count = value == null ? 0 : value.Length;")
                    .AppendLine("            ConfigReadContext.CheckLength(count);")
                    .AppendLine("            writer.Write(count);")
                    .AppendLine("            for (int i = 0; i < count; i++)")
                    .AppendLine("                " + Write(type.Element!,
                    "value![i]",
                    "depth + 1"));
                break;
            case ConfigTypeKind.Nullable:
                sb.AppendLine("            ConfigReadContext.CheckDepth(depth);")
                    .AppendLine("            writer.Write(value.HasValue);")
                    .AppendLine("            if (value.HasValue) " + Write(type.Element!,
                    "value.Value",
                    "depth + 1"));
                break;
            case ConfigTypeKind.Dictionary:
                sb.AppendLine("            ConfigReadContext.CheckDepth(depth);")
                    .AppendLine("            int count = value == null ? 0 : value.Count;")
                    .AppendLine("            ConfigReadContext.CheckLength(count);")
                    .AppendLine("            writer.Write(count);")
                    .AppendLine("            if (count == 0) return;")
                    .AppendLine($"            {type.Key!.CsName}[] keys = System.Buffers.ArrayPool<{type.Key.CsName}>.Shared.Rent(count);")
                    .AppendLine("            try")
                    .AppendLine("            {")
                    .AppendLine("                value!.Keys.CopyTo(keys, 0);")// String keys are references; the non-generic overload avoids allocating comparer.Compare delegates.
                .AppendLine(type.Key.ClrType == typeof(string) ? "                Array.Sort((Array)keys, 0, count, StringComparer.Ordinal);" : type.Key.Kind == ConfigTypeKind.Enum ? $"                ConfigKeySort.Sort(keys, count, s_compare{id});" : $"                Array.Sort(keys, 0, count, Comparer<{type.Key.CsName}>.Default);")
                    .AppendLine("                for (int i = 0; i < count; i++)")
                    .AppendLine("                {")
                    .AppendLine($"                    {type.Key.CsName} key = keys[i];")
                    .AppendLine("                    " + Write(type.Key,
                    "key",
                    "depth + 1"))
                    .AppendLine("                    " + Write(type.Element!,
                    "value[key]",
                    "depth + 1"))
                    .AppendLine("                }")
                    .AppendLine("            }")
                    .AppendLine("            finally")
                    .AppendLine("            {")
                    .AppendLine($"                System.Buffers.ArrayPool<{type.Key.CsName}>.Shared.Return(keys, clearArray: {(type.Key.IsReference ? "true" : "false")});")
                    .AppendLine("            }");
                break;
            case ConfigTypeKind.RawJson:
                sb.AppendLine("            BinaryFormat.WriteString(writer, value.Text ?? \"null\");");
                break;
            case ConfigTypeKind.Kv:
                sb.AppendLine("            writer.Write(value.Key); writer.Write(value.Value);");
                break;
            case ConfigTypeKind.Vector2:
                sb.AppendLine("            writer.Write(value.X); writer.Write(value.Y);");
                break;
            case ConfigTypeKind.Vector3:
                sb.AppendLine("            writer.Write(value.X); writer.Write(value.Y); writer.Write(value.Z);");
                break;
            case ConfigTypeKind.Converter:
                sb.AppendLine($"            s_converter{id}.Write(writer, value);");
                break;
            case ConfigTypeKind.Object:
                if (type.IsPolymorphic)
                {
                    sb.AppendLine("            if (value == null) { writer.Write(-1); return; }")
                        .AppendLine("            Type actual = value.GetType();");
                    foreach (ConfigType variant in type.Variants.Prepend(type).Where(t => !t.ClrType.IsAbstract))
                    {
                        sb.AppendLine($"            if (actual == typeof({variant.CsName})) {{ writer.Write({Array.IndexOf(_tags, variant)}); WriteBody{_ids[variant]}(writer, ({variant.CsName})value, depth); return; }}");
                    }

                    sb.AppendLine("            throw new InvalidDataException(\"Unknown polymorphic type; regenerate config code.\");");
                }
                else
                {
                    if (type.IsReference)
                    {
                        sb.AppendLine("            writer.Write(value != null); if (value == null) return;");
                    }

                    sb.AppendLine($"            WriteBody{id}(writer, value, depth);");
                }

                break;
        }
    }

    private void ReadBody(StringBuilder sb, ConfigType type)
    {
        int id = _ids[type];
        switch (type.Kind)
        {
            case ConfigTypeKind.Primitive:
            case ConfigTypeKind.TableReference:
                sb.AppendLine($"            return {PrimitiveRead(type.ClrType)};");
                break;
            case ConfigTypeKind.Enum:
                sb.AppendLine($"            var value = ({type.CsName}){PrimitiveRead(Enum.GetUnderlyingType(type.ClrType))};");
                object[] distinct = Enum.GetValues(type.ClrType).Cast<object>().Distinct().ToArray();
                if (distinct.Length == 0)
                {
                    sb.AppendLine("            throw new InvalidDataException(\"Empty enum.\");");
                }
                else
                {
                    string checks = string.Join(" && ", distinct.Select(v => $"value != {type.CsName}.@{Enum.GetName(type.ClrType, v)}"));
                    sb.AppendLine($"            if ({checks}) throw new InvalidDataException(\"Invalid enum value.\");")
                        .AppendLine("            return value;");
                }

                break;
            case ConfigTypeKind.Array:
                if (type.ClrType.GetArrayRank() > 1)
                {
                    ReadMatrix(sb, type);
                    break;
                }

                sb.AppendLine("            ConfigReadContext.CheckDepth(depth);")
                    .AppendLine("            int count = context.ReadLength(reader);")
                    .AppendLine($"            if (count == 0) return Array.Empty<{type.Element!.CsName}>();")
                    .AppendLine($"            {ArrayVariableType(type)} value = {NewArray(type.Element.ClrType, "count")};")
                    .AppendLine("            for (int i = 0; i < count; i++)")
                    .AppendLine($"                try{{value[i] = {Read(type.Element, "depth + 1")};}}catch(Exception error){{throw global::ZrConfig.Runtime.Server.DecodeGuard.AtField(error,\"[\"+i.ToString(System.Globalization.CultureInfo.InvariantCulture)+\"]\");}}")
                    .AppendLine("            return value;");
                break;
            case ConfigTypeKind.Nullable:
                sb.AppendLine("            ConfigReadContext.CheckDepth(depth);")
                    .AppendLine("            byte present = reader.ReadByte();")
                    .AppendLine("            if (present > 1) throw new InvalidDataException(\"Invalid nullable marker.\");")
                    .AppendLine($"            return present == 1 ? ({type.CsName}){Read(type.Element!, "depth + 1")} : null;");
                break;
            case ConfigTypeKind.Dictionary:
                sb.AppendLine("            ConfigReadContext.CheckDepth(depth);")
                    .AppendLine("            int count = context.ReadLength(reader);")
                    .AppendLine($"            var value = new {type.CsName}(count);")
                    .AppendLine("            for (int i = 0; i < count; i++)")
                    .AppendLine("            {")
                    .AppendLine($"                {type.Key!.CsName} key;try{{key = {Read(type.Key, "depth + 1")};}}catch(Exception error){{throw global::ZrConfig.Runtime.Server.DecodeGuard.AtField(error,\"[\"+i.ToString(System.Globalization.CultureInfo.InvariantCulture)+\"].key\");}}")
                    .AppendLine("                if (value.ContainsKey(key)) throw new InvalidDataException(\"Duplicate dictionary key.\");")
                    .AppendLine($"                try{{value.Add(key, {Read(type.Element!, "depth + 1")});}}catch(Exception error){{throw global::ZrConfig.Runtime.Server.DecodeGuard.AtField(error,\"[\"+i.ToString(System.Globalization.CultureInfo.InvariantCulture)+\"].value\");}}")
                    .AppendLine("            }")
                    .AppendLine("            return value;");
                break;
            case ConfigTypeKind.Kv:
                sb.AppendLine($"            return new {type.CsName}(reader.ReadInt32(), reader.ReadInt32());");
                break;
            case ConfigTypeKind.Vector2:
                sb.AppendLine($"            return new {type.CsName}(reader.ReadSingle(), reader.ReadSingle());");
                break;
            case ConfigTypeKind.Vector3:
                sb.AppendLine($"            return new {type.CsName}(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());");
                break;
            case ConfigTypeKind.RawJson:
                sb.AppendLine($"            return new {type.CsName}(BinaryFormat.ReadString(reader) ?? \"null\");");
                break;
            case ConfigTypeKind.Converter:
                sb.AppendLine($"            return s_converter{id}.Read(reader);");
                break;
            case ConfigTypeKind.Object:
                if (type.IsPolymorphic)
                {
                    sb.AppendLine("            switch (context.ReadType(reader))")
                        .AppendLine("            {")
                        .AppendLine("                case -1: return null!;");
                    foreach (ConfigType variant in type.Variants.Prepend(type).Where(t => !t.ClrType.IsAbstract))
                    {
                        sb.AppendLine($"                case {Array.IndexOf(_tags, variant)}: return ReadBody{_ids[variant]}(reader, context, depth);");
                    }

                    sb.AppendLine("                default: throw new InvalidDataException(\"Type is not assignable to this field.\");")
                        .AppendLine("            }");
                }
                else
                {
                    if (type.IsReference)
                    {
                        sb.AppendLine("            if (!reader.ReadBoolean()) return null!;");
                    }

                    sb.AppendLine($"            return ReadBody{id}(reader, context, depth);");
                }

                break;
        }
    }

    private static string ArrayVariableType(ConfigType type)
    {
        ConfigType element = type;
        while (element.Kind is ConfigTypeKind.Array or ConfigTypeKind.Nullable)
        {
            element = element.Element!;
        }

        return element.Kind == ConfigTypeKind.Primitive ? type.CsName : "var";
    }

    private static string NewArray(Type element, string count)
    {
        string suffix = "";
        while (element.IsArray)
        {
            suffix += "[" + new string(',', element.GetArrayRank() - 1) + "]";
            element = element.GetElementType()!;
        }

        return $"new {ConfigTypeRegistry.CsName(element)}[{count}]" + suffix;
    }

    private void WriteMatrix(StringBuilder sb, ConfigType type)
    {
        int rank = type.ClrType.GetArrayRank();
        sb.AppendLine("            ConfigReadContext.CheckDepth(depth);")
            .AppendLine("            ConfigReadContext.CheckLength(value == null ? 0 : value.Length);");
        for (int i = 0; i < rank; i++)
        {
            sb.AppendLine($"            int d{i} = value == null ? 0 : value.GetLength({i});")
                .AppendLine($"            ConfigReadContext.CheckLength(d{i}); writer.Write(d{i});");
        }

        MatrixLoops(sb, rank);
        sb.Append(' ',
            12 + rank * 4)
            .AppendLine(Write(type.Element!,
            "value![" + string.Join(", ",
            Enumerable.Range(0,
            rank)
            .Select(i => "i" + i)) + "]",
            "depth + 1"));
    }

    private void ReadMatrix(StringBuilder sb, ConfigType type)
    {
        int rank = type.ClrType.GetArrayRank();
        sb.AppendLine("            ConfigReadContext.CheckDepth(depth);");
        for (int i = 0; i < rank; i++)
        {
            sb.AppendLine($"            int d{i} = ConfigReadContext.ReadDimension(reader);");
        }

        sb.AppendLine("            context.ClaimElements(" + string.Join(" * ",
            Enumerable.Range(0,
            rank)
            .Select(i => i == 0 ? "(long)d0" : "d" + i)) + ");")
            .AppendLine($"            {ArrayVariableType(type)} value = {NewArray(type.Element!.ClrType, string.Join(", ", Enumerable.Range(0, rank).Select(i => "d" + i)))};");
        MatrixLoops(sb, rank);
        sb.Append(' ',
            12 + rank * 4)
            .AppendLine($"try{{value[{string.Join(", ", Enumerable.Range(0, rank).Select(i => "i" + i))}] = {Read(type.Element, "depth + 1")};}}catch(Exception error){{throw global::ZrConfig.Runtime.Server.DecodeGuard.AtField(error,\"[\"+({string.Join(" + ", Enumerable.Range(0,rank).Select(i => "i"+i+string.Concat(Enumerable.Range(i+1,rank-i-1).Select(j=>"*d"+j))))}).ToString(System.Globalization.CultureInfo.InvariantCulture)+\"]\");}}")
            .AppendLine("            return value;");
    }

    private static void MatrixLoops(StringBuilder sb, int rank)
    {
        for (int i = 0; i < rank; i++)
        {
            sb.Append(' ', 12 + i * 4).AppendLine($"for (int i{i} = 0; i{i} < d{i}; i{i}++)");
        }
    }

    private static string PrimitiveRead(Type type) => type == typeof(string) ? "BinaryFormat.ReadString(reader) ?? string.Empty" :
        type == typeof(bool) ? "ZrConfig.Runtime.Server.DecodeGuard.ReadBoolean(reader)" :
        type == typeof(float) || type == typeof(double) ? "ZrConfig.Runtime.Server.DecodeGuard.Finite(reader.Read" + Type.GetTypeCode(type) + "())" : "reader.Read" + Type.GetTypeCode(type) + "()";
}
