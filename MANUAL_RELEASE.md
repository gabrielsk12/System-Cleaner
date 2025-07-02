# Manual Release Creation Instructions

Since GitHub CLI authentication is pending, here's how to manually create the release:

## 1. Go to GitHub Repository
Visit: https://github.com/gabrielsk12/System-Cleaner

## 2. Create New Release
1. Click "Releases" (on the right side)
2. Click "Create a new release"
3. Choose tag: v1.0.0
4. Release title: "Windows System Cleaner Pro v1.0.0 - Initial Release"

## 3. Upload Installer
1. Drag and drop: WindowsCleanerPro_Setup.exe (72MB)
2. Or click "Choose your files" and select the installer

## 4. Release Description
Copy the content from RELEASE_NOTES.md into the description box

## 5. Publish Release
1. Ensure "Set as the latest release" is checked
2. Click "Publish release"

## Files to Upload:
- WindowsCleanerPro_Setup.exe (72,579,590 bytes)

## Alternative: GitHub Actions
The workflow should automatically build and release when the v1.0.0 tag is pushed.
Check: https://github.com/gabrielsk12/System-Cleaner/actions
