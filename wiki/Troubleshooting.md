# Troubleshooting

Common issues and solutions for **Windows System Cleaner Pro**.

## 🚨 Installation Issues

### "This app can't run on your PC"
**Cause**: Incompatible Windows version or architecture
**Solutions**:
- ✅ Ensure you have Windows 10 (1903+) or Windows 11
- ✅ Verify 64-bit Windows (32-bit not supported)
- ✅ Check Windows version: Win+R → `winver`

### "Windows protected your PC" (SmartScreen)
**Cause**: Windows SmartScreen blocking unsigned executable
**Solutions**:
- ✅ Click "More info" → "Run anyway"
- ✅ Right-click installer → Properties → Unblock → OK
- ✅ Temporarily disable SmartScreen (not recommended)

### ".NET Runtime not found"
**Cause**: Missing .NET 8.0 runtime
**Solutions**:
- ✅ Installer should handle this automatically
- ✅ Manual download: [Microsoft .NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- ✅ Restart after installation

### "Access denied" during installation
**Cause**: Insufficient privileges
**Solutions**:
- ✅ Right-click installer → "Run as administrator"
- ✅ Disable antivirus temporarily during installation
- ✅ Ensure user account has admin rights

## ⚙️ Application Issues

### Application won't start
**Symptoms**: No window appears, immediate crash
**Solutions**:
- ✅ Run as administrator (essential for system cleaning)
- ✅ Check Windows Event Viewer for errors
- ✅ Verify .NET 8.0 is installed: `dotnet --version`
- ✅ Restart computer and try again

### Slow scanning/cleaning performance
**Symptoms**: Taking very long to scan or clean
**Causes & Solutions**:
- ✅ **First-time use**: Normal for systems with many files
- ✅ **Antivirus interference**: Temporarily disable real-time scanning
- ✅ **Network drives**: Disconnect mapped drives temporarily
- ✅ **Low disk space**: Ensure at least 10% free space

### Files not being deleted
**Symptoms**: Cleaning completes but files remain
**Solutions**:
- ✅ Close all applications before cleaning
- ✅ Restart computer to release file locks
- ✅ Run as administrator
- ✅ Check if antivirus is protecting files

### Settings not saving
**Symptoms**: Preferences reset after restart
**Solutions**:
- ✅ Ensure admin rights for registry access
- ✅ Check if `%LOCALAPPDATA%\WindowsCleanerPro` exists
- ✅ Verify folder permissions
- ✅ Run once as admin to create initial settings

## 📅 Scheduling Issues

### Scheduled cleaning not running
**Symptoms**: Automatic cleaning doesn't occur
**Solutions**:
- ✅ Check Windows Task Scheduler:
  - Win+R → `taskschd.msc`
  - Look for "WindowsCleanerPro_AutoClean"
- ✅ Verify computer is on at scheduled time
- ✅ Ensure user account has "Log on as batch job" rights
- ✅ Re-create schedule in application settings

### Schedule created but shows errors
**Symptoms**: Task exists but fails to run
**Solutions**:
- ✅ Check task "Last Run Result" in Task Scheduler
- ✅ Verify executable path is correct
- ✅ Ensure account has admin privileges
- ✅ Test manual run: Right-click task → Run

## 🎨 Interface Issues

### Theme not changing
**Symptoms**: Theme selection doesn't affect appearance
**Solutions**:
- ✅ Restart application after theme change
- ✅ Check Windows theme settings
- ✅ Clear application cache:
  - Delete `%LOCALAPPDATA%\WindowsCleanerPro\cache`
- ✅ Reset to default theme

### Window too small/large
**Symptoms**: Window doesn't fit screen properly
**Solutions**:
- ✅ Reset window settings:
  - Delete `%LOCALAPPDATA%\WindowsCleanerPro\window-state.json`
- ✅ Check display scaling settings
- ✅ Try different monitor if using multiple displays

### File Explorer tab not working
**Symptoms**: Cannot browse files or folders
**Solutions**:
- ✅ Run as administrator
- ✅ Check drive permissions
- ✅ Verify Windows Explorer is working normally
- ✅ Restart Windows Explorer process

## 🔒 Permission Issues

### "Access denied" errors
**Common Locations**:
- `C:\Windows\Temp`
- `C:\Windows\SoftwareDistribution`
- System registry keys

**Solutions**:
- ✅ Always run as administrator
- ✅ Check folder ownership and permissions
- ✅ Disable antivirus temporarily
- ✅ Use built-in Administrator account if needed

### Registry access denied
**Symptoms**: Cannot save settings or create scheduled tasks
**Solutions**:
- ✅ Run as administrator
- ✅ Check UAC settings (don't disable, but ensure prompts appear)
- ✅ Verify user is in Administrators group
- ✅ Check group policy restrictions

## 💾 Storage Issues

### Less space freed than expected
**Symptoms**: Actual free space less than reported
**Explanations**:
- ✅ **Normal**: File system overhead and compression differences
- ✅ **Recycle Bin**: Some files moved to Recycle Bin first
- ✅ **System Restore**: Windows may create restore points
- ✅ **Shadow Copies**: Volume Shadow Service snapshots

**Solutions**:
- ✅ Restart computer to see accurate space
- ✅ Run Windows Disk Cleanup for comparison
- ✅ Check Recycle Bin contents

### Cannot clean certain files
**Symptoms**: Some files skipped during cleaning
**Reasons**:
- ✅ **File in use**: Another process accessing file
- ✅ **System protection**: Windows protecting critical files
- ✅ **Permissions**: Insufficient access rights
- ✅ **Corrupted**: File system errors

**Solutions**:
- ✅ Close all applications
- ✅ Restart in Safe Mode for stubborn files
- ✅ Use built-in Disk Cleanup for comparison
- ✅ Check file properties and permissions

## 🔄 Update Issues

### Cannot check for updates
**Symptoms**: Update check fails or times out
**Solutions**:
- ✅ Check internet connection
- ✅ Verify firewall isn't blocking application
- ✅ Try manual download from GitHub releases
- ✅ Check proxy settings if applicable

### Update installation fails
**Symptoms**: New version won't install over existing
**Solutions**:
- ✅ Uninstall old version first
- ✅ Run installer as administrator
- ✅ Close application completely before updating
- ✅ Restart computer between uninstall and reinstall

## 🛡️ Security Software Conflicts

### Antivirus blocking application
**Symptoms**: App deleted, quarantined, or blocked
**Solutions**:
- ✅ Add to antivirus exclusions:
  - `C:\Program Files\Windows System Cleaner Pro\`
  - `%LOCALAPPDATA%\WindowsCleanerPro\`
- ✅ Submit false positive report to antivirus vendor
- ✅ Temporarily disable real-time protection during use

### Windows Defender warnings
**Symptoms**: SmartScreen or Defender alerts
**Solutions**:
- ✅ This is normal for new system optimization tools
- ✅ Application is safe and open source
- ✅ Add folder exclusions in Windows Security
- ✅ Download only from official GitHub releases

## 📊 Performance Issues

### High CPU usage during cleaning
**Symptoms**: Computer becomes slow during operation
**Explanations**:
- ✅ **Normal**: File scanning is CPU-intensive
- ✅ **Many files**: First-time scans process thousands of files
- ✅ **Antivirus**: Real-time scanning of accessed files

**Solutions**:
- ✅ Close other applications during cleaning
- ✅ Temporarily disable antivirus real-time scanning
- ✅ Run during off-hours or when not using computer
- ✅ Use Task Manager to monitor resource usage

### Memory usage increasing
**Symptoms**: RAM usage grows during operation
**Explanations**:
- ✅ **File caching**: Windows caches accessed files
- ✅ **Progress tracking**: Application stores scan results
- ✅ **Normal operation**: Will release memory when complete

**Solutions**:
- ✅ Ensure adequate RAM (4GB+ recommended)
- ✅ Close other memory-intensive applications
- ✅ Restart application if memory usage excessive
- ✅ Consider cleaning smaller sections at a time

## 🆘 Getting Additional Help

### Log Files
Application logs are stored in:
`%LOCALAPPDATA%\WindowsCleanerPro\logs\`

Include relevant log excerpts when reporting issues.

### System Information
When reporting issues, include:
- Windows version and build
- Application version
- Error messages (exact text)
- Steps to reproduce
- System specifications

### Reporting Issues
1. **Search existing issues**: [GitHub Issues](https://github.com/gabrielsk12/System-Cleaner/issues)
2. **Create new issue**: Include system information and logs
3. **Community discussion**: [GitHub Discussions](https://github.com/gabrielsk12/System-Cleaner/discussions)

### Emergency Recovery
If the application causes system issues:
1. **System Restore**: Restore to point before cleaning
2. **Safe Mode**: Boot into Safe Mode to troubleshoot
3. **Backup**: Restore files from backup if available
4. **Windows Recovery**: Use Windows built-in recovery tools

---

**Still having issues?** Check the [FAQ](FAQ) or [create an issue](https://github.com/gabrielsk12/System-Cleaner/issues/new) with detailed information.
