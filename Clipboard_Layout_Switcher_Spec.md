# Clipboard Layout Switcher (.NET 8 / WPF)

## Goal

Develop a small Windows background utility (WPF, .NET 8) that fixes text
typed using the wrong keyboard layout.

Example:

    ghbdtn

↓

    привет

or

    руддщ

↓

    hello

------------------------------------------------------------------------

## Main Workflow

When the user presses a global hotkey, the application should:

1.  Save the current clipboard contents.
2.  Send `Ctrl+C` to the active window.
3.  Wait until the clipboard changes (do **not** use a fixed delay; wait
    with a timeout).
4.  If the clipboard doesn't contain text, abort.
5.  Detect the conversion direction automatically.
6.  Convert the text using a physical keyboard mapping (not a
    dictionary).
7.  Put the converted text into the clipboard.
8.  Send `Ctrl+V`.
9.  Restore the previous clipboard contents if possible. Failure to
    restore should not be treated as an error.

------------------------------------------------------------------------

## Hotkeys

Configurable.

Suggested default:

-   `Ctrl + Shift + Space`

or

-   `Ctrl + Alt + Q`

Use `RegisterHotKey()` instead of a low-level keyboard hook.

------------------------------------------------------------------------

## Background Operation

The application should:

-   Start minimized.
-   Run in the system tray.
-   Have no main window during normal operation.

Tray menu:

-   Switch Layout
-   Settings
-   Exit

------------------------------------------------------------------------

## Settings

Store settings in JSON.

Configuration options:

-   Hotkey
-   Restore clipboard (true/false)
-   Automatically switch Windows keyboard layout after conversion
-   Start with Windows
-   Play sound after successful conversion

------------------------------------------------------------------------

## Windows Keyboard Layout

Optionally switch the active Windows keyboard layout to match the
converted text.

Example:

    ghbdtn

↓

    привет

and automatically switch to the Russian layout.

------------------------------------------------------------------------

## Clipboard

Work with **text only**.

If the clipboard contains:

-   Images
-   Files
-   HTML

the application should do nothing.

Restore only previous text content if text existed.

------------------------------------------------------------------------

## Conversion

Use a physical key mapping only.

Example:

    q -> й
    w -> ц
    e -> у
    ...

and the reverse mapping.

Support:

-   Uppercase letters
-   CapsLock
-   Shift
-   Punctuation
-   Brackets
-   Quotes
-   Comma
-   Dot
-   Colon
-   Question mark

------------------------------------------------------------------------

## Architecture

Use dependency injection.

Suggested services:

``` text
IClipboardService
IHotkeyService
IKeyboardService
ILayoutConverter
ITrayIconService
ISettingsService
```

Avoid placing business logic in `App.xaml.cs`.

Use `Microsoft.Extensions.DependencyInjection`.

------------------------------------------------------------------------

## Quality Requirements

-   .NET 8
-   WPF
-   MVVM
-   Nullable enabled
-   Async/await
-   Proper IDisposable usage
-   Logging via Microsoft.Extensions.Logging
-   All code comments must be written in English.
-   Always use braces for the body of every `if`, `for`, `while`, etc.
-   One statement per line.
-   Avoid third-party libraries except Microsoft.Extensions.\* when
    appropriate.

------------------------------------------------------------------------

## Additional Requirements

-   Do not use `Thread.Sleep()`.
-   Prefer clipboard sequence number or clipboard change notifications
    over delays.
-   Correctly unregister `RegisterHotKey()`.
-   Never leave temporary clipboard contents after failures.
-   Keep the architecture extensible for:
    -   Additional keyboard layouts
    -   Custom layout mappings
    -   Automatic language detection
    -   Conversion history

------------------------------------------------------------------------

## Nice-to-Have Features

Future extensions:

-   Automatically fix the current word when no text is selected.
-   Multiple language pairs.
-   Optional conversion history.
-   User-defined layout tables.
