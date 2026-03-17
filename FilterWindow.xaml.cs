using SelectionAggregate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SelectionAggregate
{
    /// <summary>
    /// Interaction logic for FilterWindow.xaml
    /// </summary>
    public partial class FilterWindow : Window
    {
        private List<ParameterOption> _parameters;

        public FilterRule ResultRule { get; private set; }
        public FilterWindow(List<ParameterOption> parameters)
        {
            InitializeComponent();
            _parameters = parameters;
            LoadFilterData();


        }

        private void LoadFilterData()
        {
            FilterParameterComboBox.ItemsSource = _parameters;
            FilterConditionComboBox.ItemsSource = Enum.GetValues(typeof(FilterCondition)).Cast<FilterCondition>();
            if (_parameters.Count > 0)
                FilterParameterComboBox.SelectedIndex = 0;

            FilterConditionComboBox.SelectedItem = FilterCondition.Equals;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilterParameterComboBox.SelectedItem is not ParameterOption parameter)
            {
                MessageBox.Show("Please select a parameter.");
                return;
            }

            if (FilterConditionComboBox.SelectedItem is not FilterCondition condition)
            {
                MessageBox.Show("Please select a rule.");
                return;
            }

            double? value = null;

            if (condition != FilterCondition.HasValue &&
                condition != FilterCondition.HasNoValue)
            {
                if (!double.TryParse(FilterValueTextBox.Text, out double parsed))
                {
                    MessageBox.Show("Please enter a valid number.");
                    return;
                }

                value = parsed;
            }

            ResultRule = new FilterRule
            {
                ParameterOption = parameter,
                Condition = condition,
                Value = value
            };

            DialogResult = true;
            Close();
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Review this method. Dealing with has value and has no value conditons
        private void FilterConditionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterConditionComboBox.SelectedItem is FilterCondition condition)
            {
                bool needsValue =
                    condition != FilterCondition.HasValue &&
                    condition != FilterCondition.HasNoValue;

                FilterValueTextBox.IsEnabled = needsValue;

                if (!needsValue)
                    FilterValueTextBox.Text = "";
            }
        }
    }

}
