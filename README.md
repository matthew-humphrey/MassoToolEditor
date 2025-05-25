# Masso Tool Editor

A Windows WPF application for editing Masso 5-Axis tool files (.htg format).

## ⚠️ IMPORTANT DISCLAIMER

**Please back up your Masso settings before using this tool. You assume all risk if this application causes any problems with your Masso controller.**

This software is provided as-is. The authors are not responsible for any damage to your Masso controller, CNC machine, or any other equipment that may result from using this software.

## Features

- **Grid-based editing**: View and edit tool records in an intuitive data grid
- **File operations**: Open, save, and save-as for .htg binary files
- **CSV import/export**: Import tool data from CSV files or export current data

## Requirements

- Windows 10 or later
- .NET 8.0 Runtime
- Masso 5-axis CNC mill controller (tested with a Masso G3)

## Installation

1. Download the latest release from the releases page
2. Extract the files to a folder of your choice
3. Run `MassoToolEditor.exe`

## Usage

### Saving the configuration from your Masso controller

1. In your Masso, with a USB flash drive loaded into the machine, go to the **F1 Setup** screen. If necessary, enter your password.
2. Double-tap the **Save & Load Settings** option on the left near the bottom
3. Press **Save to file** to save the configuration to the flash drive. You may also want to press **Save to printable file** so you have a human-readable record of the settings in the event of a problem.
4. Remove the flash drive, and bring it back to your computer.

### Opening a File to Edit the Tools

1. On your computer, insert the flash drive and examine its contents. On the drive, there should be a folder named `MASSO` and within that a folder named `Machine Settings`. *It is recommended that you back these files up somewhere in case you need to undo any changes.* 
2. Run the Masso Tool Editor application (`MassoToolEditor.exe`), and select the units in use on your machine. 
3. Click **File** button on the toolbar.
4. Select the `MASSO_5-Axis_Tools.htg` file from the flash drive
5. The application will validate the file contents and load all 104 tool records

### Editing Tools

- Click on any cell in the grid to edit tool properties. Note that you may have to click twice if you're just selecting that row, or if you're clicking away from another cell you were editing.
- To clear out a tool, just clear the tool name and set the numeric fields to zero.
- To clear all tools, press the **Clear** button. This operation cannot be undone

### Importing and Exporting CSV

- To export a CSV file that you can open in a spreadsheet program like Microsoft Excel, click the **Export CSV** button and enter the file name and location to save this file. You can then load it in your spreadsheet for editing.
- When importing, the file must be a .csv file with the same columns as you see in the exported file. Note that the import matches rows in the grid by using the `Tool No.` field, and if the .csv file
does not have a row for a particular tool number, it will be left alone during the import. If you want to start fresh and only have tools from your .csv, you can press the **Clear** button before 
importing.

Note that the `Tool No.` field controls which record you are editing, not the order of the rows in the spreadsheet. 
This allows you to do things like, for example, sort your tools by name in the spreadsheet without impacting the
order they will appear in the toold database.

- Tool numbers must be between 1-104
- Tool names are limited to 29 characters and will be truncated if longer
- Values should be in the units selected when you started the application
- CSV files with embedded quotes are supported (Excel-style escaping)

### Saving Changes

- Use the **Save** button to save the file.
- Use the **Save As** button if you want to save the file to a new location.
- The application will prompt to save unsaved changes when closing

### Loading your changes back into Masso

The updated file, `MASSO_5-Axis_Tools.htg` should be placed on the USB stick in the same folder (`MASSO\Machine Settings`)
where it was written by the Masso controller. In this directory you must also have the unmodified `MASSO_5-Axis_Settings.htg`
file created by the controller. If both files are not present, the controller will not load them.

1. Insert the flash drive with the modified tools file into the Masso controller.
2. Go to the **F1 Setup** screen. If necessary, enter your password.
3. Double-tap the **Save & Load Settings** option on the left near the bottom
4. Press **Load from file**. The Masso controller will load your settings, including your modified tool file, and prompt
to reboot. Let it reboot.
5. After reboot, go to **F4 TOOLS & OFFSET** screen and verify your updated tool configuration is present.
