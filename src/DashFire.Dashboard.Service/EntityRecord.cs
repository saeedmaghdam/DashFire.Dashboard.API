using System;
using DashFire.Dashboard.Framework.Constants;
using DashFire.Dashboard.Framework.Services;

namespace DashFire.Dashboard.Service
{
    public class EntityRecord : IEntityRecord
    {
        public long Id
        {
            get;
            set;
        }

        public Guid ViewId
        {
            get;
            set;
        }

        public DateTime RecordInsertDateTime
        {
            get;
            set;
        }

        public DateTime RecordUpdateDateTime
        {
            get;
            set;
        }

        public RecordStatus RecordStatus
        {
            get;
            set;
        }
    }
}
