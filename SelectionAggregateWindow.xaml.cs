using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.VisualBasic;
using SelectionAggregate.Models;
using SelectionAggregate.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;


namespace SelectionAggregate
{
    public partial class SelectionAggregateWindow : Window
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;
        private readonly SelectionAnalysisService _selectionService;
        private List<Element> _selectedElements = new List<Element>();

        private readonly AggregateCalculationService _calculationService = new AggregateCalculationService();

        private readonly SelectionUndoManager _undoManager = new SelectionUndoManager();
        private readonly ObservableCollection<SavedResult> _savedResults = new ObservableCollection<SavedResult>();

        private SavedResult _selectedSavedResult;
        public SelectionAggregateWindow(UIDocument uidoc)
        {
            InitializeComponent();

            _uidoc = uidoc;
            _doc = uidoc.Document;
            _selectionService = new SelectionAnalysisService(_uidoc, _doc);

            SavedResultsListBox.ItemsSource = _savedResults;

            LoadFromLocal();
            LoadSelectionData();
            UpdateUndoButtonState();
        }

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
                OperationComboBox.SelectedIndex = 0;
                FilterButton.IsEnabled = true;
                ParameterComboBox.IsEnabled = true;
                OperationComboBox.IsEnabled = true;
                CalculateButton.IsEnabled = true;
                SaveResultButton.IsEnabled = false;
                ResultTextBlock.Text = "Ready to calculate.";
            }
            else
            {
                OperationComboBox.IsEnabled = false;
                FilterButton.IsEnabled = false;
                ParameterComboBox.IsEnabled = false;
                OperationComboBox.IsEnabled = false;
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

            if (OperationComboBox.SelectedItem is not System.Windows.Controls.ComboBoxItem selectedCalcItem)
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
            filterWindow.Owner = this;
            filterWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            bool? dialogResult = filterWindow.ShowDialog();

            if (dialogResult != true || filterWindow.ResultRule == null)
            {
                return;
            }
                

            try
            {
                PushCurrentSelectionToUndo();
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


        private string GenerateNextSavedResultTitle()
        {
            int index = 1;

            while (true)
            {
                string candidate = $"Result{index}";
                bool exists = _savedResults.Any(x =>
                    string.Equals(x.Title, candidate, System.StringComparison.OrdinalIgnoreCase));

                if (!exists)
                    return candidate;

                index++;
            }
        }

        private void SaveResultButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedElements == null || !_selectedElements.Any())
                return;

            var savedResult = new SavedResult
            {
                Title = GenerateNextSavedResultTitle(),
                SelectionSummaryText = _selectionService.GetSelectionSummary(_selectedElements).Description,
                ElementIds = _selectedElements.Select(x => x.Id.Value).ToList(),
                AggregateParameter = (ParameterComboBox.SelectedItem as ParameterOption)?.InternalName ?? string.Empty,
                AggregateOperation = (OperationComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty,
                AggregateValueText = ResultTextBlock.Text ?? string.Empty,
            };

            _savedResults.Insert(0, savedResult);
            SaveToLocal();
        }

        private void SavedResultsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedSavedResult = SavedResultsListBox.SelectedItem as SavedResult;
        }


        private void RenameSavedResultMenuItem_Click(object sender, RoutedEventArgs e)
        {

            var savedResult = _selectedSavedResult;

            if (savedResult == null) return;




            string newTitle = Interaction.InputBox(
                "Enter a new title",
                "Rename Saved Result",
                savedResult.Title);

            if (string.IsNullOrWhiteSpace(newTitle))
                return;

            newTitle = newTitle.Trim();

            if (_savedResults.Any(x => x != savedResult &&
                string.Equals(x.Title, newTitle, System.StringComparison.OrdinalIgnoreCase)))
            {
                System.Windows.MessageBox.Show(
                    "The title already exists.",
                    "Rename Saved Result",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
                return;
            }

            savedResult.Title = newTitle;
            RefreshSavedResultsList();
            SaveToLocal();
        }


        private void RefreshSavedResultsList()
        {
            var items = _savedResults.ToList();
            _savedResults.Clear();

            foreach (var item in items)
            {
                _savedResults.Add(item);
            }
        }

        private void SelectSavedResultElementsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var savedResult = _selectedSavedResult;
            if (savedResult == null) return;

            var allIds = savedResult.ElementIds
                .Select(x => new ElementId(x))
                .ToList();

            var validIds = allIds
                .Where(id => _doc.GetElement(id) != null)
                .ToList();

            if (validIds.Count != allIds.Count)
            {
                System.Windows.MessageBox.Show(
                    $"Cannot select all elements.\n\nThis record has {allIds.Count} elements, but only {validIds.Count} exist in the project.\n\nRight-click and choose \"Update Element Selection\" to sync the record.",
                    "Saved Result",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                savedResult.AggregateValueText = "(outdated)";
                RefreshSavedResultsList();
                return;
            }

            PushCurrentSelectionToUndo();

            _uidoc.Selection.SetElementIds(validIds);
            LoadSelectionData();
        }


        private void UpdateSavedResultElementsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var savedResult = _selectedSavedResult;
            if (savedResult == null) return;

            var allIds = savedResult.ElementIds
                .Select(x => new ElementId(x))
                .ToList();

            var validIds = allIds
                .Where(id => _doc.GetElement(id) != null)
                .Select(id => id.Value)
                .ToList();

            int removedCount = allIds.Count - validIds.Count;

            if (removedCount == 0)
            {
                System.Windows.MessageBox.Show(
                    "No update is needed.",
                    "Update Element Selection",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
                return;
            }

            var elements = GetElementsFromSavedResult(savedResult);
            savedResult.ElementIds = validIds;
            savedResult.SelectionSummaryText = _selectionService.GetSelectionSummary(elements).Description;
            savedResult.AggregateValueText = RecalculateSavedResult(_selectedSavedResult); 

            RefreshSavedResultsList();

            System.Windows.MessageBox.Show(
                $"Updated successfully.\n\nRemoved {removedCount} missing element(s).\n{validIds.Count} element(s) remain in this record.",
                "Update Element Selection",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
            SaveToLocal();
        }

        private void DeleteSavedResultMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var savedResult = _selectedSavedResult;
            if (savedResult == null) return;

            _savedResults.Remove(savedResult);
            SaveToLocal();
        }

        private void PushCurrentSelectionToUndo()
        {
            var currentIds = _uidoc.Selection.GetElementIds()
                .Select(x => x.Value)
                .ToList();

            if (currentIds.Any())
            {
                _undoManager.Push(currentIds);
                UpdateUndoButtonState();
            }
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            var snapshot = _undoManager.Pop();
            if (snapshot == null)
                return;

            var validIds = snapshot.ElementIds
                .Select(x => new ElementId(x))
                .Where(id => _doc.GetElement(id) != null)
                .ToList();

            if (!validIds.Any())
            {
                System.Windows.MessageBox.Show(
                    "The previous selection is no longer available in the project.",
                    "Undo Selection",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
                UpdateUndoButtonState();
                return;
            }

            _uidoc.Selection.SetElementIds(validIds);
            LoadSelectionData();
            UpdateUndoButtonState();
        }

        private void UpdateUndoButtonState()
        {
            if (UndoButton != null)
                UndoButton.IsEnabled = _undoManager.CanUndo;
        }

        private void SavedResultBorder_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item)
            {
                
                item.IsSelected = true;
                item.Focus();
                _selectedSavedResult = item.DataContext as SavedResult;
            }
        }

        private void SaveToLocal()
        {
            try
            {
                var folder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "SelectionAggregate");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var file = Path.Combine(folder, "saved_results.json");

                var store = new SavedResultStore
                {
                    Version = 1,
                    Items = _savedResults.ToList()
                };

                var json = JsonSerializer.Serialize(
                    store,
                    new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(file, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save results.\n\n{ex.Message}",
                    "Saved Results",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        private void LoadFromLocal()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SelectionAggregate");

            var file = Path.Combine(folder, "saved_results.json");

            if (!File.Exists(file))
                return;

            try
            {
                var json = File.ReadAllText(file);

                var store = JsonSerializer.Deserialize<SavedResultStore>(json);

                if (store == null || store.Version != 1 || store.Items == null)
                    throw new InvalidOperationException("Unsupported saved result file format.");

                _savedResults.Clear();

                foreach (var item in store.Items)
                {
                    _savedResults.Add(item);
                }
            }
            catch
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // ignore delete failure
                }

                _savedResults.Clear();

                MessageBox.Show(
                    "The saved results file is from an unsupported format and has been reset.",
                    "Saved Results",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private string RecalculateSavedResult(SavedResult result)
        {


            var elements = GetElementsFromSavedResult(result);
            if (elements == null || elements.Count == 0)
                return "(no valid elements)";
            string value = _calculationService.Calculate(
                elements,
                _doc.GetUnits(),
                result.AggregateParameter,
                result.AggregateOperation
            );

            return value;
        }

        private List<Element> GetElementsFromSavedResult(SavedResult result)
        {
            var elements = result.ElementIds.Select(x => new ElementId(x)).Where(id => _doc.GetElement(id) != null).Select(id => _doc.GetElement(id)).ToList();
            return elements;
        }
    }
}