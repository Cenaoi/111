using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintTerminalService.Bll
{
    public class TPrinterConfig
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static TPrinterConfig _Default = null;

        private static string _DefaultConfigPath = "\\Config\\PrinterConfig.json";

        public static string DefaultConfigPath
        {
            get 
            {
                string basePath = System.AppDomain.CurrentDomain.BaseDirectory;

                return basePath + _DefaultConfigPath;
            }
            set
            {
                _DefaultConfigPath = value;
            }
        }

        /// <summary>
        /// 默认
        /// </summary>
        public static TPrinterConfig Default
        {
            get
            {
                if (_Default == null)
                {
                    _Default = GetDefaultConfig();
                }

                return _Default;
            }
        }

        public string TPGuid { get; set; } = Guid.NewGuid().ToString().Replace("-", "");

        public int Count 
        {
            get
            {
                return List.Count;
            }
        }

        public List<TPrinterInfo> List { get; set; } = new List<TPrinterInfo>();

        /// <summary>
        /// 读取打印机配置信息
        /// </summary>
        /// <returns></returns>
        public static TPrinterConfig GetDefaultConfig()
        {
            TPrinterConfig tpc = new TPrinterConfig();

            try
            {
                if (!File.Exists(DefaultConfigPath))
                {
                    return tpc;
                }

                string json = File.ReadAllText(DefaultConfigPath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return tpc;
                }

                tpc = Newtonsoft.Json.JsonConvert.DeserializeObject<TPrinterConfig>(json);

                return tpc;
            }
            catch (Exception ex)
            {
                log.Error($"读取打印机配置信息文件出错了, 文件路径:[{DefaultConfigPath}]", ex);

                return tpc;
            }
        }

        /// <summary>
        /// 保存打印机配置信息
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            try
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(Default);

                string configDir = Path.GetDirectoryName(DefaultConfigPath);

                if (!Directory.Exists(configDir)) 
                {
                    Directory.CreateDirectory(configDir);
                }

                File.WriteAllText(DefaultConfigPath, json);

                return true;
            }
            catch (Exception ex)
            {
                log.Error($"保存打印机配置信息文件出错了, 文件路径:[{DefaultConfigPath}]", ex);

                return false;
            }
        }



    }
}
