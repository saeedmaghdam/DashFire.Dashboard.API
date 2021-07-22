using System.ComponentModel.DataAnnotations.Schema;
using DashFire.Dashboard.Framework.Domains;

namespace DashFire.Dashboard.Domain
{
    [Table("log", Schema = "job")]
    public class Log : Entity
    {
        [ForeignKey("JobId")]
        public Job Job
        {
            get;
            set;
        }

        [Column("job_id")]
        public long JobId
        {
            get;
            set;
        }

        [Column("message")]
        public string Message
        {
            get;
            set;
        }

        [Column("create_date")]
        public long CreateDate
        {
            get;
            set;
        }
    }
}
