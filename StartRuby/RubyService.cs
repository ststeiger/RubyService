
using System.Linq;
using System.ServiceProcess;


namespace StartRuby
{

    // https://stackoverflow.com/questions/37346383/hosting-asp-net-core-as-windows-service
    public partial class RubyService : ServiceBase
    {
        

        static string REDMINE_DIR = "C:\\Redmine\\redmine-3.2.4";
        static string LOG_FILE = REDMINE_DIR + "\\log\\service.log";


        public static void Log(string format, params object[] objects)
        {
            string content = null;
            if(format != null)
                content = string.Format(format, objects);
            System.Console.WriteLine(content);
            content += System.Environment.NewLine;
            System.IO.File.AppendAllText(LOG_FILE, content, System.Text.Encoding.UTF8);
        } // End Sub Log


        public static void OnUnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            // File.open(LOG_FILE, 'a+'){ | f | f.puts " ***Daemon failure #{Time.now} exception=#{e.inspect}\n#{e.backtrace.join($/)}" }
            Log(" ***Daemon failure {0}", System.DateTime.Now.ToString("dddd, dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
            Log(e.ExceptionObject.ToString());
        } // End Sub OnUnhandledException 



        public System.Diagnostics.Process webServer;

        public RubyService()
        {
            try
            {
                if (System.IO.File.Exists(LOG_FILE))
                    System.IO.File.Delete(LOG_FILE);
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
                Log("Process unexpectedly exited per {0}", System.DateTime.Now.ToString("dddd, dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
                // https://msdn.microsoft.com/en-us/library/windows/desktop/ms685996(v=vs.85).aspx
                this.ExitCode = 1; // FAILURE
                this.Stop();
                return;
            } // End if (!m_processShuttingDown) 

            Log("Process exited by request per {0}", System.DateTime.Now.ToString("dddd, dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
        } // End Sub OnProcessExited 


        protected override void OnStart(string[] args)
        {
            try
            {
                // bundle exec rails server webrick -e production
                // thin start -e production - p 3000
                // puma --env production --dir C:\Redmine\redmine-3.2.4 -p 3000
                
                

                // File.open(LOG_FILE, 'a'){ | f | f.puts "Initializing service #{Time.now}" }
                // @server_pid = Process.spawn 'puma --env production --dir C:\\Redmine\\redmine-3.2.4 -p 3000', :chdir => REDMINE_DIR, :err => [LOG_FILE, 'a']


                Log("Initializing service {0}", System.DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
                // webServer = System.Diagnostics.Process.Start(psi);
                webServer = new System.Diagnostics.Process();
                webServer.StartInfo.UseShellExecute = false;
                webServer.StartInfo.WorkingDirectory = REDMINE_DIR;
                

                // webServer.StartInfo.EnvironmentVariables["PATH"] = System.Environment.GetEnvironmentVariable("PATH");
                System.Collections.IDictionary dict = System.Environment.GetEnvironmentVariables();
                foreach (object objKey in dict.Keys)
                {
                    string key = System.Convert.ToString(objKey);
                    string value = System.Convert.ToString(dict[objKey]);
                    webServer.StartInfo.EnvironmentVariables[key] = value;
                    // Log("Setting env[\"{0}\"] = \"{1}\"", key, value);
                } // Next objKey 

                webServer.StartInfo.EnvironmentVariables["RAILS_ENV"] = "production";
                Log("Rails: env[\"{0}\"] = \"{1}\"", "RAILS_ENV", webServer.StartInfo.EnvironmentVariables["RAILS_ENV"]);

                // Manipulate dictionary...
                // System.Collections.Specialized.StringDictionary dictionary = psi.EnvironmentVariables;

                // where rails
                // where puma
                //webServer.StartInfo.FileName = @"C:\Ruby21-x64\bin\ruby.exe";
                //webServer.StartInfo.FileName = @"C:\Ruby21-x64\bin\rails.bat";
                webServer.StartInfo.FileName = @"C:\Ruby21-x64\bin\puma.bat"; // where puma ==> C:\Ruby21-x64\bin\puma; C:\Ruby21-x64\bin\puma.bat
                //webServer.StartInfo.Arguments = @"puma --env production --dir " + REDMINE_DIR + " -p 3000";
                webServer.StartInfo.Arguments = @"--env production --dir " + REDMINE_DIR + " -p 3000";



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
                Log("Stopping service {0}", System.DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
                // system "taskkill /PID #{@server_pid} /T /F"
                //System.Diagnostics.Process proc = System.Diagnostics.Process.GetProcessById(server_pid);

                if (webServer != null)
                {
                    if (!webServer.HasExited)
                        webServer.Kill();
                    else
                        Log("BadBad - WebServer has already exited...");
                } // End if (webServer != null) 

                // Process.waitall
                // File.open(LOG_FILE, 'a'){ | f | f.puts "Service stopped #{Time.now}" }
                Log("Service stopped {0}", System.DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
            } // End Try 
            catch (System.Exception ex)
            {
                Log(ex.ToString());
            } // End Catch 

        } // End Sub OnStop 


    } // End Class 


} // End Namespace 
