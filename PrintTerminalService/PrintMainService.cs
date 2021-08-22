using PrintTerminalService.Bll;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PrintTerminalService
{
    public partial class PrintMainService : ServiceBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PrintMainService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            PrinterManage.Init();

            AppHelper.InitHelper();

            log.Info("服务已启动，初始化成功.");
        }

        protected override void OnStop()
        {
            log.Info("服务已停止");
        }
    }
}
