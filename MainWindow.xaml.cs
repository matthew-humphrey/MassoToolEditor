using MassoToolEditor.Models;
using MassoToolEditor.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MassoToolEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {    private ObservableCollection<ToolRecord> _toolRecords;
        private string? _currentFilePath;
        private bool _isModified;
        private Units _currentUnits = Units.Millimeters;
          public MainWindow()
        {
            try
            {
                InitializeComponent();
                
                _toolRecords = new ObservableCollection<ToolRecord>();
                ToolGrid.ItemsSource = _toolRecords;
                
                // Set up radio button event handlers after initialization
                RadioMm.Checked += RadioUnits_Checked;
                RadioInch.Checked += RadioUnits_Checked;
                
                // Set up keyboard shortcuts
                CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, (s, e) => MenuOpen_Click(s, null)));
                CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, (s, e) => MenuSave_Click(s, null)));
                CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, (s, e) => MenuSaveAs_Click(s, null)));
                
                InputBindings.Add(new KeyBinding(ApplicationCommands.Open, Key.O, ModifierKeys.Control));
                InputBindings.Add(new KeyBinding(ApplicationCommands.Save, Key.S, ModifierKeys.Control));
                InputBindings.Add(new KeyBinding(ApplicationCommands.SaveAs, Key.S, ModifierKeys.Control | ModifierKeys.Shift));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing MainWindow:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                               "MainWindow Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void MenuOpen_Click(object? sender, RoutedEventArgs? e)
        {
            if (_isModified && !PromptSaveChanges())
                return;

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Masso Tool Files (*.htg)|*.htg|All Files (*.*)|*.*",
                Title = "Open Masso Tool File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    LoadFile(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading file: {ex.Message}", "Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    DisableAllFunctionality();
                }
            }
        }

        private void LoadFile(string filePath)
        {
            StatusText.Text = "Loading file...";
            
            var records = HtgFileService.LoadFromFile(filePath);
            
            _toolRecords.Clear();
            foreach (var record in records)
            {
                // Convert from mm to current units if needed
                if (_currentUnits == Units.Inches)
                {
                    if (record.ToolNumber > 0) // Don't convert record 0
                    {
                        record.ZOffset = UnitConverter.ConvertValue(record.ZOffset, Units.Millimeters, Units.Inches);
                        record.ToolDiameter = UnitConverter.ConvertValue(record.ToolDiameter, Units.Millimeters, Units.Inches);
                        record.ToolDiaWear = UnitConverter.ConvertValue(record.ToolDiaWear, Units.Millimeters, Units.Inches);
                    }
                }
                
                record.PropertyChanged += Record_PropertyChanged;
                _toolRecords.Add(record);
            }

            _currentFilePath = filePath;
            _isModified = false;
            EnableAllFunctionality();
            UpdateTitle();
            UpdateStatusBar();
            
            StatusText.Text = $"Loaded {records.Count} tool records";
        }

        private void Record_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is ToolRecord record && record.ToolNumber > 0) // Don't mark as modified for record 0
            {
                _isModified = true;
                UpdateTitle();
                
                // Recalculate CRC when record is modified
                UpdateRecordCrc(record);
            }
        }        private void UpdateRecordCrc(ToolRecord record)
        {
            // Record 0 should always have CRC = 0, don't recalculate it
            if (record.ToolNumber == 0)
            {
                record.CRC = 0;
                return;
            }
            
            // For records 1-104, mark as needing recalculation (will be done when saving)
            record.CRC = 0; // Will be recalculated when saving
        }

        private void MenuSave_Click(object? sender, RoutedEventArgs? e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                MenuSaveAs_Click(sender, e);
                return;
            }

            SaveFile(_currentFilePath);
        }

        private void MenuSaveAs_Click(object? sender, RoutedEventArgs? e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Masso Tool Files (*.htg)|*.htg|All Files (*.*)|*.*",
                Title = "Save Masso Tool File",
                FileName = "MASSO_5-Axis_Tools.htg"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveFile(saveFileDialog.FileName);
            }
        }

        private void SaveFile(string filePath)
        {
            try
            {
                StatusText.Text = "Saving file...";
                
                // Convert records to mm for saving if currently in inches
                var recordsToSave = new List<ToolRecord>();
                foreach (var record in _toolRecords)
                {
                    var recordCopy = new ToolRecord
                    {
                        ToolNumber = record.ToolNumber,
                        ToolName = record.ToolName,
                        ZOffset = record.ZOffset,
                        ToolDiameter = record.ToolDiameter,
                        ToolDiaWear = record.ToolDiaWear,
                        CRC = record.CRC
                    };
                    
                    if (_currentUnits == Units.Inches && record.ToolNumber > 0)
                    {
                        recordCopy.ZOffset = UnitConverter.ConvertValue(record.ZOffset, Units.Inches, Units.Millimeters);
                        recordCopy.ToolDiameter = UnitConverter.ConvertValue(record.ToolDiameter, Units.Inches, Units.Millimeters);
                        recordCopy.ToolDiaWear = UnitConverter.ConvertValue(record.ToolDiaWear, Units.Inches, Units.Millimeters);
                    }
                    
                    recordsToSave.Add(recordCopy);
                }
                
                HtgFileService.SaveToFile(filePath, recordsToSave);
                
                _currentFilePath = filePath;
                _isModified = false;
                UpdateTitle();
                UpdateStatusBar();
                
                StatusText.Text = "File saved successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuImportCsv_Click(object? sender, RoutedEventArgs? e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Import CSV File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var importedRecords = CsvService.ImportFromCsv(openFileDialog.FileName, _currentUnits);
                    
                    // Update existing records with imported data
                    foreach (var importedRecord in importedRecords)
                    {
                        var existingRecord = _toolRecords.FirstOrDefault(r => r.ToolNumber == importedRecord.ToolNumber);
                        if (existingRecord != null && existingRecord.ToolNumber > 0) // Don't update record 0
                        {
                            existingRecord.ToolName = importedRecord.ToolName;
                            existingRecord.ToolDiameter = importedRecord.ToolDiameter;
                            existingRecord.ToolDiaWear = importedRecord.ToolDiaWear;
                            // Keep existing Z offset, don't overwrite from CSV
                        }
                    }
                    
                    StatusText.Text = $"Imported {importedRecords.Count} tool records from CSV";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing CSV: {ex.Message}", "Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuExportCsv_Click(object? sender, RoutedEventArgs? e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                Title = "Export CSV File",
                FileName = "masso_tools.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    CsvService.ExportToCsv(saveFileDialog.FileName, _toolRecords.ToList(), _currentUnits);
                    StatusText.Text = "Tools exported to CSV successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting CSV: {ex.Message}", "Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuClear_Click(object? sender, RoutedEventArgs? e)
        {
            var result = MessageBox.Show("This will clear all tool records 1-104. Are you sure?", 
                                       "Confirm Clear", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                for (int i = 1; i < _toolRecords.Count; i++)
                {
                    var record = _toolRecords[i];
                    record.ToolName = string.Empty;
                    record.ZOffset = 0;
                    record.ToolDiameter = 0;
                    record.ToolDiaWear = 0;
                }
                
                StatusText.Text = "Cleared tool records 1-104";
            }
        }

        private void MenuExit_Click(object? sender, RoutedEventArgs? e)
        {
            Close();
        }

        private void MenuAbout_Click(object? sender, RoutedEventArgs? e)
        {
            MessageBox.Show("Masso Tool Editor v1.0\n\n" +
                          "A Windows WPF application for editing Masso 5-Axis tool files (.htg format).\n\n" +
                          "MIT License\n\n" +
                          "DISCLAIMER: Please back up your Masso settings before using this tool. " +
                          "You assume all risk if this application causes any problems with your Masso controller.",
                          "About Masso Tool Editor", MessageBoxButton.OK, MessageBoxImage.Information);
        }        private void RadioUnits_Checked(object? sender, RoutedEventArgs? e)
        {
            if (_toolRecords == null || _toolRecords.Count == 0)
                return;

            var newUnits = RadioMm.IsChecked == true ? Units.Millimeters : Units.Inches;
            
            if (newUnits != _currentUnits)
            {
                // Convert all values except record 0
                for (int i = 1; i < _toolRecords.Count; i++)
                {
                    var record = _toolRecords[i];
                    record.ZOffset = UnitConverter.ConvertValue(record.ZOffset, _currentUnits, newUnits);
                    record.ToolDiameter = UnitConverter.ConvertValue(record.ToolDiameter, _currentUnits, newUnits);
                    record.ToolDiaWear = UnitConverter.ConvertValue(record.ToolDiaWear, _currentUnits, newUnits);
                }
                
                _currentUnits = newUnits;
                UpdateColumnHeaders();
                StatusText.Text = $"Units changed to {(_currentUnits == Units.Millimeters ? "millimeters" : "inches")}";
            }
        }

        private void UpdateColumnHeaders()
        {
            string unit = UnitConverter.GetUnitAbbreviation(_currentUnits);
            
            if (ToolGrid.Columns.Count >= 6)
            {
                ((DataGridTextColumn)ToolGrid.Columns[2]).Header = $"Z Offset ({unit})";
                ((DataGridTextColumn)ToolGrid.Columns[3]).Header = $"Tool Diameter ({unit})";
                ((DataGridTextColumn)ToolGrid.Columns[4]).Header = $"Tool Dia Wear ({unit})";
            }
        }

        private void ToolGrid_BeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Item is ToolRecord record && record.IsReadOnly)
            {
                e.Cancel = true;
                StatusText.Text = "Record 0 is read-only and cannot be modified";
            }
        }

        private void ToolGrid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                StatusText.Text = "Record modified";
            }
        }

        private void EnableAllFunctionality()
        {
            MenuSave.IsEnabled = true;
            MenuSaveAs.IsEnabled = true;
            MenuImportCsv.IsEnabled = true;
            MenuExportCsv.IsEnabled = true;
            MenuClear.IsEnabled = true;
            
            BtnSave.IsEnabled = true;
            BtnImportCsv.IsEnabled = true;
            BtnExportCsv.IsEnabled = true;
            
            RadioMm.IsEnabled = true;
            RadioInch.IsEnabled = true;
            
            MainToolbar.IsEnabled = true;
        }

        private void DisableAllFunctionality()
        {
            MenuSave.IsEnabled = false;
            MenuSaveAs.IsEnabled = false;
            MenuImportCsv.IsEnabled = false;
            MenuExportCsv.IsEnabled = false;
            MenuClear.IsEnabled = false;
            
            BtnSave.IsEnabled = false;
            BtnImportCsv.IsEnabled = false;
            BtnExportCsv.IsEnabled = false;
            
            RadioMm.IsEnabled = false;
            RadioInch.IsEnabled = false;
            
            MainToolbar.IsEnabled = false;
            
            _toolRecords.Clear();
            _currentFilePath = null;
            _isModified = false;
            UpdateTitle();
            UpdateStatusBar();
        }

        private void UpdateTitle()
        {
            string title = "Masso Tool Editor";
            if (!string.IsNullOrEmpty(_currentFilePath))
            {
                title += $" - {Path.GetFileName(_currentFilePath)}";
                if (_isModified)
                    title += "*";
            }
            Title = title;
        }

        private void UpdateStatusBar()
        {
            FilePathText.Text = _currentFilePath ?? string.Empty;
        }

        private bool PromptSaveChanges()
        {
            if (!_isModified)
                return true;

            var result = MessageBox.Show("You have unsaved changes. Do you want to save them?", 
                                       "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            
            switch (result)
            {
                case MessageBoxResult.Yes:
                    MenuSave_Click(null, null);
                    return !_isModified; // Return true if save was successful
                case MessageBoxResult.No:
                    return true;
                case MessageBoxResult.Cancel:
                default:
                    return false;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!PromptSaveChanges())
            {
                e.Cancel = true;
                return;
            }
            
            base.OnClosing(e);
        }
    }
}
