namespace DashFire.Dashboard.Framework.Services.Job
{
    public interface IJobParameterValue
    {
        string ParameterName
        {
            get;
            set;
        }

        string Value
        {
            get;
            set;
        }
    }
}
