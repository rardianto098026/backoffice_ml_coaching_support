using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace ML_Coaching_Support.Pages.Login
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; }

        public class LoginInputModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public class UserResponse
        {
            public string UserID { get; set; }
            public string Token { get; set; }
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }


            string apiUrl_Login = _configuration.GetSection("ApiUrls:apiUrl_Login").Value; 

            // Create an instance of HttpClient
            using (HttpClient client = new HttpClient())
            {
                // Build the request URL
                string requestUrl = apiUrl_Login;

                // Create the request body as key-value pairs
                var requestBody = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("Username", Input.Username),
                    new KeyValuePair<string, string>("Password", Input.Password)
                });

                try
                {
                    // Send the POST request and retrieve the response
                    HttpResponseMessage response = await client.PostAsync(requestUrl, requestBody);

                    // Ensure the response is successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content
                    string responseBody = await response.Content.ReadAsStringAsync();
                    List<UserResponse> userResponses = JsonConvert.DeserializeObject<List<UserResponse>>(responseBody);

                    if(userResponses.Count() > 0)
                    {
                        Common.Common.GlobalVariables.UserID = userResponses[0].UserID;
                        Common.Common.GlobalVariables.Token = userResponses[0].Token;

                        // Redirect to the desired page passing the token as a query parameter
                        return Redirect($"/User/AddUser");
                    }
                    else
                    {
                        // Authentication failed
                        string errorMessage = $"alert('Invalid Username or Password');";

                        // Register the generated JavaScript code to be rendered on the page
                        ViewData["ErrorMessageScript"] = errorMessage;
                        return Page();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                    // Authentication failed
                    string errorMessage = $"alert('An error occurred: {ex.Message}');";

                    // Register the generated JavaScript code to be rendered on the page
                    ViewData["ErrorMessageScript"] = errorMessage;
                    return Page();
                }
            }
        }
    }
}
