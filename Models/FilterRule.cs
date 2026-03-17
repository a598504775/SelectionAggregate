namespace SelectionAggregate.Models
{
    public class FilterRule
    {
        public ParameterOption ParameterOption { get; set; }
        public FilterCondition Condition { get; set; }
        public double? Value { get; set; }
    }
}