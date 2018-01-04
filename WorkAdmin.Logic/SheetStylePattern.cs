using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using System.Drawing;

namespace WorkAdmin.Logic
{
    public class SheetStylePattern
    {
        public SheetStylePattern()
        {
            book = new XSSFWorkbook();
            titleStyle = _titleStyle();
            textStyle = _textStyle();
            markStyle = _markStyle();
            alterOddStyle = _alterOddStyle();
            alterEvenStyle = _alterEvenStyle();
            totalStyle = _totalStyle();
            totalPercentStyle = _totalPercentStyle();
            markBoldStyle = _markBoldStyle();
            //border
            textBorderStyle = _textBorderStyle();
            
            markBorderStyle = _markBorderStyle();
            
            markBorderBoldStyle = _markBorderBoldStyle();
            
            alterBorderOddStyle = _alterBorderOddStyle();
            
            alterBorderEvenStyle = _alterBorderEvenStyle();
            
            totalBorderStyle = _totalBorderStyle();

            totalBorderPercentStyle = _totalBorderPercentStyle();
        }

        private ICellStyle _titleStyle()
        {
            ICellStyle style = book.CreateCellStyle();
            //对齐方式
            style.Alignment = HorizontalAlignment.Center;
            //字体设置
            IFont font = book.CreateFont();
            font.IsBold = true;
            font.FontHeightInPoints = 11;
            font.FontName = "calibri";
            style.SetFont(font);
            return style;
        }

        private ICellStyle _textStyle()
        {
            ICellStyle style = book.CreateCellStyle();
            //对齐方式
            style.Alignment = HorizontalAlignment.Center;
            //字体设置
            IFont font = book.CreateFont();
            font.IsBold = false;
            font.FontHeightInPoints = 11;
            font.FontName = "calibri";
            style.SetFont(font);
            return style;
        }

        private ICellStyle _markStyle()
        {
            ICellStyle style = book.CreateCellStyle();
            //添加系统颜色到XSSFColor中
            System.Drawing.Color cl = Color.FromArgb(252, 228, 214);
            XSSFColor color = new XSSFColor(cl);
            ((XSSFCellStyle)style).SetFillForegroundColor(color);
            style.FillPattern = FillPattern.SolidForeground;
            //右边框描黑
            style.BorderRight = BorderStyle.Thin;
            style.BorderBottom = BorderStyle.None;
            //对齐方式
            style.Alignment = HorizontalAlignment.Center;
            //字体设置
            IFont font = book.CreateFont();
            font.IsBold = false;
            font.FontHeightInPoints = 11;
            font.FontName = "calibri";
            style.SetFont(font);
            return style;
        }

        private ICellStyle _markBoldStyle()
        {
            ICellStyle style = book.CreateCellStyle();
            //添加系统颜色到XSSFColor中
            System.Drawing.Color cl = Color.FromArgb(252, 228, 214);
            XSSFColor color = new XSSFColor(cl);
            ((XSSFCellStyle)style).SetFillForegroundColor(color);
            style.FillPattern = FillPattern.SolidForeground;
            //右边框描黑
            style.BorderRight = BorderStyle.Thin;
            style.BorderBottom = BorderStyle.None;
            //对齐方式
            style.Alignment = HorizontalAlignment.Center;
            //字体设置
            IFont font = book.CreateFont();
            font.IsBold = true;
            font.FontHeightInPoints = 11;
            font.FontName = "calibri";
            style.SetFont(font);
            return style;
        }

        private ICellStyle _alterOddStyle()
        {
            ICellStyle style = book.CreateCellStyle();
            //添加系统颜色到XSSFColor中
            System.Drawing.Color cl = Color.FromArgb(217, 225, 242);
            XSSFColor color = new XSSFColor(cl);
            ((XSSFCellStyle)style).SetFillForegroundColor(color);
            style.FillPattern = FillPattern.SolidForeground;
            //对齐方式
            style.Alignment = HorizontalAlignment.Center;
            //字体设置
            IFont font = book.CreateFont();
            font.IsBold = false;
            font.FontHeightInPoints = 8;
            font.FontName = "calibri";
            style.SetFont(font);
            return style;
        }

