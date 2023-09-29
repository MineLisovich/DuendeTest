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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Weather()
        {
            using var client = new HttpClient();
            // var token = await _tokenService.GetTokenAsync("weatherApiResurs.read");
            var token = await HttpContext.GetTokenAsync("access_token");

            //client.SetBearerToken(token.AccessToken);
            client.SetBearerToken(token);

            var result = await client.GetAsync("https://localhost:5445/WeatherForecast");
            if (result.IsSuccessStatusCode)
            {
                var model = await result.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<WeatherData>>(model);
                return View(data);
            }
            throw new Exception("Error to get contetn");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}