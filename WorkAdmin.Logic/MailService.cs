using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using WorkAdmin.Models.Entities;
using WorkAdmin.Logic;
using System.Data;
using System.Configuration;

namespace WorkAdmin.Logic
{
    public class MailService
    {
        public void MailConfiguration(string address, string password, MailAddress toAddress, string receiver, DataTable salaryTable, string month, string tableName, string emailStyle)
        {
            MailMessage mailContent = new MailMessage();
            mailContent.To.Add(toAddress);
            string displayName = ConfigurationManager.AppSettings["salaryMailSenderDisplayName"];
            string signatureName = ConfigurationManager.AppSettings["salaryMailSenderSignature"];
            mailContent.From = new MailAddress(address, displayName);
            List<string> title = new List<string>();
            List<string> content = new List<string>();
            DataTable dt = new DataTable();
            DataTable dtNew = new DataTable();

            foreach (DataColumn col in salaryTable.Columns)
            {
                dt.Columns.Add(col.ColumnName);
            }

            for (int i = 0; i < salaryTable.Rows.Count; i++)
            {
                if (salaryTable.Rows[i][1].ToString() == receiver)
                {
                    DataRow dr = dt.NewRow();
                    for (int j = 0; j < salaryTable.Columns.Count; j++)
                    {
                        dr[j] = salaryTable.Rows[i][j].ToString();
                    }
                    dt.Rows.Add(dr);
                }
            }

            dtNew.Columns.Add("ColumnName", typeof(string));
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dtNew.Columns.Add("Column" + (i + 1).ToString(), typeof(string));
            }
            foreach (DataColumn dc in dt.Columns)
            {
                DataRow drNew = dtNew.NewRow();
                drNew["ColumnName"] = dc.ColumnName;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    drNew[i + 1] = dt.Rows[i][dc].ToString();
                }
                dtNew.Rows.Add(drNew);
            }

            string[] monthFilter = month.Split(@"/".ToArray());
            string monthShow = monthFilter[0];
            string yearShow = monthFilter[1];
            string mailBody = "";
            mailContent.Subject = tableName;
            mailBody = emailStyle.Replace("\n", "<br />") + "<table style='border-collapse:collapse;'>";

            for (int i = 0; i < dtNew.Rows.Count; i++)
            {
                mailBody += "<tr>";
                for (int j = 0; j < dtNew.Columns.Count; j++)
                {
                    mailBody += "<td style='border: 1px solid red'>" + dtNew.Rows[i][j] + "</td>";
                }
                mailBody += "</tr>";
            }

            mailBody += "</table><p>BR,</p><p>" + signatureName  + "</p>";
            mailContent.Body = mailBody;
            mailContent.BodyEncoding = System.Text.Encoding.UTF8;
            mailContent.IsBodyHtml = true;
            mailContent.Priority = MailPriority.Normal;
            mailContent.Headers.Add("X-MimeOLE", "Produced By Microsoft MimeOLE V6.00.2900.2869");
            mailContent.Headers.Add("ReturnReceipt", "1");
            SmtpClient mailClient = new SmtpClient();
            mailClient.UseDefaultCredentials = false;
            mailClient.Host = "smtp.263.net";
            mailClient.Credentials = new System.Net.NetworkCredential(address, password);
            mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            mailClient.Send(mailContent);
        }

        public List<MailAddress> AddressName(List<string> addressName)
        {
            List<MailAddress> receivers = new List<MailAddress>();
            foreach (string addressDetail in addressName)
            {
                MailAddress receiver = new MailAddress(addressDetail);
                receivers.Add(receiver);
            }
            return receivers;
        }

        public static Dictionary<string, string> GetAllName()
        {

            var listUsers = UserService.GetAllUsers();
            Dictionary<string, string> dic = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var user in listUsers)
            {
                if (!string.IsNullOrWhiteSpace(user.ChineseName) && !dic.ContainsKey(user.ChineseName))
                    dic.Add(user.ChineseName, user.EmailAddress);
            }
            return dic;
        }
    }
}
