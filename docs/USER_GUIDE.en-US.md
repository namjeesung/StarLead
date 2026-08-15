# StarLead User Guide

> For StarLead v0.31 on Windows 11 x64.

[中文手册](USER_GUIDE.zh-CN.md) · [Project home](../README.md)

## 1. What StarLead is

StarLead is a local Windows action launcher. Bind frequently used apps, files, folders, and the built-in notebook to visible keys, open the panel with a global hotkey, then launch an action with the mouse or its displayed key.

StarLead provides two independent panels:

- **Keyboard panel:** arranged as `1–0`, `Q–P`, `A–L`, and `Z–M` for muscle memory.
- **Linear panel:** an unlimited horizontal list designed for mouse-wheel browsing.

Settings, bindings, and notes stay on your computer by default.

## 2. Requirements

- Windows 11 x64
- Intel or AMD x64 processor
- The installer includes the required .NET runtime

## 3. Install, update, and uninstall

### Install

1. Download the latest `StarLead-Setup-x64-v*.exe` from GitHub Releases.
2. Run the installer.
3. Choose whether to create a desktop shortcut.
4. Launch StarLead after installation.

### Update

Run a newer installer over the existing installation. Setup closes the old process and replaces application files without deleting `%LOCALAPPDATA%\StarLead\data.json`.

### Uninstall

Use **Windows Settings → Apps → Installed apps**. The uninstaller does not automatically remove local user data. To erase everything, exit StarLead and delete:

```text
%LOCALAPPDATA%\StarLead
```

## 4. Quick start

1. Press `Ctrl + Space` to open StarLead.
2. Double-click an empty key.
3. Bind an app, file, folder, or notebook action.
4. Click the icon, or press its displayed number/letter while the panel is open.
5. Press `Esc` or the activation hotkey again to close the panel.

You can also drag apps, files, or folders directly from Desktop or File Explorer onto an empty key.

## 5. Keyboard panel

### Launch actions

- Click a bound icon to launch it.
- While the panel is open, press the key shown on the icon.
- A drag gesture suppresses the click action, preventing accidental launches while arranging keys.

### Bind actions

Double-click an empty key and choose:

- Bind app
- Bind file
- Bind folder
- Bind notebook

Dropping an item from Desktop or File Explorer also binds it immediately.

### Arrange and remove

- Drag one key onto another to swap their bindings.
- The target key highlights during the drag.
- Right-click a bound icon to open, rebind, or remove it.
- The notebook behaves like every other action: it can be moved, swapped, removed, and bound again.

### Visible keys

Use **Settings → Visible keys** to hide individual keys. Hiding a key does not delete its binding.

## 6. Linear panel

### Add and launch

- Drop an app, file, or folder onto empty panel space to append it.
- There is no fixed item limit.
- Click an icon to launch it.
- The first ten actions can be launched with `1–0`.

### Browse

- Rotate the mouse wheel to move through icons.
- The selected icon highlights and scrolls smoothly into view.
- Hovering an icon enlarges it naturally; it returns to normal when the pointer leaves.

### Reorder and remove

- Drag icons to reorder them.
- Right-click an icon to open or remove it.
- The notebook is a regular linear action and can also be moved or deleted.

### Width and spacing

- Drag either panel edge to resize the panel directly.
- Right-click empty space to choose adaptive or custom width.
- Adjust icon spacing live from the same menu.

## 7. Windows system entries

The keyboard panel can show independent shortcuts for:

- This PC
- Downloads
- Recycle Bin

Toggle each entry under **Settings → System components**.

### Empty Recycle Bin

Right-click the Recycle Bin icon and select **Empty Recycle Bin**. StarLead opens the native Windows confirmation dialog; nothing is removed if you cancel.

## 8. Notebook

The notebook is a regular action that can be bound to any keyboard key or linear position. Removing its launcher does not remove saved notes; bind the notebook again to reopen them.

### Create and edit

