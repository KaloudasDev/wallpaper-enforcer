## Wallpaper Protector

Professional Windows utility that prevents unauthorized desktop wallpaper changes. Features real-time monitoring, registry protection, and Windows startup integration. Built with C# and .NET for maximum reliability.

## Overview

This application ensures a specific wallpaper remains set on Windows systems, reverting unauthorized changes. Ideal for educational institutions, corporate environments, or personal use where maintaining a consistent desktop appearance is required.

## Key Features

- **Real-time Monitoring** - Checks wallpaper every 2 seconds, instantly reverting changes
- **Registry Protection** - Locks wallpaper settings through Windows Registry policies
- **Auto-Start Integration** - Optional startup with Windows (silent mode available)
- **Custom Wallpaper Support** - Set any image as the protected wallpaper
- **Complete Cleanup** - Full registry restoration and wallpaper reset on unlock
- **Explorer Integration** - Automatic restart of Explorer.exe for complete reset
- **Administrator Control** - Requires admin rights for full functionality

## Technology Stack

| Component | Technology |
|-----------|------------|
| Platform | Windows x64 |
| UI | Console Application |
| Registry | Microsoft.Win32 |
| Framework | .NET 8.0-windows |
| Runtime | Self-contained executable |
| Image Processing | System.Drawing.Common |
| System APIs | P/Invoke (user32.dll, kernel32.dll) |


## Installation

```bash
# Clone repository
git clone https://github.com/yourusername/wallpaper-protector.git

# Navigate to project
cd wallpaper-protector

# Restore dependencies
dotnet restore

# Build project
dotnet build -c Release

# Publish self-contained executable
dotnet publish -c Release -r win-x64 --self-contained true
```

### Silent Mode
Run with `/silent` parameter for background operation:
```bash
WallpaperProtector.exe /silent
```

## Menu Options Explained

| Option | Function |
|--------|----------|
| **1. Lock & Protect** | Activates wallpaper protection, creates default wallpaper, locks registry |
| **2. Unlock & Remove** | Complete cleanup: removes registry locks, restores default settings, restarts Explorer |
| **3. Set Custom Wallpaper** | Copies and sets any image as the protected wallpaper |
| **4. Check Status** | Displays current protection status, registry state, and wallpaper information |
| **5. Install Auto-Start** | Adds program to Windows startup with silent mode |
| **X. Exit** | Stops monitoring and exits application |

## What Happens When You Unlock

The `Unlock & Remove` option performs complete cleanup:

1. **Stops Background Monitor** - Ends the wallpaper checking thread
2. **Registry Cleanup**:
   - Deletes `NoDispBackgroundPage`, `NoDisplayBackground`, `NoChangingWallPaper`
   - Removes entire `Policies\System` and `Policies\ActiveDesktop` trees
3. **Restores Default Settings**:
   - Resets `Wallpaper` to empty
   - Sets `WallpaperStyle = 10` (Fill)
   - Sets `TileWallpaper = 0`
4. **Forces Wallpaper Update** via SystemParametersInfo
5. **Restarts Windows Explorer** for complete reset
6. **Opens Settings** to `ms-settings:personalization-background`

## Project Structure

```
wallpaper-protector/
├── Program.cs           # Main application code
├── protector.csproj     # Project configuration
├── README.md           # Documentation
└── bin/                # Build output (after compilation)
    └── Release/
        └── win-x64/
            └── publish/ # Self-contained executable
```

## Requirements

- **Operating System**: Windows 10/11 (x64)
- **.NET Runtime**: Self-contained (no runtime installation needed)
- **Permissions**: Administrator rights for registry operations
- **Disk Space**: ~50MB for published application

> [!IMPORTANT]
> Administrator privileges are required for full functionality. The application will automatically restart with admin rights when needed.

## Legal Disclaimer

> [!CAUTION]
> **USE AT YOUR OWN RISK**
>
> This software modifies Windows Registry settings and system behavior. While designed to be safe and reversible, improper use could affect system stability. The authors assume no responsibility for any issues arising from the use of this software.

## Important Notes

- Registry modifications are isolated to **HKEY_CURRENT_USER** (current user only)
- All changes are fully reversible via the Unlock option
- The wallpaper is stored at `C:\Windows\Web\Wallpaper\School\default.jpg`
- Silent mode runs without any visible interface
- Explorer restart may cause temporary screen flicker

## Troubleshooting

**Q: Wallpaper keeps reverting even after Unlock?**  
A: Ensure you've stopped the background monitor (option 2 does this automatically). If issues persist, restart your computer.

**Q: Can't set custom wallpaper?**  
A: Verify the image file exists and is accessible. Supported formats: JPEG, PNG, BMP.

**Q: Auto-start not working?**  
A: Check that the shortcut exists in `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup`

**Q: Registry keys not deleting?**  
A: Ensure you're running as Administrator. Some keys may require Group Policy overrides.

## Contributing

Contributions are welcome! Please ensure:
- Code follows existing patterns and practices
- Registry operations include proper error handling
- Changes are tested on Windows 10/11
- Documentation is updated accordingly

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) file for details.

## Educational Context

Developed as a system administration utility demonstrating:
- Windows Registry manipulation
- System API integration (P/Invoke)
- Background process management
- Windows startup integration
- File system operations
- Security and permission handling

> [!NOTE]
> This tool is intended for legitimate system administration purposes. Please ensure you have proper authorization before deploying on any system you do not personally own.
