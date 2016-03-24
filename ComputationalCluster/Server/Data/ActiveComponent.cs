using System.Diagnostics;
using CommunicationsUtils.Messages;
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming

namespace Server.Data
{
    /// <summary>
    /// Structure for active component.
    /// It stores information about component.
    /// Will be used in the future development.
    /// </summary>
    public class ActiveComponent
    {
        //componentId is not necessary, it's the key of a dict
        public ComponentType ComponentType { get; set;}
        public string[] SolvableProblems { get; set; }
        public Stopwatch StatusWatch { get; set; }

        public ActiveComponent()
        {
            StatusWatch = new Stopwatch();
            StatusWatch.Start();
        }
    }
}