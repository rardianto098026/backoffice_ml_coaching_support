using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ML_Coaching_Support.Pages.User
{
    public class ListUserModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public List<UserModel> Users { get; set; }
        public string api_url_delete { get; set; }
        public ListUserModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnGet()
        {
            Users = await GetUsersAsync();
            api_url_delete = _configuration.GetValue<string>("ApiUrls:apiUrl_DeleteUserUrl");
        }

        private async Task<List<UserModel>> GetUsersAsync()
        {
            string apiUrl = _configuration.GetValue<string>("ApiUrls:apiUrl_GetAllUsers");

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    List<UserModel> userList = JsonConvert.DeserializeObject<List<UserModel>>(json);
                    return userList;
                }
                else
                {
                    // Handle API error response
                    throw new Exception($"Failed to retrieve users. Status code: {response.StatusCode}");
                }
            }
        }

        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                string apiUrlDeleteUser = _configuration["ApiUrls:apiUrl_DeleteUser"];

                // Send a DELETE request to the API endpoint to delete the user
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.DeleteAsync($"{apiUrlDeleteUser}/{id}");

                    if (response.IsSuccessStatusCode)
                    {
                        // User deleted successfully
                        return RedirectToPage();
                    }
                    else
                    {
                        // Handle the error response accordingly
                        string errorMessage = $"Failed to delete the user. Status code: {response.StatusCode}";
                        // ...
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle the exception accordingly
                string errorMessage = $"An error occurred while deleting the user: {ex.Message}";
                // ...
            }

            return RedirectToPage();
        }

        public class UserModel
        {
            public int UserID { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public int RoleID { get; set; }
            public string Email { get; set; }
            public string CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public int Status { get; set; }
        }
    }
}
