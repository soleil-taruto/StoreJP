CALL C:\Factory\SetEnv.bat
CALL Clean.bat
cx **
IF ERRORLEVEL 1 C:\app\MsgBox\MsgBox.exe E "BUILD ERROR"

acp Enrica20200001\Enrica20200001\bin\Release\Enrica20200001.exe out\*P.exe
acp doc\Readme.txt out\*P.txt

xcp out C:\Factory\apps2
