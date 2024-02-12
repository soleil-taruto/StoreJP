CALL C:\Factory\SetEnv.bat
CALL Clean.bat
cx **
IF ERRORLEVEL 1 C:\app\MsgBox\MsgBox.exe E "BUILD ERROR"

acp Silvia20200001\Silvia20200001\bin\Release\Silvia20200001.exe out\*P.exe
xcp doc out

C:\Factory\SubTools\zip.exe /D out
