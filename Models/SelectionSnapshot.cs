using System;
using System.Collections.Generic;

namespace SelectionAggregate.Models
{
    public class SelectionSnapshot
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<long> ElementIds { get; set; } = new List<long>();
    }
}
