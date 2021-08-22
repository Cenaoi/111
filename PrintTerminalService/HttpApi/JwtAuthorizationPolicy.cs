using EC5.WebSite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintTerminalService.HttpApi
{
    /// <summary>
    /// JWT 的授权
    /// </summary>
    public class JwtAuthorizationPolicy : AuthorizationPolicy
    {
        public override bool Authorize(HttpContext context, IAuthorizeData authorData)
        {
            bool isAuth = true;

            if (authorData is AuthorizeAttribute auth)
            {
                string token = context.Request.GetHeader("token");

                //Console.WriteLine($"拿到 token={token}");

                //context.Response.Status = "401 Unauthorized";

                //context.Response.End();
            }

            return isAuth;
        }
    }
}
