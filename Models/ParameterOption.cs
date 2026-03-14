using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionAggregate.Models
{
    public class ParameterOption
    {
        public string DisplayName { get; set; } = "";
        public string InternalName { get; set; } = "";

        public override string ToString()
        {
            return DisplayName;
        }
    }
}