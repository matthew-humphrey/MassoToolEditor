# Masso Tool Editor

A Windows WPF application for editing Masso 5-Axis tool files (.htg format).

## ⚠️ IMPORTANT DISCLAIMER

**Please back up your Masso settings before using this tool. You assume all risk if this application causes any problems with your Masso controller.**

This software is provided as-is under the MIT License. The authors are not responsible for any damage to your Masso controller, CNC machine, or any other equipment that may result from using this software.

## Features

- **Grid-based editing**: View and edit tool records in an intuitive data grid
- **File operations**: Open, save, and save-as for .htg binary files
- **CSV import/export**: Import tool data from CSV files or export current data
- **Data validation**: Automatic CRC32 calculation and validation
- **Protected record 0**: Record 0 is read-only to prevent accidental modification
- **Unit conversion**: Switch between millimeters and inches with automatic conversion
- **Modern UI**: Clean WPF interface with menu bar and status updates

## Requirements

- Windows 10 or later
- .NET 6.0 Runtime
- Masso 5-Axis controller with .htg tool files

## Installation

1. Download the latest release from the releases page
2. Extract the files to a folder of your choice
3. Run `MassoToolEditor.exe`

## Usage

### Opening a File

1. Click **File → Open** or use the toolbar button
2. Select your `MASSO_5-Axis_Tools.htg` file
3. The application will validate the file and load all 105 tool records

### Choosing Units

- Use the radio buttons in the toolbar to select between **Millimeters** and **Inches**
- Values will be automatically converted when switching units
- The .htg file always stores values in millimeters internally

### Editing Tools

- Click on any cell in the grid to edit tool properties
- **Record 0 is read-only** and cannot be modified (highlighted in yellow)
- Records 1-104 can be freely edited
- CRC values are automatically recalculated when records are modified

### Clearing Records

- Use **Edit → Clear Records 1-104** to reset all tools except record 0
- This operation cannot be undone

### CSV Import/Export

#### Importing from CSV
1. Click **File → Import CSV**
2. Select a CSV file with the correct format
3. Tool records will be updated with the imported data

#### Exporting to CSV
1. Click **File → Export CSV**
2. Choose a location to save the CSV file
3. All tools (except record 0) will be exported

#### CSV Format

The CSV file should have the following columns:
```csv
Tool No.,Tool Name,Tool Diameter,Tool Dia Wear
1,End Mill 6mm,6.000,0.000
2,Drill 3mm,3.000,0.050
```

**Notes:**
- Tool numbers must be between 1-104
- Tool names are limited to 29 characters and will be truncated if longer
- Values should be in the units selected in the application
- CSV files with embedded quotes are supported (Excel-style escaping)

### Saving Changes

- Use **File → Save** (Ctrl+S) to save to the current file
- Use **File → Save As** (Ctrl+Shift+S) to save to a new location
- The application will prompt to save unsaved changes when closing

## File Format Details

The application works with binary files containing 105 tool records, each 64 bytes in size:

```
struct ToolRecord {
    char toolName[30];      // ASCII string, null-terminated
    uint16 unknown1;        // Always zero
    uint32 unknown2;        // Always zero
    uint32 unknown3;        // Always zero (partial)
    float zOffset;          // Z offset in millimeters
    uint32 unknown4;        // Always zero
    float toolDiaWear;      // Tool diameter wear in millimeters
    float toolDiameter;     // Tool diameter in millimeters
    uint32 unknown5;        // Always zero
    uint32 crc;             // CRC32 of preceding bytes
};
```

## Error Handling

The application includes comprehensive error handling for:
- Invalid file formats
- Incorrect file sizes (must be exactly 6,720 bytes)
- CRC validation failures
- Invalid unknown field values
- CSV parsing errors
- File I/O exceptions

If any validation errors occur when opening a file, the application will display an error message and disable all functionality except for loading another file.

## Building from Source

### Prerequisites
- Visual Studio 2022 or Visual Studio Code with C# extension
- .NET 6.0 SDK

### Build Steps
1. Clone the repository
2. Open the solution in Visual Studio or the folder in VS Code
3. Build the solution (Ctrl+Shift+B)
4. Run the application (F5)

## License

MIT License

Copyright (c) 2025

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## Support

For issues, feature requests, or questions, please use the GitHub issue tracker.

## Changelog

### Version 1.0.0
- Initial release
- Basic HTG file reading and writing
- Grid-based editing interface
- CSV import/export functionality
- Unit conversion (mm/inches)
- CRC32 validation
- Protected record 0
