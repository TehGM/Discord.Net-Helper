using System;
using System.Collections.Generic;
using System.Text;

namespace TehGM.DiscordNetBot.CommandsProcessing
{
    /// <summary>Handlers marked with this attribute will not be created when a debugger is attached.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ProductionOnlyAttribute : Attribute
    {
    }
}
