# FlipTextLayout

Small Windows tray utility for fixing text typed with the wrong English/Russian keyboard layout.

Default hotkey: `Ctrl + Shift + Space`.

## Features

- Global hotkey via `RegisterHotKey`.
- Clipboard workflow that waits for the clipboard sequence to change after `Ctrl+C`.
- Physical EN/RU keyboard mapping without dictionaries.
- Tray menu with `Switch Layout`, `Settings`, and `Exit`.
- JSON settings under `%AppData%\FlipTextLayout\settings.json`.
- Optional Windows startup registration, keyboard layout switching, and success sound.

## Build

```powershell
dotnet build .\FlipTextLayout.sln
```
