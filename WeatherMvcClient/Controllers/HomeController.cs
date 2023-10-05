using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using WeatherMvcClient.Models;
using WeatherMvcClient.Services;

namespace WeatherMvcClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITokenService _tokenService;
        public HomeController(ILogger<HomeController> logger, ITokenService tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View();
            }
            using var client = new HttpClient();
            var token = await HttpContext.GetTokenAsync("access_token");
            client.SetBearerToken(token);
            var result = await client.GetAsync("https://localhost:5443/Account/GetUser");
            if (result.IsSuccessStatusCode)
            {
                var model = await result.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject(model);
                ViewBag.User = data;
                return View(ViewBag.User);
            }
            return Redirect("Index");

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Weather()
        {
            using var client = new HttpClient();

            //interactive
            var token = await HttpContext.GetTokenAsync("access_token");  
            client.SetBearerToken(token);

            //m2m 
            //var token = await _tokenService.GetTokenAsync("weatherApiResurs.read");
            //client.SetBearerToken(token.AccessToken);

            var result = await client.GetAsync("https://localhost:5445/WeatherForecast");
            if (result.IsSuccessStatusCode)
            {
                var model = await result.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<WeatherData>>(model);
                return View(data);
            }
            //if (result.StatusCode.ToString() == "Unauthorized") 
            //{
            //    await HttpContext.SignOutAsync();
            //}
            return Redirect("Index");
        }
        [Authorize]
        public async Task<IActionResult> GetMess()
        {
            using var client = new HttpClient();

            //interactive
            var token = await HttpContext.GetTokenAsync("access_token");
            client.SetBearerToken(token);

            //m2m 
            //var token = await _tokenService.GetTokenAsync("weatherApiResurs.read");
            //client.SetBearerToken(token.AccessToken);

            var result = await client.GetAsync("https://localhost:5445/Test");
            if (result.IsSuccessStatusCode)
            {
                var model = await result.Content.ReadAsStringAsync();

               MessModel data = new MessModel();
                data.Message = JsonConvert.DeserializeObject(model).ToString();
                return View(data);
            }
            return Redirect("Index");
        }


        [Authorize]
        public  IActionResult Login()
        {
            
            return Redirect("https://localhost:5444/Home/Index");
        }
        [AllowAnonymous]
        public async Task <IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("https://localhost:5443/Account/Logout");
        }

    
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}