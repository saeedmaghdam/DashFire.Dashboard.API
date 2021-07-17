using System;
using DashFire.Dashboard.Framework.Constants;

namespace DashFire.Dashboard.Framework.Services
{
    public interface IEntityRecord
    {
        long Id
        {
            get;
            set;
        }

        Guid ViewId
        {
            get;
            set;
        }

        DateTime RecordInsertDateTime
        {
            get;
            set;
        }

        DateTime RecordUpdateDateTime
        {
            get;
            set;
        }

        RecordStatus RecordStatus
        {
            get;
            set;
        }
    }
}
