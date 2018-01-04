using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkAdmin.Models.Entities;

namespace WorkAdmin.Logic
{
    public class WorkReportPropertyService
    {
        public static int SaveWorkReportProperty(WorkReportProperty property)
        {
            using (MyDbContext db = new MyDbContext())
            {
                db.WorkReportProperties.Add(property);
                return db.SaveChanges();
            }
        }

        public static WorkReportProperty GetWorkReportProperty(int year, int month)
        {
            using (MyDbContext db = new MyDbContext())
            {
                return db.WorkReportProperties.Find(year,month);
            }
        }
    }
}
