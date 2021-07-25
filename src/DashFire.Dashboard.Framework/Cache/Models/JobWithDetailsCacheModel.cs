using System.Collections.Generic;
using DashFire.Dashboard.Framework.Constants;
using MessagePack;

namespace DashFire.Dashboard.Framework.Cache.Models
{
    [MessagePackObject]
    public class JobWithDetailsCacheModel
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
        public string SystemName
        {
            get;
            set;
        }

        [Key(3)]
        public string DisplayName
        {
            get;
            set;
        }

        [Key(4)]
        public string Description
        {
            get;
            set;
        }

        [Key(5)]
        public bool RegistrationRequired
        {
            get;
            set;
        }

        [Key(6)]
        public List<JobParameterCacheModel> Parameters
        {
            get;
            set;
        }

        [Key(7)]
        public JobExecutionMode JobExecutionMode
        {
            get;
            set;
        }

        [Key(8)]
        public string OriginalInstanceId
        {
            get;
            set;
        }

        [Key(9)]
        public bool IsOnline
        {
            get;
            set;
        }

        [Key(10)]
        public long LastExecutionDateTime
        {
            get;
            set;
        }

        [Key(11)]
        public long NextExecutionDateTime
        {
            get;
            set;
        }

        [Key(12)]
        public string LastStatusMessage
        {
            get;
            set;
        }

        [Key(13)]
        public JobStatus Status
        {
            get;
            set;
        }

        [Key(14)]
        public long HeartBitDateTimeTicks
        {
            get;
            set;
        }
    }
}
