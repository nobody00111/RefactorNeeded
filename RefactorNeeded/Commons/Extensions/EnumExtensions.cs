using System;
using System.Linq;

namespace RefactorNeeded.Commons.Extensions
{
    public static class EnumExtensions
    {
        public static bool In<TEnum>(this TEnum @enum, params TEnum[] values)
            where TEnum : Enum
        {
            return values.Contains(@enum);
        }
    }
}