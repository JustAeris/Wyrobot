using System;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using DSharpPlus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Wyrobot.Core;

namespace Wyrobot.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        [HttpGet("~/signin")]
        public IActionResult SignIn()
        {
            var returnUrl = HttpContext.Request.Query["ReturnUrl"].ToString();

            if (
                string.IsNullOrWhiteSpace(returnUrl)
                || !Url.IsLocalUrl(returnUrl)
            )
            {
                returnUrl = "/";
            }

            return Challenge(
                new AuthenticationProperties {RedirectUri =  returnUrl},
                "Discord");
        }

        [HttpGet("~/signout")]
        [HttpPost("~/signout")]
        public IActionResult SignOut()
        {
            // Instruct the cookies middleware to delete the local cookie created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow.
            return SignOut(new AuthenticationProperties {RedirectUri = "/"},
                CookieAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet("~/signin-discord")]
        public string SignInDiscord(string code, string state)
        {
            var client = new DiscordRestClient(new DiscordConfiguration
            {
                Token = state,
                MinimumLogLevel = LogLevel.Debug,
                TokenType = TokenType.Bearer
            });

            return client.CurrentUser.Email;
        }
    }
}
