using System;

namespace ExcelMagic {
    /// <summary>
    ///  Abstract class defining a class to create and handle Excel files. There can exist multiple versions of this class with possible future code in common, 
    /// therefore using it as abstract instead of interface.
    /// </summary>
	public abstract class ExcelDoc {
		public abstract void CreateHeaders (int row, int col, string htext, string cell1, string cell2, int mergeColumns, string color, bool boldFont, int columnSize, string fcolor);
		public abstract void AddData (int row, int col, string data, string cell1, string cell2, string format);
		public abstract void SaveAndClose (string filename);
	}
}