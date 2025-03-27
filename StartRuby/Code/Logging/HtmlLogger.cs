
namespace StartRuby
{


    class HtmlLogger
        : TrivialLogger
    {


        private const string template = @"<!DOCTYPE html>
<html>
<head>
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge,chrome=1"" />
    <meta charset=""utf-8"" />
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />

    <meta http-equiv=""cache-control"" content=""max-age=0"" />
    <meta http-equiv=""cache-control"" content=""no-cache"" />
    <meta http-equiv=""expires"" content=""0"" />
    <meta http-equiv=""expires"" content=""Tue, 01 Jan 1980 1:00:00 GMT"" />
    <meta http-equiv=""pragma"" content=""no-cache"" />


    <title>Portal: COR Maps</title>

    <link rel=""shortcut icon"" type=""image/x-icon"" href=""../favicon.ico"" />
	
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.00, minimum-scale=1.00, maximum-scale=1.00"" />
    <meta http-equiv=""cache-control"" content=""no-cache"" />
	<!--
    <link href=""../w8/Layout.ashx?r=map3"" rel=""stylesheet"" media=""all"" type=""text/css"" />
    <script src=""../w8/Script.ashx?r=map3"" type=""text/javascript""></script>
	-->
    <style type=""text/css"">
        html, body 
		{
            margin: 0px;
            padding: 0px;
            width: 100%;
            height: 100%;
        }

        td
        {
            vertical-align: top;
        }
        
        table tr:nth-child(odd) td
        {
            background-color: #D9E1F2;
        }

        table tr:nth-child(even) td
        {
            background-color: #B4C6E7;
        }

        table tr:first-child td 
        {
            background-color: #4472C4;
            color: white;
            font-weight: bold;
        }

	</style>
	
	<script type=""text/javascript"">
		
	</script>
	
</head>
<body>

<table>


";

        public string FilePath;



        public HtmlLogger(string path)
            : base()
        {
            // ”D:\IDGLog.txt”
            this.FilePath = path;
            this.LogLevel = LogLevel_t.Everything;

            string dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            using (System.IO.Stream strm = System.IO.File.Open(this.FilePath, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read))
            {

                using (System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(strm, System.Text.Encoding.UTF8))
                {
                    if (strm.Length < 1)
                    {
                        streamWriter.WriteLine(template);
                    } // End if (strm.Length < 1) 

                } // End Using streamWriter 

            } // End Using strm 

        } // End Constructor 


        public HtmlLogger()
            : this(null)
        { }


        public override void Log(LogLevel_t logLevel, string message, System.Exception exception)
        {
            if (!LogLevel.HasFlag(logLevel))
                return;

            lock (lockObj)
            {

                using (System.IO.Stream strm = System.IO.File.Open(this.FilePath, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read))
                {

                    using (System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(strm, System.Text.Encoding.UTF8))
                    {
                        // 2009-06-05 08:15:23 [INFO] User credentials entered, Acc={123765987}
                        streamWriter.WriteLine("<tr>");

                        streamWriter.Write("<td>");
                        streamWriter.Write(System.Web.HttpUtility.HtmlEncode(this.Time));
                        streamWriter.WriteLine("</td>");

                        string ll = "[" + System.Convert.ToString(logLevel, System.Globalization.CultureInfo.InvariantCulture) + "]";
                        streamWriter.Write("<td>");
                        streamWriter.Write(System.Web.HttpUtility.HtmlEncode(ll));
                        streamWriter.WriteLine("</td>");

                        streamWriter.Write("<td>");

                        if (message == null)
                            streamWriter.WriteLine("Error: ");
                        else
                            streamWriter.WriteLine(System.Web.HttpUtility.HtmlEncode(message));

                        streamWriter.WriteLine("<br />");


                        System.Exception thisError = exception;
                        while (thisError != null)
                        {
                            streamWriter.Write("Type: ");
                            streamWriter.WriteLine(System.Web.HttpUtility.HtmlEncode(thisError.GetType().FullName));
                            streamWriter.WriteLine("<br />");

                            streamWriter.Write("Source: ");
                            streamWriter.WriteLine(System.Web.HttpUtility.HtmlEncode(thisError.Source));
                            streamWriter.WriteLine("<br />");

                            streamWriter.WriteLine(System.Web.HttpUtility.HtmlEncode(thisError.Message));
                            streamWriter.WriteLine("<br />");

                            streamWriter.WriteLine("StackTrace: ");
                            streamWriter.WriteLine("<br />");

                            if (thisError.StackTrace != null)
                            {
                                streamWriter.WriteLine(
                                   System.Web.HttpUtility.HtmlEncode(thisError.StackTrace)
                                   .Replace("\r\n", "\n").Replace("\n", "<br />")
                                   );
                            } // End if (thisError.StackTrace != null) 

                            if (thisError.InnerException != null)
                            {
                                streamWriter.WriteLine("<br />");
                                streamWriter.WriteLine("<br />");
                                streamWriter.WriteLine("Inner Exception:");
                                streamWriter.WriteLine("<br />");
                            } // End if (thisError != null) 

                            thisError = thisError.InnerException;

                            streamWriter.Flush();
                            strm.Flush();
                        } // Whend 

                        streamWriter.WriteLine("</td>");
                        streamWriter.WriteLine("</tr>");


                        streamWriter.Flush();
                        strm.Flush();
                        streamWriter.Close();
                    } // End Using streamWriter 

                } // End Using strm 

            } // End Lock 

        } // End Sub Log 


    } // End Class HtmlLogger 


} // End Namespace RamMonitor.Logging 
