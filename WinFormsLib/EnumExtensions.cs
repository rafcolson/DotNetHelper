using System.Reflection;

namespace WinFormsLib
{
    public static class EnumExtensions
    {
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

        public static T ToEnum<T>(this Enum super) where T : Enum => (T)Enum.ToObject(typeof(T), Convert.ToUInt32(super));

        public static bool ContainsFlag(this Enum super, Enum flag)
        {
            uint i = Convert.ToUInt32(flag);
            if (i != 0)
            {
                uint n = Convert.ToUInt32(super);
                return ((n & i) == i);
            }
            return false;
        }

        public static T[] GetContainingFlags<T>(this T comboFlag, bool includeCombo = false) where T : Enum
        {
            IEnumerable<T> values = Enum.GetValues(typeof(T)).Cast<T>();
            IEnumerable<T> relevantFlags = values.Where(v => comboFlag.ContainsFlag(v));
            if (!includeCombo)
            {
                uint i = Convert.ToUInt32(comboFlag);
                List<T> l = new();
                while (i != 0)
                {
                    if (relevantFlags.FirstOrDefault() is T f)
                    {
                        l.Add(f);
                        i ^= Convert.ToUInt32(f);
                        comboFlag = (T)Enum.ToObject(typeof(T), i);
                    }
                }
                return l.ToArray();
            }
            return relevantFlags.ToArray();
        }

        public static int GetFlagIndex<T>(this T enumFlag) where T : Enum => Array.IndexOf(Enum.GetValues(typeof(T)), enumFlag);
    }
}
