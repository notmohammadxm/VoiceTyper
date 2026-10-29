@echo off
setlocal
cd /d "%~dp0"
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe

"%CSC%" /nologo /codepage:65001 /target:winexe /optimize+ /out:VoiceTyper-Setup.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll VoiceTyper.cs
if errorlevel 1 (
  echo.
  echo Build failed. Please send the error text above.
  pause
  exit /b 1
)

echo.
echo Done: VoiceTyper-Setup.exe created.
start "" "%~dp0VoiceTyper-Setup.exe"
