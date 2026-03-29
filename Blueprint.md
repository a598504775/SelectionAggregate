#SelectionAggregate Blueprint

###Introduction
This is a Revit plugin that aggregates selected elements in a Revit model. It allows users to calculate the common calculable properties of the selected elements, in additon to the restricted Revit schedule. That means this plugin can be used to calculate item properties that are non-schedulable. 

###How to use
- Select the elements you want to aggregate.The plugin UI will display the number of items selected and provide the description of those items. 
For example, if you select 5 lines, it will display "5 lines selected". If you selected 3 walls and 2 windows, it will display 5 items selected.
That means if the items are of the same category, it will display the category name. If the items are of different categories, it will display "items".
- (Optional) You can click the filter button to filter the items using the rules. This feature can be done the same way as the filter function in the Schedule-Filter and Visibility Graphics-Filter. 
Basically, the Revit built-in filter is good enough to be used in this plugin, if we can use it. But the filter for selected items is not good enough, so we will use the filters mentioned above to improve it.
- Click the "Parameter" dropdown menu to select the common property you want to calculate. If there is no calculable common property, the dropdown menu will be empty.For example, if the only common property are "Base level" and "Material", then the dropdown menu will only show those two. But the "Calculation" dropdown menu will be greyed out, and the result box will display "Incalculable".
- Click the "Calculate" dropdown menu to select the calculation method you want to use. It includes "Sum", "Average", "Min", "Max" for calculable properties.
- (Optional) You can click "Save Result" button to save the result to the list at the bottom of the UI. The saved result will include all displayed information stated above. Besides, it adds a new column of the name of the result.
Behind the scene, the result will save the element ids of the selection group. If some elements are deleted in the model, an red "!" icon will appear in front of the result name. You can hover over the icon to see the error message, such as "2 items are missing". Once you click "Update" button in that message window, the result will be updated.

###UI
####Main UI
Description: Open the plugin, and you will see the main UI as below.

-----------------------------------------
Selection Aggregate
-----------------------------------------
5 items selected                 [Filter]
Parameter             [Area            ▼]
Calculation           [Sum             ▼]
-----------------------------------------
Result: 1250                  [Calculate]
-----------------------------------------
History                     [Save Result]
-----------------------------------------
  Name        Value      Status
▶ Test1       1250 		   ✓
▼ Test2       600          !
  --------------------------------------
 | Selection Parameter Calculation      |
 | 5 lines   Length    Sum              |
 | Error: 2 items are missing. [Update] |
  --------------------------------------
-----------------------------------------

####Filter UI
Description: Click "Filter" button, the Filter UI will show up. The "Value" is formatted according to the parameter type.

--------------------------------------
Filter
--------------------------------------
Parameter   Rule                Value
[Area ▼]    [is less than ▼]    [30  ]
--------------------------------------
                      [Apply] [Cancel]
--------------------------------------


###Notes:
Put SelectionAggregate.addin into C:\ProgramData\Autodesk\Revit\Addins\2026\
Revit 2026 API changes: https://www.revitapidocs.com/2026/news


###TODO:
- Implement the history panel to save the results. Clicking on the history will select the corresponding elements in the model. The history can be exported as .csv or excel files.
- Advanced filter function: Can support more type of parameters and not limited by Dimensions. 
- Advanced filter function: Introducing Family and Type parameters and string type filter operation. But not need to implement Catagory level filter since Revit has that built.
- Advanced filter function: Introducting a special Value input box that not only can input but also with drop down menu, reading all existing values of this parameter.
- Implement dynamic selection. When the user clicks on the model and changes the selection, the UI will refresh automatically. The plugin will not block the Revit UI, so the user can interact with the model while using the plugin.
- Revise UI layout to be closer to the UI drafted above. And the UI can be snap to the side of the Revit window.
- Advanced filter function: Revising filter's Value input to show units if applicable.
- Advanced filter function: Undo and trace back previous selection
- Overall UI improvement: Save selection for the previous selection, if applicable.
- PushCurrentSelectionToUndo() only called when the selection number is changed.
- History panel: Saved results persistant. Simple jSON version.

###Before publish：
- (Completed) Refine filter window UI.
- Provide formal Icon on Revit Add-Ins panel.
- Filter UI will appear on to of the main UI
- Using own input WPF box instead of buildin input box.
- Check if 122 items selected will cause performance issue. And if it will spill the UI.