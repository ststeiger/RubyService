
using System.Linq;
using System.ServiceProcess;


namespace StartRuby
{


    // https://stackoverflow.com/questions/37346383/hosting-asp-net-core-as-windows-service
    public partial class RubyService : ServiceBase
    {

        protected static RedmineConfiguration m_Configuration;


        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = typeof(RubyService).Assembly.CodeBase;

                // System.UriBuilder ub = new System.UriBuilder(codeBase);
                // string path = System.Uri.UnescapeDataString(ub.Path);

                System.Uri myuri = new System.Uri(codeBase, System.UriKind.RelativeOrAbsolute);
                string path = myuri.LocalPath;

                return System.IO.Path.GetDirectoryName(path);
            }
        }


        public static string ConfigFile
        {
            get
            {
                string configFile = System.IO.Path.GetFileNameWithoutExtension(typeof(RubyService).Assembly.Location) + ".config.json";
                return System.IO.Path.Combine(AssemblyDirectory, configFile);
            }
        }




        public static void CreateDefaultConfigFileIfNotExists()
        {
            if (System.IO.File.Exists(ConfigFile))
                return;

            RedmineConfiguration conf = new RedmineConfiguration();

            conf.Redmine_Directory = "C:\\Redmine\\redmine-3.2.4";
            conf.LogFile_Directory = System.IO.Path.Combine(conf.Redmine_Directory, "Log", "service.log");
            conf.DateTimeFormat = "dddd, dd.MM.yyyy HH:mm:ss";

            conf.OverwriteEnvironmentVariables["RAILS_ENV"] = "production";
            conf.AppendEnvironmentVariables["PATH"] = @"C:\Ruby21-x64\bin";
            
            conf.StartProgram = "puma.bat"; // where puma ==> C:\Ruby21-x64\bin\puma; C:\Ruby21-x64\bin\puma.bat
            conf.StartProgramArguments = "--env production --dir \"" + conf.Redmine_Directory + "\" - p 3000";


            System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
            string json = ser.Serialize(conf);
            // json = JSON_PrettyPrinter.Process(json);
            json = JSON_PrettyPrinter.FormatOutput(json);
            System.IO.File.WriteAllText(ConfigFile, json);
        }


        private static RedmineConfiguration GetConfiguration()
        {
            try
            {
                System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();

                CreateDefaultConfigFileIfNotExists();

                if (!System.IO.File.Exists(ConfigFile))
                    return null;

                string json = System.IO.File.ReadAllText(ConfigFile, System.Text.Encoding.UTF8);
                return ser.Deserialize<RedmineConfiguration>(json);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                System.Console.WriteLine(ex.StackTrace);
            }

            return null;
        }


        public static string LogFile
        {
            get
            {
                string logFilePath = m_Configuration.LogFile_Directory;
                if (!System.IO.Directory.Exists(logFilePath))
                    System.IO.Directory.CreateDirectory(logFilePath);

                return System.IO.Path.Combine(logFilePath, "service_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".log");
            }
        }


        static RubyService()
        {
            m_Configuration = GetConfiguration();
        }
        

        public class RedmineConfiguration
        {
            public string LogFile_Directory;
            public string Redmine_Directory;
            public string DateTimeFormat;

            public System.Collections.Generic.Dictionary<string, string> OverwriteEnvironmentVariables = new System.Collections.Generic.Dictionary<string, string>();
            public System.Collections.Generic.Dictionary<string, string> AppendEnvironmentVariables = new System.Collections.Generic.Dictionary<string, string>();


            public string StartProgram;
            public string StartProgramArguments;

            // webServer.StartInfo.FileName = @"C:\Ruby21-x64\bin\puma.bat"; // where puma ==> C:\Ruby21-x64\bin\puma; C:\Ruby21-x64\bin\puma.bat
            // webServer.StartInfo.Arguments = @"puma --env production --dir " + REDMINE_DIR + " -p 3000";
            // webServer.StartInfo.Arguments = @"--env production --dir " + REDMINE_DIR + " -p 3000";

        }


        public static void Log(string format, params object[] objects)
        {
            string content = null;
            if(format != null)
                content = string.Format(format, objects);

            System.Console.WriteLine(content);
            content += System.Environment.NewLine;
            
            System.IO.File.AppendAllText(LogFile, content, System.Text.Encoding.UTF8);
        } // End Sub Log


        public static void OnUnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            // File.open(LOG_FILE, 'a+'){ | f | f.puts " ***Daemon failure #{Time.now} exception=#{e.inspect}\n#{e.backtrace.join($/)}" }
            Log(" ***Daemon failure {0}", System.DateTime.Now.ToString(m_Configuration.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture));
            Log(e.ExceptionObject.ToString());
        } // End Sub OnUnhandledException 



        public System.Diagnostics.Process webServer;

        public RubyService()
        {
            try
            {
                if (System.IO.File.Exists(LogFile))
                    System.IO.File.Delete(LogFile);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }

            System.AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            InitializeComponent();
        } // End Constructor 


        // FindAppInPathDirectories("ruby.exe");
        public string FindAppInPathDirectories(string app)
        {
            string enviromentPath = System.Environment.GetEnvironmentVariable("PATH");

            System.Console.WriteLine(enviromentPath);
            string[] paths = enviromentPath.Split(';');
            string exePath = paths.Select(thisPath => System.IO.Path.Combine(thisPath, app))
                               .Where(thisFileName => System.IO.File.Exists(thisFileName))
                               .FirstOrDefault();

            return exePath;
            // System.Console.WriteLine(exePath);
            // if (string.IsNullOrWhiteSpace(exePath) == false) { Process.Start(exePath); }
        }


        // Write output of process the service started.
        public void OnDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if(e != null)
                Log(e.Data);
        } // End Sub OnDataReceived 


        bool m_processShuttingDown = false;

        public void OnProcessExited(object sender, System.EventArgs e)
        {
            if (!m_processShuttingDown)
            {
                Log("Process unexpectedly exited per {0}", System.DateTime.Now.ToString(m_Configuration.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture));
                // https://msdn.microsoft.com/en-us/library/windows/desktop/ms685996(v=vs.85).aspx
                this.ExitCode = 1; // FAILURE
                this.Stop();
                return;
            } // End if (!m_processShuttingDown) 

            Log("Process exited by request per {0}", System.DateTime.Now.ToString(m_Configuration.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture));
        } // End Sub OnProcessExited 




        public static string AppendEnvVariable(string variableName, string value)
        {
            string enviromentPath = System.Environment.GetEnvironmentVariable(variableName);
            if (!enviromentPath.EndsWith(";"))
                enviromentPath += ";";

            enviromentPath += value;

            if (!enviromentPath.EndsWith(";"))
                enviromentPath += ";";

            return enviromentPath;
        }
        



        protected override void OnStart(string[] args)
        {
            try
            {
                // bundle exec rails server webrick -e production
                // thin start -e production - p 3000
                // puma --env production --dir C:\Redmine\redmine-3.2.4 -p 3000
                
                

                // File.open(LOG_FILE, 'a'){ | f | f.puts "Initializing service #{Time.now}" }
                // @server_pid = Process.spawn 'puma --env production --dir C:\\Redmine\\redmine-3.2.4 -p 3000', :chdir => REDMINE_DIR, :err => [LOG_FILE, 'a']


                Log("Initializing service {0}", System.DateTime.Now.ToString(m_Configuration.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture));
                // webServer = System.Diagnostics.Process.Start(psi);
                webServer = new System.Diagnostics.Process();
                webServer.StartInfo.UseShellExecute = false;
                webServer.StartInfo.WorkingDirectory = m_Configuration.Redmine_Directory;
                

                // webServer.StartInfo.EnvironmentVariables["PATH"] = System.Environment.GetEnvironmentVariable("PATH");
                System.Collections.IDictionary dict = System.Environment.GetEnvironmentVariables();
                foreach (object objKey in dict.Keys)
                {
                    string key = System.Convert.ToString(objKey);
                    string value = System.Convert.ToString(dict[objKey]);
                    webServer.StartInfo.EnvironmentVariables[key] = value;
                    // Log("Setting env[\"{0}\"] = \"{1}\"", key, value);
                } // Next objKey 

                foreach (System.Collections.Generic.KeyValuePair<string, string> kvp in m_Configuration.OverwriteEnvironmentVariables)
                {
                    webServer.StartInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
                }

                foreach (System.Collections.Generic.KeyValuePair<string, string> kvp in m_Configuration.AppendEnvironmentVariables)
                {
                    webServer.StartInfo.EnvironmentVariables[kvp.Key] = AppendEnvVariable(kvp.Key, kvp.Value);
                }


                // webServer.StartInfo.EnvironmentVariables["RAILS_ENV"] = "production";


                if(webServer.StartInfo.EnvironmentVariables.ContainsKey("RAILS_ENV"))
                    Log("Rails: env[\"{0}\"] = \"{1}\"", "RAILS_ENV", webServer.StartInfo.EnvironmentVariables["RAILS_ENV"]);
                else
                    Log("WARNING: Rails: env[\"{0}\"] = \"{1}\"", "RAILS_ENV", "NOT SET");

                // Manipulate dictionary...
                // System.Collections.Specialized.StringDictionary dictionary = psi.EnvironmentVariables;

                // where rails
                // where puma
                //webServer.StartInfo.FileName = @"C:\Ruby21-x64\bin\ruby.exe";
                //webServer.StartInfo.FileName = @"C:\Ruby21-x64\bin\rails.bat";
                // webServer.StartInfo.FileName = @"C:\Ruby21-x64\bin\puma.bat"; // where puma ==> C:\Ruby21-x64\bin\puma; C:\Ruby21-x64\bin\puma.bat
                webServer.StartInfo.FileName = m_Configuration.StartProgram;
                //webServer.StartInfo.Arguments = @"puma --env production --dir " + REDMINE_DIR + " -p 3000";

                // @"--env production --dir " + m_Configuration.Redmine_Directory + " -p 3000";
                webServer.StartInfo.Arguments = m_Configuration.StartProgramArguments.Replace("{Redmine_Directory}", m_Configuration.Redmine_Directory);


                webServer.StartInfo.CreateNoWindow = true;
                webServer.StartInfo.RedirectStandardError = true;
                webServer.StartInfo.RedirectStandardOutput = true;
                webServer.EnableRaisingEvents = true;
                webServer.ErrorDataReceived += OnDataReceived;
                webServer.OutputDataReceived += OnDataReceived;
                webServer.Start();
                webServer.BeginOutputReadLine();
                webServer.BeginErrorReadLine();
                webServer.Exited += OnProcessExited;


                Log("Initialized service with PID {0}", webServer.Id);
            } // End Try
            catch (System.Exception ex)
            {
                Log(ex.ToString());
            } // End Catch 

        } // End Sub OnStart


        protected override void OnStop()
        {
            try
            {
                Log("Stopping service {0}", System.DateTime.Now.ToString(m_Configuration.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture));
                // system "taskkill /PID #{@server_pid} /T /F"
                //System.Diagnostics.Process proc = System.Diagnostics.Process.GetProcessById(server_pid);

                if (webServer != null)
                {
                    if (!webServer.HasExited)
                    {
                        // https://stackoverflow.com/questions/13952635/what-are-the-differences-between-kill-process-and-close-process
                        this.m_processShuttingDown = true;
                        // webServer.Close(); // Process.Close() isn't meant to abort the process - it's just meant to release your "local" view on the process, and associated resources.
                        // webServer.CloseMainWindow(); // Closes a process that has a user interface by sending a close message to its main window.
                        webServer.Kill(); // Kill forces a termination of the process, while CloseMainWindow only requests a termination. 
                        
                        while (!webServer.HasExited)
                        {
                            System.Windows.Forms.Application.DoEvents();
                            System.Threading.Thread.Sleep(50);
                            System.Windows.Forms.Application.DoEvents();
                        }

                        this.m_processShuttingDown = false;
                    }
                    else
                        Log("BadBad - WebServer has already exited...");
                } // End if (webServer != null) 

                // Process.waitall
                // File.open(LOG_FILE, 'a'){ | f | f.puts "Service stopped #{Time.now}" }
                Log("Service stopped {0}", System.DateTime.Now.ToString(m_Configuration.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture));
            } // End Try 
            catch (System.Exception ex)
            {
                Log(ex.ToString());
            } // End Catch 

        } // End Sub OnStop 


    } // End Class 


} // End Namespace 
