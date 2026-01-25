# CS2-Translator

Cross-platform real-time chat translation tool for Counter-Strike 2.

CS2-Translator reads the CS2 console output, detects chat messages, and translates them automatically, so you can understand teammates and enemies without leaving the game.

Works on **Windows and Linux**, runs as a **single file**, and requires only official CS2 launch options.

## Features

- Real-time chat translation inside CS2
- Cross-platform (Windows & Linux)
- Single-file executable
- Google Translate integration
- Translation cache (same messages = no re-translation)
- Automatic language detection
- Works with any CS2 install location
- In-game workflow (no alt-tab needed)
- Debug logging (`-debug` flag)


## How it works

1. CS2 writes console output using `-condebug`
2. CS2-Translator parses chat messages
3. Messages are translated and displayed instantly

No mods. No game file changes.


## Installation

### 1) Download

Grab the latest release:
https://github.com/ParadoxLeon/CS2-Translator/releases

### 2) Set CS2 launch options

Add this in Steam:
```
-condebug
```

### 3) Start

1. Launch CS2
2. Start CS2-Translator
3. Configure your Settings


## Debug Mode

Start CS2-Translator with:
```
-debug
```

This prints detailed logs for troubleshooting.


## Config & Data Location

### Linux
```
~/.config/CS2-Translator
```

### Windows (Roaming)
```
%APPDATA%/CS2-Translator
```

This folder contains:
- config files
- translation cache
- logs (if debug enabled)


## Supported Languages

CS2-Translator supports all Google Translate languages.  
Full list:
https://cloud.google.com/translate/docs/languages


## Update

To update:
1. Download the latest release
2. Delete the old version
3. Start the new one

No installer. No migration needed.


## Limitations

- Google Translate is rate-limited (~100 requests/hour depending on usage)
- Some community servers use custom chat formats that may not be detected


## Roadmap

- Custom translation providers
- UI improvements
- Overlay

