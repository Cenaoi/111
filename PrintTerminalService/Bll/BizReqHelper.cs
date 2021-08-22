using HWQ.Entity.LightModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintTerminalService.Bll
{
    /// <summary>
    /// 业务请求帮助类
    /// </summary>
    public class BizReqHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 请求地址
        /// </summary>
        public static string ReqBaseUrl { get; set; } = "http://localhost:1789/App/InfoGrid2/GBZZZD/Api/Index.ashx";

        /// <summary>
        /// 
        /// </summary>
        public static string ReqBaseDomain { get; set; } = "http://localhost:1789";

        /// <summary>
        /// 命令接口集合
        /// </summary>
        static Dictionary<string, string> _ApiList = new Dictionary<string, string>()
        {
            //新增
            ["create"] = ""
        };

        /// <summary>
        /// 返回完整提交地址
        /// </summary>
        /// <param name="apiName"></param>
        /// <returns></returns>
        public static string GetReqFullUrl(string apiName)
        {
            string url = _ApiList[apiName];

            return ReqBaseUrl + url;
        }


        /// <summary>
        /// 拼接URL参数字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GetQueryString(SModel data)
        {
            string query = "?";

            string[] keys = data.GetFields();

            int i = 0;

            foreach (var item in keys)
            {
                if (i++ > 0)
                {
                    query += "&";
                }

                query += $"{item}={data.GetString(item)}";
            }

            return query;
        }


        /// <summary>
        /// 提交打印机列表
        /// </summary>
        /// <returns></returns>
        public static BizReqResult SubmitPrinterList(string tpGuid, string printerNames)
        {
            SModel reqParams = new SModel()
            {
                ["action"] = "SUBMIT_PRINTER_LIST",
                ["tpGuid"] = tpGuid,
                ["printerNames"] = printerNames
            };

            return Post(reqParams);
        }

        /// <summary>
        /// 获取打印文件列表
        /// </summary>
        /// <returns></returns>
        public static BizReqResult GetPrintFileList(string printerNo)
        {
            SModel reqParams = new SModel()
            {
                ["action"] = "GET_PRINT_FILE_LIST",
                ["printerNo"] = printerNo
            };

            return Post(reqParams);
        }

        /// <summary>
        /// 获取打印文件流
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public static BizReqResult GetPrintFileStream(int fileId)
        {
            string reqUrl = ReqBaseDomain + "/App/InfoGrid2/GBZZZD/Api/DownloadPrintFile.ashx?fileId=" + fileId;

            byte[] data = null;

            try
            {
                using (WebClientEx client = new WebClientEx())
                {
                    data = client.DownloadData(reqUrl);
                }
            }
            catch (Exception ex)
            {
                log.Error("下载打印文件流出错了", ex);

                return BizReqResult.Error("下载打印文件流失败");
            }

            if (data == null)
            {
                return BizReqResult.Error("下载打印文件流失败");
            }

            return BizReqResult.SuccessData(data);
        }

        /// <summary>
        /// 更新打印文件状态
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="printStatus"></param>
        /// <returns></returns>
        public static BizReqResult UpdatePrintFileStatus(int fileId, PrintFileStatus printStatus)
        {
            int status = 0;

            if (printStatus == PrintFileStatus.Process)
            {
                status = 4;
            }
            else if (printStatus == PrintFileStatus.Finish)
            {
                status = 999;
            }

            SModel reqParams = new SModel()
            {
                ["action"] = "UPDATE_PRINT_FILE_STATUS",
                ["fileId"] = fileId,
                ["status"] = status
            };

            return Post(reqParams);
        }


        /// <summary>
        /// Post
        /// </summary>
        /// <param name="reqParams"></param>
        /// <returns></returns>
        public static BizReqResult Post(SModel reqParams)
        {
            string submit_full_url = ReqBaseUrl;

            log.Debug($"准备请求: {submit_full_url}");

            NameValueCollection collection = new NameValueCollection();

            foreach (var item in reqParams.GetFields())
            {
                collection.Add(item, reqParams.GetString(item));
            }

            Stopwatch sw = null;

            try
            {
                byte[] ret = null;

                using (WebClientEx client = new WebClientEx())
                {
                    sw = Stopwatch.StartNew();

                    ret = client.UploadValues(submit_full_url, collection);
                }

                sw.Stop();

                log.Debug($"请求用时毫秒：{sw.ElapsedMilliseconds}");

                string str = Encoding.UTF8.GetString(ret);

                log.Debug($"提交地址:[{submit_full_url}], ret:[{str}]");

                SModel res = SModel.ParseJson(str);

                return BizReqResult.SuccessData(res["Data"]);
            }
            catch (System.Net.Sockets.SocketException reqEx1)
            {
                log.Warn($"提交出错了. --- 提交地址:[{submit_full_url}], error:[{reqEx1.Message}]");

                return BizReqResult.Error("提交出错了");
            }
            catch (System.Net.Http.HttpRequestException reqEx2)
            {
                log.Warn($"提交出错了. --- 提交地址:[{submit_full_url}], error:[{reqEx2.Message}]");

                return BizReqResult.Error("提交出错了");
            }
            catch (Exception ex)
            {
                log.Error($"提交出错了. --- 提交地址:[{submit_full_url}]", ex);

                return BizReqResult.Error("提交出错了");
            }
        }
        

    }

    /// <summary>
    /// 业务请求结果
    /// </summary>
    public class BizReqResult
    {
        public string Code { get; set; } = "";

        public bool Success { get; set; } = false;

        public string Message { get; set; } = "";

        public object Data { get; set; } = null;

        public string ErrorMsg { get; set; } = "";

        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static BizReqResult SuccessData(object data)
        {
            BizReqResult res = new BizReqResult()
            {
                Success = true,
                Data = data,
                Message = "ok"
            };

            return res;
        }

        /// <summary>
        /// 错误
        /// </summary>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static BizReqResult Error(string errorMsg)
        {
            BizReqResult res = new BizReqResult()
            {
                Success = false,
                ErrorMsg = errorMsg
            };

            return res;
        }
    }



}
