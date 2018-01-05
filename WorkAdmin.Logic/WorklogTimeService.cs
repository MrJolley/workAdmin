using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkAdmin.Models.Entities;
using System.Data.SqlClient;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using System.Data;
using NPOI.SS.Util;

namespace WorkAdmin.Logic
{
    public class WorklogTimeService
    {
        public static int SaveWorkLogTimes(IEnumerable<WorklogTime> workLogTimes)
        {
            using (MyDbContext db = new MyDbContext())
            {
                db.Record.AddRange(workLogTimes);
                return db.SaveChanges();
            }
        }

        public static int ClearWorklogTimesRecord(IEnumerable<DateTime> logDates)
        {
            if (logDates.Count() > 0)
            {
                using (MyDbContext db = new MyDbContext())
                {
                    foreach (DateTime dt in logDates)
                    {
                        string sql = @"DELETE FROM [dbo].[WorklogTimes] WHERE logDate = @dt";
                        db.Database.ExecuteSqlCommand(sql, new SqlParameter("@dt", dt));
                    }
                    return db.SaveChanges();
                }
            }
            return 0;
        }

        public static List<string> GetHeyiStaff()
        {
            using (MyDbContext db = new MyDbContext())
            {
                string sql = @"SELECT [ChineseName] FROM [dbo].[Users] WHERE IsHeyiMember = 1 AND [ChineseName] IS NOT NULL";
                return db.Database.SqlQuery<string>(sql).ToList();
            }
        }

        public static List<WorklogTime> GetWorklogTimesRecord(DateTime dt)
        {
            using (MyDbContext db = new MyDbContext())
            {
                string sql = @"SELECT * FROM [dbo].[WorklogTimes] WHERE year(logDate) = @year and month(logDate) = @month";
                return db.Database.SqlQuery<WorklogTime>(sql, new SqlParameter("@year", dt.Year), new SqlParameter("@month", dt.Month)).ToList();
            }
        }

        #region 文件下载
        public static bool GetWorklogTimesExist(DateTime dt)
        {
            var data = GetWorklogTimesRecord(dt);
            if (data.Count > 0)
            {
                return true;
            }
            return false;
        }

        public static XSSFWorkbook GetWorklogTimesExcelModel(DateTime dt)
        {
            var data = GetWorklogTimesRecord(dt);
            var curMonth = data.AsEnumerable().Select(r => r.logDate).Distinct().ToList();
            var workbook = BuildExcelData(data, curMonth);
            return workbook;
        }

        public static XSSFWorkbook BuildExcelData(List<WorklogTime> records, List<DateTime> logDates)
        {
            SheetStylePattern styles = new SheetStylePattern();
            XSSFWorkbook workbook = styles.book;
            foreach (DateTime logDate in logDates)
            {
                string sheetName = string.Format("{0}.{1}", logDate.Month, logDate.Day);
                //构造表头部分
                ISheet sheet = workbook.CreateSheet(sheetName);
                BuildRowData(sheet.CreateRow(0), colName.Keys, styles.titleStyle);
                //构造表内容记录
                var list = records.Where(r => r.logDate == logDate).ToList();
                double totalTime = list.Sum(r => r.worklog);
                for (int i = 0; i < list.Count(); i++)
                {
                    BuildRowData(sheet.CreateRow(i + 1),
                        new List<string>() { 
                        list[i].userName,
                        list[i].rank,
                        list[i].department,
                        list[i].position,
                        list[i].project,
                        list[i].worklog.ToString(),
                        }, styles.textStyle);
                }
                //构造表尾时间总和
                BuildRowData(sheet.CreateRow(list.Count + 1), new string[] { "", "", "", "", "", totalTime.ToString() }, styles.textStyle);
                sheet.SetColumnWidth(0, 10 * 256);
            }

            List<WorklogTime> lianheRecords = new List<WorklogTime>();
            List<WorklogTime> heyiRecords = new List<WorklogTime>();
            var heyiNames = GetHeyiStaff();
            foreach (var record in records)
            {
                if (heyiNames.Contains(record.userName))
                {
                    heyiRecords.Add(record);
                }
                else
                {
                    lianheRecords.Add(record);
                }
            }
            List<string> logShortDate = new List<string>();
            foreach (DateTime logDate in logDates)
            {
                logShortDate.Add(string.Format("{0}.{1}", logDate.Month, logDate.Day));
            }
            //联和统计表数据
            var lianheStaffs = lianheRecords.AsEnumerable().Select(r => r.userName).Distinct().ToList();
            var lianhePortfolio = lianheRecords.AsEnumerable().OrderBy(r => r.portfolio).Select(r => r.portfolio).Distinct().ToList();
            DataSet lianheData = GetMonthlyTimeslog(lianheStaffs, lianhePortfolio, lianheRecords, logDates);
            DataSet lianheTotal = GetMonthlyTotalTimeslog(lianheStaffs, lianhePortfolio, lianheRecords);
            GetAllMonthlyDataStatistic(workbook, "联和", lianhePortfolio, logShortDate, lianheStaffs, lianheData, lianheTotal, styles);
            //和逸统计表数据
            var heyiStaffs = heyiRecords.AsEnumerable().Select(r => r.userName).Distinct().ToList();
            var heyiPortfolio = heyiRecords.AsEnumerable().OrderBy(r => r.portfolio).Select(r => r.portfolio).Distinct().ToList();
            DataSet heyiData = GetMonthlyTimeslog(heyiStaffs, heyiPortfolio, heyiRecords, logDates);
            DataSet heyiTotal = GetMonthlyTotalTimeslog(heyiStaffs, heyiPortfolio, heyiRecords);
            GetAllMonthlyDataStatistic(workbook, "和逸", heyiPortfolio, logShortDate, heyiStaffs, heyiData, heyiTotal, styles);

            return workbook;
        }

