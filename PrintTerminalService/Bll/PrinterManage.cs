using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Drawing.Printing;
using System.Printing;
using HWQ.Entity.LightModels;

namespace PrintTerminalService.Bll
{
    /// <summary>
    /// 打印机管理 (首先获取本机MAC和打印机信息提交到后台，判断是否新的然后入库, 然后返回打印机信息添加打印机管理去做打印任务)
    /// </summary>
    public class PrinterManage
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 打印机列表
        /// </summary>
        public static ConcurrentDictionary<int, TPrinter> PrinterList = new ConcurrentDictionary<int, TPrinter>();

        /// <summary>
        /// 获取打印机
        /// </summary>
        /// <param name="printerId"></param>
        /// <returns></returns>
        public static TPrinter GetPrinter(int printerId)
        {
            if (!PrinterList.TryGetValue(printerId, out TPrinter printer)) 
            {
                return null;
            }

            return printer;
        }

        /// <summary>
        /// 添加打印机
        /// </summary>
        /// <param name="printer"></param>
        public static void AddPrinter(TPrinter printer)
        {
            if (GetPrinter(printer.PrinterInfo.Id) != null)
            {
                return;
            }

            printer.PrintFileManage.Printer = printer;

            PrinterList.TryAdd(printer.PrinterInfo.Id, printer);
        }


        /// <summary>
        /// 获取所有打印机名称(列表)
        /// </summary>
        public static void Init()
        {
            GetPrinterList();

            PrinterList = new ConcurrentDictionary<int, TPrinter>();

            foreach (var item in TPrinterConfig.Default.List)
            {
                TPrinter printer = new TPrinter();
                printer.PrinterInfo = item;

                AddPrinter(printer);
            }
        }


        public static void GetPrinterList()
        {
            string guid = TPrinterConfig.Default.TPGuid;

            List<string> list = new List<string>();

            foreach (string printers_name in PrinterSettings.InstalledPrinters)
            {
                list.Add(printers_name);
            }

            if (list.Count == 0)
            {
                list.Add("Printer1");
                list.Add("Printer2");
                list.Add("Printer3");
            }

            string strNames = string.Join(",", list.ToArray());

            try
            {
                BizReqResult result = BizReqHelper.SubmitPrinterList(guid, strNames);

                if (!result.Success)
                {
                    log.Fatal("提交打印列表信息失败");

                    return;
                }

                SModelList pList = result.Data as SModelList;

                TPrinterConfig.Default.List = new List<TPrinterInfo>();

                foreach (var item in pList)
                {
                    TPrinterInfo printerInfo = new TPrinterInfo()
                    {
                        Id = item.GetInt("ROW_IDENTITY_ID"),
                        Name = item.GetString("COL_1"),
                        Code = item.GetString("COL_2"),
                        DefaultTemplate = item.GetString("COL_3")
                    };

                    TPrinterConfig.Default.List.Add(printerInfo);
                }

                TPrinterConfig.Default.Save();
            }
            catch (Exception ex)
            {
                log.Error("提交打印列表信息出错", ex);
            }
        }


    }


    /// <summary>
    /// 打印机信息
    /// </summary>
    public class TPrinter
    {

        public TPrinterInfo PrinterInfo { get; set; }


        private PrintFileManage _PrintFileManage;

        /// <summary>
        /// 打印文件管理
        /// </summary>
        public PrintFileManage PrintFileManage 
        {
            get
            {
                if (this._PrintFileManage == null)
                {
                    this._PrintFileManage = new PrintFileManage();
                    this._PrintFileManage.Printer = this;
                }

                return this._PrintFileManage;
            }
        }

        /// <summary>
        /// 获取打印机状态text
        /// </summary>
        /// <returns></returns>
        public static string GetPrinterStateText()
        {
            PrintQueue printer_state = LocalPrintServer.GetDefaultPrintQueue();

            string state_text = "未知状态";

            switch (printer_state.QueueStatus)
            {
                //处干节能状态
                case PrintQueueStatus.PowerSave:
                    state_text = "打印机处于节能模式";
                    break;
                //处于错误状态
                case PrintQueueStatus.ServerUnknown:
                    state_text = "打印机处于错误状态";
                    break;
                //打印机上的门已打开
                case PrintQueueStatus.DoorOpen:
                    state_text = "打印机上的门已打开";
                    break;
                //打印机无可用内存
                case PrintQueueStatus.OutOfMemory:
                    state_text = "打印机无可用内存";
                    break;
                case PrintQueueStatus.UserIntervention:
                    state_text = "打印机要求通过用户操作来更正错误情况";
                    break;
                case PrintQueueStatus.PagePunt:
                    state_text = "打印机不能打印当前页";
                    break;
                //打印机墨粉已用完 
                case PrintQueueStatus.NoToner:
                    state_text = "打印机墨粉用完";
                    break;
                case PrintQueueStatus.TonerLow:
                    state_text = "打印机中只剩下少量墨粉";
                    break;
                case PrintQueueStatus.WarmingUp:
                    state_text = "打印机正在预热";
                    break;
                case PrintQueueStatus.Initializing:
                    state_text = "打印机正在初始化";
                    break;
                case PrintQueueStatus.Waiting:
                    state_text = "打印机正在等待打印作业";
                    break;
                case PrintQueueStatus.NotAvailable:
                    state_text = "状态信息不可用";
                    break;
                case PrintQueueStatus.OutputBinFull:
                    state_text = "打印机的输出纸盒已满";
                    break;
                case PrintQueueStatus.Busy:
                    state_text = "打印机正忙";
                    break;
                case PrintQueueStatus.IOActive:
                    state_text = "打印机正在与打印服务器交换数据";
                    break;
                case PrintQueueStatus.Offline:
                    state_text = "打印机正忙";
                    break;
                case PrintQueueStatus.PaperProblem:
                    state_text = "打印机中的纸张导致未指定的错误情况";
                    break;
                case PrintQueueStatus.ManualFeed:
                    state_text = "打印机正在等待用户将打印介质放入手动送纸盒";
                    break;
                //打印机缺纸
                case PrintQueueStatus.PaperOut:
                    state_text = "打印机中没有或已用完当前打印作业所需的纸张类型";
                    break;
                //打印机卡纸
                case PrintQueueStatus.PaperJam:
                    state_text = "打印机卡纸";
                    break;
                case PrintQueueStatus.PendingDeletion:
                    state_text = "打印队列正在删除打印作业";
                    break;
                case PrintQueueStatus.Paused:
                    state_text = "打印队列已暂停";
                    break;
                case PrintQueueStatus.None:
                    state_text = "未指定状态";
                    break;
                case PrintQueueStatus.Printing:
                    state_text = "设备正在打印";
                    break;
                case PrintQueueStatus.Error:
                    state_text = "由于错误情况, 打印机无法打印";
                    break;
                default:
                    state_text = "未知状态";
                    break;
            }

            return state_text;
        }

    }


    public class TPrinterInfo 
    {
        /// <summary>
        /// 打印机ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 打印编号
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 打印名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 打印机类型
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        /// 默认打印模板
        /// </summary>
        public string DefaultTemplate { get; set; }
        /// <summary>
        /// 打印机状态
        /// </summary>
        public bool State { get; set; } = false;

        /// <summary>
        /// 最近打印时间
        /// </summary>
        public DateTime? LastPrintTime { get; set; } = DateTime.Now;
    }


    /// <summary>
    /// 打印机状态
    /// </summary>
    public enum PrinterStatus
    {

    }

}
