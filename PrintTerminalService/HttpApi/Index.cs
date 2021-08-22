using EC5.IO;
using EC5.WebSite;
using EC5.WebSite.Utilitys;
using EC5.WebSite.WebAPI;
using HWQ.Entity.LightModels;
using PrintTerminalService.Bll;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintTerminalService.HttpApi
{
    public class Index : AjaxHandler, IHttpHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Description("")]
        [Ajax(Alias = "test")]
        public HttpResult GetTest(HttpContext context)
        {
            return HttpResult.Success("ok");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Description("")]
        [Ajax(Alias = "get_printer_list")]
        public HttpResult GetPrinterList(HttpContext context)
        {
            List<TPrinter> list = PrinterManage.PrinterList.Values.ToList();

            SModelList res = new SModelList();

            foreach (var item in list)
            {
                TPrinterInfo printerInfo = item.PrinterInfo;

                SModel printer = new SModel()
                {
                    ["id"] = printerInfo.Id,
                    ["name"] = printerInfo.Name,
                    ["code"] = printerInfo.Code,
                    ["print_type"] = printerInfo.TypeName,
                    ["default_template"] = printerInfo.DefaultTemplate,
                    ["state"] = printerInfo.State,
                    ["group_code"] = TPrinterConfig.Default.TPGuid
                };

                if (printerInfo.LastPrintTime != null)
                {
                    printer["last_print_time"] = printerInfo.LastPrintTime.Value;
                }

                res.Add(printer);
            }

            return HttpResult.Success(res);
        }


        /// <summary>
        /// 通知打印
        /// </summary>
        /// <returns></returns>
        [Description("")]
        [Ajax(Alias = "notice_print")]
        public HttpResult NoticePrint(HttpContext context)
        {
            int printerId = HttpUtil.FormInt("printerId");

            if (printerId == 0)
            {
                return HttpResult.Error("请传入打印机Id");
            }

            TPrinter printer = PrinterManage.GetPrinter(printerId);

            if (printer == null)
            {
                return HttpResult.Error("找不到这个打印机");
            }

            printer.PrintFileManage.GetPrintFileList();

            if (!printer.PrintFileManage.TaskRunning) 
            {
                printer.PrintFileManage.StartProcessTask();
            }

            return HttpResult.Success("ok");
        }


        /// <summary>
        /// 保存打印机配置信息
        /// </summary>
        /// <returns></returns>
        [Description("")]
        [Ajax(Alias = "save_printer_config")]
        public HttpResult SavePrinterConfig(HttpContext context)
        {
            string strPrintersInfo = HttpUtil.FormTrim("printersInfo");

            if (string.IsNullOrWhiteSpace(strPrintersInfo))
            {
                return HttpResult.Error("请传入打印机配置信息");
            }

            SModel printersInfo = null;

            SModelList list = new SModelList();

            try
            {
                printersInfo = SModel.ParseJson(strPrintersInfo);

                list = printersInfo["list"];
            }
            catch (Exception ex)
            {
                log.Error("保存打印机配置信息，数据格式错误， json = " + strPrintersInfo, ex);

                return HttpResult.Error("打印机配置信息数据格式错误");
            }

            TPrinterConfig.Default.List = new List<TPrinterInfo>(); 

            foreach (var item in list)
            {
                int printerId = item.GetInt("id");

                TPrinter printer = PrinterManage.GetPrinter(printerId);

                if (printer == null)
                {
                    continue;
                }

                TPrinterInfo printerInfo = printer.PrinterInfo;

                printerInfo.TypeName = item.GetString("print_type");
                printerInfo.DefaultTemplate = item.GetString("default_template");

                TPrinterConfig.Default.List.Add(printerInfo);
            }

            TPrinterConfig.Default.Save();

            return HttpResult.Success("ok");
        }


        /// <summary>
        /// 根据打印机类型获取打印机信息
        /// </summary>
        /// <returns></returns>
        [Description("")]
        [Ajax(Alias = "get_printer_by_type")]
        public HttpResult GetPrinterByType(HttpContext context)
        {
            string type = HttpUtil.FormTrim("type");

            if (string.IsNullOrWhiteSpace(type)) 
            {
                return HttpResult.Error("请传入打印机类型");
            }

            List<TPrinter> list = PrinterManage.PrinterList.Values.ToList();

            SModel printer = null;

            foreach (var item in list)
            {
                TPrinterInfo printerInfo = item.PrinterInfo;

                if (printerInfo.TypeName != type) 
                {
                    continue;
                }

                printer = new SModel()
                {
                    ["id"] = printerInfo.Id,
                    ["name"] = printerInfo.Name,
                    ["code"] = printerInfo.Code,
                    ["print_type"] = printerInfo.TypeName,
                    ["default_template"] = printerInfo.DefaultTemplate,
                    ["state"] = printerInfo.State,
                    ["last_print_time"] = "",
                    ["group_code"] = TPrinterConfig.Default.TPGuid
                };

                if (printerInfo.LastPrintTime.HasValue)
                {
                    printer["last_print_time"] = printerInfo.LastPrintTime.Value;
                }

                break;
            }

            return HttpResult.Success(printer);
        }

    }
}
