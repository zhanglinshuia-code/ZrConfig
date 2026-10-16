// Shared by the exporter and Unity. The tool updates recorded, unmodified versions and preserves custom edits.
using System;

namespace ZrConfig.Types
{
    [Serializable]
    public readonly struct Kv
    {
        public readonly int Key;
        public readonly int Value;
        public Kv(int key, int value)
        {
            Key = key;
            Value = value;
        }
    }

    [Serializable]
    public readonly struct Vector2
    {
        public readonly float X;
        public readonly float Y;
        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    [Serializable]
    public readonly struct Vector3
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    [Serializable]
    public readonly struct RawJson
    {
        public readonly string Text;
        public RawJson(string text)
        {
            Text = text;
        }

        public override string ToString() => Text;
    }
}
