using MessagePack;

namespace DashFire.Dashboard.Framework.Cache.Models
{
    [MessagePackObject]
    public class JobParameterCacheModel
    {
        [Key(0)]
        public string ParameterName
        {
            get;
            set;
        }

        [Key(1)]
        public string TypeFullName
        {
            get;
            set;
        }

        [Key(2)]
        public string DisplayName
        {
            get;
            set;
        }

        [Key(3)]
        public string Description
        {
            get;
            set;
        }
    }
}
