namespace Server.Interfaces
{
    /// <summary>
    /// Base interface for running and joining multiple threads.
    /// </summary>
    public interface IRunnable
    {
        /// <summary>
        /// Starts some processing in additional threads.
        /// </summary>
        void Run();
        /// <summary>
        /// Joins started threads, cleans up.
        /// </summary>
        void Stop();
    }
}