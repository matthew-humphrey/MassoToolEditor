using MassoToolEditor.Models;
using System.Windows;

namespace MassoToolEditor
{
    public partial class UnitSelectionDialog : Window
    {
        public Units SelectedUnits { get; private set; } = Units.Millimeters;

        public UnitSelectionDialog()
        {
            InitializeComponent();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            SelectedUnits = RadioMm.IsChecked == true ? Units.Millimeters : Units.Inches;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
