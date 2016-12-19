
License: MIT


Build-status

[![Status][1]][1]


  [1]: https://travis-ci.org/ststeiger/RubyService.svg?branch=master



# Ruby Service for Windows

A simple ruby server for windows. 


## Suggestions, requests, love letters...

We are open to suggestions, don't hesitate to [submit one](https://github.com/ststeiger/RubyService/issues).

## How to get it

As a [NuGet package](https://www.nuget.org/packages/RubyService).

## How to use it

### Instantiation

```csharp
    ServiceBase[] ServicesToRun;
    ServicesToRun = new ServiceBase[]
    {
        new RubyService()
    };
```

### Configuration

#### XML-configuration

A few config variables are needed
*  The logfile-directory
*  The WebRoot (your web application's root folder)
*  DateTimeFormat, used in Logfiles
*  The rails environment
*  The Path to Ruby.exe
*  The server you want to start
*  The command line arguments for the server.


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

