using System;
using System.Collections.Generic;
using System.Text;

namespace TehGM.DiscordNetBot.CommandsProcessing
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ProductionOnlyAttribute : Attribute
    {
    }
}
