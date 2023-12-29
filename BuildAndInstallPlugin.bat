
dotnet build -c Release
taskkill /F /IM PowerToys.exe
robocopy /e /w:5 "C:\Users\m_sno\PowerToys-Run-Spotify\bin\Release\net8.0-windows" "C:\Users\m_sno\PowerToys\x64\Release\RunPlugins\PowerToys-Run-Spotify"
start "" "C:\Users\m_sno\PowerToys\x64\Release\PowerToys.exe"