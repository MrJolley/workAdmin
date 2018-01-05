using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WorkAdmin.Logic;
using WorkAdmin.Models.ViewModels;
using WorkAdmin.Models;
using WorkAdmin.Models.Entities;
using System.Data;
using System.IO;
using System.Web.Routing;
using Newtonsoft.Json;
using WorkAdmin.Logic.HolidayLogic;
using NPOI.HSSF.UserModel;

namespace WorkAdmin.Site.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public User CurrentUser = new MyHttpUtilities().CurrentUser;
        public string LoginName = new MyHttpUtilities().LoginName;
        public ActionResult Index()
        {
            //return View();
            return RedirectToAction("WorkLog");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        public ActionResult WorkLog(WorkLogViewModel model)
        {
            //string selectedDate = Request.QueryString["month"];
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            if (!string.IsNullOrWhiteSpace(model.Month))
            {
                string[] arr = model.Month.Split("/".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                int yearTemp, monthTemp;
                if (int.TryParse(arr[0], out monthTemp) && int.TryParse(arr[1], out yearTemp) && monthTemp > 0 && yearTemp > 1900)
                {
                    year = yearTemp;
                    month = monthTemp;
                }
            }

            if (CurrentUser.IsManager)
                model = WorkLogService.GetAllUserWorkLogs(year, month, model.SortField, model.SortDirection, model.Filter);
            else
                model = WorkLogService.GetWorkLogs(CurrentUser.Id, year, month);
            return View(model);
        }


        public ActionResult WorkReport(WorkReportViewModel model)
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            if (!string.IsNullOrWhiteSpace(model.Month))
            {
                string[] arr = model.Month.Split("/".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                int yearTemp, monthTemp;
                if (int.TryParse(arr[0], out monthTemp) && int.TryParse(arr[1], out yearTemp) && monthTemp > 0 && yearTemp > 1900)
                {
                    year = yearTemp;
                    month = monthTemp;
                }
            }
            if (CurrentUser.IsManager)
                model = WorkReportService.GetAllUserWorkReports(year, month, model.SortField, model.SortDirection, model.Filter);
            else
                model = WorkReportService.GetWorkReports(CurrentUser.Id, year, month);
            return View(model);
        }

        public ActionResult UploadWorkFile()
        {
            if (!CurrentUser.IsManager)
                return RedirectToAction("Error", new { msg = "You are not authorized enough permission to view this page" });
            var fileTypeSelectItems = new List<SelectListItem>();
            foreach (EWorkFileType val in Enum.GetValues(typeof(EWorkFileType)))
            {
                fileTypeSelectItems.Add(new SelectListItem()
                {
                    Text = val.GetDescription(),
                    Value = val.ToString()
                });
            }
            ViewBag.FileTypeSelectItems = fileTypeSelectItems;

            return View(new UploadWorkFileViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadWorkFile(UploadWorkFileViewModel model)
        {
            if (ModelState.IsValid)
            {
                string localFileFullName = Path.GetTempFileName();
                try
                {
                    string[] arr = model.Month.Split("/".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                    int selectedYear = Convert.ToInt32(arr[1]);
                    int selectedMonth = Convert.ToInt32(arr[0]);

                    if (model.UploadedFile.ContentLength == 0)
                    {
                        ModelState.AddModelError("", "The uploaded file is empty");
                    }
                    else
                    {
                        string uploadFileName = model.UploadedFile.FileName;
                        localFileFullName = Path.ChangeExtension(localFileFullName, Path.GetExtension(uploadFileName));
                        model.UploadedFile.SaveAs(localFileFullName);

                        if (model.WorkFileType == EWorkFileType.WorkLog)
                        {
                            WorkLogUploadService service = new WorkLogUploadService(uploadFileName, localFileFullName, selectedYear, selectedMonth, LoginName);
                            ReturnResult rst = service.ReadFile();
                            if (rst.Succeeded)
                            {
                                WorkLogService.ClearWorkLogs(selectedYear, selectedMonth);
                                List<WorkLog> workLogs = service.GetWorkLogs();
                                WorkLogProperty workLogProperty = service.GetWorkLogProperty();
                                WorkLogService.SaveWorkLogs(workLogs);
                                WorkLogPropertyService.SaveWorkLogProperty(workLogProperty);

                                var dic = new RouteValueDictionary();
                                dic.Add("month", selectedMonth + "/" + selectedYear);
                                return RedirectToAction("WorkLog", dic);
                            }
                            else
                                ViewBag.ErrorMsg = rst.ErrorMsg;
                        }
                        else if (model.WorkFileType == EWorkFileType.WorkReport)
                        {
                            WorkReportUploadService service = new WorkReportUploadService(uploadFileName, localFileFullName, selectedYear, selectedMonth, LoginName);
                            ReturnResult rst = service.ReadFile();
                            if (rst.Succeeded)
                            {
                                WorkReportService.ClearWorkReports(selectedYear, selectedMonth);
                                List<WorkReport> workReports = service.GetWorkReports();
                                WorkReportService.SaveWorkReports(workReports);
                                WorkReportProperty workReportProperty = service.GetWorkReportProperty();
                                WorkReportPropertyService.SaveWorkReportProperty(workReportProperty);

                                var dic = new RouteValueDictionary();
                                dic.Add("month", selectedMonth + "/" + selectedYear);
                                return RedirectToAction("WorkReport", dic);
                            }
                            else
                                ViewBag.ErrorMsg = rst.ErrorMsg;
                        }
                        else
                            throw new Exception("No implemented service for " + model.WorkFileType.GetDescription());
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMsg = ex.Message;
                    if (ex.InnerException != null && ex.InnerException.InnerException != null)
                        ViewBag.ErrorMsg = ex.Message + " Inner Exception:" + ex.InnerException.InnerException.Message;
                }
                finally
                {
                    if (System.IO.File.Exists(localFileFullName))
                        System.IO.File.Delete(localFileFullName);
                }
            }
            var fileTypeSelectItems = new List<SelectListItem>();
            foreach (EWorkFileType val in Enum.GetValues(typeof(EWorkFileType)))
            {
                fileTypeSelectItems.Add(new SelectListItem()
                {
                    Text = val.GetDescription(),
                    Value = val.ToString()
                });
            }
            ViewBag.FileTypeSelectItems = fileTypeSelectItems;
            return View(model);
        }

        public ActionResult UserList(UserListViewModel model)
        {
            if (!CurrentUser.IsManager)
                return RedirectToAction("Error", new { msg = "You are not authorized enough the permission to view this page" });

            UserService.SyncNewUsersFromAdService(LoginName);
            model = UserService.GetIncludedUsers(model.Filter);
            return View(model);
        }

        public ActionResult ExcludedUserList()
        {
            if (!CurrentUser.IsManager)
                return RedirectToAction("Error", new { msg = "You are not authorized enough the permission to view this page" });

            List<User> listExludedUsers = UserService.GetExcludedUsers();
            return View(listExludedUsers);
        }

        public ActionResult UpdateUser(int id, string from = "UserList", string filter = null)
        {
            ViewBag.From = from;
            ViewBag.Filter = filter;
            var user = UserService.GetUser(id);
            var positions = UserService.GetUserPositions();
            ViewData["VDRankLevel"] = GetEnumDropdown(positions["职级"], user.rankLevel);
            ViewData["VDDepartment"] = GetEnumDropdown(positions["部门"], user.department);
            ViewData["VDProject"] = GetEnumDropdown(positions["项目"], user.project);
            ViewData["VDPosition"] = GetEnumDropdown(positions["职位"], user.position);
            return PartialView(user);
        }

        public List<SelectListItem> GetEnumDropdown(List<string> source, string selected)
        {
            List<SelectListItem> rlt = new List<SelectListItem>();
            foreach (string name in source)
            {
                rlt.Add(new SelectListItem() { Text = name, Value = name, Selected = selected == name });
            }
            return rlt;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateUser(User model, string from = "UserList", string filter = null)
        {
            var user = UserService.GetUser(model.Id);
            user.EmailAddress = model.EmailAddress;
            user.EnglishName = model.EnglishName;
            user.ChineseName = model.ChineseName;
            user.IsManager = model.IsManager;
            user.IsHeyiMember = model.IsHeyiMember;
            user.rankLevel = model.rankLevel;
            user.department = model.department;
            user.project = model.project;
            user.position = model.position;
            user.IsWorkingAtHome = model.IsWorkingAtHome;
            user.IsExcluded = model.IsExcluded;
            UserService.UpdateUser(user);
            return RedirectToAction(from, new { filter = filter });
        }

        public ActionResult Error(string msg)
        {
            return View(msg as object);
        }

        public ActionResult JiraWorkLog()
        {
            var model = new JiraWorkLogViewModel();
            JiraQueryService service = new JiraQueryService();
            model.JiraWorklogProject = service.GetJiraWorklogProject();
            model.JiraWorklogUsersName = service.GetJiraUsersName();
            ViewBag.firstShow = true;
            ViewBag.showOrHint = true;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult JiraWorkLog(JiraSearchInputViewModel inputModel)
        {
            JiraQueryService service = new JiraQueryService();
            var model = new JiraWorkLogViewModel();
            model.JiraWorklogProject = service.GetJiraWorklogProject();
            model.JiraWorklogUsersName = service.GetJiraUsersName();
            model.SearchInput = inputModel;
            ViewBag.firstShow = false;
            if (ModelState.IsValid)
            {
                List<JiraWorkLogDetail> modelUser = new List<JiraWorkLogDetail>();
                List<JiraWorklogTimeUsed> modelTimeUsed = new List<JiraWorklogTimeUsed>();
                List<JiraWorklogMember> modelMember = new List<JiraWorklogMember>();
                ViewBag.search = true;
                if (inputModel.SearchType == ESearchType.SearchUser)
                {
                    ViewBag.showOrHint = false;

                    if (string.IsNullOrWhiteSpace(inputModel.SelectedUser))
                    {
                        model.WorkLogDetails = service.GetJiraWorkLogsByUsers(inputModel.StartDate, inputModel.EndDate, ESortDirection.Desending);
                        model.WorkLogTimeUsedData = service.GetJiraWorklogTimeUsedUser(inputModel.StartDate, inputModel.EndDate);
                    }
                    else
                    {
                        model.WorkLogDetails = service.GetJiraWorkLogsByUser(inputModel.SelectedUser, inputModel.StartDate, inputModel.EndDate);
                        model.WorkLogTimeUsedData = service.GetJiraWorklogTimeUsedUser(inputModel.SelectedUser, inputModel.StartDate, inputModel.EndDate);
                    }

                }
                else if (inputModel.SearchType == ESearchType.SearchProject)
                {
                    ViewBag.showOrHint = true;
                    model.SearchInput.SelectedUser = "";
                    model.SearchInput.SelectedProject = inputModel.SelectedProject;
                    if (inputModel.SelectedProject == "All")
                    {
                        model.WorkLogDetails = service.GetJiraWorkLogsByUsers(inputModel.StartDate, inputModel.EndDate, ESortDirection.Desending);
                        model.WorkLogTimeUsedData = service.GetJiraWorklogTimeUsedProject(inputModel.StartDate, inputModel.EndDate);
                        model.WorkLogMembers = service.GetJiraWorklogMembers(inputModel.EndDate);
                    }
                    else
                    {
                        model.WorkLogDetails = service.GetJiraWorkLogsByProject(inputModel.SelectedProject, inputModel.StartDate, inputModel.EndDate);
                        model.WorkLogTimeUsedData = service.GetJiraWorklogTimeUsedProject(inputModel.SelectedProject, inputModel.StartDate, inputModel.EndDate);
                        model.WorkLogMembers = service.GetJiraWorklogMembers(inputModel.SelectedProject, inputModel.EndDate);
                    }
                }
                return View(model);
            }
            return View(model);
        }

        public ActionResult SalaryDetails()
        {
            return View(new SalaryDetailsViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SalaryDetails(SalaryDetailsViewModel inputModel)
        {
            SalaryDetailsViewModel outputModel = new SalaryDetailsViewModel();
            if (ModelState.IsValidField("Month") && ModelState.IsValidField("UploadedFile"))
            {
                string localFileFullName = Path.GetTempFileName();
                try
                {
                    string[] arr = inputModel.Month.Split("/".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                    string selectedYear = arr[1];
                    string selectedMonth = arr[0];
                    if (inputModel.UploadedFile.ContentLength == 0)
                    {
                        ViewBag.ErrorMsg = "The uploaded file is empty";
                        return View(inputModel);
                    }
                    else
                    {
                        Stream uploadFileStream = inputModel.UploadedFile.InputStream;
                        string uploadFileFullName = inputModel.UploadedFile.FileName;
                        string TableName = uploadFileFullName.Split(".".ToArray())[0];
                        SalaryUploadService service = new SalaryUploadService(uploadFileFullName, uploadFileStream, selectedYear, selectedMonth, LoginName);
                        ReturnResultTable rrt = service.ReadFile();

                        if (rrt.Succeeded)
                        {

                            DataTable dt = rrt.SalaryTable;
                            outputModel.SalaryTable = dt;
                            outputModel.TableName = TableName;
                            ViewBag.showTable = true;
                        }
                        else
                        {
                            ViewBag.ErrorMsg = rrt.ErrorMsg;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMsg = ex.Message;
                }
                ModelState.Clear();
                return View(outputModel);
            }
            else
                ViewBag.ErrorMsg = "the month or upload file is required";
            return View(outputModel);
        }

        public JsonResult SendMail(string[] head, string[][] body, string fromAddress, string fromPassword, string[] checkedValue, string selectMonth, string tableName, string emailStyle)
        {
            DataTable salaryTable = ConvertTable(head, body);
            List<string> allEmail = new List<string>();
            List<string> correctName = new List<string>();
            List<string> errorName = new List<string>();
            List<string> allName = new List<string>();
            for (int i = 0; i < checkedValue.Count(); i++)
            {
                try
                {
                    string name = MailService.GetAllName()[checkedValue[i]];
                    allEmail.Add(name);
                    allName.Add(checkedValue[i]);
                }
                catch (Exception)
                {
                    errorName.Add(checkedValue[i]);
                }
            }

            MailService mailService = new MailService();
            for (int i = 0; i < mailService.AddressName(allEmail).Count; i++)
            {
                try
                {
                    string emailName = MailService.GetAllName()[allName[i]];
                }
                catch (Exception)
                {
                    errorName.Add(checkedValue[i]);
                    continue;
                }
                string chineseName = allName[i];
                try
                {
                    mailService.MailConfiguration(fromAddress, fromPassword, mailService.AddressName(allEmail)[i], chineseName, salaryTable, selectMonth, tableName, emailStyle);
                    correctName.Add(chineseName);
                }
                catch (Exception)
                {
                    errorName.Add(chineseName);
                }
            }
            return Json(correctName);
        }

        public static DataTable ConvertTable(string[] ColumnNames, string[][] Arrays)
        {
            DataTable dt = new DataTable();

            foreach (string ColumnName in ColumnNames)
            {
                dt.Columns.Add(ColumnName, typeof(string));
            }

            for (int i = 0; i < Arrays.GetLength(0); i++)
            {
                DataRow dr = dt.NewRow();
                for (int j = 0; j < ColumnNames.Length; j++)
                {
                    dr[j] = Arrays[i][j].ToString();
                }
                dt.Rows.Add(dr);
            }
            return dt;

        }

        /// <summary>
        /// 调休，节假日时间核对表邮件系统
        /// </summary>
        /// <returns></returns>
        public ActionResult HolidayMail()
        {
            return View(new UserHoliday());
        }

        /// <summary>
        /// 调休，节假日excel上传服务
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult HolidayFileHandler()
        {
            object responseData = new object();
            string ErrorMsg = "";

            if (Request.Files.Count == 0)
            {
                ErrorMsg = "上传失败：请选择上传的文件，文件不能为空";
                return Json(ErrorMsg);
            }
            HttpPostedFileBase file = Request.Files[0];
            if (file.ContentLength > 0)
            {
                string fileExtend = Path.GetExtension(file.FileName);
                if (fileExtend.ToLower().StartsWith(".xls"))
                {
                    string fileType = Request.Form["fileType"].ToLower();
                    Stream stream = file.InputStream;
                    //年假数据读取
                    if (fileType.Equals("annual"))
                    {
                        FileUploadHelper helper = new FileUploadHelper(stream, file.FileName, LoginName);
                        var result = helper.ReadHolidayFile();
                        if (result.hasError)
                        {
                            ErrorMsg = result.ErrorMsg;
                            return Json(ErrorMsg);
                        }
                        else
                        {
                            //响应读取的数据结构
                            responseData = new
                            {
                                type = fileType,
                                data = result.result
                            };
                            return Json(responseData);
                        }
                    }
                    else if (fileType.Equals("transfer"))
                    {
                        FileUploadHelper helper = new FileUploadHelper(stream, file.FileName, LoginName);
                        var result = helper.ReadTransferFile();
                        if (result.hasError)
                        {
                            ErrorMsg = result.ErrorMsg;
                            return Json(ErrorMsg);
                        }
                        else
                        {
                            //响应读取的数据结构
                            responseData = new
                            {
                                type = fileType,
                                data = result.result
                            };
                            return Json(responseData);
                        }
                    }
                    else
                    {
                        ErrorMsg = "上传失败：文件类型有误，请至少选择一种文件类型";
                        return Json(ErrorMsg);
                    }
                }
                else
                {
                    ErrorMsg = "上传失败：请选择正确的文件类型，文件类型只能为.xls/.xlsx";
                    return Json(ErrorMsg);
                }
            }
            else
            {
                ErrorMsg = "上传失败：请选择正确的文件，文件内容不能为空";
                return Json(ErrorMsg);
            }
        }

        /// <summary>
        /// 调休，节假日邮件发送服务
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult MailSendingHandler()
        {
            string mailType = Request.Form["mailType"];
            string email = Request.Form["email"];
            string password = Request.Form["password"];
            string CCAddress = System.Configuration.ConfigurationManager.AppSettings["mailCCAddress"];

            var service = new HolidayTransferService();
            HolidayTransferMailResult result = new HolidayTransferMailResult();
            switch (mailType)
            {
                case "annual":
                    List<UserHoliday> luhl = JsonConvert.DeserializeObject<List<UserHoliday>>(Request.Form["staffList"]);

                    foreach (UserHoliday ul in luhl)
                    {
                        if (ul.PaidLeaveRemainingHours == 0)
                        {
                            continue;
                        }
                        string htmlData = string.Empty;
                        try
                        {
                            htmlData = service.HolidayDataConvert2Html(ul);
                        }
                        catch (Exception ex)
                        {
                            result.FailureList.Add(ul.StaffName);
                            result.FailureMsg.Add(ex.Message);
                            continue;
                        }
                        try
                        {
                            service.MailSending(email, password, ul.StaffEmail, htmlData, "annual", CCAddress);
                            result.SuccessList.Add(ul.StaffName);
                        }
                        catch (Exception ex)
                        {
                            result.FailureList.Add(ul.StaffName);
                            result.FailureMsg.Add(ex.Message);
                        }
                    }
                    break;
                case "transfer":
                    List<UserTransferList> lutl = JsonConvert.DeserializeObject<List<UserTransferList>>(Request.Form["staffList"]);
                    foreach (UserTransferList ul in lutl)
                    {
                        string htmlData = string.Empty;
                        var details = ul.UserTransferDetail.AsEnumerable().Where(r => r.TransferRemainingTime != 0);
                        if (details.Count() == 0)
                        {
                            continue;
                        }
                        try
                        {
                            htmlData = service.TransferDataConvert2Html(ul);
                        }
                        catch (Exception ex)
                        {
                            result.FailureList.Add(ul.StaffName);
                            result.FailureMsg.Add(ex.Message);
                            continue;
                        }
                        try
                        {
                            service.MailSending(email, password, ul.StaffEmail, htmlData, "transfer", CCAddress);
                            result.SuccessList.Add(ul.StaffName);
                        }
                        catch (Exception ex)
                        {
                            result.FailureList.Add(ul.StaffName);
                            result.FailureMsg.Add(ex.Message);
                        }
                    }
                    break;
                default:
                    result.FailureList.Add("系统错误");
                    result.FailureMsg.Add("邮件类型错误，请返回重新上传文件");
                    break;
            }
            return Json(result);
        }

        public ActionResult WorklogTime()
        {
            return View();
        }

        [HttpPost]
        public JsonResult WorklogTimeFileHandler()
        {
            HttpPostedFileBase file = Request.Files[0];
            int year = int.Parse(Request.Form["date"]);
            Stream stream = file.InputStream;
            FileUploadHelper helper = new FileUploadHelper(stream, file.FileName, LoginName);
            var result = helper.ReadWorklogFile(year);
            if (!result.hasError)
            {
                WorklogTimeService.ClearWorklogTimesRecord(result.logDates);
                WorklogTimeService.SaveWorkLogTimes(result.result);
                return Json("上传成功");
            }
            else
            {
                return Json("上传失败：" + result.ErrorMsg);
            }
        }

        public JsonResult GetWorklogFileExist()
        {
            int year = int.Parse(Request["year"]);
            int month = int.Parse(Request["month"]);
            DateTime dt = new DateTime(year, month, 1);
            return Json(WorklogTimeService.GetWorklogTimesExist(dt));
        }

        public ActionResult GetWorklogTimesFile()
        {
            int year = int.Parse(Request["year"]);
            int month = int.Parse(Request["month"]);
            DateTime dt = new DateTime(year, month, 1);
            using (MemoryStream ms = new MemoryStream())
            {
                var rlt = WorklogTimeService.GetWorklogTimesExcelModel(dt);
                rlt.Write(ms);
                return File(ms.ToArray(), "application/vnd.ms-excel", string.Format("{0}-{1}-worklog统计表.xlsx", year, month));
            }
        }

        public ActionResult ExportWorkReport()
        {
            return View();
        }

        public ActionResult GetWorkReportExist()
        {
            int year = int.Parse(Request["year"]);
            int month = int.Parse(Request["month"]);
            DateTime dt = new DateTime(year, month, 1);
            return Json(MailPopService.GetWorkReportExist(dt));
        }

        public ActionResult GetWorkReportFile()
        {
            int year = int.Parse(Request["year"]);
            int month = int.Parse(Request["month"]);
            DateTime dt = new DateTime(year, month, 1);
            using (MemoryStream ms = new MemoryStream())
            {
                var rlt = MailPopService.GetWorkReportExcelModel(dt);
                rlt.Write(ms);
                return File(ms.ToArray(), "application/vnd.ms-excel",
                    string.Format("Statistic of Work Report({0}年{1}月).xlsx", year, month));
            }
        }
    }
}