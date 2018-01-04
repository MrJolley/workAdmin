using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using WorkAdmin.Models;
using WorkAdmin.Models.Entities;

namespace WorkAdmin.Logic
{
    public class WorkLogUploadService
    {
        #region Private Fields
        private string m_uploadFileName;
        private string m_fileFullName;
        private int m_year;
        private int m_month;
        private IWorkbook m_workbook;
        private ISheet m_sheet;
        private string m_loginName;

        private readonly string m_uploadFileNameMustContentLowered = "work log";
        private readonly string m_pinYinHeaderLowered = "英文名";
        private readonly string m_chineseNameHeaderLowered = "中文名";
        private readonly int m_maxNameLength = 50;
        private readonly string m_employeeTypeHeaderLowered = "人员性质";
        private readonly string m_fileAsOfDateIndicatorLowered = "统计截止时间";
        private readonly string m_fileCommentIndicatorLowered = "备注";
        private readonly List<string> m_listFileUserLeftIndicators = new List<string>() { "离职", "已离职" };

        private Dictionary<string, int> m_dicNameMapping = new Dictionary<string,int> ();
        private Dictionary<int, string> m_dicChineseNameMapping = new Dictionary<int, string>();

        private List<WorkLog> m_workLogs = new List<WorkLog>();
        private WorkLogProperty m_workLogProperty = new WorkLogProperty();
        #endregion


        public WorkLogUploadService(string uploadFileName,string fileFullName, int year, int month, string loginName = null)
        {
            this.m_uploadFileName = uploadFileName;
            this.m_fileFullName = fileFullName;
            this.m_year = year;
            this.m_month = month;
            this.m_loginName = loginName;
        }

        public List<WorkLog> GetWorkLogs()
        {
            return this.m_workLogs;
        }

        public WorkLogProperty GetWorkLogProperty()
        {
            return this.m_workLogProperty;
        }

