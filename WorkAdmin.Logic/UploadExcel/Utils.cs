using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

namespace WorkAdmin.Logic.UploadExcel
{
    public class Utils
    {
        /// <summary>
        /// 读取内存中文件，并转化为NPOI EXCEL格式
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="file">文件流</param>
        /// <returns></returns>
        public static IWorkbook BuildWorkbook(string fileName, Stream file)
        {
            IWorkbook workbook;
            string fileExt = Path.GetExtension(fileName);
            switch (fileExt)
            {
                case ".xls":
                    workbook = new HSSFWorkbook(file);
                    break;
                case ".xlsx":
                    workbook = new XSSFWorkbook(file);
                    break;
                default:
                    throw new Exception("Excel文件格式错误：" + fileName);
            }
            return workbook;
        }
    }
}
