using System;
using System.Collections.Generic;
using System.Text;

namespace TehGM.DiscordBot.CommandsProcessing
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ProductionOnlyAttribute : Attribute
    {
    }
}
