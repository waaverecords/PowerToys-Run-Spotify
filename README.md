# Spotify Plugin for PowerToys Run

This is a plugin for [PowerToys](https://github.com/microsoft/PowerToys) Run that allows you to search Spotify and control its player.

For the Command Palette version, see [CmdPal.Ext.Spotify](https://github.com/waaverecords/CmdPal.Ext.Spotify).

<p align="center">
    <img src="./demo.gif" width="760" />
</p>

## Features

- Search for songs, albums, artists and playlists
- Play songs, albums, artists and playlists
- Add song to queue (Shift+Enter)
- Pause and resume track
- Go to previous or next track
- Turn shuffle on and off
- Set repeat to track, context or off

## Installation

> [!IMPORTANT]
> Spotify Premium is necessary to control the player.

1. Ensure you have the [latest version](https://github.com/microsoft/PowerToys/releases/latest) of PowerToys installed.
2. Download the [newest release](https://github.com/waaverecords/PowerToys-Run-Spotify/releases/latest) zip file.
3. Extract the content of the zip file to the `RunPlugins` folder of your PowerToys' installation. Usually `C:\Program Files\PowerToys\RunPlugins`, or `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins`.
4. Restart PowerToys.
5. Head to your Spotify [developer dashboard](https://developer.spotify.com/).
6. Create a new app with:
    - `Redirect URI` set to `http://localhost:5543/callback`
    - `Web API` and `Web Playback SDK` checked
7. Go to the settings of the newly created app and save somewhere the value of `Client ID`. It is needed later.
8. Open PowerToys and go to the PowerToys Run section. Scroll down until you find the Spotify section. Click on it.
9. Set the value of `Client ID` with the value saved earlier.
10. Activate PowerToys Run and type `sp`. You should see a result asking you to login to your Spotify account. Hit `enter` and go through the login process.
11. Activate PowerToys Run again and type `sp lofi`. If the installation was a succes, you should see results.

## Update the plugin

1. Ensure you have the [latest version](https://github.com/microsoft/PowerToys/releases/latest) of PowerToys installed.
2. Close PowerToys.
2. Download the [newest release](https://github.com/waaverecords/PowerToys-Run-Spotify/releases/latest) zip file.
3. Extract the content of the zip file to the `RunPlugins` folder of your PowerToys' installation. Usually `C:\Program Files\PowerToys\RunPlugins`, or `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins`.
4. Restart PowerToys.

## Usage

Open PowerToys Run (default shortcut is ```Alt+Space```).

### Play a song

1. Type ```sp``` followed by your search query.
2. Select a song, artist or playlist and press ```Enter``` to play it.

### Control the playback

1. Type ```sp```.
2. Select your desired action and press ```Enter```.

## Contributing

Contributions are welcome! If you have any ideas, improvements, or bug fixes, please open an issue or submit a pull request.

To contribute to PowerToys-Run-Spotify, follow these steps:

1. Fork the repository.
2. Create a new branch for your feature/fix.
3. Make your changes and commit them with descriptive commit messages.
4. Push your changes to your forked repository.
5. Submit a pull request to the main repository.

Please ensure that your code adheres to the existing code style. Also, make sure to update the documentation as needed.

Together, we can make PowerToys-Run-Spotify better!

## Development

To build and install the plugin, run `BuildAndInstallPlugin.bat`. This will build the repo then copy the files to the plugins folder for PowerToys Run.

## License

This project is licensed under the [MIT License](LICENSE)
