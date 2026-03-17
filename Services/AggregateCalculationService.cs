using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SelectionAggregate.Services
{
    public class AggregateCalculationService
    {
        public string Calculate(
            List<Element> elements,
            Units units,
            string parameterName,
            string operation)
        {
            var values = new List<double>();
            
            Parameter firstParam = elements
                .Select(e => FindParameterByName(e, parameterName))
                .FirstOrDefault(p => p != null);
            
            if (firstParam == null)
                throw new InvalidOperationException("Parameter not found.");

            ForgeTypeId specTypeId = firstParam.Definition.GetDataType();

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

            double result = operation switch
            {
                "Sum" => values.Sum(),
                "Average" => values.Average(),
                "Min" => values.Min(),
                "Max" => values.Max(),
                _ => throw new NotSupportedException($"Unsupported operation: {operation}")
            };

            string formattedResult = UnitFormatUtils.Format(units, specTypeId, result, false);

            return formattedResult;
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