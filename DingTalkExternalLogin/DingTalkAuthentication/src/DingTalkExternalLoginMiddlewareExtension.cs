using System;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 钉钉外部登录扩展
    /// </summary>
    public static class DingTalkExternalLoginMiddlewareExtension
    {
        /// <summary>
        /// 提供DingDing外部登陆
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static AuthenticationBuilder AddDingTalk(this AuthenticationBuilder builder, Action<DingTalkOptions> configureOptions) =>
            builder.AddOAuth<DingTalkOptions, DingTalkHandler>("DingTalk", "DingTalk", configureOptions);

        /// <summary>
        /// 提供DingDing外部登陆
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="displayName">显示的名称</param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static AuthenticationBuilder AddDingTalk(this AuthenticationBuilder builder, string displayName, Action<DingTalkOptions> configureOptions) =>
           builder.AddOAuth<DingTalkOptions, DingTalkHandler>("DingTalk", displayName, configureOptions);
    }
}
