using System;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Ding Ding 外部登录选项
    /// </summary>
    public class DingTalkOptions : OAuthOptions
    {
        public DingTalkOptions()
        {
            CallbackPath = new PathString("/signin-dingtalk");
            AuthorizationEndpoint = "https://oapi.dingtalk.com/connect/qrconnect";
            TokenEndpoint = "https://NOT_USED";
            UserInformationEndpoint = "https://oapi.dingtalk.com/user/get";

            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "userid");
            ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
            ClaimActions.MapJsonKey(ClaimTypes.MobilePhone, "mobile");
            ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        }

        /// <summary>
        /// 用于扫码登陆的 App Id
        /// </summary>
        public string QrLoginAppId { get; set; }

        /// <summary>
        /// 用于扫码登陆的 App Secret
        /// </summary>
        public string QrLoginAppSecret { get; set; }


        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrEmpty(QrLoginAppId))
            {
                throw new ArgumentException(nameof(QrLoginAppId));
            }

            if (string.IsNullOrEmpty(QrLoginAppId))
            {
                throw new ArgumentException(nameof(QrLoginAppSecret));
            }
        }
    }

}
