
dotnet build -c Release
taskkill /F /IM PowerToys.exe
robocopy /e /w:5 "%~dp0\bin\Release\net8.0-windows" "C:\Program Files\PowerToys Run\Plugins\PowerToys-Run-Spotify"
start "" "C:\Program Files\PowerToys\PowerToys.exe"