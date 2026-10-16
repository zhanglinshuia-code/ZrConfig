#nullable enable
using System;
using System.IO;

namespace ZrConfig.Runtime
{
    public static class ConfigLimits
    {
        public const int MaxRows = 5_000_000;
        public const int MaxArrayLength = 1_000_000;
        public const int MaxDepth = 64;
    }

    /// <summary>JSON 中必须显式填写；零、false 等值仍是合法值。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ConfigRequiredAttribute : Attribute
    {
    }

    /// <summary>特殊标量的扩展点。更改二进制布局时必须升级 version。</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class ConfigConverterAttribute : Attribute
    {
        public Type ConverterType
        {
            get;
        }
        public string Version
        {
            get;
        }

        public ConfigConverterAttribute(Type converterType, string version)
        {
            ConverterType = converterType;
            Version = version;
        }
    }

    public interface IConfigConverter<T>
    {
        T Parse(string text);
        void Write(BinaryWriter writer, T value);
        T Read(BinaryReader reader);
    }

    /// <summary>声明特殊转换器实际写入的成员及顺序，供 JSON 和跨语言读取器使用；成员编码必须遵循标准类型协议。</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class ConfigWireLayoutAttribute : Attribute
    {
        public string[] Members { get; }

        public ConfigWireLayoutAttribute(params string[] members) => Members = members;
    }
}
