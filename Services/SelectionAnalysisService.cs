using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SelectionAggregate.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SelectionAggregate.Services
{
    public class SelectionAnalysisService
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;

        public SelectionAnalysisService(UIDocument uidoc, Document doc)
        {
            _uidoc = uidoc;
            _doc = doc;
        }

        public List<Element> GetSelectedElements()
        {
            return _uidoc.Selection
                .GetElementIds()
                .Select(id => _doc.GetElement(id))
                .Where(e => e != null)
                .ToList();
        }

        public SelectionSummary GetSelectionSummary(List<Element> elements)
        {
            int count = elements.Count;
            if (count == 0)
            {
                return new SelectionSummary
                {
                    Count = 0,
                    Description = "0 items selected"
                };
            }

            var categoryNames = elements
                .Select(e => e.Category?.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .ToList();

            if (categoryNames.Count == 1)
            {
                string categoryName = categoryNames[0];
                return new SelectionSummary
                {
                    Count = count,
                    Description = $"{count} {categoryName.ToLower()} selected"
                };
            }

            return new SelectionSummary
            {
                Count = count,
                Description = $"{count} items selected"
            };
        }

        public List<ParameterOption> GetCommonCalculableParameters(List<Element> elements)
        {
            if (elements == null || elements.Count == 0)
                return new List<ParameterOption>();

            var firstParams = elements[0].Parameters
                .Cast<Parameter>()
                .Where(IsCalculable)
                .Where(p => IsInTargetGroup(p, "Dimensions"))
                .ToDictionary(
                    p => GetParameterKey(p),
                    p => p,
                    System.StringComparer.OrdinalIgnoreCase);

            var commonKeys = new HashSet<string>(firstParams.Keys, System.StringComparer.OrdinalIgnoreCase);

            for (int i = 1; i < elements.Count; i++)
            {
                var currentKeys = elements[i].Parameters
                    .Cast<Parameter>()
                    .Where(IsCalculable)
                    .Where(p => IsInTargetGroup(p, "Dimensions"))
                    .Select(GetParameterKey)
                    .ToHashSet(System.StringComparer.OrdinalIgnoreCase);

                commonKeys.IntersectWith(currentKeys);
            }

            return commonKeys
                .Select(k => new ParameterOption
                {
                    InternalName = k,
                    DisplayName = k,
                    specTypeId = firstParams[k].Definition.GetDataType()
                })
                .OrderBy(x => x.DisplayName)
                .ToList();
        }

        public List<Element> ApplyFilter(List<Element> elements, Models.FilterRule rule)
        {
            if (elements == null)
                return new List<Element>();

            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            elements = elements.Where(e => PassesFilter(e, rule)).ToList();
            return elements;

        }

        private bool PassesFilter(Element element, Models.FilterRule rule)
        {
            Parameter param = null;
            param = FindParameterByName(element, rule.ParameterOption.DisplayName);
            if (param == null)
            {
                return false;
            }

            bool conditionMet = rule.Condition switch
            {
                FilterCondition.GreaterThan => GetNumericParameterValue(param) > ConvertFilterValueToInternalUnits(rule),
                FilterCondition.GreaterThanOrEqual => GetNumericParameterValue(param) >= ConvertFilterValueToInternalUnits(rule),
                FilterCondition.LessThan => GetNumericParameterValue(param) < ConvertFilterValueToInternalUnits(rule),
                FilterCondition.LessThanOrEqual => GetNumericParameterValue(param) <= ConvertFilterValueToInternalUnits(rule),
                FilterCondition.Equals => Math.Abs(GetNumericParameterValue(param) - ConvertFilterValueToInternalUnits(rule)) < 1e-6,
                FilterCondition.NotEquals => Math.Abs(GetNumericParameterValue(param) - ConvertFilterValueToInternalUnits(rule)) >= 1e-6,
                FilterCondition.HasValue => param.HasValue,
                FilterCondition.HasNoValue => !param.HasValue,
                _ => throw new InvalidOperationException("Unsupported filter condition")
            };


            return conditionMet;
        }

        private Parameter FindParameterByName(Element element, string parameterName)
        {
            foreach(Parameter p in element.Parameters)
            {
                if(p.Definition?.Name == parameterName)
                {
                    return p;
                }
            }
            return null;
        }

        private double ConvertFilterValueToInternalUnits(Models.FilterRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }
            if (rule.ParameterOption == null)
            {
                throw new InvalidOperationException("FilterRule is missing parameter options.");
            }
            if (!rule.Value.HasValue)
            {
                throw new InvalidOperationException("Filter rule is missing value");
            }

            ForgeTypeId specTypeId = rule.ParameterOption.specTypeId;
            double inputValue = rule.Value.Value;

            if (specTypeId == SpecTypeId.Number || specTypeId == SpecTypeId.Int.Integer)
                return inputValue;
            FormatOptions formatOptions = _doc.GetUnits().GetFormatOptions(specTypeId);
            ForgeTypeId unitTypeId = formatOptions.GetUnitTypeId();

            return UnitUtils.ConvertToInternalUnits(inputValue, unitTypeId);

        }
        private bool IsCalculable(Parameter p)
        {
            if (p == null) return false;

            return p.StorageType == StorageType.Double ||
                   p.StorageType == StorageType.Integer;
        }



        private string GetParameterKey(Parameter p)
        {
            if (p.Definition == null) return "";

            return p.Definition.Name ?? "";
        }

        private static bool IsInTargetGroup(Parameter parameter, string targetGroupLabel)
        {
            if (parameter?.Definition == null) return false;

            ForgeTypeId groupTypeId = parameter.Definition.GetGroupTypeId();
            string groupLabel = LabelUtils.GetLabelForGroup(groupTypeId);

            return string.Equals(groupLabel, targetGroupLabel, System.StringComparison.OrdinalIgnoreCase);
        }

        private double GetNumericParameterValue(Parameter param)
        {
            if (param.StorageType == StorageType.Double)
            {
                return param.AsDouble();
            }
            else if (param.StorageType == StorageType.Integer)
            {
                return param.AsInteger();
            }
            throw new InvalidOperationException("Parameter is not numeric");
        }
    }
}