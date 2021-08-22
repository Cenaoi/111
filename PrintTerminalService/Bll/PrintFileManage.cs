using HWQ.Entity.LightModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintTerminalService.Bll
{
    /// <summary>
    /// 打印文件管理
    /// </summary>
    public class PrintFileManage
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public TPrinter Printer { get; set; }

        private static PrintFileManage _Instance = null;

        /// <summary>
        /// 实例
        /// </summary>
        public static PrintFileManage Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new PrintFileManage();
                }

                return _Instance;
            }
        }

        /// <summary>
        /// 打印文件列队
        /// </summary>
        ConcurrentQueue<PrintFileInfo> _PrintFileQueue = new ConcurrentQueue<PrintFileInfo>();

        /// <summary>
        /// 打印文件列表
        /// </summary>
        ConcurrentDictionary<int, PrintFileInfo> _FileList = new ConcurrentDictionary<int, PrintFileInfo>();

        /// <summary>
        /// 获取打印文件信息
        /// </summary>
        /// <param name="printerId"></param>
        /// <returns></returns>
        public PrintFileInfo GetPrintFile(int fileId)
        {
            if (!_FileList.TryGetValue(fileId, out PrintFileInfo fileInfo))
            {
                return null;
            }

            return fileInfo;
        }

        /// <summary>
        /// 添加打印文件
        /// </summary>
        /// <param name="fileInfo"></param>
        public void Add(PrintFileInfo fileInfo)
        {
            if (GetPrintFile(fileInfo.FileId) != null)
            {
                return;
            }

            _FileList.TryAdd(fileInfo.FileId, fileInfo);

            _PrintFileQueue.Enqueue(fileInfo);
        }

        /// <summary>
        /// 开始处理任务
        /// </summary>
        public void StartProcessTask()
        {
            this._TaskRunning = true;

            Task task = Task.Factory.StartNew((state) =>
            {
                PrintFileManage pfm = state as PrintFileManage;

                while (pfm._TaskRunning)
                {
                    foreach (var item in _FileList)
                    {
                        PrintFileInfo pf = item.Value;

                        if (!pf.PrintDateTime.HasValue) 
                        {
                            continue;
                        }

                        if (pf.PrintDateTime.Value.AddMinutes(1) > DateTime.Now) 
                        {
                            continue;
                        }

                        _FileList.TryRemove(pf.FileId, out PrintFileInfo delFile);
                    }

                    ProcessPrint();

                    System.Threading.Thread.Sleep(100);
                }
            }, this);

            task.ContinueWith(t =>
            {
                // 任务结束...
            });
        }

        /// <summary>
        /// 任务运行状态
        /// </summary>
        private bool _TaskRunning = false;

        /// <summary>
        /// 任务运行状态
        /// </summary>
        public bool TaskRunning { get { return this._TaskRunning; } }

        /// <summary>
        /// 处理文件
        /// </summary>
        private void ProcessPrint()
        {
            PrintFileInfo file = null;

            //取一个文件处理
            if (!this._PrintFileQueue.TryDequeue(out file))
            {
                return;
            }

            try
            {
                //1.准备打印
                //2.更改打印开始状态
                //3.开始打印
                //4.更改打印完成状态
                PrintHandle phandle = new PrintHandle();
                phandle.Printer = this.Printer;
                phandle.PrintFile = file;
                phandle.Start();
            }
            catch (Exception ex)
            {
                log.Error("打印出错了", ex);
            }
        }

        /// <summary>
        /// 停止处理任务
        /// </summary>
        public void StopProcessTask()
        {
            this._TaskRunning = false;
        }

        /// <summary>
        /// 获取打印文件列表
        /// </summary>
        public void GetPrintFileList()
        {
            try
            {
                BizReqResult result = BizReqHelper.GetPrintFileList(this.Printer.PrinterInfo.Code);

                if (!result.Success)
                {
                    return;
                }

                SModelList list = result.Data as SModelList;

                foreach (var item in list)
                {
                    PrintFileInfo pfi = new PrintFileInfo
                    {
                        FileId = item.GetInt("ROW_IDENTITY_ID"),
                        FileName = item.GetString("COL_9")
                    };

                    this.Add(pfi);
                }
            }
            catch (Exception ex)
            {
                log.Error($"获取打印文件列表出错了, 打印机Id:[{this.Printer.PrinterInfo.Id}]", ex);
            }
        }

    }


    /// <summary>
    /// 打印文件信息
    /// </summary>
    public class PrintFileInfo 
    {
        /// <summary>
        /// 打印文件ID
        /// </summary>
        public int FileId { get; set; } = 0;

        /// <summary>
        /// 打印时间
        /// </summary>
        public DateTime? PrintDateTime { get; set; }

        /// <summary>
        /// 打印文件名
        /// </summary>
        public string FileName { get; set; } = "";

        /// <summary>
        /// 打印文件流
        /// </summary>
        public Stream FileStream { get; set; } = null;

        private PrintFileStatus _PrintStatus = PrintFileStatus.Pending;

        /// <summary>
        /// 打印状态
        /// </summary>
        public PrintFileStatus PrintStatus 
        {
            get
            {
                return this._PrintStatus;
            }
            set
            {
                if (value != this._PrintStatus)
                {
                    BizReqHelper.UpdatePrintFileStatus(this.FileId, value);
                }

                if (value == PrintFileStatus.Failure || value == PrintFileStatus.Finish) 
                {
                    this.PrintDateTime = DateTime.Now;
                }

                this._PrintStatus = value;
            }
        }

        /// <summary>
        /// 打印失败描述
        /// </summary>
        public string FailureDesc { get; set; } = "";
    }


    /// <summary>
    /// 文件打印状态
    /// </summary>
    public enum PrintFileStatus
    {
        //默认
        None = 0,
        /// <summary>
        /// 等待打印（获取到打印文件流）
        /// </summary>
        Pending = 1,
        /// <summary>
        /// 正在打印
        /// </summary>
        Process = 2,
        /// <summary>
        /// 打印失败
        /// </summary>
        Failure = 3,
        /// <summary>
        /// 打印成功
        /// </summary>
        Finish = 4
    }


}
