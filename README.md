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

Contributions are welcome! Create a PR and explain the purpose of the changes.

## Development

To build and install the plugin, first modify paths found in `BuildAndInstallPlugin.bat` at the root of this repo. Then it run it. This will build the repo then copy the files to the plugins folder for PowerToys Run.

# License
This project is licensed under the [MIT License](LICENSE)