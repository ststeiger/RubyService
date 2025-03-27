
namespace StartRuby
{

    using Microsoft.Extensions.Configuration; // for .Get<T>
    using Microsoft.Extensions.DependencyInjection; // for Configure, AddHostedService
    

    public class Program
    {
        private static TrivialLogger s_logger;

        private static string s_ProgramDirectory;
        private static string s_CurrentDirectory;
        private static string s_BaseDirectory;
        private static string s_ExecutablePath;
        private static string s_ExecutableDirectory;
        private static string s_Executable;
        private static string s_ContentRootDirectory;

        static Program()
        {
            try
            {

                System.AppDomain.CurrentDomain.UnhandledException += ExceptionHelper.OnUnhandledException;
                System.Threading.Tasks.TaskScheduler.UnobservedTaskException += ExceptionHelper.OnUnobservedTaskException!;

                s_ProgramDirectory = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
                s_CurrentDirectory = System.IO.Directory.GetCurrentDirectory();
                s_BaseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
                s_ExecutablePath = System.Diagnostics.Process.GetCurrentProcess()!.MainModule!.FileName;
                s_ExecutableDirectory = System.IO.Path.GetDirectoryName(s_ExecutablePath)!;
                s_Executable = System.IO.Path.GetFileNameWithoutExtension(s_ExecutablePath);

                string? logFilePath = null;
                string fileName = @"ServiceStartupLog.htm";

                if ("dotnet".Equals(s_Executable, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    s_ContentRootDirectory = s_ProgramDirectory;
                    logFilePath = System.IO.Path.Combine(s_ProgramDirectory, fileName);
                }
                else
                {
                    s_ContentRootDirectory = s_ExecutableDirectory;
                    logFilePath = System.IO.Path.Combine(s_ExecutableDirectory, fileName);
                }

                if (System.IO.File.Exists(logFilePath))
                    System.IO.File.Delete(logFilePath);

                s_logger = new HtmlLogger(logFilePath);



                s_logger.Log(LogLevel_t.Information, "Program Directory: {0}", s_ProgramDirectory);
                s_logger.Log(LogLevel_t.Information, "Current Directory: {0}", s_CurrentDirectory);
                s_logger.Log(LogLevel_t.Information, "Base Directory: {0}", s_BaseDirectory);
                s_logger.Log(LogLevel_t.Information, "Logfile Directory: {0}", s_ContentRootDirectory);
                s_logger.Log(LogLevel_t.Information, "Executable Path: {0}", s_ExecutablePath);
                s_logger.Log(LogLevel_t.Information, "Executable Directory: {0}", s_ExecutableDirectory);
                s_logger.Log(LogLevel_t.Information, "Executable: {0}", s_Executable);

                CertificateCallback.TrustAll();
            } // End Try 
            catch (System.Exception ex)
            {
                ExceptionHelper.DisplayError(ex);
                System.Environment.Exit(ex.HResult);
            } // End Catch 

        } // End Static Constructor 


        public static async System.Threading.Tasks.Task<int> Main(string[] args)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);


            string logDir = System.IO.Path.Combine(s_ContentRootDirectory, "Log");

            if (!System.IO.Directory.Exists(logDir))
                System.IO.Directory.CreateDirectory(logDir);


            Microsoft.Extensions.Hosting.HostApplicationBuilder builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);

            // Configure RedmineConfiguration from appsettings.json
            builder.Services.Configure<RedmineConfiguration>(builder.Configuration.GetSection("RedmineConfiguration"));

            builder.Services.AddHostedService<Worker>();

            Microsoft.Extensions.Hosting.IHost host = builder.Build();

            // Access RedmineConfiguration directly using IConfiguration
            RedmineConfiguration? redmineConfig = builder.Configuration.GetSection("RedmineConfiguration").Get<RedmineConfiguration>();
            // System.Console.WriteLine(redmineConfig);



            await Microsoft.Extensions.Hosting.HostingAbstractionsHostExtensions.RunAsync(host);

            return 0;
        } // End Task Main 


    } // End Class Program 


} // End Namespace 