- Double-click empty notebook space to create a card.
- Double-click a card to edit it.
- Titles and body text save automatically on this device.

### Select and arrange

- Click a card to select it.
- Use `Ctrl + click` to select multiple cards.
- In Free layout, moved cards snap to a straight grid.
- Auto layout sorts pinned notes first, then by last modified time.
- **Arrange** reorganizes all cards immediately.

### Context menu

- Edit
- Copy text
- Pin / Unpin
- Apply Default, Yellow, Blue, Green, Pink, or Purple
- Delete

Applying a color while multiple cards are selected updates all selected cards.

## 9. Settings reference

### Language

Choose Chinese or English. The interface changes and saves immediately.

### Appearance

Choose Light or Dark mode. Changes apply immediately.

### Interface style

- Liquid Glass
- Ocean Blue
- Aurora
- Graphite

Every style supports both Light and Dark mode.

### Panel style

Keyboard and Linear panels keep independent action lists. The selected panel becomes the default immediately.

### Background opacity

- Range: `0–100%`
- Drag or click the slider for a live preview.
- Clicking anywhere on the track jumps directly to that value.
- Icons remain opaque.
- At `0%`, the background, outer border, and shadow disappear.

### Linear panel length

Adjust the maximum width in Settings, or drag the left/right edge of the linear panel.

### Icon names

Show or hide names on both panels. Key letters and order numbers remain visible.

### Activation hotkey

The default is `Ctrl + Space`. Choose **None** as the modifier to use a single symbol key such as `` ` ``, `-`, `=`, `[`, or `]`.

If another application owns the selected hotkey, choose a different one.

### Start with Windows

Enable **Start StarLead when signing in to Windows** to launch it for the current Windows user.

### Panel position

- Remember last position
- Always center

Drag any empty panel area outside keys, buttons, and sliders. Expensive shadows are temporarily disabled during movement and restored afterward for smoother dragging.

## 10. Local data and privacy

StarLead stores its data at:

```text
%LOCALAPPDATA%\StarLead\data.json
```

This includes:

- App, file, and folder bindings
- Panel positions and appearance settings
- Linear icon order
- Local notebook content

StarLead currently has no cloud sync and does not intentionally upload this data.

### Back up data

1. Exit StarLead from its tray icon.
2. Copy the entire `%LOCALAPPDATA%\StarLead` folder.
3. Restore it to the same location when needed.

## 11. Troubleshooting

### The activation hotkey does nothing

- Check whether another app uses the same hotkey.
- Select another hotkey in Settings and save.
- Exit StarLead from the tray and start it again.

### An icon looks blurry

Bind the original `.exe`, file, or folder again. StarLead requests a high-resolution icon from Windows Shell.

### Downloads opens an unexpected location

StarLead uses the Downloads known-folder location registered by Windows rather than hard-coding `C:\Users\name\Downloads`. Check the Downloads location in File Explorer.

### Are notes lost after removing the notebook launcher?

No. Removing the launcher does not remove notebook data. Double-click an empty key and select **Bind notebook**.

### Context-menu text is hard to read

Starting with v0.31, StarLead uses the `Microsoft YaHei UI / Segoe UI` font fallback and forces context menus to use dark text on white. Exit an older running process before installing the latest version.

## 12. Keyboard and mouse reference

| Action | Result |
|---|---|
| `Ctrl + Space` | Default show/hide hotkey |
| `Esc` | Close the panel |
| Click a bound icon | Launch action |
| Double-click an empty key | Create a binding |
| Drag a key or icon | Swap or reorder |
| Right-click an action | Open, rebind, or remove |
| Press a displayed key | Launch that action |
| Mouse wheel on Linear panel | Browse and highlight icons |
| `Ctrl + click` in Notebook | Select multiple notes |
| Right-click Recycle Bin | Empty Recycle Bin |

## 13. Getting help

When filing a GitHub issue, include:

- StarLead version
- Windows version
- A screenshot
- Reproduction steps
- Light/Dark mode and interface style in use

