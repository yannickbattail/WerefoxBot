using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Werefox.Interfaces;

namespace Werefox.Engine
{
    public static class Utils
    {
        public static string ToDescription<TEnum>(this TEnum enumValue)
        {
            FieldInfo info = enumValue.GetType().GetField(enumValue.ToString());
            var attributes = (DescriptionAttribute[])info.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes?[0].Description ?? enumValue.ToString();
        }

        public static string DisplayPlayerList(IEnumerable<IPlayer> players)
        {
            return string.Join(", ", players.Select(p => p.GetMention()));
        }
    }
}