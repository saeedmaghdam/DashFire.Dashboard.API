using System;
using DashFire.Dashboard.Framework.Constants;
using MessagePack;

namespace DashFire.Dashboard.Framework.Cache.Models
{
    [MessagePackObject]
    public class JobDetailsCacheModel
    {
        [Key(0)]
        public string Key
        {
            get;
            set;
        }

        [Key(1)]
        public string InstanceId
        {
            get;
            set;
        }

        [Key(2)]
        public bool IsOnline
        {
            get;
            set;
        }

        [Key(3)]
        public long LastExecutionDateTime
        {
            get;
            set;
        }

        [Key(4)]
        public long NextExecutionDateTime
        {
            get;
            set;
        }

        [Key(5)]
        public string LastStatusMessage
        {
            get;
            set;
        }

        [Key(6)]
        public JobStatus Status
        {
            get;
            set;
        }

        [Key(7)]
        public long HeartBitDateTimeTicks
        {
            get;
            set;
        }
    }
}
