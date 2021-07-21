using System;
using DashFire.Dashboard.Framework.Constants;

namespace DashFire.Dashboard.Framework.Services.Job
{
    public interface ICachedJob
    {
        string Key
        {
            get;
            set;
        }

        string InstanceId
        {
            get;
            set;
        }

        bool IsOnline
        {
            get;
            set;
        }

        DateTime LastExecutionDateTime
        {
            get;
            set;
        }

        DateTime NextExecutionDateTime
        {
            get;
            set;
        }

        string LastStatusMessage
        {
            get;
            set;
        }

        JobStatus Status
        {
            get;
            set;
        }
    }
}
