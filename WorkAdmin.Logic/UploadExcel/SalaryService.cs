using System;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using WorkAdmin.Models;
using System.IO;
using NPOI.HSSF.UserModel;
using System.Data;

namespace WorkAdmin.Logic.UploadExcel
{
    public class SalaryService
    {
        #region private field
        private Stream _file;
        private string _fileName;
        private string _year;
        private string _month;
        private IWorkbook _workbook;
        private ISheet _sheet;
        private string _logName;
        private DataTable _myTable;
        private string title;
        #endregion

        public SalaryService(string fileName, Stream file, string year, string month, string loginName = null)
        {
            _file = file;
            _fileName = fileName;
            _year = year;
            _month = month;
            _logName = loginName;
            _myTable = new DataTable();
        }

        public ReturnResultTable ReadFile()
        {
            DataTable dt = new DataTable();
            if (_workbook == null)
                _workbook = Utils.BuildWorkbook(_fileName, _file);
            _sheet = _workbook.GetSheetAt(0);
            if (_sheet == null)
            {
                return new ReturnResultTable() { Succeeded = false, ErrorMsg = "No sheet  was found" };
            }

            IRow titleRow = null;
            IRow subRow = null;
            int maxCellNumber = 0;
            _sheet.ForceFormulaRecalculation = false;
            for (int rowIndex = _sheet.FirstRowNum; rowIndex <= _sheet.LastRowNum; rowIndex++)
            {
                // 处理标题
                if (rowIndex == 0)
                {
                    var temp = GetCellValue(_sheet, rowIndex, 0);
                    title = temp != string.Empty ? temp : "未知明细";
                    continue;
                }
                // 处理列头
                if (rowIndex == 1)
                {
                    titleRow = _sheet.GetRow(rowIndex);
                    subRow = _sheet.GetRow(rowIndex + 1);
                    if (titleRow.LastCellNum > maxCellNumber)
                    {
                        maxCellNumber = titleRow.LastCellNum;
                    }
                    for (int colIndex = titleRow.FirstCellNum; colIndex < maxCellNumber; colIndex++)
                    {
                        string titleVal = GetCellValue(titleRow, colIndex);
                        string subVal = GetCellValue(subRow, colIndex);
                        if (titleVal == string.Empty && subVal != string.Empty)
                        {
                            for (int i = colIndex - 1; i >= 0; i--)
                            {
                                string preTitle = GetCellValue(titleRow, i);
                                if (preTitle != string.Empty)
                                {
                                    dt.Columns.Add($"{preTitle}-{subVal}");
                                    break;
                                }
                            }
                            continue;
                        }
                        else if (titleVal != string.Empty && subVal != string.Empty)
                        {
                            dt.Columns.Add($"{titleVal}-{subVal}");
                            continue;
                        }
                        if (titleVal != string.Empty && subVal == string.Empty)
                        {
                            dt.Columns.Add(titleVal);
                            continue;
                        }
                        if (titleVal == string.Empty && subVal == string.Empty)
                            break;
                    }
                    continue;
                }
                if (rowIndex == 2)
                    continue;
                // 处理数据记录
                IRow row = _sheet.GetRow(rowIndex);
                var firstValue = GetCellValue(row, 0);
                if (firstValue != string.Empty)
                {
                    DataRow dr = dt.NewRow();
                    for (int colIndex = 0; colIndex < dt.Columns.Count; colIndex++)
                    {
                        dr[colIndex] = GetCellFormulaValue(row, colIndex);
                    }
                    dt.Rows.Add(dr);
                }
            }
            return new ReturnResultTable() { Succeeded = true, ErrorMsg = "", SalaryTable = dt, TableName = _sheet.SheetName };
        }

        public string GetCellValue(ISheet sheet, int rowIndex, int cellIndex)
        {
            var value = sheet.GetRow(rowIndex).GetCell(cellIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString();
            return value;
        }
        public string GetCellValue(IRow row, int cellIndex)
        {
            var value = row.GetCell(cellIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString();
            return value;
        }
        public string GetCellFormulaValue(IRow row, int cellIndex)
        {
            ICell cell = row.GetCell(cellIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK);
            // NPOI中数字和日期都是NUMERIC类型的
            if (cell.CellType == CellType.Numeric)
            {
                if (DateUtil.IsCellDateFormatted(cell))
                {
                    return cell.DateCellValue.ToString("yyyy-MM-dd");
                }
                return cell.NumericCellValue.ToString();
            }
            else if (cell.CellType == CellType.Formula)
            {
                return cell.NumericCellValue.ToString("f2");
            }
            else if (cell.CellType == CellType.Blank)
            {
                return string.Empty;
            }
            return cell.ToString();
        }
    }
}
