using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkAdmin.Models.Entities;

namespace WorkAdmin.Logic
{
    public class WorkLogPropertyService
    {
        public static int SaveWorkLogProperty(WorkLogProperty property)
        {
            using (MyDbContext db = new MyDbContext())
            {
                db.WorkLogProperties.Add(property);
                return db.SaveChanges();
            }
        }

        public static WorkLogProperty GetWorkLogProperty(int year, int month)
        {
            using (MyDbContext db = new MyDbContext())
            {
                return db.WorkLogProperties.Find(year,month);
            }
        }
    }
}
