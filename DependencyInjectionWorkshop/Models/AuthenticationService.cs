using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string account, string password, string inputOtp)
        {
            using (var httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")})
            {
                var isAccountLocked = httpClient.PostAsJsonAsync("api/FailCounter/Get", account).Result;
                if (isAccountLocked.IsSuccessStatusCode == false)
                {
                    throw new Exception($"web api error, accountId:{account}");
                }

                isAccountLocked.EnsureSuccessStatusCode();
                if (isAccountLocked.Content.ReadAsAsync<bool>().Result)
                {
//                    throw new FailedTooManyTimesException();
                    throw new Exception("FailedTooManyTimesException");
                }
            }

            var hashPassword = "";
            using (var connection = new SqlConnection("my connection string"))
            {
                hashPassword = connection.Query<string>("spGetUserPassword", new {Id = account},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var currentOtp = "";

            using (var httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")})
            {
                var response = httpClient.PostAsJsonAsync("api/otps", account).Result;
                if (response.IsSuccessStatusCode)
                {
                    currentOtp = response.Content.ReadAsAsync<string>().Result;
                }
                else
                {
                    throw new Exception($"web api error, accountId:{account}");
                }
            }

            if (inputOtp == currentOtp && hash.ToString() == hashPassword)
            {
                using (var httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")})
                {
                    var response = httpClient.PostAsJsonAsync("api/FailCounter/Reset", account).Result;
                    if (response.IsSuccessStatusCode == false)
                    {
                        throw new Exception($"web api error, accountId:{account}");
                    }
                }
                return true;
            }

            // 通知
            var slackClient = new SlackClient("my Api token");
            slackClient.PostMessage(r => { }, "mychannel", "message");

            // 計算失敗次數
            using (var httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")})
            {
                var response = httpClient.PostAsJsonAsync("api/FailCounter/Add", account).Result;
                if (response.IsSuccessStatusCode == false)
                {
                    throw new Exception($"web api error, accountId:{account}");
                }
            }

            return false;
        }
    }
}