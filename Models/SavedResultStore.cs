using System.Collections.Generic;

namespace SelectionAggregate.Models
{
    public class SavedResultStore
    {
        public int Version { get; set; } = 1;
        public List<SavedResult> Items { get; set; } = new List<SavedResult>();
    }
}