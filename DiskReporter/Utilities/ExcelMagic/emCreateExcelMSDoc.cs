using System;
using System.Reflection;
using Excel = Microsoft.Office.Interop.Excel; //Requires having Excel installed - not used in this version of the system

namespace DiskReporter.Utilities.ExcelMagic {
	class CreateExcelMSDoc : ExcelDoc{
      private Excel.Application app = null;
      private Excel.Workbook workbook = null;
      private Excel.Worksheet worksheet = null;
      private Excel.Range workSheet_range = null;
	  protected string SheetName { get; set; }

	  public CreateExcelMSDoc(string sheetName) {
		this.SheetName = sheetName;
        CreateDoc();
      }
      private void CreateDoc() {
         try {
            app = new Excel.Application();
            app.Visible = true;
            workbook = app.Workbooks.Add(1);
            worksheet = (Excel.Worksheet)workbook.Sheets[1];
         } catch (Exception e) {
            Console.Write("Error: " + e.ToString());
         } 
      }
        /// <summary>
        ///  Creates the headers to be used in the Excel report
        /// </summary>
        /// <param name="row">Start row</param>
        /// <param name="col">Start Column</param>
        /// <param name="htext">Text for header</param>
        /// <param name="cell1">Start Cell for instance A1</param>
        /// <param name="cell2">End cell for instance A2</param>
        /// <param name="mergeColumns"># of columns to merge with</param>
        /// <param name="color">YELLOW, GRAY, GAINSBORO, TURQUOISE, PEACHPUFF</param>
        /// <param name="boldFont">true / false</param>
        /// <param name="columnSize">Width of column</param>
        /// <param name="fcolor">Empty is white, all other is black - needs updating</param>
          public void CreateHeaders(int row, int col, string htext, string cell1, string cell2, int mergeColumns, string color, bool boldFont, int columnSize, string fcolor) {
            worksheet.Cells[row, col] = htext;
            workSheet_range = worksheet.get_Range(cell1, cell2);
            workSheet_range.Merge(mergeColumns);
            workSheet_range.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

            switch (color) {
                case "YELLOW":
                   workSheet_range.Interior.Color = System.Drawing.Color.Yellow.ToArgb();
                   break;
                case "GRAY":
                   workSheet_range.Interior.Color = System.Drawing.Color.Gray.ToArgb();
                   break;
                case "GAINSBORO":
                   workSheet_range.Interior.Color = System.Drawing.Color.Gainsboro.ToArgb();
                   break;
                case "Turquoise":
                   workSheet_range.Interior.Color = System.Drawing.Color.Turquoise.ToArgb();
                   break;
                case "PeachPuff":
                   workSheet_range.Interior.Color = System.Drawing.Color.PeachPuff.ToArgb();
                   break;
                default:
                   //  workSheet_range.Interior.Color = System.Drawing.Color..ToArgb();
                   break;
            }

            workSheet_range.Borders.Color = System.Drawing.Color.Black.ToArgb();
            workSheet_range.Font.Bold = boldFont;
            workSheet_range.ColumnWidth = columnSize;
            if (fcolor.Equals("")) {
            workSheet_range.Font.Color = System.Drawing.Color.White.ToArgb();
            } else {
            workSheet_range.Font.Color = System.Drawing.Color.Black.ToArgb();
            }
          }
        /// <summary>
        ///  Adds data to the cells in the Excel sheet to populate it
        /// </summary>
        /// <param name="row">Row of cell</param>
        /// <param name="col">Column og cell</param>
        /// <param name="data">Text for header</param>
        /// <param name="cell1">Start Cell for instance A1</param>
        /// <param name="cell2">End cell for instance A2</param>
        /// <param name="format">outpuformat, blank string if none</param>
		public void AddData(int row, int col, string data, string cell1, string cell2, string format) {
             worksheet.Cells[row, col] = data;
             workSheet_range = worksheet.get_Range(cell1, cell2);
             workSheet_range.Borders.Color = System.Drawing.Color.Black.ToArgb();
             workSheet_range.NumberFormat = format;
        }
        /// <summary>
        ///  Saves the excel report to file
        /// </summary>
        /// <param name="filename">The name of the file we want to save the report to</param>
        public void SaveAndClose(string filename) {
             workbook.SaveAs(filename, Excel.XlFileFormat.xlOpenXMLWorkbook, Missing.Value, Missing.Value, false, false, Excel.XlSaveAsAccessMode.xlNoChange, Excel.XlSaveConflictResolution.xlUserResolution, true, Missing.Value, Missing.Value, Missing.Value);
             workbook.Close(false, "", true);
             app.Quit();
        }
    }
}
