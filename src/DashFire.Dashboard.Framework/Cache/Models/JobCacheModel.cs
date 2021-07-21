using System.Collections.Generic;
using MessagePack;

namespace DashFire.Dashboard.Framework.Cache.Models
{
    [MessagePackObject]
    public class JobCacheModel
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
    }
}
