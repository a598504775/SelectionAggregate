using System;
using System.Collections.Generic;


namespace SelectionAggregate.Models
{
    public class SavedResult
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Title { get; set; } = string.Empty;

        public List<long> ElementIds { get; set; } = new List<long>();

        public string AggregateParameter { get; set; } = string.Empty;

        public string AggregateOperation { get; set; } = string.Empty;

        public string AggregateValueText { get; set; } = string.Empty;

        // Don't need this
        public string FilterSummary { get; set; } = string.Empty;

        public int Count => ElementIds?.Count ?? 0;
    }
}