        private ICellStyle _alterEvenStyle()
        {
            ICellStyle style = book.CreateCellStyle();
            
            //添加系统颜色到XSSFColor中
            System.Drawing.Color cl = Color.FromArgb(155, 194, 230);
            XSSFColor color = new XSSFColor(cl);
            ((XSSFCellStyle)style).SetFillForegroundColor(color);
            style.FillPattern = FillPattern.SolidForeground;
            //对齐方式
            style.Alignment = HorizontalAlignment.Center;
            //字体设置
            IFont font = book.CreateFont();
            font.IsBold = false;
            font.FontHeightInPoints = 8;
            font.FontName = "calibri";
            style.SetFont(font);
            return style;
        }

        private ICellStyle _totalStyle()
        {
            ICellStyle style = book.CreateCellStyle();

            //添加系统颜色到XSSFColor中
            System.Drawing.Color cl = Color.FromArgb(255, 242, 204);
            XSSFColor color = new XSSFColor(cl);
            ((XSSFCellStyle)style).SetFillForegroundColor(color);
            style.FillPattern = FillPattern.SolidForeground;
            //对齐方式
            style.Alignment = HorizontalAlignment.Center;
            //字体设置
            IFont font = book.CreateFont();
            font.IsBold = false;
            font.FontHeightInPoints = 8;
            font.FontName = "calibri";
            style.SetFont(font);
            return style;
        }

        private ICellStyle _totalPercentStyle()
        {
            ICellStyle style = book.CreateCellStyle();

            //添加系统颜色到XSSFColor中
            System.Drawing.Color cl = Color.FromArgb(221, 235, 247);
            XSSFColor color = new XSSFColor(cl);
            ((XSSFCellStyle)style).SetFillForegroundColor(color);
            style.FillPattern = FillPattern.SolidForeground;
            //对齐方式
            style.Alignment = HorizontalAlignment.Center;
            //字体设置
            IFont font = book.CreateFont();
            font.IsBold = false;
            font.FontHeightInPoints = 8;
            font.FontName = "calibri";
            style.SetFont(font);
            return style;
        }

        private ICellStyle _textBorderStyle()
        {
            ICellStyle style = book.CreateCellStyle();
            //对齐方式
            style.Alignment = HorizontalAlignment.Center;
            //border描黑
            style.BorderBottom = BorderStyle.Thin;
            //字体设置
            IFont font = book.CreateFont();
            font.IsBold = false;
            font.FontHeightInPoints = 11;
            font.FontName = "calibri";
            style.SetFont(font);
            return style;
        }

        private ICellStyle _markBorderStyle()
        {
            ICellStyle style = book.CreateCellStyle();
            //添加系统颜色到XSSFColor中
            System.Drawing.Color cl = Color.FromArgb(252, 228, 214);
            XSSFColor color = new XSSFColor(cl);
            ((XSSFCellStyle)style).SetFillForegroundColor(color);
            style.FillPattern = FillPattern.SolidForeground;
            //右边框描黑
            style.BorderRight = BorderStyle.Thin;
            //border描黑
            style.BorderBottom = BorderStyle.Thin;
            //对齐方式
            style.Alignment = HorizontalAlignment.Center;
            //字体设置
            IFont font = book.CreateFont();
            font.IsBold = false;
            font.FontHeightInPoints = 11;
            font.FontName = "calibri";
            style.SetFont(font);
            return style;
        }

        private ICellStyle _markBorderBoldStyle()
        {
            ICellStyle style = book.CreateCellStyle();
            //添加系统颜色到XSSFColor中
            System.Drawing.Color cl = Color.FromArgb(252, 228, 214);
            XSSFColor color = new XSSFColor(cl);
            ((XSSFCellStyle)style).SetFillForegroundColor(color);
            style.FillPattern = FillPattern.SolidForeground;
            //右边框描黑
            style.BorderRight = BorderStyle.Thin;
            //border描黑
            style.BorderBottom = BorderStyle.Thin;
            //对齐方式
            style.Alignment = HorizontalAlignment.Center;
            //字体设置
            IFont font = book.CreateFont();
            font.IsBold = true;
            font.FontHeightInPoints = 11;
            font.FontName = "calibri";
            style.SetFont(font);
            return style;
        }

        private ICellStyle _alterBorderOddStyle()
        {
            ICellStyle style = book.CreateCellStyle();
            //添加系统颜色到XSSFColor中
            System.Drawing.Color cl = Color.FromArgb(217, 225, 242);
            XSSFColor color = new XSSFColor(cl);
            ((XSSFCellStyle)style).SetFillForegroundColor(color);
            style.FillPattern = FillPattern.SolidForeground;
            //border描黑
            style.BorderBottom = BorderStyle.Thin;
            //对齐方式
            style.Alignment = HorizontalAlignment.Center;
            //字体设置
            IFont font = book.CreateFont();
            font.IsBold = false;
            font.FontHeightInPoints = 8;
            font.FontName = "calibri";
            style.SetFont(font);
            return style;
        }