        public static DataSet GetMonthlyTimeslog(List<string> staffs, List<string> portfolios, List<WorklogTime> records, List<DateTime> logDates)
        {
            DataSet ds = new DataSet();
            foreach (DateTime logDate in logDates)
            {
                DataTable dt = new DataTable();
                dt.TableName = string.Format("{0}.{1}", logDate.Month, logDate.Day);
                foreach (string portfolio in portfolios)
                {
                    dt.Columns.Add(portfolio, typeof(Double));
                }
                var list = records.Where(r => r.logDate == logDate).ToList();
                //每个员工当日的详细worklog时间
                foreach (string staff in staffs)
                {
                    var detail = list.AsEnumerable().Where(r => r.userName == staff).Select(r => new { worklog = r.worklog, portfolio = r.portfolio });
                    if (detail != null)
                    {
                        DataRow dr = dt.NewRow();
                        foreach (var record in detail.ToList())
                        {
                            dr[record.portfolio] = record.worklog;
                        }
                        dt.Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                    }
                }
                //每个组合的总时间
                DataRow totalRow = dt.NewRow();
                double sum = 0.0;
                foreach (string portfolio in portfolios)
                {
                    var detail = list.AsEnumerable().Where(r => r.portfolio == portfolio).Select(r => r.worklog);
                    if (detail != null)
                    {
                        totalRow[portfolio] = detail.Sum();
                        sum += detail.Sum();
                    }
                    else
                    {
                        totalRow[portfolio] = 0.0;
                    }
                }
                dt.Rows.Add(totalRow);
                //所有组合总时间
                DataRow sumRow = dt.NewRow();
                sumRow[portfolios.Count - 1] = sum;
                dt.Rows.Add(sumRow);
                ds.Tables.Add(dt);
            }
            return ds;
        }

        public static DataSet GetMonthlyTotalTimeslog(List<string> staffs, List<string> portfolios, List<WorklogTime> records)
        {
            DataSet ds = new DataSet();
            //总时间统计
            DataTable total = new DataTable();
            total.TableName = "Total";
            foreach (string portfolio in portfolios)
            {
                total.Columns.Add(portfolio, Type.GetType("System.String"));
            }
            //工时比
            DataTable ratio = new DataTable();
            ratio.TableName = "工时比";
            foreach (string portfolio in portfolios)
            {
                ratio.Columns.Add(portfolio, Type.GetType("System.String"));
            }
            foreach (string staff in staffs)
            {
                var detail = records.AsEnumerable().Where(r => r.userName == staff).GroupBy(r => r.portfolio)
                    .Select(r => new { worklogs = r.Sum(x => x.worklog), portfolio = r.Key });
                if (detail != null)
                {
                    double personSum = detail.Sum(r => r.worklogs);
                    DataRow drtotal = total.NewRow();
                    DataRow drratio = ratio.NewRow();
                    foreach (var record in detail.ToList())
                    {
                        drtotal[record.portfolio] = record.worklogs;
                        drratio[record.portfolio] = (record.worklogs / personSum).ToString("P2");
                    }
                    total.Rows.Add(drtotal);
                    ratio.Rows.Add(drratio);
                }
                else
                {
                    throw new ApplicationException("未找到匹配的记录");
                }
            }
            //总时间统计中每个组合的统计
            //空白行
            DataRow blankA = ratio.NewRow();
            ratio.Rows.Add(blankA);
            DataRow blankB = ratio.NewRow();
            ratio.Rows.Add(blankB);
            //添加工时比倒数第二行的统计数据
            DataTable ratioM = GetModifiedRatioTable(ratio);
            //每个组合的总时间和百分比
            DataRow totalRow = total.NewRow();
            DataRow percentRow = total.NewRow();
            double sum = records.AsEnumerable().Sum(r => r.worklog);
            foreach (string portfolio in portfolios)
            {
                var detail = records.AsEnumerable().Where(r => r.portfolio == portfolio).Select(r => r.worklog);
                if (detail != null)
                {
                    totalRow[portfolio] = detail.Sum();
                    percentRow[portfolio] = (detail.Sum() / sum).ToString("P2");
                }
                else
                {
                    throw new ApplicationException("未找到匹配的记录");
                }
            }
            total.Rows.Add(totalRow);
            total.Rows.Add(percentRow);
            ds.Tables.Add(total);
            ds.Tables.Add(ratioM);
            return ds;
        }

