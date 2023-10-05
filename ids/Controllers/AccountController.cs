using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;
using IdentityModel;
using ids.Models;
using ids.Views.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace ids.Controllers
{

    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;       

        public AccountController (IIdentityServerInteractionService interaction,
            IEventService events, UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _interaction = interaction;
            _events = events;
            _signInManager = signInManager;
            _userManager = userManager;
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
        public IActionResult Registration()
        {
            return View(new RegistrationModel());
        }
        [HttpPost]
        public async Task<IActionResult> Registration (RegistrationModel model)
        {
            if (model.Username == null || model.Password == null || model.Email == null)
            {
                return View(model);
            }
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ResponseModel { Status = "Error", Message = "User already exists!" });
            }
            IdentityUser user = new()
            {
                UserName = model.Username,
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            }
            //добавление роли
            await _userManager.AddToRoleAsync(user, "User");
           
            result = _userManager.AddClaimsAsync(user, new Claim[]
            {
              new Claim(JwtClaimTypes.Name, model.Username),
              new Claim(JwtClaimTypes.GivenName, model.Username),
              new Claim(JwtClaimTypes.FamilyName, model.Username),
              new Claim(JwtClaimTypes.WebSite, model.Email),
            }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            return RedirectToAction("Login", "Account");
        }
        [HttpGet]
        public  IActionResult Logout(string url)
        {
           return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();   
            return RedirectToAction("Login", "Account");
        }
    }
}
