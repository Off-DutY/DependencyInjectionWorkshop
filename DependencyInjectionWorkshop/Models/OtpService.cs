using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class OtpService
    {
        public string Get(string accountId)
        {
            var otpResponse = new HttpClient()
            {
                BaseAddress = new Uri("http://joey.com/")
            }.PostAsJsonAsync("api/otps", accountId).Result;
            if (otpResponse.IsSuccessStatusCode)
            {
                return otpResponse.Content.ReadAsAsync<string>().Result;
            }
            throw new Exception($"web api error, accountId:{accountId}");
        }
    }
}