        public static void GetAllMonthlyDataStatistic(IWorkbook workbook, string companyName, List<string> portfolio,
            List<string> logShortDate, List<string> staffs, DataSet data, DataSet total, SheetStylePattern styles)
        {
            //构建sheet表
            ISheet sheet = workbook.CreateSheet(companyName);
            //构建日期行，合并相应单元格
            var firstRow = sheet.CreateRow(0);
            SetCellValueWithStyle(firstRow.CreateCell(0), "", styles.markStyle);
            //统一列样式
            sheet.SetDefaultColumnStyle(0, styles.markStyle);
            int portfolioCount = portfolio.Count;
            for (int i = 0; i < logShortDate.Count; i++)
            {
                SetCellValueWithStyle(firstRow.CreateCell(1 + i * portfolioCount), logShortDate[i], i % 2 == 1 ? styles.alterOddStyle : styles.alterEvenStyle);
                //统一列样式
                for (int s = 1 + i * portfolioCount; s < 1 + (i + 1) * portfolioCount; s++)
                {
                    sheet.SetDefaultColumnStyle(s, i % 2 == 1 ? styles.alterOddStyle : styles.alterEvenStyle);
                }
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 1 + i * portfolioCount, (i + 1) * portfolioCount));
            }
            SetCellValueWithStyle(firstRow.CreateCell(1 + logShortDate.Count * portfolioCount), "Total", styles.totalStyle);
            //统一列样式
            for (int s = 1 + logShortDate.Count * portfolioCount; s < 1 + (logShortDate.Count + 1) * portfolioCount; s++)
            {
                sheet.SetDefaultColumnStyle(s, styles.totalStyle);
            }
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 1 + logShortDate.Count * portfolioCount, (logShortDate.Count + 1) * portfolioCount));
            SetCellValueWithStyle(firstRow.CreateCell(1 + (logShortDate.Count + 1) * portfolioCount), "工时比", styles.totalPercentStyle);
            for (int s = 1 + (logShortDate.Count + 1) * portfolioCount; s < 1 + (logShortDate.Count + 2) * portfolioCount; s++)
            {
                sheet.SetDefaultColumnStyle(s, styles.totalPercentStyle);
            }
            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 1 + (logShortDate.Count + 1) * portfolioCount, ((logShortDate.Count + 1) + 1) * portfolioCount));
            //构建标题行，包括部门，职位，项目
            List<string> departmentColName = new List<string>();
            List<string> positionColName = new List<string>();
            List<string> projectColName = new List<string>();
            foreach (string item in portfolio)
            {
                string[] sperate = item.Split("*".ToArray());
                departmentColName.Add(sperate[0]);
                projectColName.Add(sperate[1]);
                positionColName.Add(sperate[2]);
            }
            for (int j = 0; j < colTitle.Length; j++)
            {
                var row = sheet.CreateRow(j + 1);
                SetCellValueWithStyle(row.CreateCell(0), colTitle[j], j == 2 ? styles.markBorderBoldStyle : styles.markBoldStyle);
                var colNames = j == 0 ? departmentColName : j == 1 ? projectColName : positionColName;
                for (int i = 0; i < logShortDate.Count; i++)
                {
                    var style = j == 2 ? (i % 2 == 1 ? styles.alterBorderOddStyle : styles.alterBorderEvenStyle) : (i % 2 == 1 ? styles.alterOddStyle : styles.alterEvenStyle);
                    int start = colNames.Count * i + 1;
                    SetCellSeriesData(row, start, colNames, style);
                }
                SetCellSeriesData(row, colNames.Count * logShortDate.Count + 1, colNames, j == 2 ? styles.totalBorderStyle : styles.totalStyle);
                SetCellSeriesData(row, colNames.Count * (logShortDate.Count + 1) + 1, colNames, j == 2 ? styles.totalBorderPercentStyle : styles.totalPercentStyle);
            }
            //构建统计记录行，包括所有员工名和工时
            for (int y = 0; y < staffs.Count; y++)
            {
                var row = sheet.CreateRow(y + 4);
                var style = y == staffs.Count - 1 ? styles.markBorderStyle : styles.markStyle;
                SetCellValueWithStyle(row.CreateCell(0), staffs[y], style);
            }
            SetCellValueWithStyle(sheet.CreateRow(staffs.Count + 5).CreateCell(0), "", styles.markBorderStyle);

            //数据列
            for (int i = 0; i < logShortDate.Count; i++)
            {
                DataTable dt = data.Tables[logShortDate[i]];
                int start = dt.Columns.Count * i + 1;
                SetCellSeriesData(sheet, start, dt, i % 2 == 1 ? styles.alterOddStyle : styles.alterEvenStyle, i % 2 == 1 ? styles.alterBorderOddStyle : styles.alterBorderEvenStyle);
            }
            //总计和工时比
            int totalStart = data.Tables[0].Columns.Count * logShortDate.Count + 1;
            SetCellSeriesData(sheet, totalStart, total.Tables[0], styles.totalStyle, styles.totalBorderStyle);
            int ratioStart = totalStart + data.Tables[0].Columns.Count;
            SetCellSeriesData(sheet, ratioStart, total.Tables[1], styles.totalPercentStyle, styles.totalBorderPercentStyle);
            //冻结某些列和某些行
            sheet.CreateFreezePane(1, 4, 1, 4);

            //sheet.SetDefaultColumnStyle()
            //return new DataTable();
        }

        public static IRow BuildRowData<T>(IRow row, IEnumerable<T> rowData, ICellStyle style)
        {
            var data = rowData.ToList();
            for (int i = 0; i < data.Count; i++)
            {
                if (i == data.Count - 1)
                {
                    double val = 0;
                    if (double.TryParse(data[i].ToString(), out val))
                    {
                        row.CreateCell(i).SetCellValue(val);
                        row.GetCell(i).CellStyle = style;
                        continue;
                    }
                }
                row.CreateCell(i).SetCellValue(data[i].ToString());
                row.GetCell(i).CellStyle = style;
            }
            return row;
        }

        public static DataTable GetModifiedRatioTable(DataTable ratio)
        {
            int count = ratio.Rows.Count;
            if (count > 2)
            {
                foreach (DataColumn item in ratio.Columns)
                {
                    string colName = item.ColumnName;
                    double total = 0;
                    foreach (DataRow row in ratio.Rows)
                    {
                        string val = row[colName].ToString();
                        if (!string.IsNullOrEmpty(val) && double.TryParse(val.Replace("%", ""), out double time))
                        {
                            total += time/100;
                        }
                    }
                    ratio.Rows[count - 2][colName] = total.ToString("N3");
                }
            }
            return ratio;
        }

        public static ICell SetCellValueWithStyle(ICell cell, string value, ICellStyle style)
        {
            cell.CellStyle = style;
            cell.SetCellValue(value);
            return cell;
        }

        public static ICell SetCellValueWithStyle(ICell cell, double value, ICellStyle style)
        {
            cell.CellStyle = style;
            if (value == 0)
            {
                cell.SetCellValue("-");
            }
            else
            {
                cell.SetCellValue(value);
            }
            return cell;
        }

        public static IRow SetCellSeriesData(IRow row, int start, List<string> data, ICellStyle style)
        {
            int length = data.Count;
            for (int i = 0; i < length; i++)
            {
                SetCellValueWithStyle(row.CreateCell(i + start), data[i], style);
            }
            return row;
        }

        public static ISheet SetCellSeriesData(ISheet sheet, int start, DataTable data, ICellStyle style, ICellStyle borderStyle = null)
        {
            int length = data.Columns.Count;
            int rows = data.Rows.Count;
            for (int j = 0; j < rows; j++)
            {
                var spStyle = style;
                if ((j == rows - 1 || j == rows - 3) && borderStyle != null)
                {
                    spStyle = borderStyle;
                }
                var row = sheet.GetRow(j + 4);
                if (row == null)
                {
                    row = sheet.CreateRow(j + 4);
                }
                for (int i = 0; i < length; i++)
                {
                    if (Convert.IsDBNull(data.Rows[j][i]))
                    {
                        SetCellValueWithStyle(row.CreateCell(i + start), "-", spStyle);
                    }
                    else
                    {
                        double val = 0.00;
                        string text = data.Rows[j][i].ToString();
                        if (double.TryParse(text, out val))
                        {
                            SetCellValueWithStyle(row.CreateCell(i + start), val, spStyle);
                        }
                        else
                        {
                            SetCellValueWithStyle(row.CreateCell(i + start), text, spStyle);
                        }
                    }
                }
            }
            return sheet;
        }

        public static Dictionary<string, int> colName =
            new Dictionary<string, int>() 
        {
            {"姓名", 0},
            {"职级", 1},
            {"部门", 2},
            {"职位", 3},
            {"项目", 4},
            {"工作时间", 5},
        };

        public static string[] colTitle =
            new string[] 
        {
            "部门", "项目", "职位"
        };
        #endregion
    }
}
