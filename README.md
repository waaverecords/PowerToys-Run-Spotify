# Spotify Plugin for PowerToys Run

This is a plugin for [PowerToys](https://github.com/microsoft/PowerToys) Run that allows you to search Spotify and control its player.

<p align="center">
    <img src="./demo.gif" width="760" />
</p>

## Features

- Search for tracks, artists and playlists
- Play tracks, artists and playlists
- Add track to queue

## Installation

Current release version of PowerToys (Release v0.76.2) does not support dynamic loading of Dlls, and as such, installing the plugin does not work. However, dynamic loading is supported on branch Master and will be included in the next Release version.

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