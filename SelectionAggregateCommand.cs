using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace SelectionAggregate
{
    [Transaction(TransactionMode.ReadOnly)]
    public class SelectionAggregateCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            var window = new SelectionAggregateWindow(uidoc);
            window.ShowDialog();

            return Result.Succeeded;
        }
    }
}