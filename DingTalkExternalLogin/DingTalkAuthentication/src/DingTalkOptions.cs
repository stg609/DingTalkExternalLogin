using System;
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

            // 必须，用于 Asp.net core identity GetExternalLoginInfoAsync 判断用户是否存在
            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "openId");

            // 额外的标准的 claim
            ClaimActions.MapJsonKey("name", "name");
            ClaimActions.MapJsonKey("sub", "userid");
            ClaimActions.MapJsonKey("nickname", "nickname");
            ClaimActions.MapJsonKey("phone_number", "mobile");
            ClaimActions.MapJsonKey("email", "email");
            ClaimActions.MapJsonKey("picture", "avatar");

            // 额外的非标准 claim
            ClaimActions.MapJsonKey(DingTalkClaimTypes.UnionId, "unionid");
            ClaimActions.MapJsonKey(DingTalkClaimTypes.JobNumber, "jobnumber");
            ClaimActions.MapJsonKey(DingTalkClaimTypes.Position, "position");
            ClaimActions.MapJsonKey(DingTalkClaimTypes.IsSenior, "isSenior");
            ClaimActions.MapJsonKey(DingTalkClaimTypes.HiredDate, "hiredDate");
            ClaimActions.MapJsonKey(DingTalkClaimTypes.IsLeaderInDepts, "isLeaderInDepts");
            ClaimActions.MapJsonKey(DingTalkClaimTypes.IsBoss, "isBoss");
            ClaimActions.MapJsonKey(DingTalkClaimTypes.RealAuthed, "realAuthed");
            ClaimActions.MapJsonKey(DingTalkClaimTypes.OrgEmail, "orgEmail");
            ClaimActions.MapJsonKey(DingTalkClaimTypes.IsAdmin, "isAdmin");
            ClaimActions.MapJsonKey(DingTalkClaimTypes.Extattr, "extattr");
        }

        /// <summary>
        /// 企业内部开发小程序的 App Key
        /// </summary>
        public string AppKey { get; set; }

        /// <summary>
        /// 企业内部开发小程序的 App Secret
        /// </summary>
        public string AppSecret { get; set; }

        /// <summary>
        /// 是否包含该用户在企业内的 用户信息（如：UserId、姓名、手机号、工号等）
        /// </summary>
        /// <remarks>默认为 false, 只会包含用户在 钉钉中的 unionId 及 nickname。如果需要包含用户在企业内的 UserId、姓名、手机号、工号，需要提供钉钉企业内小程序对应的 AppId、Secret</remarks>
        public bool IncludeUserInfo { get; set; }


        public override void Validate()
        {
            base.Validate();

            if (IncludeUserInfo)
            {
                if (string.IsNullOrEmpty(AppKey))
                {
                    throw new ArgumentException(nameof(AppKey));
                }

                if (string.IsNullOrEmpty(AppKey))
                {
                    throw new ArgumentException(nameof(AppSecret));
                }
            }
        }
    }

}
