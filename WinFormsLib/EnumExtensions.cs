using System.Reflection;

namespace WinFormsLib
{
    /// <summary>Provides helpers for enum metadata, conversion, and flag inspection.</summary>
    public static class EnumExtensions
    {
        /// <summary>Gets the value stored in the enum member's value attribute.</summary>
        /// <typeparam name="T">The type to cast the attribute value to.</typeparam>
        /// <param name="super">The enum value to inspect.</param>
        /// <returns>The attribute value, or the default value of <typeparamref name="T"/> if no attribute is found.</returns>
        public static T? GetValue<T>(this Enum super)
        {
            Type type = super.GetType();
            return Enum.GetName(type, super) is string s
                   && type.GetField(s) is FieldInfo fi
                   && fi.GetCustomAttributes(typeof(Utils.ValueAttribute), false) is Utils.ValueAttribute[] attribs
                   && attribs.Length > 0
                ? (T)attribs[0].Value
                : default;
        }

        /// <summary>Gets the string supplied by the enum member's global string value attribute.</summary>
        /// <param name="super">The enum value to inspect.</param>
        /// <returns>The attribute value, or an empty string if no attribute is found.</returns>
        public static string GetGlobalStringValue(this Enum super)
        {
            Type type = super.GetType();
            return Enum.GetName(type, super) is string s
                   && type.GetField(s) is FieldInfo fi
                   && fi.GetCustomAttributes(typeof(Utils.GlobalStringValueAttribute), false) is Utils.GlobalStringValueAttribute[] attribs
                   && attribs.Length > 0
                ? attribs[0].Value
                : string.Empty;
        }

        /// <summary>Converts the numeric value to another enum type through an unsigned 32-bit integer.</summary>
        /// <typeparam name="T">The destination enum type.</typeparam>
        /// <param name="super">The enum value to convert.</param>
        /// <exception cref="OverflowException">The source value is outside the range of <see cref="uint"/>.</exception>
        public static T ToEnum<T>(this Enum super) where T : Enum => (T)Enum.ToObject(typeof(T), Convert.ToUInt32(super));

        /// <summary>Checks whether all bits in a nonzero flag are set in the supplied enum value.</summary>
        /// <param name="super">The enum value to inspect.</param>
        /// <param name="flag">The flag bits to check.</param>
        /// <returns><see langword="true"/> if all the flag bits are set. A zero flag returns <see langword="false"/>.</returns>
        /// <exception cref="OverflowException">An operand converted to <see cref="uint"/> is outside its range.</exception>
        public static bool ContainsFlag(this Enum super, Enum flag)
        {
            uint i = Convert.ToUInt32(flag);
            if (i != 0)
            {
                uint n = Convert.ToUInt32(super);
                return (n & i) == i;
            }
            return false;
        }

        /// <summary>Finds declared enum values whose bits are set in the supplied combination.</summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="comboFlag">The flag combination to inspect.</param>
        /// <param name="includeCombo">Whether to return all matching declarations, including composite values, rather than decompose the combination.</param>
        /// <remarks>When decomposing, any remaining bits that do not match a declared flag are ignored.</remarks>
        public static T[] GetContainingFlags<T>(this T comboFlag, bool includeCombo = false) where T : struct, Enum
        {
            IEnumerable<T> values = Enum.GetValues<T>();
            IEnumerable<T> relevantFlags = values.Where(v => comboFlag.ContainsFlag(v));
            if (!includeCombo)
            {
                uint remainingBits = Convert.ToUInt32(comboFlag);
                List<T> flags = [];
                while (remainingBits != 0)
                {
                    T flag = relevantFlags.FirstOrDefault();
                    uint flagBits = Convert.ToUInt32(flag);
                    if (flagBits == 0)
                    {
                        break;
                    }

                    flags.Add(flag);
                    remainingBits &= ~flagBits;
                    comboFlag = (T)Enum.ToObject(typeof(T), remainingBits);
                }
                return [.. flags];
            }
            return [.. relevantFlags];
        }

        /// <summary>Returns the zero-based position of the value in the enum's value list, or -1 if absent.</summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="enumFlag">The value to locate.</param>
        public static int GetFlagIndex<T>(this T enumFlag) where T : struct, Enum => Array.IndexOf(Enum.GetValues<T>(), enumFlag);
    }
}