        private ICellStyle _alterBorderEvenStyle()
        {
            ICellStyle style = book.CreateCellStyle();

            //添加系统颜色到XSSFColor中
            System.Drawing.Color cl = Color.FromArgb(155, 194, 230);
            XSSFColor color = new XSSFColor(cl);
            ((XSSFCellStyle)style).SetFillForegroundColor(color);
            style.FillPattern = FillPattern.SolidForeground;
            //border描黑
            style.BorderBottom = BorderStyle.Thin;
            //对齐方式
            style.Alignment = HorizontalAlignment.Center;
            //字体设置
            IFont font = book.CreateFont();
            font.IsBold = false;
            font.FontHeightInPoints = 8;
            font.FontName = "calibri";
            style.SetFont(font);
            return style;
        }

        private ICellStyle _totalBorderStyle()
        {
            ICellStyle style = book.CreateCellStyle();

            //添加系统颜色到XSSFColor中
            System.Drawing.Color cl = Color.FromArgb(255, 242, 204);
            XSSFColor color = new XSSFColor(cl);
            ((XSSFCellStyle)style).SetFillForegroundColor(color);
            style.FillPattern = FillPattern.SolidForeground;
            //border描黑
            style.BorderBottom = BorderStyle.Thin;
            //对齐方式
            style.Alignment = HorizontalAlignment.Center;
            //字体设置
            IFont font = book.CreateFont();
            font.IsBold = false;
            font.FontHeightInPoints = 8;
            font.FontName = "calibri";
            style.SetFont(font);
            return style;
        }

        private ICellStyle _totalBorderPercentStyle()
        {
            ICellStyle style = book.CreateCellStyle();

            //添加系统颜色到XSSFColor中
            System.Drawing.Color cl = Color.FromArgb(221, 235, 247);
            XSSFColor color = new XSSFColor(cl);
            ((XSSFCellStyle)style).SetFillForegroundColor(color);
            style.FillPattern = FillPattern.SolidForeground;
            //border描黑
            style.BorderBottom = BorderStyle.Thin;
            //对齐方式
            style.Alignment = HorizontalAlignment.Center;
            //字体设置
            IFont font = book.CreateFont();
            font.IsBold = false;
            font.FontHeightInPoints = 8;
            font.FontName = "calibri";
            style.SetFont(font);
            return style;
        }

        #region
        public XSSFWorkbook book { get; set; }

        /// <summary>
        /// 标题文本样式，文字加粗，居中显示
        /// </summary>
        public ICellStyle titleStyle { get; set; }

        /// <summary>
        /// 一般文本样式，居中显示
        /// </summary>
        public ICellStyle textStyle { get; set; }

        /// <summary>
        /// 标记文本样式，橙色背景色，居中显示
        /// </summary>
        public ICellStyle markStyle { get; set; }

        /// <summary>
        /// 标记文本样式，橙色背景色，居中显示, 文字加粗
        /// </summary>
        public ICellStyle markBoldStyle { get; set; }

        /// <summary>
        /// 数据文本样式基数记录列，浅蓝色背景，居中显示，小字号
        /// </summary>
        public ICellStyle alterOddStyle { get; set; }

        /// <summary>
        /// 数据文本样式偶数记录列，墨绿色背景，居中显示，小字号
        /// </summary>
        public ICellStyle alterEvenStyle { get; set; }

        /// <summary>
        /// 数据文本样式统计记录列，浅黄色背景，居中显示，小字号
        /// </summary>
        public ICellStyle totalStyle { get; set; }

        /// <summary>
        /// 数据文本样式统计百分比记录列，藏蓝色背景，居中显示，小字号
        /// </summary>
        public ICellStyle totalPercentStyle { get; set; }

        //border style
        public ICellStyle textBorderStyle { get; set; }
        
        public ICellStyle markBorderStyle { get; set; }
            
        public ICellStyle markBorderBoldStyle { get; set; }
            
        public ICellStyle alterBorderOddStyle { get; set; }

        public ICellStyle alterBorderEvenStyle { get; set; }
            
        public ICellStyle totalBorderStyle { get; set; }

        public ICellStyle totalBorderPercentStyle { get; set; }
        #endregion
    }
}