        public ReturnResult ReadFile()
        {
            if (!this.m_uploadFileName.Replace(" ", "").ToLower().Contains(this.m_uploadFileNameMustContentLowered.Replace(" ","")))
                return new ReturnResult() { Succeeded = false, ErrorMsg = string.Format("File name must contain words '{0}'",this.m_uploadFileNameMustContentLowered) };

            if (this.m_workbook == null)
                BuildWorkbook();

            string shtNameTarget = this.m_year + "." + this.m_month;
            this.m_sheet = this.m_workbook.GetSheet(shtNameTarget);
            if (this.m_sheet == null)
                return new ReturnResult() { Succeeded = false, ErrorMsg = string.Format("No sheet with name {0} was found", shtNameTarget) };

            var parseProperty = ParseUploadProperty();
            if (!parseProperty.Succeeded)
                return parseProperty;

            this.m_dicNameMapping=UserService.GetUserDictionary();
            List<string> listErrorUserNames = new List<string>();
            List<string> listErrorDates = new List<string>();
            IRow titleRow = null;
            int nameColIndex = -1;
            int pinYinColIndex = -1;
            int chineseNameColIndex = -1;
            int employeeTypeColIndex = -1;

            int maxCellNumber = 0;

            bool isFinished = false;
            for (int rowIndex = this.m_sheet.FirstRowNum; rowIndex < this.m_sheet.LastRowNum && !isFinished; rowIndex++)
            {
                IRow row = this.m_sheet.GetRow(rowIndex);
                if (row == null)
                    continue;
                if (row.LastCellNum > maxCellNumber)
                    maxCellNumber = row.LastCellNum;
                if (titleRow == null)
                    for (int colIndex = row.FirstCellNum; colIndex < row.LastCellNum; colIndex++)
                    {
                        string cellVal = row.GetCell(colIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim().ToLower();
                        if (cellVal== this.m_pinYinHeaderLowered)
                        {
                            titleRow = row;
                            pinYinColIndex = colIndex;
                        }
                        else if (cellVal == this.m_chineseNameHeaderLowered)
                        {
                            chineseNameColIndex = colIndex;
                            if (titleRow == null)
                                titleRow = row;
                        }
                        else if (cellVal == this.m_employeeTypeHeaderLowered)
                        {
                            employeeTypeColIndex = colIndex;
                        }
                        else if (this.m_listFileUserLeftIndicators.Contains(cellVal,StringComparer.InvariantCultureIgnoreCase))
                        {
                            isFinished = true;
                            titleRow = null;
                            break;
                        }
                    }
                if (titleRow == null)
                    continue;
                nameColIndex = pinYinColIndex >= 0 ? pinYinColIndex : chineseNameColIndex;
                string userName = row.GetCell(nameColIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                if (string.IsNullOrWhiteSpace(userName))
                {
                    titleRow = null;
                    continue;
                }              
                if (userName.ToLower() == this.m_pinYinHeaderLowered)
                    continue;
                if (userName.ToLower() == this.m_chineseNameHeaderLowered)
                    continue;
                if (userName.Length > this.m_maxNameLength)
                    continue;
                if (!this.m_dicNameMapping.ContainsKey(userName))
                {
                    listErrorUserNames.Add(userName);
                    continue;
                }

                if (pinYinColIndex >= 0 && chineseNameColIndex >= 0)
                {
                    string pinYin = row.GetCell(pinYinColIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                    string chinese = row.GetCell(chineseNameColIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(pinYin) && !string.IsNullOrWhiteSpace(chinese))
                        if (!this.m_dicChineseNameMapping.ContainsKey(this.m_dicNameMapping[pinYin]))
                            this.m_dicChineseNameMapping.Add(this.m_dicNameMapping[pinYin], chinese);
                }

                for (int colIndex = row.FirstCellNum; colIndex < maxCellNumber; colIndex++)
                {
                    string titleVal = titleRow.GetCell(colIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                    if (string.IsNullOrWhiteSpace(titleVal))
                        continue;
                    string cellVal = row.GetCell(colIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                    if (titleVal.StartsWith(this.m_month + "."))
                    {
                        try
                        {
                            string[] arrTemp = titleVal.Split(".".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                            DateTime date = new DateTime(this.m_year, this.m_month, Convert.ToInt32(arrTemp[1]));
                            WorkLog workLog = new WorkLog();
                            workLog.UserId = this.m_dicNameMapping[userName];
                            workLog.AsOfDate = date;
                            workLog.IsAbsent = !string.IsNullOrWhiteSpace(cellVal);
                            workLog.SortOrder = this.m_workLogs.Count + 1;
                            if (employeeTypeColIndex >= 0)
                                workLog.EmployeeType = row.GetCell(employeeTypeColIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                            workLog.CreatedBy = this.m_loginName;
                            workLog.CreatedTime = DateTime.Now;
                            workLog.UpdatedBy = this.m_loginName;
                            workLog.UpdatedTime = DateTime.Now;
                            this.m_workLogs.Add(workLog);
                        }
                        catch (Exception ex)
                        {
                            listErrorDates.Add(titleVal);
                            continue;
                        }
                    }
                }
            }

            if (listErrorUserNames.Count > 0)
            {
                string errNames = string.Join(",", listErrorUserNames.Distinct().ToArray());
                return new ReturnResult() { Succeeded = false, ErrorMsg = "Cannot map names: " + errNames };
            }
            if (listErrorDates.Count > 0)
            {
                string errDates = string.Join(",", listErrorDates.Distinct().ToArray());
                return new ReturnResult() { Succeeded = false, ErrorMsg = "Cannot parse dates: " + errDates };
            }

            var duplicateRecords = this.m_workLogs.GroupBy(r => new { UserId = r.UserId, AsOfDate = r.AsOfDate }).Where(r => r.Count() > 1);
            if (duplicateRecords.Count() > 0)
            {
                List<string> lstDuplicate = new List<string>();
                var distinctUsers = duplicateRecords.Select(r => r.Key.UserId).Distinct();
                foreach (var userId in distinctUsers)
                {
                    var userName = this.m_dicNameMapping.Where(r => r.Value == userId).First().Key;
                    lstDuplicate.Add(userName);
                }
                return new ReturnResult { Succeeded = false, ErrorMsg = "Duplicate records found for:" + string.Join(",", lstDuplicate.ToArray()) };
            }

            if (this.m_workLogs.Count == 0)
                return new ReturnResult() { Succeeded = false, ErrorMsg = "No available work log found" };
            FillLackingChineseNames();
            return new ReturnResult() { Succeeded = true };
        }

        #region Private Methods

        private void BuildWorkbook()
        {
            using (FileStream fs = new FileStream(this.m_fileFullName, FileMode.Open, FileAccess.Read))
            {
                string fileExt = Path.GetExtension(this.m_fileFullName);
                if (fileExt == ".xls")
                    this.m_workbook = new HSSFWorkbook(fs);
                else if (fileExt == ".xlsx")
                    this.m_workbook = new XSSFWorkbook(fs);
                else
                    throw new Exception("Excel wrong format:" + fileExt);
            }
        }

        private ReturnResult ParseUploadProperty()
        {
            this.m_workLogProperty.Year = this.m_year;
            this.m_workLogProperty.Month = this.m_month;
            this.m_workLogProperty.CreatedBy = this.m_loginName;
            this.m_workLogProperty.CreatedTime = DateTime.Now;
            this.m_workLogProperty.UpdatedBy = this.m_loginName;
            this.m_workLogProperty.UpdatedTime = DateTime.Now;
            bool commentStarted = false;
            bool commentEnded = false;
            bool asOfDateParsed = false;
            bool allParsed = false;
            for (int rowIdx = this.m_sheet.FirstRowNum; rowIdx < this.m_sheet.LastRowNum && (!allParsed); rowIdx++)
            {
                IRow row = this.m_sheet.GetRow(rowIdx);
                if (row == null)
                    continue;

                bool isComment = false;
                for (int colIdx = row.FirstCellNum; colIdx < row.LastCellNum; colIdx++)
                {
                    string cellVal = row.GetCell(colIdx, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                    if (string.IsNullOrWhiteSpace(cellVal))
                        continue;
                    if (cellVal.ToLower().StartsWith(this.m_fileAsOfDateIndicatorLowered))
                    {
                        string strAsOfDate = row.GetCell(colIdx + 1, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                        DateTime asOfDate;
                        if (DateTime.TryParse(strAsOfDate, out asOfDate))
                        {
                            this.m_workLogProperty.AsOfDate = asOfDate;
                            asOfDateParsed = true;
                        }
                        else
                        {
                            return new ReturnResult() { Succeeded = false, ErrorMsg = string.Format("Cannot parse {0} to date type", strAsOfDate) };
                        }
                        break;
                    }
                    else if (cellVal.ToLower().StartsWith(this.m_fileCommentIndicatorLowered))
                    {
                        isComment = true;
                        if (commentStarted == false)
                            commentStarted = true;
                        string comment = row.GetCell(colIdx + 1, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(comment))
                        {
                            if (this.m_workLogProperty.Comment == null)
                                this.m_workLogProperty.Comment = comment;
                            else
                                this.m_workLogProperty.Comment += "<br/>" +comment;
                        }
                        break;
                    }
                }

                if (!isComment && commentStarted)
                    commentEnded = true;

                if (commentEnded && asOfDateParsed)
                    allParsed = true;
            }

            if (this.m_workLogProperty.AsOfDate==default(DateTime))
                return new ReturnResult { Succeeded = false, ErrorMsg = "Cannot find valid stats date" };

            return new ReturnResult { Succeeded = true };
        }

        private void FillLackingChineseNames()
        {
            if (this.m_dicChineseNameMapping.Count == 0)
                return;
            var usersLackingChineseName = UserService.GetLackingChineseNameUsers();
            if (usersLackingChineseName.Count == 0)
                return;
            foreach (var user in usersLackingChineseName)
                if (this.m_dicChineseNameMapping.ContainsKey(user.Id))
                    user.ChineseName = this.m_dicChineseNameMapping[user.Id];
            usersLackingChineseName = usersLackingChineseName.Where(r => !string.IsNullOrWhiteSpace(r.ChineseName)).ToList();
            UserService.FillChineseNames(usersLackingChineseName);
        }

        #endregion
    }
}
