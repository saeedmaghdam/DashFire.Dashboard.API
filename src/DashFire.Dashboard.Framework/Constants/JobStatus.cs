namespace DashFire.Dashboard.Framework.Constants
{
    public enum JobStatus
    {
        New,            // Job has not run already.
        Registering,    // Job is registering itself at the server.
        Synchronizing,  // Job is synchronizing using heart-bit mechanism.
        Scheduled,      // Job has been scheduled and is in idle mode.
        Running,        // Job is running.
        Idle,           // When job is paused paused for some reason.
        Shutdown,       // Job has been shutdown.
        Crashed         // Job is crashed and could not continue.
    }
}
