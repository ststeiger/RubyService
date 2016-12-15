
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
            
            conf.WebRoot = "C:\\Redmine\\redmine-3.2.4";
            conf.LogFile_Directory = System.IO.Path.Combine(conf.WebRoot, "Log", "service.log");
            conf.DateTimeFormat = "dddd, dd.MM.yyyy HH:mm:ss";

            conf.OverwriteEnvironmentVariables["RAILS_ENV"] = "production";
            conf.AppendEnvironmentVariables["PATH"] = @"C:\Ruby21-x64\bin";
            
            conf.StartProgram = "puma"; // where puma ==> C:\Ruby21-x64\bin\puma.bat and not C:\Ruby21-x64\bin\puma; 
            conf.StartProgramArguments = "--env production --dir \"{WebRoot}\" - p 3000";


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

                return System.IO.Path.Combine(logFilePath, "service_" + System.DateTime.Now.ToString("yyyyMMdd") + ".log");
            }
        }


        static RubyService()
        {
            m_Configuration = GetConfiguration();
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



        private System.Diagnostics.Process m_serverProcess;
        private ProcessHelper.Job m_serverJob;

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
            string[] paths = enviromentPath.Split(';');


            foreach (string thisPath in paths)
            {
                string thisFile = System.IO.Path.Combine(thisPath, app);
                string[] executableExtensions = new string[] { ".exe", ".com", ".bat", ".sh", ".vbs", ".vbscript", ".vbe", ".js", ".rb", ".cmd", ".cpl", ".ws", ".wsf", ".msc", ".gadget" };

                foreach (string extension in executableExtensions)
                {
                    string fullFile = thisFile + extension;

                    try
                    {
                        if (System.IO.File.Exists(fullFile))
                            return fullFile;
                    }
                    catch (System.Exception ex)
                    {
                        Log("{0}:\r\n{1}",
                             System.DateTime.Now.ToString(m_Configuration.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture)
                            , "Error trying to check existence of file \"" + fullFile + "\""
                        );

                        Log("Exception details:");
                        Log(" - Exception type: {0}", ex.GetType().FullName);
                        Log(" - Exception Message:");
                        Log(ex.Message);
                        Log(" - Exception Stacktrace:");
                        Log(ex.StackTrace);
                    } // End Catch 

                } // Next extension 

            } // Next thisPath 


            foreach (string thisPath in paths)
            {
                string thisFile = System.IO.Path.Combine(thisPath, app);

                try
                {
                    if (System.IO.File.Exists(thisFile))
                        return thisFile;
                }
                catch (System.Exception ex)
                {
                    Log("{0}:\r\n{1}",
                         System.DateTime.Now.ToString(m_Configuration.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture)
                        , "Error trying to check existence of file \"" + thisFile + "\""
                    );

                    Log("Exception details:");
                    Log(" - Exception type: {0}", ex.GetType().FullName);
                    Log(" - Exception Message:");
                    Log(ex.Message);
                    Log(" - Exception Stacktrace:");
                    Log(ex.StackTrace);
                } // End Catch 

                
            } // Next thisPath 


            return app;
        } // End Function FindAppInPathDirectories 


        private System.DateTime m_lastLogActivity = new System.DateTime(1900, 1, 1, 0, 0, 0, System.DateTimeKind.Local);


        // Write output of process the service started.
        public void OnDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            if (e != null && !string.IsNullOrWhiteSpace(e.Data))
            {
                if (System.DateTime.Now.Subtract(this.m_lastLogActivity).TotalSeconds < 4)
                    Log(e.Data);
                else
                {
                    Log("{0}:\r\n{1}",
                         System.DateTime.Now.ToString(m_Configuration.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture)
                        ,e.Data
                    );

                    this.m_lastLogActivity = System.DateTime.Now;
                }

            }
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
        



        private static string AppendEnvVariable(string variableName, string value)
        {
            string enviromentPath = System.Environment.GetEnvironmentVariable(variableName);
            if (!enviromentPath.EndsWith(";"))
                enviromentPath += ";";

            enviromentPath += value;

            if (!enviromentPath.EndsWith(";"))
                enviromentPath += ";";

            return enviromentPath;
        } // End Function AppendEnvVariable 




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
                // this.m_serverProcess = System.Diagnostics.Process.Start(psi);
                this.m_serverProcess = new System.Diagnostics.Process();
                this.m_serverProcess.StartInfo.UseShellExecute = false;
                this.m_serverProcess.StartInfo.WorkingDirectory = m_Configuration.WebRoot;


                // this.m_serverProcess.StartInfo.EnvironmentVariables["PATH"] = System.Environment.GetEnvironmentVariable("PATH");
                System.Collections.IDictionary dict = System.Environment.GetEnvironmentVariables();
                foreach (object objKey in dict.Keys)
                {
                    string key = System.Convert.ToString(objKey);
                    string value = System.Convert.ToString(dict[objKey]);
                    this.m_serverProcess.StartInfo.EnvironmentVariables[key] = value;
                    // Log("Setting env[\"{0}\"] = \"{1}\"", key, value);
                } // Next objKey 

                foreach (System.Collections.Generic.KeyValuePair<string, string> kvp in m_Configuration.OverwriteEnvironmentVariables)
                {
                    this.m_serverProcess.StartInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
                }

                foreach (System.Collections.Generic.KeyValuePair<string, string> kvp in m_Configuration.AppendEnvironmentVariables)
                {
                    this.m_serverProcess.StartInfo.EnvironmentVariables[kvp.Key] = AppendEnvVariable(kvp.Key, kvp.Value);
                }


                // this.m_serverProcess.StartInfo.EnvironmentVariables["RAILS_ENV"] = "production";
                {
                    string thisVar = "RAILS_ENV";
                    if(this.m_serverProcess.StartInfo.EnvironmentVariables.ContainsKey(thisVar))
                        Log("Rails: env[\"{0}\"] = \"{1}\"", thisVar, this.m_serverProcess.StartInfo.EnvironmentVariables[thisVar]);
                    else
                        Log("WARNING: env[\"{0}\"] = \"{1}\"", thisVar, "NOT SET");
                }
                // Manipulate dictionary...
                // System.Collections.Specialized.StringDictionary dictionary = psi.EnvironmentVariables;


                {
                    string thisVar = "PATH";
                    if (this.m_serverProcess.StartInfo.EnvironmentVariables.ContainsKey(thisVar))
                        Log("Rails: env[\"{0}\"] = \"{1}\"", thisVar, this.m_serverProcess.StartInfo.EnvironmentVariables[thisVar]);
                    else
                        Log("WARNING: env[\"{0}\"] = \"{1}\"", thisVar, "NOT SET");
                }


                if (this.m_serverProcess.StartInfo.EnvironmentVariables.ContainsKey("PATH"))
                {
                    // We need this to find Ruby & Puma in PATH when starting the process
                    System.Environment.SetEnvironmentVariable("PATH", this.m_serverProcess.StartInfo.EnvironmentVariables["PATH"]);
                } // End if (this.m_serverProcess.StartInfo.EnvironmentVariables.ContainsKey("PATH"))


                // where rails
                // where puma
                // this.m_serverProcess.StartInfo.FileName = @"C:\Ruby21-x64\bin\ruby.exe";
                // this.m_serverProcess.StartInfo.FileName = @"C:\Ruby21-x64\bin\rails.bat";
                // this.m_serverProcess.StartInfo.FileName = @"C:\Ruby21-x64\bin\puma.bat"; // where puma ==> C:\Ruby21-x64\bin\puma; C:\Ruby21-x64\bin\puma.bat


                if (string.IsNullOrEmpty(m_Configuration.StartProgram))
                    throw new System.ArgumentException("StartProgram is NULL or string.empty.");


                if (m_Configuration.StartProgram.StartsWith(".") || System.IO.Path.IsPathRooted(m_Configuration.StartProgram))
                    this.m_serverProcess.StartInfo.FileName = m_Configuration.StartProgram;
                else
                {
                    this.m_serverProcess.StartInfo.FileName = FindAppInPathDirectories(m_Configuration.StartProgram);
                    Log("Searched file in PATH and found: \"{0}\"", this.m_serverProcess.StartInfo.FileName);
                }

                // this.m_serverProcess.StartInfo.Arguments = @"puma --env production --dir " + REDMINE_DIR + " -p 3000";
                Log("Process-StartInfo.FileName: \"{0}\"", this.m_serverProcess.StartInfo.FileName);



                if (m_Configuration.WebRoot != null)
                    this.m_serverProcess.StartInfo.Arguments = m_Configuration.StartProgramArguments.Replace("{WebRoot}", m_Configuration.WebRoot);
                else
                    this.m_serverProcess.StartInfo.Arguments = m_Configuration.StartProgramArguments;

                // @"--env production --dir " + m_Configuration.Redmine_Directory + " -p 3000";
                Log("Process-StartInfo.Arguments: \"{0}\"", this.m_serverProcess.StartInfo.Arguments);


                this.m_serverProcess.StartInfo.CreateNoWindow = true;
                this.m_serverProcess.StartInfo.RedirectStandardError = true;
                this.m_serverProcess.StartInfo.RedirectStandardOutput = true;
                this.m_serverProcess.EnableRaisingEvents = true;
                this.m_serverProcess.ErrorDataReceived += OnDataReceived;
                this.m_serverProcess.OutputDataReceived += OnDataReceived;
                this.m_serverProcess.Start();
                this.m_serverProcess.BeginOutputReadLine();
                this.m_serverProcess.BeginErrorReadLine();
                this.m_serverProcess.Exited += OnProcessExited;


                try
                {
                    this.m_serverJob = new ProcessHelper.Job();
                    this.m_serverJob.AddProcess(this.m_serverProcess.Handle);
                }
                catch (System.Exception ex)
                {
                    Log("Exception while trying to create a job for process {0}.", this.m_serverProcess.Id);
                    Log("This might occur on Windows 7 or Windows Vista.");
                    Log("Service will attempts managed process traversal on service-stop instead.");
                    Log("Exception details:");
                    Log(" - Exception type: {0}", ex.GetType().FullName);
                    Log(" - Exception Message:");
                    Log(ex.Message);
                    Log(" - Exception Stacktrace:");
                    Log(ex.StackTrace);
                }

                Log("Initialized service with PID {0}", this.m_serverProcess.Id);
            } // End Try
            catch (System.Exception ex)
            {
                Log("{0}: There was an error starting the service....", System.DateTime.Now.ToString(m_Configuration.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture));
                Log("Exception details:");
                Log(" - Exception type: {0}", ex.GetType().FullName);
                Log(" - Exception Message:");
                Log(ex.Message);
                Log(" - Exception Stacktrace:");
                Log(ex.StackTrace);
                this.Stop();
            } // End Catch 

        } // End Sub OnStart


        



        /// <summary>
        /// Kill a process, and all of its children, grandchildren, etc.
        /// </summary>
        /// <param name="pid">Process ID.</param>
        /// 
        private static void KillProcessAndChildren(int pid)
        {
            KillProcessAndChildren(pid, false);
        } // End Sub KillProcessAndChildren 


        private static void KillProcessAndChildren(int pid, bool isChild)
        {
            try
            {

                using (System.Management.ManagementObjectSearcher searcher =
                    new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_Process WHERE ParentProcessID = "
                    + pid.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    )
                )
                {
                    using (System.Management.ManagementObjectCollection childProcesses = searcher.Get())
                    {
                        foreach (System.Management.ManagementObject thisProcessInfo in childProcesses)
                        {
                            KillProcessAndChildren(System.Convert.ToInt32(thisProcessInfo["ProcessID"]), true);
                        } // Next thisProcessInfo 

                    } // End Using childProcesses 

                } // End Using searcher 

            }
            catch (System.Exception ex)
            {
                Log("Exception while accessing Win32_Process WHERE ParentProcessID = {0}.", pid);
                Log("Exception details:");
                Log(" - Exception type: {0}", ex.GetType().FullName);
                Log(" - Exception Message:");
                Log(ex.Message);
                Log(" - Exception Stacktrace:");
                Log(ex.StackTrace);
            }


            try
            {
                using (System.Diagnostics.Process proc = System.Diagnostics.Process.GetProcessById(pid))
                {
                    if (proc != null)
                    {
                        Log("Killing {0} process with PID {1}.", isChild ? "child" : "root", pid);   
                        proc.Kill();
                        // Why does the below construct cause an infinite loop ?
                        //while (!proc.HasExited)
                        //{
                        //    System.Windows.Forms.Application.DoEvents();
                        //    System.Threading.Thread.Sleep(50);
                        //    System.Windows.Forms.Application.DoEvents();
                        //} // Whend 
                    }
                    else
                        Log("Wanted to kill process {0} but was unable to get a hold of it.", pid);
                } // End Using proc 
            }
            catch (System.Exception ex)
            {
                Log("Exception while killing process {0}. Most likely, the process already exited.", pid);
                Log("Exception details:");
                Log(" - Exception type: {0}", ex.GetType().FullName);
                Log(" - Exception Message:");
                Log(ex.Message);
                Log(" - Exception Stacktrace:");
                Log(ex.StackTrace);
            }

        } // End Sub KillProcessAndChildren 



        protected override void OnStop()
        {
            try
            {
                Log("Stopping service {0}", System.DateTime.Now.ToString(m_Configuration.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture));
                // system "taskkill /PID #{@server_pid} /T /F"
                // System.Diagnostics.Process proc = System.Diagnostics.Process.GetProcessById(server_pid);

                if (this.m_serverProcess != null)
                {
                    if (!this.m_serverProcess.HasExited)
                    {
                        // https://stackoverflow.com/questions/13952635/what-are-the-differences-between-kill-process-and-close-process
                        this.m_processShuttingDown = true;
                        // this.m_serverProcess.Close(); // Process.Close() isn't meant to abort the process - it's just meant to release your "local" view on the process, and associated resources.
                        // this.m_serverProcess.CloseMainWindow(); // Closes a process that has a user interface by sending a close message to its main window.


                        // http://stackoverflow.com/questions/3342941/kill-child-process-when-parent-process-is-killed
                        // Application.Quit() and Process.Kill() are possible solutions, but have proven to be unreliable. 
                        // When your main application dies, you are still left with child processes running. 
                        // What we really want is for the child processes to die as soon as the main process dies.
                        // this.m_serverProcess.Kill(); // Kill forces a termination of the process, while CloseMainWindow only requests a termination. 

                        if (this.m_serverJob != null)
                        {
                            this.m_serverJob.Close();
                            this.m_serverJob.Dispose();
                            this.m_serverJob = null;
                        } // End if (this.m_serverJob != null) 
                        else
                        {
                            Log("Detected job creation failure...");
                            Log("Attempting fully managed child-process traversal for process {0}.", this.m_serverProcess.Id);
                            KillProcessAndChildren(this.m_serverProcess.Id);
                            Log("Finished process-traversal for process {0}.", this.m_serverProcess.Id);
                        }

                        // Is this potentially dangerous ? 
                        while (!this.m_serverProcess.HasExited)
                        {
                            System.Windows.Forms.Application.DoEvents();
                            System.Threading.Thread.Sleep(50);
                            System.Windows.Forms.Application.DoEvents();
                        } // Whend 

                        this.m_processShuttingDown = false;
                    }
                    else
                        Log("BadBad - WebServer has already exited...");
                } // End if (this.m_serverProcess != null) 

                // Process.waitall
                // File.open(LOG_FILE, 'a'){ | f | f.puts "Service stopped #{Time.now}" }
                Log("Service stopped {0}", System.DateTime.Now.ToString(m_Configuration.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture));
            } // End Try 
            catch (System.Exception ex)
            {
                Log(ex.ToString());
            } // End Catch 

        } // End Sub OnStop 


    } // End Class RubyService : ServiceBase 


} // End Namespace StartRuby 
