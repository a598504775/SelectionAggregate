using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SelectionAggregate.Models;
using SelectionAggregate.Services;
using System.Collections.Generic;
using System.Windows;

namespace SelectionAggregate
{
    public partial class SelectionAggregateWindow : Window
    {
        private readonly UIDocument _uidoc;
        private readonly SelectionAnalysisService _selectionService;
        private List<Element> _selectedElements = new List<Element>();

        private readonly AggregateCalculationService _calculationService = new AggregateCalculationService();


        public SelectionAggregateWindow(UIDocument uidoc)
        {
            InitializeComponent();

            _uidoc = uidoc;
            _selectionService = new SelectionAnalysisService(uidoc);

            LoadSelectionData();
        }

        private void LoadSelectionData()
        {
            _selectedElements = _selectionService.GetSelectedElements();

            var summary = _selectionService.GetSelectionSummary(_selectedElements);
            SelectionTextBlock.Text = summary.Description;

            var parameters = _selectionService.GetCommonCalculableParameters(_selectedElements);
            ParameterComboBox.ItemsSource = parameters;

            if (parameters.Count > 0)
            {
                ParameterComboBox.SelectedIndex = 0;
                CalculationComboBox.SelectedIndex = 0;
                CalculationComboBox.IsEnabled = true;
                CalculateButton.IsEnabled = true;
                ResultTextBlock.Text = "Ready";
            }
            else
            {
                CalculationComboBox.IsEnabled = false;
                CalculateButton.IsEnabled = false;
                ResultTextBlock.Text = "Incalculable";
            }
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (ParameterComboBox.SelectedItem is not ParameterOption selectedParam)
            {
                ResultTextBlock.Text = "Please select a parameter.";
                return;
            }

            if (CalculationComboBox.SelectedItem is not System.Windows.Controls.ComboBoxItem selectedCalcItem)
            {
                ResultTextBlock.Text = "Please select a calculation method.";
                return;
            }

            string operation = selectedCalcItem.Content?.ToString() ?? "";

            try
            {
                double result = _calculationService.Calculate(
                    _selectedElements,
                    selectedParam.InternalName,
                    operation);

                ResultTextBlock.Text = result.ToString("F2");
            }
            catch (System.Exception ex)
            {
                ResultTextBlock.Text = ex.Message;
            }
        }

    }
}