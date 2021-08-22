using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PrintTerminalService.Bll
{
    public class WebClientEx : WebClient
    {
        /// <summary>
        /// 获取或设置请求超时之前的时间长度（以毫秒为单位）。 默认6秒
        /// </summary>
        public int Timeout { get; set; } = 6 * 1000;

        public WebClientEx(int timeout = 6 * 1000)
        {
            Timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);

            request.Timeout = Timeout;

            return request;
        }
    }
}
