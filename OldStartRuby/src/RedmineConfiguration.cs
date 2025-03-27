
namespace StartRuby
{


    public class RedmineConfiguration
    {
        public string LogFile_Directory;
        public string WebRoot;
        public string DateTimeFormat;

        public System.Collections.Generic.Dictionary<string, string> OverwriteEnvironmentVariables = new System.Collections.Generic.Dictionary<string, string>();
        public System.Collections.Generic.Dictionary<string, string> AppendEnvironmentVariables = new System.Collections.Generic.Dictionary<string, string>();


        public string StartProgram;
        public string StartProgramArguments;

        // webServer.StartInfo.FileName = @"C:\Ruby21-x64\bin\puma.bat"; // where puma ==> C:\Ruby21-x64\bin\puma; C:\Ruby21-x64\bin\puma.bat
        // webServer.StartInfo.Arguments = @"puma --env production --dir " + REDMINE_DIR + " -p 3000";
        // webServer.StartInfo.Arguments = @"--env production --dir " + REDMINE_DIR + " -p 3000";

    }


}
