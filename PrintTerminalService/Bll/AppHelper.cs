using EC5.HttpModel;
using EC5.WebSite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintTerminalService.Bll
{
    public class AppHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 初始化
        /// </summary>
        public static void InitHelper()
        {
            EC6.UnityEngine.GlobelManager.Instance.DebugEnabled = false;

            InitWebServer();
        }

        /// <summary>
        /// 
        /// </summary>
        static WebSiteServer m_WebSS;


        /// <summary>
        /// 初始化网站服务器
        /// </summary>
        private static void InitWebServer()
        {
            HttpClientConfig.Default.Overtime_ProContentData = 1000 * 60 * 5;
            HttpClientConfig.Default.MaxPostTimeout = 1000 * 60 * 5;
            //HttpClientConfig.Default.
            HttpClientConfig.Default.Overtime_ProHeaderData = 1000 * 60 * 5;
            int port = Properties.Settings.Default.WebSitePort;

            log.Info($"初始化 WebSite; Port={port}");

            WebSiteServer wss = new WebSiteServer();
            //wss.Config("/Site", port);
            wss.BindingPort = port;
            wss.EnableDebug = false;
            wss.Route.AddPage("/api/index.ashx", typeof(HttpApi.Index));

            //wss.AddAuthentication(options =>
            //{
            //    options.AddPolicy("jwt", new HttpApi.JwtAuthorizationPolicy());
            //});

            wss.Start();

            m_WebSS = wss;
        }


    }
}
