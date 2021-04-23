using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OliveToast.Managements
{
    class HideInHelp : Attribute
    {

    }

    public static class InfoExtension
    {
        public static bool HaveAttribute<T>(this CommandInfo info)
        {
            return info.Attributes.Where(a => a.GetType() == typeof(T)).Any();
        }
        public static bool HaveAttribute<T>(this ModuleInfo info)
        {
            return info.Attributes.Where(a => a.GetType() == typeof(T)).Any();
        }
    }
}
