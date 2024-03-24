using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;


public class AuthController : Controller
{
    private readonly HttpClient _httpClient;

    public AuthController()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("https://services2.i-centrum.se/") };
    }

    public IActionResult Login()
    {
        return View("~/Views/Home/Login.cshtml");
    }

    public IActionResult Logout()
    {
        // Redirect the user to the login page
        return RedirectToAction("Login", "Auth");
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        var content = new StringContent(JsonSerializer.Serialize(new { username = username, password = password }), System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("recruitment/auth", content);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonObject = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonResponse);

            if (jsonObject.ContainsKey("token"))
            {
                var token = jsonObject["token"];
                ViewData["Token"] = token; // Set token in ViewData

                // Call GetAvatar method to retrieve the avatar image URL
                var avatarUrl = await GetAvatar(token);
                ViewData["AvatarUrl"] = avatarUrl;

                return View("~/Views/Home/Profile.cshtml"); // Redirect to profile if login successful
            }
            else
            {
                ViewData["ErrorMessage"] = "Token not found in response.";
                return View("~/Views/Home/Login.cshtml"); // Return the login view with an error message
            }
        }
        else
        {
            ViewData["ErrorMessage"] = "Login request failed. Status code: " + response.StatusCode;
            return View("~/Views/Home/Login.cshtml"); // Return the login view with an error message
        }
    }

    public async Task<string> GetAvatar(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync("recruitment/profile/avatar");

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonObject = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonResponse);

            if (jsonObject.ContainsKey("data"))
            {
                var base64Image = jsonObject["data"];
                return $"data:image/png;base64,{base64Image}";
            }
        }

        return null;
    }
}
