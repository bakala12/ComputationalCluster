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
        public RegisterType ComponentType { get; set;}
        public string[] SolvableProblems { get; set; }
        //some timeout support - not needed for now, only use of this
        //is for computation stage (and all this deregister shit)
        //public uint time etc.
        //maybe something more
    }
}