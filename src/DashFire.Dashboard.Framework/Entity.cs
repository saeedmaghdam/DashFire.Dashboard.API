using System;
using System.ComponentModel.DataAnnotations.Schema;
using DashFire.Dashboard.Framework.Constants;

namespace DashFire.Dashboard.Framework.Domains
{
    public class Entity
    {
        [Column("id")]
        public long Id
        {
            get;
            set;
        }

        [Column("view_id")]
        public Guid ViewId
        {
            get;
            set;
        }

        [Column("record_insert_date_time")]
        public DateTime RecordInsertDateTime
        {
            get;
            set;
        }

        [Column("record_update_date_time")]
        public DateTime RecordUpdateDateTime
        {
            get;
            set;
        }

        [Column("record_status")]
        public RecordStatus RecordStatus
        {
            get;
            set;
        }
    }
}
