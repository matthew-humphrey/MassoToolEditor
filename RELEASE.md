# Release Process for MassoToolEditor

This document describes how to create releases for the MassoToolEditor application.

## 🎯 Quick Start

To create a new release:

1. **Update the version** in `Directory.Build.props`:
   ```xml
   <MajorVersion>1</MajorVersion>
   <MinorVersion>1</MinorVersion>
   <PatchVersion>0</PatchVersion>
   ```

2. **Run the build script**:
   ```powershell
   .\build-release.ps1
   ```

3. **Upload to GitHub**:
   - Create a new release on GitHub
   - Upload the generated files from the `release/` folder
   - Use the generated release notes as a template

## 🧹 Cleaning Build Artifacts

To clean all build outputs and start fresh:

```powershell
.\clean-release.ps1                 # Clean everything
.\clean-release.ps1 -KeepRelease    # Clean but keep release folder
.\clean-release.ps1 -Verbose        # Show detailed cleaning output
```

The clean script removes:
- `dotnet clean` (bin, obj folders)
- `publish/` folder
- `release/` folder (unless `-KeepRelease` is used)
- Temporary files (*.tmp, *.bak, *~)

## 📁 What the Build Script Does

The `build-release.ps1` script automatically:

1. **Reads the current version** from `Directory.Build.props`
2. **Publishes the application** using `dotnet publish`
3. **Creates a release package** with all necessary files
4. **Generates checksums** (MD5, SHA256, SHA512)
5. **Creates release notes template** with version-specific information

## 📦 Generated Files

After running the script, you'll find these files in the `release/` folder:

- `MassoToolEditor-v{VERSION}-win-x64.zip` - The application package
- `MassoToolEditor-v{VERSION}-checksums.txt` - File verification hashes
- `release-notes-v{VERSION}.md` - GitHub release notes template

## 🔧 Version Management

All version numbers are centrally managed in `Directory.Build.props`:

- **Major Version**: Breaking changes or major new features
- **Minor Version**: New features, backward compatible
- **Patch Version**: Bug fixes and small improvements

The version is automatically applied to:
- Assembly metadata
- Application About dialog
- Release packages
- Release notes

## 🚀 GitHub Release Process

1. Go to your GitHub repository's "Releases" page
2. Click "Create a new release"
3. Choose a tag (e.g., `v1.1.0`)
4. Use the generated release notes as a starting point
5. Upload the three generated files:
   - The ZIP package
   - The checksums file
   - Include checksums in the release description for verification

## ✅ Testing Before Release

Always test the extracted application before releasing:

1. Extract the ZIP file to a test location
2. Run `MassoToolEditor.exe`
3. Verify the version shows correctly in the About dialog
4. Test basic functionality (open/save files, unit selection)

## 📋 Release Checklist

- [ ] Version updated in `Directory.Build.props`
- [ ] Build script ran successfully
- [ ] Application tested from extracted ZIP
- [ ] Release notes updated with actual changes
- [ ] Files uploaded to GitHub
- [ ] Release tagged and published

## 🔄 Automation Notes

The current process is semi-automated. Future improvements could include:
- GitHub Actions for automatic builds
- Automatic changelog generation
- Automated testing pipeline
