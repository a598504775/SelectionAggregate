using Autodesk.Revit.UI;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace SelectionAggregate
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // const string tabName = "Add-Ins";
            const string panelName = "SelectionAggregate";

            RibbonPanel panel = null;

            try
            {
                panel = application.CreateRibbonPanel(panelName);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Startup Error", ex.ToString());
                return Result.Failed;
            }


            if (panel == null)
                return Result.Failed;

            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            var buttonData = new PushButtonData(
                "SelectionAggregateButton",
                "Selection\nAggregate",
                assemblyPath,
                "SelectionAggregate.SelectionAggregateCommand"
            );

            PushButton pushButton = panel.AddItem(buttonData) as PushButton;

            pushButton.ToolTip = "Aggregate selected Revit elements with filtering and saved results.";

            pushButton.Image = LoadPngImage("pack://application:,,,/SelectionAggregate;component/Resources/icon16.png");
            pushButton.LargeImage = LoadPngImage("pack://application:,,,/SelectionAggregate;component/Resources/icon32.png");

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private BitmapImage LoadPngImage(string uriString)
        {
            return new BitmapImage(new Uri(uriString, UriKind.Absolute));
        }
    }
}
