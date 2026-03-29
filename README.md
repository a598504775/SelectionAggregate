# SelectionAggregate V0.1

A lightweight Revit plugin for aggregating selected elements with filtering, undo support, and saved results.

---

## Features

- Aggregate common parameters from selected Revit elements  
- Supports operations like Sum, Average, Count, Min, Max  
- Filter selected elements before calculation  
- Save results for later reuse  
- Rename, update, or delete saved results  
- Restore element selection from saved results  
- Undo previous selection changes  
- Automatically handles missing/deleted elements  
- Persist saved results locally (JSON-based)  
- Integrated directly into the Revit Add-Ins ribbon  

---

## Workflow

1. Select elements in Revit  
2. Open **SelectionAggregate** from the Add-Ins tab  
3. Choose a parameter and aggregation operation  
4. Click **Calculate**  
5. (Optional) Apply filters  
6. Click **Save Result**  
7. Manage saved results via right-click menu  

---

## Installation (Revit 2026)

1. Close Revit  
2. Copy the following files into:

C:\ProgramData\Autodesk\Revit\Addins\2026


- SelectionAggregate.dll  
- SelectionAggregate.addin  

3. Launch Revit  
4. Go to the **Add-Ins** tab  
5. Open **SelectionAggregate**

---

## Notes

- If some elements in a saved result no longer exist,  
use **Update Element Selection** to sync the record  

- Saved results are stored locally on your machine  

- This version is tested for **Revit 2026 only**

---

## Built With

- C#  
- WPF  
- Autodesk Revit API  
- .NET  

---

## Screenshot

*(Add a screenshot or GIF here later)*

---

## Future Improvements

- Multi-version Revit support  
- Dynamic selection (live updates)  
- Advanced filtering (string conditions for family/type)  
- UI polishing and customization  

---

## License

MIT License