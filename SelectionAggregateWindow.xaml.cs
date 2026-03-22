using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SelectionAggregate.Models;
using SelectionAggregate.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SelectionAggregate
{
    public partial class SelectionAggregateWindow : Window
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;
        private readonly SelectionAnalysisService _selectionService;
        private List<Element> _selectedElements = new List<Element>();

        private readonly AggregateCalculationService _calculationService = new AggregateCalculationService();


        public SelectionAggregateWindow(UIDocument uidoc)
        {
            InitializeComponent();

            _uidoc = uidoc;
            _doc = uidoc.Document;
            _selectionService = new SelectionAnalysisService(_uidoc, _doc);

            LoadSelectionData();
        }

        // Break this method into two: ReloadSelectedElements() and RefreshMainUi()
        private void LoadSelectionData()
        {
            ReloadSelectedElements();
            RefreshMainUi();
        }

        private void ReloadSelectedElements()
        {
            _selectedElements = _selectionService.GetSelectedElements();
        }

        private void RefreshMainUi()
        {
            var summary = _selectionService.GetSelectionSummary(_selectedElements);
            SelectionTextBlock.Text = summary.Description;
            var parameters = _selectionService.GetCommonCalculableParameters(_selectedElements);
            ParameterComboBox.ItemsSource = parameters;
            if (parameters.Count > 0)
            {
                ParameterComboBox.SelectedIndex = 0;
                CalculationComboBox.SelectedIndex = 0;
                FilterButton.IsEnabled = true;
                ParameterComboBox.IsEnabled = true;
                CalculationComboBox.IsEnabled = true;
                CalculateButton.IsEnabled = true;
                SaveResultButton.IsEnabled = false;
                ResultTextBlock.Text = "Ready to calculate.";
            }
            else
            {
                CalculationComboBox.IsEnabled = false;
                FilterButton.IsEnabled = false;
                ParameterComboBox.IsEnabled = false;
                CalculationComboBox.IsEnabled = false;
                CalculateButton.IsEnabled = false;
                SaveResultButton.IsEnabled = false;
                if (_selectedElements.Count == 0)
                {
                    ResultTextBlock.Text = "Select elements to begin.";
                }
                else
                {
                    ResultTextBlock.Text = "No common calculable parameters were found.";
                }
                    
            }
        }



        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedElements == null || _selectedElements.Count == 0)
            {
                ResultTextBlock.Text = "No elements selected.";
                return;
            }

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
                string result = _calculationService.Calculate(
                    _selectedElements,
                    _doc.GetUnits(),
                    selectedParam.InternalName,
                    operation);

                ResultTextBlock.Text = result;
                SaveResultButton.IsEnabled = true;
            }
            catch (System.Exception ex)
            {
                ResultTextBlock.Text = ex.Message;
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            var parameters = _selectionService.GetCommonCalculableParameters(_selectedElements);

            if (parameters.Count == 0)
            {
                ResultTextBlock.Text = "No filterable parameters found.";
                return;
            }

            var filterWindow = new FilterWindow(parameters);

            bool? dialogResult = filterWindow.ShowDialog();

            if (dialogResult != true || filterWindow.ResultRule == null)
            {
                return;
            }
                

            try
            {
                var filteredElements = _selectionService.ApplyFilter(_selectedElements, filterWindow.ResultRule);

                if (filteredElements.Count == 0)
                {
                    _uidoc.Selection.SetElementIds(new List<ElementId>());
                    LoadSelectionData();
                    ResultTextBlock.Text = "No elements matched.";
                    return;
                }

                _uidoc.Selection.SetElementIds(filteredElements.Select(x => x.Id).ToList());
                LoadSelectionData();
                ResultTextBlock.Text = "Filter applied.";
            }
            catch (System.Exception ex)
            {
                ResultTextBlock.Text = ex.Message;
            }
        }
        private void InvalidateCalculatedResult()
        {
            SaveResultButton.IsEnabled = false;
        }
        private void SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            InvalidateCalculatedResult();
        }


    }
}