using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SelectionAggregate.Models;
using SelectionAggregate.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

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

        private void LoadSelectionData()
        {
            _selectedElements = _selectionService.GetSelectedElements();

            var summary = _selectionService.GetSelectionSummary(_selectedElements);
            SelectionTextBlock.Text = summary.Description;

            var parameters = _selectionService.GetCommonCalculableParameters(_selectedElements);
            ParameterComboBox.ItemsSource = parameters;

            // Debug output for parameter groups
            Element et = _selectedElements[0];
            List<string> testList = GetParameterGroupDebugInfo(et);
            DebugComboBox.ItemsSource = testList;



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
                string result = _calculationService.Calculate(
                    _selectedElements,
                    _doc.GetUnits(),
                    selectedParam.InternalName,
                    operation);

                ResultTextBlock.Text = result;
            }
            catch (System.Exception ex)
            {
                ResultTextBlock.Text = ex.Message;
            }
        }

        private static List<string> GetParameterGroupDebugInfo(Element element)
        {
            var lines = new List<string>();

            foreach (Parameter p in element.Parameters)
            {
                if (p?.Definition == null) continue;

                ForgeTypeId groupTypeId = p.Definition.GetGroupTypeId();
                string groupLabel = LabelUtils.GetLabelForGroup(groupTypeId);

                lines.Add($"{p.Definition.Name} | Group = {groupLabel}");
            }

            return lines.OrderBy(x => x).ToList();
        }

    }
}