提供钉钉外部扫码登陆，用于集成到 Asp.Net Core Identity。

### 前提   
1. 开发者自行在钉钉开发者平台中注册 **企业内部应用-小程序**
1. 开发者自行在钉钉开发者平台中注册 **移动接入应用-登陆**

### 基本用法   
1. 引入 nuget package: `Install-Package Charlie.AspNetCore.Authentication.DingTalk`
2. 增加如下代码
```csharp
services.AddAuthentication()
    .AddDingTalk(opts =>
    {
        opts.ClientId = 钉钉小程序的 ClientId, 用于访问钉钉企业中的用户数据; 钉钉访问用户信息的时候需要使用
        opts.ClientSecret = 钉钉小程序的 ClientSecret;

        opts.QrLoginAppId = 钉钉登陆的 AppId;
        opts.QrLoginAppSecret = 钉钉登陆的 AppSecret;
     }
```

### 使用方法   
#### 使用钉钉的扫码页面   
1. 创建一个 Asp.Net Core Web 应用程序（MVC)
1. 修改**身份验证**，选择 **个人用户账户**
1. 在 `Startup.cs` 的 `ConfigureServices` 中，增加如下代码：
   ```csharp
    services.AddAuthentication()
        .AddDingTalk(opts =>
        {
            opts.ClientId = 钉钉小程序的 ClientId, 用于访问钉钉企业中的用户数据;
            opts.ClientSecret = 钉钉小程序的 ClientSecret;

            opts.QrLoginAppId = 钉钉登陆的 AppId;
            opts.QrLoginAppSecret = 钉钉登陆的 AppSecret;
         }
    ```

#### 使用自定义的扫码页面

相比钉钉自带的扫码页面，使用自定义页面需要额外几个步骤
1. 【同上】创建一个 Asp.Net Core Web 应用程序（MVC)
1. 【同上】修改**身份验证**，选择 **个人用户账户**
1. 右键单击项目，选择 **添加-新搭建基架的项目...**，然后选择 **标识**，在弹出框中，选择 **Account\Login**。这个步骤会在项目中创建 Asp.Net Core Identity 的 **Login** Razor Page
1. 在 **Areas\Identity\Pages\Account** 目录中，添加一个 **DingTalkLogin** Razor Page。这个 Page 就是用来渲染自己的 QR 页面，在 **cshtml** 中加入：
   ```javascript
    <script src="https://g.alicdn.com/dingding/dinglogin/0.0.5/ddLogin.js"></script>
    <script>
        var dingtalk = "https://oapi.dingtalk.com/connect/oauth2/sns_authorize?appid=@(Model.AppId)&response_type=@(Model.ResponseType)&scope=@(Model.Scope)&state=@(Model.State)&redirect_uri=@(Model.RedirectUri)";

            DDLogin({
                id: "code-img",
                goto: encodeURIComponent(dingtalk),
                style: "border:none;background-color:#FFFFFF;margin-top:-40px;",
                width: "400",
                height: "300"
            });
            var handleMessage = function (event) {
                if (event.origin == "https://login.dingtalk.com") {
                    window.top.location.href = dingtalk + "&loginTmpCode=" + event.data;
                }
            };
            if (typeof window.addEventListener != 'undefined') {
                window.addEventListener('message', handleMessage, false);
            } else if (typeof window.attachEvent != 'undefined') {
                window.attachEvent('onmessage', handleMessage);
            }

    </script>
    ```
1. 修改 **Areas\Identity\Pages\Account** 目录中的 Login.cshtml, 增加一个 Iframe 用于显示扫码页面：
   ```html
       <div style="height:300px">
            <form id="external-account" asp-page="./ExternalLogin" asp-route-returnUrl="@Model.ReturnUrl" method="post" target="ifmDingTalk" style="display:none">
                <div>
                    <p>
                        <button id="dingTalkLoginBtn" type="submit" class="btn btn-primary" name="provider" value="DingTalk"></button>
                    </p>
                </div>
            </form>
            <iframe name="ifmDingTalk" scrolling="no" style="width:450px;height:300px">
            </iframe>
        </div>
       
        @section Scripts {
            <script>
                $("#dingTalkLoginBtn").click();
            </script>
        }

   ```
1. 修改 `Startup.cs` 在 `ConfigureServices` 中增加:
   ```
    services.AddAuthentication().AddDingTalk(opts =>
    {
        opts.ClientId = dingTalkOpts.ClientId;
        opts.ClientSecret = dingTalkOpts.ClientSecret;

        opts.QrLoginAppId = dingTalkOpts.QrLoginAppId;
        opts.QrLoginAppSecret = dingTalkOpts.QrLoginAppSecret;


        opts.SignInScheme = IdentityConstants.ExternalScheme;

        // 由于是使用自己的 扫码 页面，则必须定义自己的 授权节点
        opts.AuthorizationEndpoint = "/Identity/Account/DingTalkLogin";

        // 演示如何把外部登陆的错误信息显示在 Razor Page 上
        opts.Events.OnRemoteFailure = async ctx =>
        {
            var tempDataProvider = ctx.HttpContext.RequestServices.GetRequiredService<ITempDataProvider>();

            tempDataProvider.SaveTempData(ctx.HttpContext, new Dictionary<string, object>
                    {
                        { "ErrorMessage",ctx.Failure.Message }
                    });
            ctx.Response.Redirect("/Identity/Account/Login");
            ctx.HandleResponse();

            await Task.CompletedTask;
        };
    });
    ```
   
#### Demo
详见解决方案中对应的 Demo 项目
   
