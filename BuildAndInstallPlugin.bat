
dotnet build -c Release
taskkill /F /IM PowerToys.exe
robocopy /e /w:5 "C:\Users\m_sno\PowerToys-Run-Spotify\bin\Release\net8.0-windows" "C:\Program Files\PowerToys\RunPlugins\PowerToys-Run-Spotify"
start "" "C:\Program Files\PowerToys\PowerToys.exe"