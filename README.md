
License: MIT


# Ruby Service for Windows   ![Continuous Integration Status](https://travis-ci.org/ststeiger/RubyService.svg?branch=master)

A simple ruby server for windows. 


## Suggestions, requests, love letters...

Don't hesitate to submit your suggestions [here](https://github.com/ststeiger/RubyService/issues).

## How to get it

From [Travis-CI](https://travis-ci.org/ststeiger/RubyService/builds) build artifacts.

## How to use it


### Installing it as windows-service
```dos
    sc create "COR_Redmine" displayname= "Redmine Puma" start= auto binPath= "C:\Redmine\redmine-3.2.4\StartRuby.exe"
    sc description "COR_Redmine" "Puma Web-Server serving redmine"
    sc config "COR_Redmine" depend= <Dependencies(separated by / (forward slash))>
```
note that you are expected to set the necessary dependencies yourselfs, as the name of your MSSQL-Server instance is not constant. 


To delete the service:
```dos
    sc delete "COR_Redmine"
```

To manually start/stop the service, type 
```dos
    services.msc
```
into Windows the "run" prompt. 


To start/stop the service from command-line: 
```dos
    net start "COR_Redmine"
	net stop "COR_Redmine"
```

or with the newer sc command, which also works over network: 
```dos
    sc start "COR_Redmine"
	sc stop "COR_Redmine"
```

See also [Difference between net and sc](http://superuser.com/questions/315166/net-start-service-and-sc-start-what-is-the-difference).




### Configuration

#### XML-configuration

A few config variables are needed
*  The config file has to be called the same as the service-executable, minus .exe and plus .json.config, and has to be in the same directory as the serice-.exe-file
*  The logfile-directory is where the service will log all output, in a separate file for each day - no autocleaning.
*  The WebRoot (your web application's root folder)
*  DateTimeFormat, that will be used when writing text into the logfile(s) 
*  The RAILS_ENV environment variable
*  The path to Ruby.exe, to be appended to the PATH environment variable 
*  StartProgram: The server you want to start (e.g. puma, thin, webrick, whatever)
*  The command line arguments for the server.


#### Example: 
```dos
REM {StartProgram} --env production --dir "{WebRoot}" -p 3000
REM e.g. 
SET RAILS_ENV="production";
SET PATH="%PATH%;C:\Ruby21-x64\bin;"
puma --env production --dir "C:\Redmine\Redmine-3.2.4" -p 3000 >> "C:\Redmine\redmine-3.2.4\Log\service_20161220.log"
```

the corresponding configuration file would be: 

```json
{
  "LogFile_Directory": "C:\\Redmine\\redmine-3.2.4\\Log\\",
  "WebRoot": "C:\\Redmine\\redmine-3.2.4",
  "DateTimeFormat": "dddd, dd.MM.yyyy HH:mm:ss",

  "OverwriteEnvironmentVariables": {
    "RAILS_ENV": "production"
  },

  "AppendEnvironmentVariables": {
    "PATH": "C:\\Ruby21-x64\\bin"
  },

  "StartProgram": "puma",
  "StartProgramArguments": "--env production --dir \"{WebRoot}\" -p 3000"
}

```


#### How to integrate with IIS: 
If you used the above config-file to launch [Redmine](http://redmine.org), then you'll have redmine running on
http://127.0.0.1:3000

You can now reverse-proxy redmine into IIS. 
E.g. create the directory "C:\inetpub\wwwroot\redmine", and put the below XML into a file called "web.config". 
Now, you need to open inetmgr, and convert the directory "redmine" into a web-application. 
Enable proxy on this machine, under "Application request routing". 

You will need to install the [Application-Request-Routing](http://go.microsoft.com/fwlink/?LinkID=615136) 
and the [URL-Rewrite](https://www.microsoft.com/en-us/download/details.aspx?id=47337) module for IIS first. 
See that you download the latest version, otherwise installation will fail with the latest IIS versions. 





Note: 
Microsoft apparently is not capable to write a proper reverse-proxy/url-rewrite module. 
So if you have redmine running in a virtual directory "redmine" in IIS (e.g. http://localhost/redmine), 
then redmine itselfs should also run in a virtual directory called "redmine" on 127.0.0.1:3000, e.g. http://127.0.0.1:3000/redmine. 
If it runs in the root domain (http://localhost), then it should run in the root-domain as well (http://127.0.0.1:3000). 
The web.config file here assumes that the ruby web-server runs under http://127.0.0.1/redmine.


```xml
<?xml version="1.0" encoding="UTF-8"?>
<!--
  For more information on how to configure your ASP.NET application, please visit
  http://go.microsoft.com/fwlink/?LinkId=169433
-->
<configuration>
  
    <system.web>
      <compilation debug="true" targetFramework="4.0" />
    </system.web>
    <system.webServer>
        <rewrite>
            <rules>
                <rule name="ReverseProxyInboundRule1" stopProcessing="true">
                    <match url="(.*)" />
                    <action type="Rewrite" url="http://127.0.0.1:3000/redmine/{R:1}" />
                </rule>
            </rules>
        </rewrite>
    </system.webServer>
    
</configuration>
```
Check out http://127.0.0.1:3000/redmine first to see whether the ruby webserver actually works. 
It should. 
Then check out http://localhost/redmine to see if the reverse-proxy works. 
It should. 


Note that you need to enable virtual directories in the redmine source-code, e.g. you need to change config.ru to 
```ruby
#map ENV['RAILS_RELATIVE_URL_ROOT'] do
map Rails.application.config.relative_url_root || "/" do
    run RedmineApp::Application
end
```

and add 
```ruby
  config.relative_url_root = '/redmine'
```
to 
C:\Redmine\redmine-3.2.4\config\environments\production.rb
