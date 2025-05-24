# Masso Tool Editor

A Windows WPF application for editing Masso 5-Axis tool files (.htg format).

## Features

- **Grid-based editing**: View and edit tool records in an intuitive data grid
- **File operations**: Open, save, and save-as for .htg binary files
- **CSV import/export**: Import tool data from CSV files or export current data
- **Data validation**: Automatic CRC32 calculation and validation
- **Protected record 0**: Record 0 is read-only to prevent accidental modification
- **Modern UI**: Clean WPF interface with menu bar and status updates

## File Format

The application works with binary files containing 105 tool records, each 64 bytes in size:

```
struct ToolRecord {
    char toolName[30];      // ASCII string, null-terminated
    uint16 unknown1;        // Always zero
    uint32 unknown2;        // Always zero
    uint32 unknown3;        // Always zero
    float zOffset;          // Z offset in millimeters
    uint32 unknown4;        // Always zero
    float toolDiaWear;      // Tool diameter wear in millimeters
    float toolDiameter;     // Tool diameter in millimeters
    uint32 unknown5;        // Always zero
    uint32 crc;             // CRC32 of preceding bytes
};
```

## Usage

1. **Open a file**: Use File → Open to load an existing `MASSO_5-Axis_Tools.htg` file
2. **Choose Units**: A radio button allows the user to select the appropriate units (inches or millimeters).
3. **Edit tools**: Click on any cell in the grid to edit tool properties (except record 0)
4. **Clear**: Clears records 1-104.
5. **Import CSV**: Use File → Import CSV to batch update tools from a CSV file
6. **Export CSV**: Use File → Export CSV to save current tool data as CSV
7. **Save changes**: Use File → Save or Save As to write changes back to the binary file

### CSV Format

The CSV file should have the following columns:
- Tool No. (integer, 1-104)
- Tool Name (string, max 29 characters)
- Tool Diameter (float, millimeters)
- Tool Dia Wear (float, millimeters)

Example:
```csv
Tool No.,Tool Name,Tool Diameter,Tool Dia Wear
1,End Mill 6mm,6.000,0.000
2,Drill 3mm,3.000,0.050
```

## Error Handling 

The application includes comprehensive error handling for:
- Invalid file formats
- Incorrect file sizes
- CSV parsing errors
- File I/O exceptions
- Data validation failures

## Important Notes

- The user MUST start by importing an .htg file so that the contents of this record can be 
  determined. Until a valid .htg file has been loaded, all other functionality, including the associated
  UI elements, should be disabled.
- When opening an .htg file, as the application parses the records, it should check that the file is the 
  appropriate length (105 * 64 bytes), that the CRC values for each record are correct, and that the 
  unknown fields in all records except record 0 are set to 0. If any of these conditions are not met, 
  the application should display an error message, stop processing of the file, and disable all functionality
  except for loading another file.
- Record 0 should always be preserved, unmodified, as it was in the original .htg binary file
- With the exception of record 0, the `unknown*` fields should all be populated with zeroes.
- As records are edited in the grid or overwritten by a CSV import the CRC value for each record should
  be updated.
- The CSV file may have embedded quote characters. These must be handled appropriately. 
  The escaping of these characters follows the rules for Microsoft Excel. 
- If the input CSV file `Tool Name` field is too long, it should be truncated.
- The input and output .htg files will always have the 3 float values in millimeter units. If the user has
  selected inches, when opening the .htg file, the values should be converted from millimeters to inches
  and displayed in the grid as inches. When writing the .htg file back out, if the user has selected
  inches as the units, the values should be converted to millimeters in the output. CSV import values
  should always be assumed to be in whatever unit the user has selected. If the user changes the 
  unit value selection, any values in the grid should be converted (if necessary) to the appropriate
  unit. 
- Tool names use ASCII encoding with null termination
- All numeric values are stored in little-endian format.

## Technology
- .NET 9.0 or later
- Windows OS
- Visual Studio 2022 or Visual Studio Code with C# extension
- WPF

## Testing

Do not create any test file generation code. I will manually test it with files from my Masso controller.

## License

The application should use the MIT License. There should be a clear disclaimer somewhere
(the readme?) explaining that the user should back up their Masso settings before using
this, and that they assume all risk if it causes any problems with their Masso controller.
