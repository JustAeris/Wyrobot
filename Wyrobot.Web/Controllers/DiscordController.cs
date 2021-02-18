using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wyrobot.Web.Http.Discord;

namespace Wyrobot.Web.Controllers
{
    public class DiscordController : Controller
    {
        private DiscordAuthClient _client;

        public DiscordController()
        {
            _client = new DiscordAuthClient
            {
                // TODO: Yup, some hardcoded tokens. These give access to a test bot of mine (DelightedCat),
                // we gotta store these credentials in a more secure way in the future though
                ClientId = "754358455099457637",
                ClientSecret = "D_orFHwivVYFYMyYMydXChqnljEU6m7x",
                RedirectUrl = "https://localhost:44380/Discord/Redirect/",
                Scopes = new List<string>
                {
                    "identify", "guilds", // TODO: Add more scopes if necessary
                }
            };
        }

        public IActionResult Authorize()
        {
            return new RedirectResult(_client.BuildAuthorizeUrl(), false);
        }

        public async Task<IActionResult> Redirect()
        {
            if (!Request.Query.ContainsKey("code")) return new RedirectResult("/", false);

            var response = await _client.RequestAccessToken(Request.Query["code"]);

            Response.Cookies.Append("DiscordAuth", JsonConvert.SerializeObject(response), new CookieOptions
            {
                Secure = true,
                HttpOnly = true,
                Expires = DateTimeOffset.FromUnixTimeSeconds(int.MaxValue),
                IsEssential = true
            });

            return Redirect("/");
        }
    }
}
