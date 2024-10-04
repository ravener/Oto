# Oto

An osu! hitsound copier for mappers, built with GTK for Linux users.

The word Oto (音) means 'sound' in Japanese.

## Requirements

- .NET 8 SDK
- GTK 4.8+
- Adwaita 1.2+

The app currently targets libraries available on Debian 12, so almost any recent Linux distribution can build and run it without flatpak.

```sh
$ git clone https://github.com/ravener/Oto
$ cd Oto/Oto
$ dotnet run
```

A flatpak release is also eventually planned.

This is built using [Mapping_Tools_Core](https://github.com/OliBomby/Mapping_Tools_Core), only hitsounds copier is implemented for now but perhaps a long term vision for this project would be to have the entirety of [Mapping Tools](https://mappingtools.github.io/) available for Linux users with a nice UI.

A windows and maybe macOS release is also planned depending on how complex it'd be to get it working.

This is still early in development and feedback is appreciated to make the tool more convenient to use.
