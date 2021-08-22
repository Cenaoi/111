using EC5.WindowService;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace PrintTerminalService.Bll
{
    /// <summary>
    /// 打印机帮助类
    /// </summary>
    public class PrintHandle
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 打印机
        /// </summary>
        public TPrinter Printer { get; set; }

        /// <summary>
        /// 检测打印任务时间间隔
        /// </summary>
        public static int PrintDetectionInterval = 500;

        /// <summary>
        /// 
        /// </summary>
        bool m_PrintRunding = true;

        /// <summary>
        /// 检测打印任务开关
        /// </summary>
        bool m_PrintSwitch = false;

        /// <summary>
        /// 打印任务心跳间隔时间 (15秒)
        /// </summary>
        EC5.Utility.STimerTask m_Heartbeat = new EC5.Utility.STimerTask(1000 * 15);

        public Exception Error { get; private set; }

        public bool IsStarted { get; private set; }

        public bool IsFaulted { get; private set; }

        public bool IsComplete { get; private set; }

        /// <summary>
        /// 打印的文件
        /// </summary>
        public PrintFileInfo PrintFile { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            if (this.IsStarted)
            {
                return;
            }

            this.IsStarted = true;

            BizReqResult result = BizReqHelper.GetPrintFileStream(this.PrintFile.FileId);

            if (!result.Success)
            {
                this.PrintFile.PrintStatus = PrintFileStatus.Failure;
                this.PrintFile.FailureDesc = result.ErrorMsg;

                return;
            }

            MemoryStream ms = new MemoryStream((byte[])result.Data);

            this.PrintFile.FileStream = ms;

            PrintDocument pd = new PrintDocument();
            pd.PrintPage += M_PrintDoc_PrintPage;
            pd.EndPrint += M_PrintDoc_EndPrint; ;
            pd.BeginPrint += M_PrintDoc_BeginPrint;
            //m_PrintDoc.DefaultPageSettings.PrinterSettings.Copies = m_PrintCopies;

            pd.DefaultPageSettings.Margins.Top = 0;
            pd.DefaultPageSettings.Margins.Left = 0;
            pd.DefaultPageSettings.Margins.Right = 0;
            pd.DefaultPageSettings.Margins.Bottom = 0;

            //隐藏 对话框
            PrintController printController = new StandardPrintController();
            pd.PrintController = printController;

            pd.PrinterSettings.PrinterName = this.Printer.PrinterInfo.Name;

            this.Printer.PrinterInfo.LastPrintTime = DateTime.Now;

            try
            {
                pd.Print();
            }
            catch (Exception ex)
            {
                log.Error("打印失败.", ex);

                this.Error = ex;
                this.IsFaulted = true;
                this.IsComplete = true;

                this.PrintFile.PrintStatus = PrintFileStatus.Failure;
            }
        }


        private void M_PrintDoc_BeginPrint(object sender, PrintEventArgs e)
        {
            log.Info("开始打印...");

            this.PrintFile.PrintStatus = PrintFileStatus.Process;
        }


        private void M_PrintDoc_EndPrint(object sender, PrintEventArgs e)
        {
            this.IsComplete = true;

            this.IsFaulted = false;

            log.Info("开始结束...");

            this.PrintFile.PrintStatus = PrintFileStatus.Finish;
        }


        private void M_PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            g.PageUnit = GraphicsUnit.Point;

            Metafile metaFile = null;

            try
            {
                metaFile = Metafile.FromStream(this.PrintFile.FileStream) as Metafile;
            }
            catch (Exception ex)
            {
                log.Error("打开 Metafile 文件失败.", ex);
                return;
            }

            if (metaFile is null)
            {
                log.Warn("绘制到打印机失败, 打开 Metafile 文件错误.");
            }
            else
            {
                log.Debug($"打印尺寸 >>> Width:[{metaFile.Width}], Height:[{metaFile.Height}]");
            }

            RectangleF rect = new RectangleF(0, 0, metaFile.Width, metaFile.Height);

            g.DrawImage(metaFile, 0, 0, rect, GraphicsUnit.Point);
        }





    }
}
