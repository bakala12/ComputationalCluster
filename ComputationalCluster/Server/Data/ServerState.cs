namespace Server.Data
{
    /// <summary>
    /// Indicates the current state of the server.
    /// </summary>
    public enum ServerState
    {
        /// <summary>
        /// Means that server is a primary server.
        /// </summary>
        Primary,
        /// <summary>
        /// Means that server is currently a backup server.
        /// </summary>
        Backup
    }
}
