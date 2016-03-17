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
        /// <summary>
        /// Id of a component
        /// </summary>
        public ulong componentId { get; private set; }
        /// <summary>
        /// Status of a component
        /// </summary>
        public StatusThreadState componentStatus { get; private set; }
    }
}