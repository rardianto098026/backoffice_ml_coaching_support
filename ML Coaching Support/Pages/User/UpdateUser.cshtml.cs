using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;

namespace ML_Coaching_Support.Pages.User
{
    public class UpdateUserModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public UpdateUserModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public class UserModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.EmailAddress)]
            public string Email { get; set; }

            [Required]
            public string Role { get; set; }

            public List<SelectListItem> AvailableRoles { get; set; }
        }

        [BindProperty]
        public UserModel User { get; set; }

        public async Task<IActionResult> OnGet()
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                var common = new Common.Common();
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    string userID = Common.Common.GlobalVariables.UserID;
                    string token = Common.Common.GlobalVariables.Token;

                    if (userID != null && token != null)
                    {
                        string apiUrl_CheckLogin = _configuration.GetSection("ApiUrls:apiUrl_CheckLogin").Value;
                        string apiUrlWithParams = apiUrl_CheckLogin.Replace("{userID}", userID).Replace("{token}", token);
                        using (HttpClient client = new HttpClient())
                        {
                            // Send the GET request to the endpoint URL with the query parameters
                            HttpResponseMessage response = await client.GetAsync(apiUrlWithParams);

                            if (response.ToString() == "true")
                            {
                                // User has a token, redirect to another page
                                return Redirect("/Login/Index");
                            }
                        }

                        User = new UserModel
                        {
                            AvailableRoles = new List<SelectListItem>
                            {
                                new SelectListItem { Value = "1", Text = "Super Admin" },
                                new SelectListItem { Value = "2", Text = "Admin" },
                                new SelectListItem { Value = "3", Text = "User" },
                            }
                        };
                    }
                    else
                    {
                        return Redirect("/Login/Index");
                    }

                }

            }
            catch (Exception ex)
            {
                //Handle
                // Generate JavaScript code to show the error message
                string errorMessage = $"alert('An error occurred: {ex.Message}');";

                // Register the generated JavaScript code to be rendered on the page
                ViewData["ErrorMessageScript"] = errorMessage;
            }
            return Page();
        }

        public async Task<IActionResult> OnPost(UserModel user)
        {
            string apiUrl = _configuration.GetValue<string>("ApiUrls:apiUrl_AddUser");

            // Send the POST request
            using (HttpClient client = new HttpClient())
            {
                // Create the request body as key-value pairs
                var requestBody = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("Username", user.Username),
                    new KeyValuePair<string, string>("Password", user.Password),
                    new KeyValuePair<string, string>("RoleId", user.Role),
                    new KeyValuePair<string, string>("Email", user.Email),
                    new KeyValuePair<string, string>("UserID", Common.Common.GlobalVariables.UserID ),
                    new KeyValuePair<string, string>("Token", Common.Common.GlobalVariables.Token )
                });

                HttpResponseMessage response = await client.PostAsync(apiUrl, requestBody);

                // Check the response status
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("SuccessAction"); // Redirect to a success action
                }
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    // Log or handle the error
                    // You can also examine response.StatusCode or responseContent to get more information about the error

                    return RedirectToAction("ErrorAction"); // Redirect to an error action
                }
            }
        }
    }
}
