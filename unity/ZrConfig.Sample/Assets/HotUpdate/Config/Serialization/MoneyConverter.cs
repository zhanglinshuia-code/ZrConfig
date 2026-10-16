using System;
using System.Globalization;
using System.IO;
using ZrConfig.Runtime;

namespace Game.Config
{
    public sealed class MoneyConverter : IConfigConverter<Money>
    {
        public Money Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new Money(0);
            }

            decimal cents = decimal.Parse(text,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture) * 100;
            if (decimal.Truncate(cents) != cents)
            {
                throw new FormatException("Money 最多支持两位小数。");
            }

            return new Money(checked((int)cents));
        }

        public void Write(BinaryWriter writer, Money value) => writer.Write(value.Cents);
        public Money Read(BinaryReader reader) => new Money(reader.ReadInt32());
    }
}
