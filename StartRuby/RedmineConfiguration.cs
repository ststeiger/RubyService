
namespace StartRuby
{


    public class RedmineConfiguration
    {
        public string LogFile_Directory { get; set; } = string.Empty;
        public string WebRoot { get; set; } = string.Empty;
        public string DateTimeFormat { get; set; } = "dddd, dd.MM.yyyy HH:mm:ss";

        public System.Collections.Generic.Dictionary<string, string> OverwriteEnvironmentVariables { get; set; } = new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.InvariantCultureIgnoreCase);
        public System.Collections.Generic.Dictionary<string, string> AppendEnvironmentVariables { get; set; } = new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.InvariantCultureIgnoreCase);

        public string StartProgram { get; set; } = string.Empty;
        public string StartProgramArguments { get; set; } = string.Empty;
    }


}