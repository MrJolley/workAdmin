using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using WorkAdmin.Models.ViewModels;
using System.Data;
using WorkAdmin.Models.Entities;

namespace WorkAdmin.Logic.HolidayLogic
{
    public class FileUploadHelper
    {
        #region private field
        private Stream _fileStream;
        private IWorkbook _workBook;
        string _fileName;
        const string _chineseLocationName = "姓名";
        private string _loginName;
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileName">文件名，包括后缀</param>
        public FileUploadHelper(Stream stream, string fileName, string loginName)
        {
            _fileStream = stream;
            _fileName = fileName;
            _loginName = loginName;
            BuildWorkbook();
        }

        /// <summary>
        /// 读取当前年假，上一年年假结余的excel信息
        /// </summary>
        public HolidayResult ReadHolidayFile()
        {
            var sheet = _workBook.GetSheetAt(0);
            if (sheet == null)
            {
                return new HolidayResult()
                {
                    hasError = true,
                    ErrorMsg = "当前文件无工作表数据"
                };
            }
            IRow title = sheet.GetRow(1);
            if (!title.GetCell(0, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim().Equals("序号"))
            {
                return new HolidayResult()
                {
                    hasError = true,
                    ErrorMsg = "当前文件数据格式不正确，请确保'A2'单元格为数据起始点，其值为'序号'"
                };
            }
            //读取特定的列数据，包括姓名，年假区间，上期/本期剩余年假时间，总共剩余年假
            int nameCol = 0, regionCol = 0, usedCol = 0;
            foreach (var item in title.Cells)
            {
                string val = item.ToString().Trim();
                if (val.Equals("姓名"))
                {
                    nameCol = item.ColumnIndex;
                }
                else if (val.Equals("年假区间"))
                {
                    regionCol = item.ColumnIndex;
                }
                else if (val.StartsWith("已使用年假"))
                {
                    usedCol = item.ColumnIndex;
                }
            }
            if (nameCol == 0 || regionCol == 0 || usedCol == 0)
            {
                return new HolidayResult()
                {
                    hasError = true,
                    ErrorMsg = "当前文件数据格式不正确，请确保标题行含有'姓名'，'年假区间'，'剩余年假'列"
                };
            }
            //读取所有年假信息
            int cols = title.LastCellNum;
            List<UserHoliday> result = new List<UserHoliday>();
            var users = UserService.GetAllUsers();
            string errorName = string.Empty;
            try
            {
                for (int row = 3; row < sheet.LastRowNum + 1; row++)
                {
                    IRow sRow = sheet.GetRow(row);
                    ICell sCell = sRow.GetCell(nameCol);
                    if (sCell != null)
                    {

                        string name = sRow.GetCell(nameCol).ToString().Trim();
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            break;
                        }
                        if (!users.Any(r => r.ChineseName == name))
                        {
                            return new HolidayResult()
                            {
                                hasError = true,
                                ErrorMsg = $"当前文件数据姓名格式不正确，位置：‘{name}’, 请确保中文名已持久化到数据库"
                            };
                        }
                        //读取所有基本信息详情
                        try
                        {
                            var user = users.Where(r => r.ChineseName == name).First();
                            result.Add(new UserHoliday()
                            {
                                StaffName = name,
                                StaffEmail = user.EmailAddress,
                                PaidLeaveBeginDate = Convert.ToDateTime(GetCellFormulaValue(sRow, regionCol)),
                                PaidLeaveEndDate = Convert.ToDateTime(GetCellFormulaValue(sRow, regionCol + 1)),
                                BeforeRemainingHours = double.Parse(GetCellFormulaValue(sRow, regionCol + 3)),
                                CurrentLegalHours = double.Parse(GetCellFormulaValue(sRow, regionCol + 4)),
                                CurrentWelfareHours = double.Parse(GetCellFormulaValue(sRow, regionCol + 5)),
                                CurrentUsedHours = double.Parse(GetCellFormulaValue(sRow, usedCol)),
                            });
                        }
                        catch (Exception)
                        {
                            errorName = name;
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new HolidayResult()
                {
                    hasError = true,
                    ErrorMsg = "错误定位=>姓名：" + (string.IsNullOrWhiteSpace(errorName) ? "无" : errorName) +
                    ",错误信息：" + ex.Message,
                };
            }
            return new HolidayResult()
            {
                hasError = false,
                ErrorMsg = "",
                result = result
            };
        }

        /// <summary>
        /// 读取所有的调休excel信息
        /// </summary>
        public TransferResult ReadTransferFile()
        {
            var num = this._workBook.NumberOfSheets;
            List<UserTransferList> result = new List<UserTransferList>();
            while (num > 0)
            {
                var sheet = this._workBook.GetSheetAt(num - 1);
                if (sheet == null)
                {
                    return new TransferResult()
                    {
                        hasError = true,
                        ErrorMsg = $"错误定位：工作表=>{sheet.SheetName},<br />" +
                        $"错误信息：当前工作表无数据"
                    };
                }
                IRow title = sheet.GetRow(0);
                if (!title.GetCell(0, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim().Equals("日期"))
                {
                    return new TransferResult()
                    {
                        hasError = true,
                        ErrorMsg = $"错误定位：工作表=>{sheet.SheetName},<br />" +
                        $"错误信息：当前工作表数据格式不正确，请确保'A1'单元格为数据起始点，其值为日期"
                    };
                }
                //读取所有调休信息
                int cols = title.LastCellNum;
                UserTransferList list = new UserTransferList();
                var allUsers = UserService.GetAllUsers();
                try
                {
                    for (int row = 1; row < sheet.LastRowNum + 1; row++)
                    {
                        IRow sRow = sheet.GetRow(row);
                        if (sRow == null || sRow.Cells.Count < 2)
                        {
                            continue;
                        }
                        string name = sheet.GetRow(row).GetCell(1).ToString();
                        bool nameExt = name != null && name != ""; //名字单元格不为空
                                                                   //确保第一个名字单元格存在
                        if (nameExt)
                        {
                            var user = allUsers.Where(r => r.LetterName == name.ToLower().Trim());
                            if (user.Count() > 0)
                            {
                                var userModel = user.FirstOrDefault();
                                try
                                {
                                    //读取所有基本信息详情
                                    var staffRecord = result.Where(r => r.StaffName == name);
                                    if (result.Count > 0 && staffRecord.Count() > 0)
                                    {
                                        staffRecord.First().UserTransferDetail.Add(
                                        new TransferDetail()
                                        {
                                            ExtraWorkDate = DateTime.Parse(sRow.GetCell(0, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim()).ToString("yyyy-MM-dd"),
                                            ExtraWorkPeriod = sRow.GetCell(2, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim(),
                                            ExtraWorkTime = double.Parse(sRow.GetCell(3, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim()),
                                            TransferPeriod = sRow.GetCell(4, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim(),
                                            TransferRemainingTime = double.Parse(sRow.GetCell(5, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim()),
                                        });
                                    }
                                    else
                                    {
                                        var detail = new TransferDetail()
                                        {
                                            ExtraWorkDate = DateTime.Parse(sRow.GetCell(0, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim()).ToString("yyyy-MM-dd"),
                                            ExtraWorkPeriod = sRow.GetCell(2, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim(),
                                            ExtraWorkTime = double.Parse(sRow.GetCell(3, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim()),
                                            TransferPeriod = sRow.GetCell(4, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim(),
                                            TransferRemainingTime = double.Parse(sRow.GetCell(5, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim()),
                                        };
                                        var userlist = new UserTransferList()
                                        {
                                            StaffName = name,
                                            StaffEmail = userModel.EmailAddress,
                                        };
                                        userlist.UserTransferDetail.Add(detail);
                                        result.Add(userlist);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    return new TransferResult()
                                    {
                                        hasError = true,
                                        ErrorMsg = $"错误定位：工作表=>{sheet.SheetName}, 姓名=>{name}, <br />" +
                                $"错误信息：当前文件字段类型有误，无法转换字段: {ex.Message}" +
                                $"错误追踪：{ex.StackTrace}"
                                    };
                                }
                            }
                            else
                            {
                                return new TransferResult()
                                {
                                    hasError = true,
                                    ErrorMsg = $"错误定位：工作表=>{sheet.SheetName}, 姓名=>{name}, <br />" +
                                "错误信息：当前文件姓名有误，无法匹配数据库员工字段"
                                };
                            }
                        }
                        //数据读取结束，返回result
                    }
                }
                catch (Exception ex)
                {
                    return new TransferResult()
                    {
                        hasError = true,
                        ErrorMsg = $"错误定位：工作表=>{sheet.SheetName}, <br />" +
                            $"错误追踪：{ex.StackTrace}"
                    };
                }
                num--;
            }
            return new TransferResult()
            {
                hasError = false,
                ErrorMsg = "",
                result = result
            };
        }

        /// <summary>
        /// 读取worklog的excel信息
        /// </summary>
        /// <param name="year">worklog时间所在的年份，具体月日日期由sheet名称读取</param>
        public WorklogTimeResult ReadWorklogFile(int year)
        {
            WorklogTimeResult wtr = new WorklogTimeResult();

            foreach (ISheet sheet in this._workBook)
            {
                try
                {
                    if (sheet == null || sheet.SheetName.IndexOf(".") == -1)
                    {
                        continue;
                    }
                    string[] sheetNameArray = sheet.SheetName.Trim().Split(".".ToArray());
                    int month = 0, day = 0;
                    if (!int.TryParse(sheetNameArray[0], out month) || !int.TryParse(sheetNameArray[1], out day))
                    {
                        continue;
                    }
                    DateTime date = new DateTime(year, month, day);
                    IRow title = sheet.GetRow(0);
                    if (!title.GetCell(0).StringCellValue.Trim().Equals(_chineseLocationName))
                    {
                        wtr.hasError = true;
                        wtr.ErrorMsg = "sheet表必须以'姓名'开头";
                        return wtr;
                    }
                    else
                    {
                        for (int i = 0; i < title.Cells.Count; i++)
                        {
                            string item = title.GetCell(i).StringCellValue.Trim();
                            if (colName.Keys.Contains(item))
                            {
                                colName[item] = i;
                            }
                        }
                    }
                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row != null)
                        {
                            string staffName = row.GetCell(colName["姓名"], MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString();
                            if (!string.IsNullOrWhiteSpace(staffName))
                            {
                                try
                                {
                                    WorklogTime wlt = new WorklogTime();
                                    wlt.userName = staffName;
                                    wlt.rank = row.GetCell(colName["职级"], MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString();
                                    wlt.department = row.GetCell(colName["部门"], MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString();
                                    wlt.position = row.GetCell(colName["职位"], MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString();
                                    wlt.project = row.GetCell(colName["项目"], MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString();
                                    wlt.worklog = double.Parse(row.GetCell(colName["工作时间"], MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim());
                                    wlt.logDate = date;
                                    wlt.updateDate = DateTime.Now;
                                    wlt.updateUser = this._loginName;
                                    wtr.result.Add(wlt);
                                    wtr.logDates.Add(date);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("出错信息行：" + staffName + "," + ex);
                                }
                            }
                        }
                        else { continue; }
                    }
                }
                catch (Exception ex)
                {
                    wtr.hasError = true;
                    wtr.ErrorMsg = "位置：" + sheet.SheetName + "." + "错误：" + ex.Message;
                }
            }

            return wtr;
        }

        public InsuranceRadixResult ReadInsuranceRadixFile(int year)
        {
            var sheet = this._workBook.GetSheetAt(0);
            InsuranceRadix result = new InsuranceRadix();
            if (sheet == null)
            {
                return new InsuranceRadixResult()
                {
                    hasError = true,
                    ErrorMsg = "当前文件无工作表数据"
                };
            }
            //定位标题行
            var num = sheet.LastRowNum;
            int titleNum = -1;
            for (int i = 0; i < num + 1; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row != null)
                {
                    ICell cell = sheet.GetRow(i).GetCell(0, MissingCellPolicy.CREATE_NULL_AS_BLANK);
                    if (cell != null && cell.ToString().Trim().Equals("序号"))
                    {
                        titleNum = i;
                        break;
                    }
                }
            }
            if (titleNum == -1)
            {
                return new InsuranceRadixResult()
                {
                    hasError = true,
                    ErrorMsg = "数据格式错误：当前文件无起始数据行，标题行第一列必须包含‘序号’两个字"
                };
            }
            //读取数据
            var users = UserService.GetAllUsers().ToList();
            result.Year = year.ToString();
            string errorName = string.Empty;
            try
            {
                for (int j = titleNum + 1; j < sheet.LastRowNum + 1; j++)
                {
                    IRow data = sheet.GetRow(j);
                    string flag = data.GetCell(0, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString();
                    if (flag == null || flag.Equals(string.Empty))
                    {
                        break;
                    }
                    int.TryParse(flag, out int cols);
                    if (cols == 0)
                    {
                        continue;
                    }
                    string name = data.GetCell(1, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString();
                    try
                    {
                        var user = users.Where(r => r.ChineseName == name).First();
                        result.InsuranceRadixDetails.Add(new InsuranceRadix.UserInfo()
                        {
                            ChineseName = name,
                            AunualIncome = double.Parse(data.GetCell(2, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString()),
                            Email = user.EmailAddress
                        });
                    }
                    catch (Exception)
                    {
                        errorName = name;
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                return new InsuranceRadixResult()
                {
                    hasError = true,
                    ErrorMsg = "错误定位=>姓名：" + (string.IsNullOrWhiteSpace(errorName) ? "无" : errorName) +
                    ",错误信息：" + ex.Message,
                };
            }
            return new InsuranceRadixResult()
            {
                hasError = false,
                ErrorMsg = "",
                result = result
            };
        }

        private void BuildWorkbook()
        {
            string fileExt = Path.GetExtension(_fileName).ToLower();
            if (fileExt == ".xls")
                this._workBook = new HSSFWorkbook(_fileStream);
            else if (fileExt == ".xlsx")
                this._workBook = new XSSFWorkbook(_fileStream);
            else
                throw new Exception("Excel wrong format:" + fileExt);
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

        public class HolidayResult
        {
            public bool hasError { get; set; }

            public string ErrorMsg { get; set; }

            public List<UserHoliday> result { get; set; }
        }

        public class TransferResult
        {
            public bool hasError { get; set; }

            public string ErrorMsg { get; set; }

            public List<UserTransferList> result { get; set; }
        }

        public class InsuranceRadixResult
        {
            public bool hasError { get; set; }

            public string ErrorMsg { get; set; }

            public InsuranceRadix result { get; set; }
                = new InsuranceRadix();
        }

        #region worklog Time 数据结构
        public class WorklogTimeResult
        {
            public WorklogTimeResult()
            {
                result = new List<WorklogTime>();
                logDates = new List<DateTime>();
            }

            public bool hasError { get; set; }

            public string ErrorMsg { get; set; }

            public List<DateTime> logDates { get; set; }

            public List<WorklogTime> result { get; set; }
        }

        public Dictionary<string, int> colName =
            new Dictionary<string, int>()
        {
            {"姓名", 0},
            {"职级", 1},
            {"部门", 2},
            {"职位", 3},
            {"项目", 4},
            {"工作时间", 5},
        };
        #endregion
    }
}
