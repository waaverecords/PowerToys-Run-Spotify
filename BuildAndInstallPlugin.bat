@echo on
cd "%~dp0\"
dotnet build -c Release
taskkill /F /IM PowerToys.exe
robocopy /e /w:5 "%~dp0\bin\Release\net9.0-windows" "C:\Program Files\PowerToys\RunPlugins\PowerToys-Run-Spotify"
start "" "C:\Program Files\PowerToys\PowerToys.exe"
pause