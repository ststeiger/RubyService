@echo off
:: Check for administrative privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Requesting administrative privileges...
    powershell -Command "Start-Process '%~0' -Verb RunAs"
    exit /b
)

sc create COR_Redmine binPath= "C:\Ruby34-x64\Service\StartRuby.exe" ^
   DisplayName= "Redmine Puma" start= auto depend= MSSQLSERVER

sc failure COR_Redmine reset= 86400 actions= restart/900000/restart/900000/restart/900000

echo Service COR_Redmine installed and configured successfully.
pause
