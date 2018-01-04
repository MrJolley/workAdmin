using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using WorkAdmin.Models;
using System.IO;
using NPOI.HSSF.UserModel;
using System.Data;


namespace WorkAdmin.Logic
{
    public class SalaryUploadService
    {
        #region private field
        private string m_uploadFileFullName;
        private string m_year;
        private string m_month;
        private IWorkbook m_workbook;
        private ISheet m_sheet;
        private string m_loginName;
        private DataTable m_myTable;
        private Stream m_uploadFile;
        private string titleText;
        #endregion

        public SalaryUploadService(string uploadFileFullName, Stream uploadFile, string year, string month, string loginName = null)
        {
            this.m_uploadFileFullName = uploadFileFullName;
            this.m_year = year;
            this.m_month = month;
            this.m_loginName = loginName;
            this.m_myTable = new DataTable();
            this.m_uploadFile = uploadFile;
        }

        public ReturnResultTable ReadFile()
        {
            DataTable dt = new DataTable();

            if (this.m_workbook == null)
                BuildWorkbook(m_uploadFile);

            this.m_sheet = this.m_workbook.GetSheetAt(0);
            if (this.m_sheet == null)
            {
                return new ReturnResultTable() { Succeeded = false, ErrorMsg = "No sheet  was found" };
            }
                IRow titleRow = null;
                //need a blank row for that don't have a sub title.
                IRow subRow = null;
                int maxCellNumber = 0;

                this.m_sheet.ForceFormulaRecalculation = false;
                for (int rowIndex = this.m_sheet.FirstRowNum; rowIndex <= this.m_sheet.LastRowNum; rowIndex++)
                {
                    if (rowIndex == 0)
                    {
                        var temp = this.m_sheet.GetRow(rowIndex).GetCell(0, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString();
                        if (temp != string.Empty)
                            titleText = temp;
                        else
                            titleText = "未知明细";
                        continue;
                    }
                    if (rowIndex == 1)
                    {
                        titleRow = this.m_sheet.GetRow(rowIndex);
                        subRow = this.m_sheet.GetRow(rowIndex + 1);
                        if (titleRow.LastCellNum > maxCellNumber)
                            maxCellNumber = titleRow.LastCellNum;
                        for (int colIndex = titleRow.FirstCellNum; colIndex < titleRow.LastCellNum; colIndex++)
                        {
                            string titleCellVal = titleRow.GetCell(colIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim().ToLower();
                            string subCellVal = subRow.GetCell(colIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim().ToLower();
                            if (titleCellVal == string.Empty && subCellVal != string.Empty)
                            {
                                for (int i = colIndex - 1; i >= 0; i--)
                                {
                                    string preTitle = titleRow.GetCell(i, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString();
                                    if (preTitle != string.Empty)
                                    {
                                        dt.Columns.Add(string.Format("{0}/{1}", preTitle, subCellVal));
                                        break;
                                    }
                                }
                                continue;
                            }
                            if (titleCellVal != string.Empty && subCellVal != string.Empty)
                            {
                                dt.Columns.Add(string.Format("{0}/{1}", titleCellVal, subCellVal));
                                continue;
                            }
                            if (titleCellVal != string.Empty && subCellVal == string.Empty)
                            {
                                dt.Columns.Add(titleCellVal);
                                continue;
                            }
                            if (titleCellVal == string.Empty && subCellVal == string.Empty)
                                break;
                        }
                        continue;
                    }
                    if (rowIndex == 2)
                        continue;
                    IRow row = this.m_sheet.GetRow(rowIndex);
                    ICell firstCell = row.GetCell(0, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                    if (firstCell.ToString() != string.Empty)
                    {
                        DataRow dr = dt.NewRow();
                        for (int colIndex = 0; colIndex < dt.Columns.Count; colIndex++)
                        {
                            ICell cell = row.GetCell(colIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                            if (cell.CellType == CellType.Numeric)
                            {
                                dr[colIndex] = cell.NumericCellValue.ToString();
                            }
                            else if (cell.CellType == CellType.Formula)
                            {
                                dr[colIndex] = cell.NumericCellValue.ToString("f2");
                            }
                            else
                                dr[colIndex] = cell.ToString();
                        }
                        dt.Rows.Add(dr);
                    }     
                }
                return new ReturnResultTable() { Succeeded = true, ErrorMsg = "", SalaryTable = dt, TableName = this.m_sheet.SheetName };
        }

        private void BuildWorkbook(Stream file)
        {
            string fileExt = Path.GetExtension(this.m_uploadFileFullName);
            if (fileExt == ".xls")
                this.m_workbook = new HSSFWorkbook(file);
            else if (fileExt == ".xlsx")
                this.m_workbook = new XSSFWorkbook(file);
            else
                throw new Exception("Excel wrong format:" + fileExt);
        }
    }
}
