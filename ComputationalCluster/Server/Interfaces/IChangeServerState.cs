using Server.Data;

namespace Server.Interfaces
{
    public interface IChangeServerState : IRunnable
    {
        /// <summary>
        /// Gets the current ServerState
        /// </summary>
        ServerState State { get; }
        /// <summary>
        /// Runs server as Primary server.
        /// </summary>
        void RunAsPrimary();
        /// <summary>
        /// Runs server as Backup server.
        /// </summary>
        void RunAsBackup();
        /// <summary>
        /// Changes server state for the given one
        /// </summary>
        /// <param name="state">New server state.</param>
        void ChangeState(ServerState state);
    }
}
