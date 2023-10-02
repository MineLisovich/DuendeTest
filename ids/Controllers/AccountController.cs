using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using IdentityServerHost.Pages;
using IdentityServerHost.Pages.Login;
using ids.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ids.Controllers
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController (IIdentityServerInteractionService interaction,
            IEventService events,
            SignInManager<IdentityUser> signInManager)
        {
            _interaction = interaction;
            _events = events;
            _signInManager = signInManager;
        }
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginModel { ReturnUrl = returnUrl });
        }
        [HttpPost]
        public async Task <IActionResult> Login(LoginModel model)
        {
            //Если модель не заполнена
            if (model.Username == null && model.Password == null) { return View(model); }

            // проверка, находимся ли мы в контексте запроса на авторизацию 
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);
            var user = await _signInManager.UserManager.FindByNameAsync(model.Username);

                // проверка имени пользователя/пароля по хранилищу в памяти
                if (user !=null && (await _signInManager.CheckPasswordSignInAsync(user, model.Password,false)) == Microsoft.AspNetCore.Identity.SignInResult.Success)
                {
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName, clientId: context?.Client.ClientId));

                   // установка явного срока действия только в том случае, если пользователь выберет "запомнить меня". 
                   // в противном случае срок действия будет по умолчанию (настроенный в промежуточном программном обеспечении cookie.)
                    AuthenticationProperties props = null;
                    if (LoginOptions.AllowLocalLogin && model.RememberLogin)
                    {
                        props = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.Add(LoginOptions.RememberMeLoginDuration)
                        };
                    }
                    //выдать файл cookie для аутентификации с ID и UserName пользователя
                    var isuser = new IdentityServerUser(user.Id)
                    {
                        DisplayName = user.UserName
                    };

                    await HttpContext.SignInAsync(isuser, props);
                    if (context != null)
                    {
                       //возвращаем пользователя на ту страницу с которой он был перенаправлен на авторизацию
                        return Redirect(model.ReturnUrl);
                    }
                    //если пользователь сам перешёл на страницу авторизации (то есть, не было перенаправления). То допустим можно направить его на страницу со списком систем
                   return RedirectToAction("Index", "Home");

                }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
