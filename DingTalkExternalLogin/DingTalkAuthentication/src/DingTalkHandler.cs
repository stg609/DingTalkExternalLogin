using System;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using DingTalk.Api;
using DingTalk.Api.Request;
using DingTalk.Api.Response;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{


    /// <summary>
    /// DingDing External Login Hanlder
    /// </summary>
    /// <see cref="https://ding-doc.dingtalk.com/document#/org-dev-guide/kymkv6"/>
    public class DingTalkHandler : OAuthHandler<DingTalkOptions>
    {
        public DingTalkHandler(IOptionsMonitor<DingTalkOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {
            var state = Options.StateDataFormat.Protect(properties);
            var endpoint = QueryHelpers.AddQueryString(Options.AuthorizationEndpoint, "appid", Options.QrLoginAppId);

            // 拼接钉钉的地址，如：https://oapi.dingtalk.com/connect/qrconnect?appid=APPID&response_type=code&scope=snsapi_login&state=STATE&redirect_uri=REDIRECT_URI
            endpoint = QueryHelpers.AddQueryString(endpoint, "response_type", "code");
            endpoint = QueryHelpers.AddQueryString(endpoint, "scope", "snsapi_login");
            endpoint = QueryHelpers.AddQueryString(endpoint, "state", state);
            endpoint = QueryHelpers.AddQueryString(endpoint, "redirect_uri", redirectUri);

            return endpoint;
        }

        /// <summary>
        /// 利用钉钉返回的 临时 Code 得到用户信息 (及 access token)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Task<OAuthTokenResponse> ExchangeCodeAsync(OAuthCodeExchangeContext context)
        {
            // 1. 根据 code 获取 unionId
            DefaultDingTalkClient userInfoClient = new DefaultDingTalkClient("https://oapi.dingtalk.com/sns/getuserinfo_bycode");
            OapiSnsGetuserinfoBycodeRequest userInfoReq = new OapiSnsGetuserinfoBycodeRequest();
            userInfoReq.TmpAuthCode = context.Code;
            OapiSnsGetuserinfoBycodeResponse userInfoResp = userInfoClient.Execute(userInfoReq, Options.QrLoginAppId, Options.QrLoginAppSecret);

            // 2. 获取 access token
            IDingTalkClient getTokenClient = new DefaultDingTalkClient("https://oapi.dingtalk.com/gettoken");
            OapiGettokenRequest getTokenReq = new OapiGettokenRequest();
            getTokenReq.SetHttpMethod("GET");
            getTokenReq.Appkey = Options.ClientId;
            getTokenReq.Appsecret = Options.ClientSecret;
            OapiGettokenResponse getTokenResp = getTokenClient.Execute(getTokenReq);

            // 3. 根据 unionId, access token 去获取 userId，因为只有 userId 才是企业内用户的唯一编号
            IDingTalkClient getUserIdClient = new DefaultDingTalkClient("https://oapi.dingtalk.com/user/getUseridByUnionid");
            OapiUserGetUseridByUnionidRequest getUserIdReq = new OapiUserGetUseridByUnionidRequest();
            getUserIdReq.SetHttpMethod("GET");
            getUserIdReq.Unionid = userInfoResp.UserInfo.Unionid;
            OapiUserGetUseridByUnionidResponse getUserIdResp = getUserIdClient.Execute(getUserIdReq, getTokenResp.AccessToken);

            // 4. 生成 Response
            if (getUserIdResp != null && !String.IsNullOrWhiteSpace(getUserIdResp.Userid))
            {
                var payload = JsonDocument.Parse($@"{{""access_token"":""{getTokenResp.AccessToken}"",""expires_in"":""{getTokenResp.ExpiresIn}"",""userid"":""{getUserIdResp.Userid}""}}");
                return Task.FromResult(OAuthTokenResponse.Success(payload));
            }
            else
            {
                return Task.FromResult(OAuthTokenResponse.Failed(new Exception(getUserIdResp.Errmsg)));
            }
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            // 钉钉提供的获取用户信息的地址
            string endpoint = QueryHelpers.AddQueryString(Options.UserInformationEndpoint, "access_token", tokens.AccessToken);
            string userId = tokens.Response.RootElement.GetString("userid");

            endpoint = QueryHelpers.AddQueryString(endpoint, "userid", userId);

            // 获取钉钉的用户信息
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

            var response = await Backchannel.SendAsync(request, Context.RequestAborted);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"An error occurred when retrieving Dingtalk user information ({response.StatusCode}).");
            }

            string rawPayload = await response.Content.ReadAsStringAsync();
            var dingTalkUserInfo = JsonSerializer.Deserialize<DingTalkUserInfoDto>(rawPayload, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (dingTalkUserInfo.Errcode != 0)
            {
                throw new HttpRequestException($"An error occurred when retrieving Dingtalk user information ({dingTalkUserInfo.Errmsg}).");
            }

            var payload = JsonDocument.Parse(rawPayload);
            var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme, Options, Backchannel, tokens, payload.RootElement);
            context.RunClaimActions();

            await Events.CreatingTicket(context);
            return new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);
        }
    }

}
