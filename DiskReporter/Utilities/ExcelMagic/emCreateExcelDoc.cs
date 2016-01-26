using System;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace DiskReporter.Utilities.ExcelMagic {
	public class CreateExcelDoc : ExcelDoc {
		HSSFWorkbook workbook = null;
		ISheet worksheet = null;
		protected string SheetName { get; set; }

		public CreateExcelDoc (string sheetName) {
			workbook = new HSSFWorkbook();
		 	worksheet = workbook.CreateSheet(sheetName);
			this.SheetName = sheetName;
		}
		/// <summary>
		///  Creates the headers to be used in the Excel report
		/// </summary>
		/// <param name="row">Start row</param>
		/// <param name="col">Start Column</param>
		/// <param name="htext">Text for header</param>
		/// <param name="cell1">Obsolete, needed in MS version only</param>
		/// <param name="cell2">Obsolete, needed in MS version only</param>
		/// <param name="mergeColumns"># of columns to merge with</param>
		/// <param name="color">YELLOW, GRAY, GAINSBORO, TURQUOISE, PEACHPUFF</param>
		/// <param name="boldFont">true / false</param>
		/// <param name="columnSize">Width of column</param>
		/// <param name="fcolor">Empty is white, all other is black - needs updating</param>
		public override void CreateHeaders(int row, int col, string htext, string cell1, string cell2, int mergeColumns, string color, bool boldFont, int columnSize, string fcolor) {
			IRow sheetRow = worksheet.CreateRow(row);
			HSSFCellStyle style = (HSSFCellStyle)workbook.CreateCellStyle();
			ICell ourCell = sheetRow.CreateCell(col);
			IFont font = workbook.CreateFont();

			worksheet.AddMergedRegion(new CellRangeAddress(row, row, col , col + mergeColumns));

			font.FontHeightInPoints = 12;
			font.FontName = "Calibri";
			if(boldFont) font.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
			font.Color = string.IsNullOrEmpty(fcolor) ||  string.IsNullOrWhiteSpace(fcolor) ? HSSFColor.White.Index : HSSFColor.Black.Index;

			switch (color.ToUpper()) {
				case "YELLOW":
					style.FillForegroundColor = HSSFColor.Yellow.Index;
					break;
				case "GRAY":
					style.FillForegroundColor = HSSFColor.Grey50Percent.Index;
					break;
				case "GAINSBORO":
					style.FillForegroundColor = HSSFColor.BlueGrey.Index;
					break;
				case "TURQUOISE":
					style.FillForegroundColor = HSSFColor.Turquoise.Index;
					break;
				case "PEACHPUFF":
					style.FillForegroundColor = HSSFColor.Tan.Index;
					break;
				default:
					style.FillForegroundColor = HSSFColor.Automatic.Index;
					break;
			}
			style.FillPattern = FillPattern.SolidForeground;
			style.Alignment = HorizontalAlignment.Center;
			style.BorderRight = NPOI.SS.UserModel.BorderStyle.Medium;
			style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Medium;
			style.SetFont(font);

			ourCell.CellStyle = style;
			ourCell.Sheet.SetColumnWidth(col, columnSize);
			ourCell.SetCellValue(htext);
			worksheet.AutoSizeColumn(col);
		}

		/// <summary>
		///  Adds data to the cells in the Excel sheet to populate it
		/// </summary>
		/// <param name="row">Row of cell</param>
		/// <param name="col">Column og cell</param>
		/// <param name="data">Text for header</param>
		/// <param name="cell1">Obsolete</param>
		/// <param name="cell2">Obsolete</param>
		/// <param name="format">outpuformat, blank string if none</param>
		public override void AddData(int row, int col, string data, string cell1, string cell2, string format) {
			IRow sheetRow = worksheet.GetRow(row);
			if(sheetRow == null) sheetRow = worksheet.CreateRow(row);
			ICreationHelper createHelper = workbook.GetCreationHelper ();
			ICell ourCell = sheetRow.CreateCell (col);
			HSSFCellStyle style = (HSSFCellStyle)workbook.CreateCellStyle();

			style.BorderRight = NPOI.SS.UserModel.BorderStyle.Medium;
			style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Medium;
			style.FillPattern = FillPattern.SolidForeground;

			if(string.IsNullOrEmpty(format) || string.IsNullOrWhiteSpace(format)) style.DataFormat = createHelper.CreateDataFormat().GetFormat(format);
			ourCell.CellStyle = style;
			ourCell.SetCellValue (data);
			worksheet.AutoSizeColumn(col);
		}

		/// <summary>
		///  Saves the excel report to file
		/// </summary>
		/// <param name="filename">The name of the file we want to save the report to</param>
		public override void SaveAndClose(string filename) {
			using (var fileData = new FileStream(filename, FileMode.Create)) {
				workbook.Write(fileData);
			}
		}
	}
}