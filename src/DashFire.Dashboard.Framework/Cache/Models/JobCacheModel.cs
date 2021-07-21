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
    }
}
