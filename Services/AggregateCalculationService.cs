using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SelectionAggregate.Services
{
    public class AggregateCalculationService
    {
        public double Calculate(
            List<Element> elements,
            string parameterName,
            string operation)
        {
            var values = new List<double>();

            foreach (var element in elements)
            {
                Parameter param = FindParameterByName(element, parameterName);
                if (param == null) continue;

                if (param.StorageType == StorageType.Double)
                {
                    values.Add(param.AsDouble());
                }
                else if (param.StorageType == StorageType.Integer)
                {
                    values.Add(param.AsInteger());
                }
            }

            if (values.Count == 0)
                throw new InvalidOperationException("No calculable values found.");

            return operation switch
            {
                "Sum" => values.Sum(),
                "Average" => values.Average(),
                "Min" => values.Min(),
                "Max" => values.Max(),
                _ => throw new NotSupportedException($"Unsupported operation: {operation}")
            };
        }

        private Parameter FindParameterByName(Element element, string parameterName)
        {
            foreach (Parameter p in element.Parameters)
            {
                if (p.Definition?.Name == parameterName)
                    return p;
            }
            return null;
        }
    }
}