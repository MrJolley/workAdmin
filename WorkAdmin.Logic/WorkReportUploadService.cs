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
    public class WorkReportUploadService
    {
        #region Private Fields
        private string m_uploadFileName;
        private string m_fileFullName;
        private int m_month;
        private int m_year;
        private IWorkbook m_workbook;
        private ISheet m_sheet;
        private readonly string m_uploadFileNameMustContentLowered = "work report";
        private readonly string m_pinYinHeaderLowered = "英文名";
        private readonly string m_chineseNameHeaderLowered = "中文名";
        private readonly int m_maxNameLength = 50;
        private readonly string m_employeeTypeHeaderLowered = "人员性质";
        private readonly string m_fileAsOfDateIndicatorLowered = "统计截止时间";
        private readonly string m_fileCommentIndicatorLowered = "备注";
        private int m_userTypeColIndex = -1;
        private readonly List<string> m_listTypicalUserTypes = new List<string>() {"mentor","mentee" };
        private readonly List<string> m_listFileUserLeftIndicators = new List<string>() { "离职", "已离职" };
        private readonly List<string> m_listValidMorningTitles = new List<string> { "早"};
        private readonly List<string> m_listValidNoonTitles = new List<string>() { "中","中午"};
        private readonly List<string> m_listValidAfternoonTitles = new List<string>() { "下午"};
        private readonly List<string> m_listValidEveningTitles = new List<string>() { "晚"};
        private string m_loginName;

        private Dictionary<string, int> m_dicNameMapping = new Dictionary<string, int>();
        private Dictionary<int, string> m_dicChineseNameMapping = new Dictionary<int, string>();

        private List<WorkReport> m_workReports = new List<WorkReport>();
        private WorkReportProperty m_workReportProperty = new WorkReportProperty();
        #endregion

        public WorkReportUploadService(string uploadFileName,string fileFullName, int year, int month, string loginName = null)
        {
            this.m_uploadFileName = uploadFileName;
            this.m_fileFullName = fileFullName;
            this.m_year = year;
            this.m_month = month;
            this.m_loginName = loginName;
        }

        public List<WorkReport> GetWorkReports()
        {
            return this.m_workReports;
        }

        public WorkReportProperty GetWorkReportProperty()
        {
            return this.m_workReportProperty;
        }

        public ReturnResult ReadFile()
        {
            if (!this.m_uploadFileName.Replace(" ", "").ToLower().Contains(this.m_uploadFileNameMustContentLowered.Replace(" ", "")))
                return new ReturnResult() { Succeeded = false, ErrorMsg = string.Format("File name must contain words '{0}'", this.m_uploadFileNameMustContentLowered) };

            if (this.m_workbook == null)
                BuildWorkbook();

            string shtNameTarget = this.m_year + "." + this.m_month;
            this.m_sheet = this.m_workbook.GetSheet(shtNameTarget);
            if (this.m_sheet == null)
                return new ReturnResult() { Succeeded = false, ErrorMsg = string.Format("No sheet with name {0} was found", shtNameTarget) };

            var parseProperty = ParseUploadProperty();
            if (!parseProperty.Succeeded)
                return parseProperty;

            FillUserTypeColIndex();
            if (this.m_userTypeColIndex < 0)
                return new ReturnResult() { Succeeded = false, ErrorMsg = "Mentor, Mentee column is missing" };

            this.m_dicNameMapping = UserService.GetUserDictionary();
            List<string> listErrorUserNames = new List<string>();
            List<string> listErrorDates = new List<string>();
            IRow titleDateRow = null;
            IRow titleRow = null;
            int nameColIndex = -1;
            int pinYinColIndex = -1;
            int chineseNameColIndex = -1;
            int employeeTypeColIndex = -1;

            int maxCellNumber = 0;
            string currentUserType = "";

            bool isFinished = false;
            for (int rowIndex = this.m_sheet.FirstRowNum; rowIndex < this.m_sheet.LastRowNum&&!isFinished; rowIndex++)
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
                        if (cellVal == this.m_pinYinHeaderLowered)
                        {
                            titleRow = row;
                            titleDateRow = this.m_sheet.GetRow(rowIndex - 1);
                            pinYinColIndex = colIndex;
                        }
                        else if (cellVal == this.m_chineseNameHeaderLowered)
                        {
                            chineseNameColIndex = colIndex;
                            if (titleRow == null)
                            {
                                titleRow = row;
                                titleDateRow = this.m_sheet.GetRow(rowIndex - 1);
                            }                                
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

                var userTypeVal = row.GetCell(this.m_userTypeColIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                if (string.IsNullOrWhiteSpace(currentUserType) && string.IsNullOrWhiteSpace(userTypeVal))
                    return new ReturnResult { Succeeded = false, ErrorMsg = "User type is not correctly specified" };
                if (!string.IsNullOrWhiteSpace(userTypeVal))
                    currentUserType = userTypeVal;

                WorkReport workReport = null;
                DateTime currAsOfDate;
                for (int colIndex = row.FirstCellNum; colIndex <maxCellNumber; colIndex++)
                {
                    string titleVal = titleRow.GetCell(colIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                    string titleDateVal = titleDateRow.GetCell(colIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                    if (string.IsNullOrWhiteSpace(titleVal))
                        continue;
                    if (!this.m_listValidMorningTitles.Contains(titleVal) && !this.m_listValidNoonTitles.Contains(titleVal)
                        && !this.m_listValidAfternoonTitles.Contains(titleVal) && !this.m_listValidEveningTitles.Contains(titleVal))
                        continue;
                    if (!string.IsNullOrWhiteSpace(titleDateVal) && titleDateVal.StartsWith(this.m_month + "."))
                    {
                        try
                        {
                            string[] arrTemp = titleDateVal.Split(".".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                            DateTime date = new DateTime(this.m_year, this.m_month, Convert.ToInt32(arrTemp[1]));
                            currAsOfDate = date;
                            if (workReport != null)
                                this.m_workReports.Add(workReport);
                            workReport = new WorkReport();
                            workReport.UserId = m_dicNameMapping[userName];
                            if (employeeTypeColIndex >= 0)
                                workReport.EmployeeType = row.GetCell(employeeTypeColIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                            workReport.AsOfDate = currAsOfDate;
                            workReport.UserType = currentUserType;
                            workReport.SortOrder = this.m_workReports.Count + 1;
                            workReport.CreatedBy = this.m_loginName;
                            workReport.CreatedTime = DateTime.Now;
                            workReport.UpdatedBy = this.m_loginName;
                            workReport.UpdatedTime = DateTime.Now;
                        }
                        catch (Exception ex)
                        {
                            listErrorDates.Add(titleDateVal);
                            continue;
                        }
                    }
                    if (workReport == null)
                        continue;
                    string cellVal = row.GetCell(colIndex, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                    if (this.m_listValidMorningTitles.Contains(titleVal))
                        workReport.MorningReportAbsent = !string.IsNullOrWhiteSpace(cellVal);
                    else if (this.m_listValidNoonTitles.Contains(titleVal))
                        workReport.NoonReportAbsent = !string.IsNullOrWhiteSpace(cellVal);
                    else if (this.m_listValidAfternoonTitles.Contains(titleVal))
                        workReport.AfternoonReportAbsent = !string.IsNullOrWhiteSpace(cellVal);
                    else if (this.m_listValidEveningTitles.Contains(titleVal))
                        workReport.EveningReportAbsent = !string.IsNullOrWhiteSpace(cellVal);
                }
                if (workReport != null)
                    this.m_workReports.Add(workReport);
            }

            if (listErrorUserNames.Count > 0)
            {
                string errNames = string.Join(",", listErrorUserNames.Distinct().ToArray());
                return new ReturnResult() { Succeeded = false, ErrorMsg = "Wrong names found: " + errNames };
            }
            if (listErrorDates.Count > 0)
            {
                string errDates = string.Join(",", listErrorDates.Distinct().ToArray());
                return new ReturnResult() { Succeeded = false, ErrorMsg = "Wrong dates found: " + errDates };
            }

            var duplicateRecords = this.m_workReports.GroupBy(r => new { r.UserId, r.AsOfDate, r.UserType }).Where(r => r.Count() > 1);
            if (duplicateRecords.Count() > 0)
            {
                List<string> lstDuplicate = new List<string>();
                var distinctUsers = duplicateRecords.Select(r => r.Key.UserId).Distinct();
                foreach (var userId in distinctUsers)
                {
                    var userName = m_dicNameMapping.Where(r => r.Value == userId).First().Key;
                    lstDuplicate.Add(userName);
                }
                return new ReturnResult { Succeeded = false, ErrorMsg = "Duplicate records found for:" + string.Join(",", lstDuplicate.ToArray()) };
            }

            if (this.m_workReports.Count == 0)
                return new ReturnResult() { Succeeded = false, ErrorMsg = "No available work report found" };

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
            this.m_workReportProperty.Year = this.m_year;
            this.m_workReportProperty.Month = this.m_month;
            this.m_workReportProperty.CreatedBy = this.m_loginName;
            this.m_workReportProperty.CreatedTime = DateTime.Now;
            this.m_workReportProperty.UpdatedBy = this.m_loginName;
            this.m_workReportProperty.UpdatedTime = DateTime.Now;
            bool commentStarted=false;
            bool commentEnded=false;
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
                        if (DateTime.TryParse(strAsOfDate,out asOfDate))
                        {
                            this.m_workReportProperty.AsOfDate = asOfDate;
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
                            if (this.m_workReportProperty.Comment == null)
                                this.m_workReportProperty.Comment = comment;
                            else
                                this.m_workReportProperty.Comment += "<br/>" + comment;
                        }
                        break;
                    }
                }

                if (!isComment && commentStarted)
                    commentEnded = true;

                if (commentEnded && asOfDateParsed)
                    allParsed = true;
            }

            if (this.m_workReportProperty.AsOfDate == DateTime.MinValue)
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

        private void FillUserTypeColIndex()
        {
            for (int rowIdx = this.m_sheet.FirstRowNum; rowIdx < this.m_sheet.LastRowNum && this.m_userTypeColIndex < 0; rowIdx++)
            {
                IRow row = this.m_sheet.GetRow(rowIdx);
                if (row == null)
                    continue;
                for (int colIdx = row.FirstCellNum; colIdx < row.LastCellNum; colIdx++)
                {
                    string cellVal = row.GetCell(colIdx, MissingCellPolicy.CREATE_NULL_AS_BLANK).ToString().Trim();
                    if (cellVal == "")
                        continue;
                    if (this.m_listTypicalUserTypes.Contains(cellVal, StringComparer.InvariantCultureIgnoreCase))
                    {
                        this.m_userTypeColIndex = colIdx;
                        break;
                    }
                }
            }
        }

        #endregion
    }
}
