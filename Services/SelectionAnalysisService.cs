using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SelectionAggregate.Models;
using System.Collections.Generic;
using System.Linq;

namespace SelectionAggregate.Services
{
    public class SelectionAnalysisService
    {
        private readonly UIDocument _uidoc;
        private readonly Document _doc;

        public SelectionAnalysisService(UIDocument uidoc)
        {
            _uidoc = uidoc;
            _doc = uidoc.Document;
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
                    DisplayName = k
                })
                .OrderBy(x => x.DisplayName)
                .ToList();
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
    }
}