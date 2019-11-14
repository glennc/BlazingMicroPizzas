using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace BlazingPizza.Web
{
    [ApiController]
    public class UserController : Controller
    {
        private static UserInfo LoggedOutUser = new UserInfo { IsAuthenticated = false };

        [HttpGet("user")]
        public UserInfo GetUser()
        {
            return User.Identity.IsAuthenticated
                ? new UserInfo { Name = User.Identity.Name, IsAuthenticated = true }
                : LoggedOutUser;
        }

        [HttpGet("user/signin")]
        public async Task SignIn(string redirectUri)
        {
            if (string.IsNullOrEmpty(redirectUri) || !Url.IsLocalUrl(redirectUri))
            {
                redirectUri = "/";
            }

            await HttpContext.ChallengeAsync(
                MicrosoftAccountDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = redirectUri });
        }

        [HttpGet("user/signout")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("~/");
        }

        [Authorize]
        [HttpGet("user/signincompleted")]
        public IActionResult SignInCompleted()
        {
            var userState = GetUser();
            return Content($@"
                <script>
                    window.opener.onLoginPopupFinished({JsonConvert.SerializeObject(userState)});
                    window.close();
                </script>", "text/html");
        }
    }
}
