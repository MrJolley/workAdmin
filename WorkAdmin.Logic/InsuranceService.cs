using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkAdmin.Models.ViewModels;

namespace WorkAdmin.Logic
{
    public class InsuranceService
    {
        const string newLine = "<br />";
        /// <summary>
        /// 给员工发送邮件
        /// </summary>
        /// <param name="account">发件人账号</param>
        /// <param name="password">发件人密码</param>
        /// <param name="name">收件人姓名</param>
        /// <param name="content">收件内容-HTML格式</param>
        public void MailSending(string account, string password, string userAddress, string content, string year, string ccList = null)
        {
            MailConfiguration config = new MailConfiguration();
            var client = config.GetMailClient(account, password);

            string subject = year + "年社保及公积金基数调整";
            var message = config.GetMailMessage(subject, account, content, ccList);
            //配置收件人地址
            var address = config.GetMailAddress(userAddress);
            foreach (var mail in address)
            {
                message.To.Add(mail);
            }
            client.Send(message);
        }

        /// <summary>
        /// 社保基数HTML数据
        /// </summary>
        /// <param name="radix"></param>
        /// <returns></returns>
        public string RadixDataConvert2Html(InsuranceRadix.UserInfo radix, string year, string up, string down)
        {
            StringBuilder result = new StringBuilder();
            string name = radix.ChineseName;
            //数据结构转化为HTML表格
            DataTable detail = JsonConvert.DeserializeObject<DataTable>(
                JsonConvert.SerializeObject(new List<object> {
                    new {
                        name = radix.ChineseName,
                        income = radix.AunualIncome
                    }
                }));
            string data = RadixTableConvertHtml(detail, year);

            result.Append(up.Replace("\n", "<br />")).Append(newLine).Append(newLine)
                .Append(data).Append(newLine).Append(newLine).Append(down.Replace("\n", "<br />"));
            return result.ToString();
        }

        private string RadixTableConvertHtml(DataTable data, string year)
        {
            StringBuilder sb = new StringBuilder();
            int rows = data.Rows.Count;

            sb.Append("<table style='border-collapse:collapse;text-align:center;'>").Append("<tr>");
            sb.Append("<td style=\"border: 1px solid black; width: 100px; \">").Append("姓名").Append("</td>");
            sb.Append("<td style=\"border: 1px solid black; width:200px; \">").Append(year).Append("年度月平均工资").Append("</td>");
            sb.Append("<tr>");
            foreach (DataRow dr in data.Rows)
            {
                sb.Append("<tr>");
                for (int i = 0; i < data.Columns.Count; i++)
                {
                    sb.Append("<td style=\"border: 1px solid black;\">").Append(dr[i]).Append("</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("</table>");
            return
                sb.ToString();
        }
    }
}
