# Security Policy 🔒

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | :white_check_mark: |

## Reporting a Vulnerability

If you discover a security vulnerability within this project, please **DO NOT** disclose it publicly through GitHub issues.

### Responsible Disclosure Steps:

1. **Contact us directly** via email at [YOUR-EMAIL]
2. **Provide detailed information** about the vulnerability
3. **Include steps to reproduce** the issue
4. **Allow reasonable time** for investigation and fix before public disclosure

## Security Considerations

### Administrator Privileges
This application requires and requests administrator privileges for:
- Registry modifications
- System wallpaper changes
- Explorer process management

### Registry Modifications
The application modifies the following registry keys:
```
HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System
HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\ActiveDesktop
HKEY_CURRENT_USER\Control Panel\Desktop
```

> [!IMPORTANT]
> All registry changes are limited to **HKEY_CURRENT_USER** and do not affect other users or system-wide settings.

### Data Collection
This application **does not**:
- Collect any personal information
- Send data over the internet
- Track user behavior
- Include telemetry or analytics
- Store credentials or sensitive data

### File System Access
The application creates/stores files only at:
- `C:\Windows\Web\Wallpaper\School\default.jpg` - Protected wallpaper
- `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\WallpaperProtector.lnk` - Auto-start shortcut (optional)

### Windows API Usage
The application uses the following Windows APIs via P/Invoke:
- `SystemParametersInfo` - For wallpaper changes
- `GetConsoleWindow` / `ShowWindow` - For console window management

## Security Recommendations for Users

### Before Installation
1. **Verify the source** - Download only from official repository
2. **Scan with antivirus** - Always scan executables before running
3. **Review the code** - Source code is available for inspection
4. **Create system restore point** - For safety

### During Usage
1. **Run only as needed** - Don't keep running if not required
2. **Monitor behavior** - Watch for any unexpected system changes
3. **Use Unlock feature** - Always use the built-in Unlock to reverse changes
4. **Keep backups** - Maintain regular system backups

## Potential Risks

| Risk | Description | Mitigation |
|------|-------------|------------|
| Registry corruption | Improper registry modifications | All changes are limited and reversible via Unlock option |
| Explorer crashes | Explorer restart may cause temporary instability | Restart is controlled and monitored |
| File permission issues | Access to system directories | Uses standard Windows APIs with proper error handling |
| Auto-start conflicts | May conflict with other startup programs | Optional feature, can be disabled |

## Safe Usage Guidelines

### ✅ DO:
- Run with administrator privileges only when necessary
- Use the Unlock feature before uninstalling
- Test in a virtual machine first
- Keep the original executable for safe removal

### ❌ DON'T:
- Modify the registry manually while program is running
- Delete the protected wallpaper file directly
- Force-kill the process without using Unlock
- Install on systems without proper authorization

## Compliance

This application is designed to comply with:
- **Windows Security Policies** - Uses only documented Windows APIs
- **User Account Control (UAC)** - Properly requests elevation when needed
- **GDPR** - No personal data collection
- **Corporate Policies** - Reversible changes with user consent

## Verification

To verify the integrity of the application:
1. **Check digital signature** (if signed)
2. **Compare checksums** with official releases
3. **Review source code** for any modifications
4. **Test in isolated environment** first

## Third-Party Dependencies

| Package | Version | Security Notes |
|---------|---------|----------------|
| System.Drawing.Common | 8.0.0 | Securely handles image processing |
| .NET Runtime | 8.0 | Microsoft-supported, regular security updates |

## Updates and Patches

Security updates will be provided through:
- GitHub releases
- Repository announcements
- Direct communication for critical issues

## Contact

For security concerns, reach out through:
- **GitHub Issues** (for non-sensitive topics)
- **Email**: [YOUR-EMAIL]
- **Discussions**: GitHub Discussions tab

---

> [!NOTE]
> This security policy is subject to change. Users are encouraged to review it periodically for updates.

---

**Last Updated**: February 2026
**Version**: 1.0.0