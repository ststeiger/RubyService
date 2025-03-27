
namespace StartRuby
{


    public class ExceptionHelper
    {


        public static void OnUnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            try
            {
                System.Console.WriteLine("Unbehandelte Ausnahme: \r\n" + e.ExceptionObject.ToString());
            }
            catch (System.Exception)
            {
                try
                {
                    string str = "Unbehandelte Ausnahme: \r\n";
                    str += "  Beschreibung: \r\n";
                    str = str + "  " + e.ExceptionObject.ToString();
                    str += "  \r\n";
                    System.Console.WriteLine(str);

                    string logFileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                    logFileName += System.IO.Path.DirectorySeparatorChar.ToString();
                    logFileName += "UnhandledException.log.txt";
                    System.Console.WriteLine(logFileName);
                    System.IO.File.WriteAllText(logFileName, str);
                }
                catch (System.Exception)
                { }

            } // End Catch 

            System.Environment.Exit(1);
        } // End Sub OnUnhandledException 


        // https://stackoverflow.com/questions/3284137/taskscheduler-unobservedtaskexception-event-handler-never-being-triggered
        public static void OnUnobservedTaskException(object sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs eventArgs)
        {
            eventArgs.SetObserved();
            ((System.AggregateException)eventArgs.Exception).Handle(ex =>
            {
                DisplayError(ex);
                return true;
            });

        }


        public static string JsonizeError(System.Exception ex)
        {
            System.Exception thisError = ex;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            using (System.IO.TextWriter tw = new System.IO.StringWriter(sb))
            {
                using (Newtonsoft.Json.JsonTextWriter jw = new Newtonsoft.Json.JsonTextWriter(tw))
                {
                    jw.Formatting = Newtonsoft.Json.Formatting.Indented;

                    int objectCount = 0;

                    while (thisError != null)
                    {
                        jw.WriteStartObject();
                        ++objectCount;

                        jw.WritePropertyName("message");
                        jw.WriteValue(thisError.Message);

                        if (thisError.StackTrace != null)
                        {
                            jw.WritePropertyName("stackTrace");
                            jw.WriteValue(thisError.StackTrace);
                        }

                        if (thisError.Source != null)
                        {
                            jw.WritePropertyName("source"); // aka Assembly
                            jw.WriteValue(thisError.Source);
                        }

                        jw.WritePropertyName("name");
                        jw.WriteValue(thisError.GetType().FullName);

                        jw.WritePropertyName("hResult");
                        jw.WriteValue(thisError.HResult);

                        if (thisError.Data != null && thisError.Data.Keys.Count > 0)
                        {
                            jw.WritePropertyName("data"); // System.Collections.IDictionary
                            jw.WriteRawValue(
                                Newtonsoft.Json.JsonConvert.SerializeObject(thisError.Data, jw.Formatting));
                        }

                        if (thisError.HelpLink != null)
                        {
                            jw.WritePropertyName("helpLink"); // URN
                            jw.WriteValue(thisError.HelpLink);
                        }

                        if (thisError.InnerException != null)
                        {
                            jw.WritePropertyName("innerException");
                        } // End if (thisError.InnerException != null) 

                        thisError = thisError.InnerException;
                    } // Whend 

                    for (int i = 0; i < objectCount; ++i)
                        jw.WriteEndObject();
                } // End Using jw 
            } // End Using tw

            string ret = sb.ToString();
            sb.Clear();
            sb = null;

            if (string.IsNullOrEmpty(ret))
                // ret = "{ \"message\": null, \"stackTrace\": null, \"innerException\": null }";
                ret = "{}";

            return ret;
        } // End Sub JsonizeError 


        public static string StringifyError(System.Exception ex)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine(System.Environment.NewLine);
            sb.AppendLine(System.Environment.NewLine);

            System.Exception thisError = ex;
            while (thisError != null)
            {
                sb.AppendLine(thisError.GetType().FullName);
                sb.AppendLine(thisError.Source);
                sb.AppendLine(thisError.Message);
                // sb.AppendLine(thisError.HResult);
                sb.AppendLine(thisError.StackTrace);

                if (thisError.InnerException != null)
                {
                    sb.AppendLine(System.Environment.NewLine);
                    sb.AppendLine("Inner Exception:");
                } // End if (thisError.InnerException != null) 

                thisError = thisError.InnerException;
            } // Whend 

            sb.AppendLine(System.Environment.NewLine);
            sb.AppendLine(System.Environment.NewLine);

            return sb.ToString();
        } // End Sub DisplayError 


        public static void DisplayError(System.Exception ex)
        {
            System.Console.WriteLine(System.Environment.NewLine);
            System.Console.WriteLine(System.Environment.NewLine);

            System.Exception thisError = ex;
            while (thisError != null)
            {
                System.Console.WriteLine(thisError.GetType().FullName);
                System.Console.WriteLine(thisError.Source);
                System.Console.WriteLine(thisError.Message);
                // System.Console.WriteLine(thisError.HResult);
                System.Console.WriteLine(thisError.StackTrace);

                if (thisError.InnerException != null)
                {
                    System.Console.WriteLine(System.Environment.NewLine);
                    System.Console.WriteLine("Inner Exception:");
                } // End if (thisError.InnerException != null) 

                thisError = thisError.InnerException;
            } // Whend 

            System.Console.WriteLine(System.Environment.NewLine);
            System.Console.WriteLine(System.Environment.NewLine);
        } // End Sub DisplayError 
        
        
    } // End Class ExceptionHelper 
    
    
} // End Namespace PdfHealthMonitor 
