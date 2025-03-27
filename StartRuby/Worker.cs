
namespace StartRuby
{

    using Microsoft.Extensions.Logging;


    public class Worker
        : Microsoft.Extensions.Hosting.BackgroundService
    {


        private readonly Microsoft.Extensions.Logging.ILogger<Worker> _logger;
        private readonly RedmineConfiguration _configuration;
        private System.Diagnostics.Process? _serverProcess;
        private readonly System.IO.FileSystemWatcher _configWatcher;
        private readonly string _configPath;


        public Worker(Microsoft.Extensions.Logging.ILogger<Worker> logger, Microsoft.Extensions.Options.IOptions<RedmineConfiguration> configuration)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _configPath = System.IO.Path.Combine(_configuration.WebRoot, "config");

            _configWatcher = new System.IO.FileSystemWatcher(_configPath)
            {
                NotifyFilter = System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.FileName,
                Filter = "*.*",
                IncludeSubdirectories = true
            };

            _configWatcher.Changed += OnConfigChanged;
            _configWatcher.Created += OnConfigChanged;
            _configWatcher.EnableRaisingEvents = true;
        }

        private void OnConfigChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            _logger.LogInformation("Config file changed: {Path}", e.FullPath);
            RestartRubyProcess();
        }

        private void RestartRubyProcess()
        {
            try
            {
                StopRubyProcess();
                StartRubyProcess();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error restarting Ruby process");
            }
        }

        private void StartRubyProcess()
        {
            if (_serverProcess != null && !_serverProcess.HasExited)
            {
                _logger.LogWarning("Ruby process is already running");
                return;
            }

            _serverProcess = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    UseShellExecute = false,
                    WorkingDirectory = _configuration.WebRoot,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = FindAppInPathDirectories(_configuration.StartProgram),
                    Arguments = _configuration.StartProgramArguments.Replace("{WebRoot}", _configuration.WebRoot)
                }
            };

            // Copy environment variables
            foreach (System.Collections.DictionaryEntry env in System.Environment.GetEnvironmentVariables())
            {
                if (env.Key is string key && env.Value is string value)
                {
                    _serverProcess.StartInfo.EnvironmentVariables[key] = value;
                }
            }

            // Override environment variables
            foreach (System.Collections.Generic.KeyValuePair<string, string> kvp in _configuration.OverwriteEnvironmentVariables)
            {
                _serverProcess.StartInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
            }

            // Append environment variables
            foreach (System.Collections.Generic.KeyValuePair<string, string> kvp in _configuration.AppendEnvironmentVariables)
            {
                string currentValue = _serverProcess.StartInfo.EnvironmentVariables[kvp.Key] ?? string.Empty;
                _serverProcess.StartInfo.EnvironmentVariables[kvp.Key] =
                    currentValue + (currentValue.EndsWith(";") ? "" : ";") + kvp.Value;
            }

            _serverProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger.LogInformation("Ruby output: {Output}", e.Data);
                }
            };

            _serverProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger.LogError("Ruby error: {Error}", e.Data);
                }
            };

            _serverProcess.EnableRaisingEvents = true;
            _serverProcess.Exited += (sender, e) =>
            {
                _logger.LogWarning("Ruby process exited with code: {ExitCode}", _serverProcess.ExitCode);
                if (!_serverProcess.HasExited)
                {
                    RestartRubyProcess();
                }
            };

            _serverProcess.Start();
            _serverProcess.BeginOutputReadLine();
            _serverProcess.BeginErrorReadLine();

            _logger.LogInformation("Started Ruby process with PID: {PID}", _serverProcess.Id);
        }

        private void StopRubyProcess()
        {
            if (_serverProcess == null || _serverProcess.HasExited)
            {
                return;
            }

            try
            {
                KillProcessAndChildren(_serverProcess.Id);
                _serverProcess.WaitForExit(5000); // Wait up to 5 seconds for graceful shutdown
                if (!_serverProcess.HasExited)
                {
                    _serverProcess.Kill(true); // Force kill if still running
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error stopping Ruby process");
            }
            finally
            {
                _serverProcess.Dispose();
                _serverProcess = null;
            }
        }

        private string FindAppInPathDirectories(string app)
        {
            string? environmentPath = System.Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(environmentPath))
            {
                return app;
            }

            string[] paths = environmentPath.Split(System.IO.Path.PathSeparator);
            string[] executableExtensions = new string[] { ".exe", ".com", ".bat", ".sh", ".vbs", ".vbscript", ".vbe", ".js", ".rb", ".cmd", ".cpl", ".ws", ".wsf", ".msc", ".gadget" };

            foreach (string path in paths)
            {
                foreach (string extension in executableExtensions)
                {
                    string fullFile = System.IO.Path.Combine(path, app + extension);
                    if (System.IO.File.Exists(fullFile))
                    {
                        return fullFile;
                    }
                }

                string fileWithoutExtension = System.IO.Path.Combine(path, app);
                if (System.IO.File.Exists(fileWithoutExtension))
                {
                    return fileWithoutExtension;
                }
            }

            return app;
        }

        private static void KillProcessAndChildren(int pid)
        {
            using System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher($"SELECT * FROM Win32_Process WHERE ParentProcessID = {pid}");
            using System.Management.ManagementObjectCollection childProcesses = searcher.Get();

            foreach (System.Management.ManagementObject process in childProcesses)
            {
                if (process["ProcessID"] != null)
                {
                    KillProcessAndChildren(System.Convert.ToInt32(process["ProcessID"]));
                }
            }

            try
            {
                System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(pid);
                if (!process.HasExited)
                {
                    process.Kill(true);
                }
            }
            catch (System.ArgumentException)
            {
                // Process already exited
            }
        }

        protected override async System.Threading.Tasks.Task ExecuteAsync(System.Threading.CancellationToken stoppingToken)
        {
            try
            {
                StartRubyProcess();
                await System.Threading.Tasks.Task.Delay(-1, stoppingToken);
            }
            catch (System.OperationCanceledException)
            {
                // Normal shutdown
            }
            finally
            {
                StopRubyProcess();
                _configWatcher.Dispose();
            }
        }
    }


}