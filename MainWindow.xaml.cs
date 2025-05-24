using MassoToolEditor.Models;
using MassoToolEditor.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
        private Units _currentUnits = Units.Millimeters;        public MainWindow()
        {
            try
            {
                InitializeComponent();
                
                _toolRecords = new ObservableCollection<ToolRecord>();
                ToolGrid.ItemsSource = _toolRecords;
                
                // Show unit selection dialog at startup
                ShowUnitSelectionDialog();
                
                // Set up keyboard shortcuts
                CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, (s, e) => BtnOpen_Click(s, null)));
                CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, (s, e) => BtnSave_Click(s, null)));
                CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, (s, e) => BtnSaveAs_Click(s, null)));
                
                InputBindings.Add(new KeyBinding(ApplicationCommands.Open, Key.O, ModifierKeys.Control));
                InputBindings.Add(new KeyBinding(ApplicationCommands.Save, Key.S, ModifierKeys.Control));
                InputBindings.Add(new KeyBinding(ApplicationCommands.SaveAs, Key.S, ModifierKeys.Control | ModifierKeys.Shift));
                
                // Update column headers with selected units
                UpdateColumnHeaders();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing MainWindow:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                               "MainWindow Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }        private void ShowUnitSelectionDialog()
        {
            var unitDialog = new UnitSelectionDialog();
            var result = unitDialog.ShowDialog();
            
            if (result == true)
            {
                _currentUnits = unitDialog.SelectedUnits;
            }
            else
            {
                // If user cancels, exit the application
                Application.Current.Shutdown();
            }
        }private void BtnOpen_Click(object? sender, RoutedEventArgs? e)
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
        }        private void LoadFile(string filePath)
        {
            StatusText.Text = "Loading file...";
            
            var records = HtgFileService.LoadFromFile(filePath);
            
            _toolRecords.Clear();
            foreach (var record in records)
            {
                // Convert from mm to current units if needed
                if (_currentUnits == Units.Inches)
                {
                    record.ZOffset = UnitConverter.ConvertValue(record.ZOffset, Units.Millimeters, Units.Inches);
                    record.ToolDiameter = UnitConverter.ConvertValue(record.ToolDiameter, Units.Millimeters, Units.Inches);
                    record.ToolDiaWear = UnitConverter.ConvertValue(record.ToolDiaWear, Units.Millimeters, Units.Inches);
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
        }        private void Record_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            _isModified = true;
            UpdateTitle();
        }

        private void BtnSave_Click(object? sender, RoutedEventArgs? e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                BtnSaveAs_Click(sender, e);
                return;
            }

            SaveFile(_currentFilePath);
        }

        private void BtnSaveAs_Click(object? sender, RoutedEventArgs? e)
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
                {                    var recordCopy = new ToolRecord
                    {
                        ToolNumber = record.ToolNumber,
                        ToolName = record.ToolName,
                        ZOffset = record.ZOffset,
                        ToolDiameter = record.ToolDiameter,
                        ToolDiaWear = record.ToolDiaWear
                    };
                      if (_currentUnits == Units.Inches)
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
        }        private void BtnImportCsv_Click(object? sender, RoutedEventArgs? e)
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
                        if (existingRecord != null)
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
        }        private void BtnExportCsv_Click(object? sender, RoutedEventArgs? e)
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
                    CsvService.ExportToCsv(saveFileDialog.FileName, _toolRecords.ToList(), _currentUnits, _currentUnits);
                    StatusText.Text = "Tools exported to CSV successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting CSV: {ex.Message}", "Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnClear_Click(object? sender, RoutedEventArgs? e)
        {
            var result = MessageBox.Show("This will clear all tool records. Are you sure?",
                                       "Confirm Clear", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var record in _toolRecords)
                {
                    record.ToolName = string.Empty;
                    record.ZOffset = 0;
                    record.ToolDiameter = 0;
                    record.ToolDiaWear = 0;
                }

                StatusText.Text = "Cleared tool records 1-104";
            }
        }

        private void BtnAbout_Click(object? sender, RoutedEventArgs? e)
        {
            MessageBox.Show("Masso Tool Editor v1.0\n\n" +
                          "A Windows application for editing Masso 5-Axis tool files (MASSO_5-Axis_Tools.htg).\n\n" +
                          "Please back up your Masso settings before using this tool. " +
                          "You assume all risk if this application causes any problems with your Masso controller.",
                          "About Masso Tool Editor", MessageBoxButton.OK, MessageBoxImage.Information);
        }        private void RadioUnits_Checked(object? sender, RoutedEventArgs? e)
        {
            // This method is no longer used - units are selected at startup
        }private void UpdateColumnHeaders()
        {
            string unit = UnitConverter.GetUnitAbbreviation(_currentUnits);
            
            if (ToolGrid.Columns.Count >= 5)
            {
                ((DataGridTextColumn)ToolGrid.Columns[2]).Header = $"Z Offset ({unit})";
                ((DataGridTextColumn)ToolGrid.Columns[3]).Header = $"Tool Diameter ({unit})";
                ((DataGridTextColumn)ToolGrid.Columns[4]).Header = $"Tool Dia Wear ({unit})";
            }
        }        private void ToolGrid_BeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
        {
            // All records are now editable since record 0 is not shown
        }

        private void ToolGrid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                StatusText.Text = "Record modified";
            }
        }        private void EnableAllFunctionality()
        {
            BtnSave.IsEnabled = true;
            BtnSaveAs.IsEnabled = true;
            BtnImportCsv.IsEnabled = true;
            BtnExportCsv.IsEnabled = true;
            BtnClear.IsEnabled = true;
            
            // Note: BtnOpen should always remain enabled
        }

        private void DisableAllFunctionality()
        {
            BtnSave.IsEnabled = false;
            BtnSaveAs.IsEnabled = false;
            BtnImportCsv.IsEnabled = false;
            BtnExportCsv.IsEnabled = false;
            BtnClear.IsEnabled = false;
            
            // Note: BtnOpen should always remain enabled
            
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
        }        private bool PromptSaveChanges()
        {
            if (!_isModified)
                return true;

            var result = MessageBox.Show("You have unsaved changes. Do you want to save them?", 
                                       "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            
            switch (result)
            {
                case MessageBoxResult.Yes:
                    BtnSave_Click(null, null);
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
