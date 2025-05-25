# MSIX Package Assets

For the MSIX package to build successfully, you need to create the following image files in this Assets folder:

## Required Images:

1. **StoreLogo.png** (50x50 pixels)
   - Used in the Windows Store and package manager

2. **Square44x44Logo.png** (44x44 pixels)
   - Small logo used in the taskbar and other small spaces

3. **Square150x150Logo.png** (150x150 pixels)
   - Medium logo used on the Start menu tile

4. **Wide310x150Logo.png** (310x150 pixels)
   - Wide logo for wide Start menu tiles

5. **SplashScreen.png** (620x300 pixels)
   - Shown when the app is launching

6. **htg-icon.png** (44x44 pixels)
   - Icon used for .htg file associations

## Creating the Images:

You can create these images from your existing `Masso_Tool_Editor.ico` file:

1. Extract the different sizes from the .ico file using an icon editor
2. Convert them to PNG format
3. Resize as needed to match the required dimensions above
4. Save them in this Assets folder with the exact names listed

## Quick Solution:

If you want to quickly test the MSIX build, you can:
1. Copy your `Masso_Tool_Editor.ico` file 6 times into this folder
2. Rename them to match the required PNG filenames above
3. The MSIX will build (though the icons won't display correctly in Windows)

For production use, proper PNG files with the correct dimensions are recommended.
