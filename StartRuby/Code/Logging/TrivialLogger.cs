
namespace StartRuby
{


    [System.Flags]
    public enum LogLevel_t
    {
        None = 0, // No logging 
        Configuration = 1, // incorrect configuration 
        Crash = 2, // Application crash, such as stackoverflow 
        Error = 4, // Error for one user, such as SQL syntax error 
        Debug = 8, // Only during development
        Warning = 16, // abnormal or unexpected circumstances
        Information = 32, // display some variables
        Trace = 64, // track the general flow of the application

        Everything = 127, // All
    }


    public abstract class TrivialLogger
    {

        protected readonly object lockObj;

        public LogLevel_t LogLevel;

        public const string DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff";


        protected string Time
        {
            get
            {
                return System.DateTime.UtcNow.ToString(DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
            }
        }


        protected TrivialLogger()
        {
            this.lockObj = new object();
        }


        public abstract void Log(LogLevel_t logLevel, string message, System.Exception Exception);


        public virtual void Log(LogLevel_t logLevel, System.Exception Exception)
        {
            this.Log(logLevel, null, Exception);
        }


        public virtual void Log(System.Exception Exception)
        {
            this.Log(LogLevel_t.Error, null, Exception);
        }


        public virtual void Log(LogLevel_t logLevel, string message, params object[] args)
        {
            string msg = string.Format(message, args);

            this.Log(logLevel, msg, (System.Exception)null);
        }


        public virtual void Log(string message, params object[] args)
        {
            this.Log(LogLevel_t.Error, message, args);
        }


    } // End Class TrivialLogger 


}
