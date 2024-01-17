# Spotify Plugin for PowerToys Run

This is a plugin for [PowerToys](https://github.com/microsoft/PowerToys) Run that allows you to search Spotify and control its player.

<p align="center">
    <img src="./demo.gif" width="760" />
</p>

## Features

- Search for tracks, albums, artists and playlists
- Play tracks, albums, artists and playlists
- Add track to queue

## Installation

> [!IMPORTANT]
> Spotify Premium is necessary to control the player.

1. Download the [newest release](https://github.com/waaverecords/PowerToys-Run-Spotify/releases) zip file.
2. Extract the content of the zip file to the `RunPlugins` folder of your PowerToys' installation. Usually `C:\Program Files\PowerToys\RunPlugins`.
3. Restart PowerToys.
4. Head to your Spotify [developper dashboard](https://developer.spotify.com/).
5. Create a new app with:
    - `Redirect URI` set to `http://localhost:5543/callback`
    - `Web API` and `Web Playback SDK` checked
6. Go to the settings of the newly created app and save somewhere the value of `Client ID`. It is needed later.
7. Open PowerToys and go to the PowerToys Run section. Scroll down until you find the Spotify section. Click on it.
8. Set the value of `Client ID` with the value saved earlier.
9. Activate PowerToys Run and type `sp`. You should see a result asking you to login to your Spotify account. Hit `enter` and go through the login process.
10. Activate PowerToys Run again and type `sp lofi`. If the installation was a succes, you should see results.

## Usage

1. Open PowerToys Run (default shortcut is ```Alt+Space```).
2. Type ```sp``` followed by your search query.
3. Select a track, artist or playlist to play and press ```Enter``` to play it.

## Contributing

Contributions are welcome! If you have any ideas, improvements, or bug fixes, please open an issue or submit a pull request.

To contribute to KickNodeJS, follow these steps:

1. Fork the repository.
2. Create a new branch for your feature/fix.
3. Make your changes and commit them with descriptive commit messages.
4. Push your changes to your forked repository.
5. Submit a pull request to the main repository.

Please ensure that your code adheres to the existing code style. Also, make sure to update the documentation as needed.

Together, we can make PowerToys-Run-Spotify better!

## Development

To build and install the plugin, first modify paths found in `BuildAndInstallPlugin.bat` at the root of this repo. Then it run it. This will build the repo then copy the files to the plugins folder for PowerToys Run.

## License
This project is licensed under the [MIT License](LICENSE)