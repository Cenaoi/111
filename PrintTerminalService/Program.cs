using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PrintTerminalService
{
    static class Program
    {
        private static log4net.ILog log;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            InitLog();

            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            bool is_rerun = true;

            if (is_rerun)
            {
                try
                {
                    CheckCurrentProcessRerunClose();
                }
                catch (Exception ex)
                {
                    log.Error("关闭相同程序的进程出错了", ex);
                }

            }

            ServiceBase[] ServicesToRun;

            ServicesToRun = new ServiceBase[]
            {
                new PrintMainService()
            };

            ServiceBase.Run(ServicesToRun);
        }


        /// <summary>
        /// 初始化 Log4Net
        /// </summary>
        private static void InitLog()
        {
            string basePath = System.AppDomain.CurrentDomain.BaseDirectory;

            FileInfo fi = new FileInfo(basePath + "\\Config\\log4net.Config");

            if (!fi.Exists)
            {
                return;
            }

            log4net.Config.XmlConfigurator.Configure(fi);
        }


        /// <summary>
        /// 避免重复运行
        /// </summary>
        /// <returns></returns>
        public static void CheckCurrentProcessRerunClose()
        {
            Process current = default(Process);
            current = Process.GetCurrentProcess();
            Process[] processes = null;
            processes = Process.GetProcessesByName(current.ProcessName);

            Process process = default(Process);

            foreach (Process tempLoopVar_process in processes)
            {
                process = tempLoopVar_process;

                if (process.Id != current.Id)
                {
                    if (System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == current.MainModule.FileName)
                    {
                        process.Kill();

                        log.Info("清理重复进程成功.");
                    }
                }
            }
        }


    }
}
