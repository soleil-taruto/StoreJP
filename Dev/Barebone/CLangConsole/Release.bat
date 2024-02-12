CALL C:\Factory\SetEnv.bat
CALL Clean.bat
cx **
IF ERRORLEVEL 1 C:\app\MsgBox\MsgBox.exe E "BUILD ERROR"

acp Rico20200001\Program.exe out\*P.exe
xcp doc out

C:\Factory\SubTools\zip.exe /D out
