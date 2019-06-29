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
            var apiUrl = "http://joey.com/";
            using (var httpClient = new HttpClient() {BaseAddress = new Uri(apiUrl)})
            {
                var isAccountLocked = httpClient.PostAsJsonAsync("api/FailCounter/Get", account).Result;
                if (isAccountLocked.IsSuccessStatusCode == false)
                {
                    throw new Exception($"web api error, accountId:{account}");
                }

                isAccountLocked.EnsureSuccessStatusCode();
                // 帳號被lock了
                if (isAccountLocked.Content.ReadAsAsync<bool>().Result)
                {
//                    throw new FailedTooManyTimesException();
                    throw new Exception("FailedTooManyTimesException");
                }
            }

            // 取得帳號的password
            var hashPassword = "";
            using (var connection = new SqlConnection("my connection string"))
            {
                hashPassword = connection.Query<string>("spGetUserPassword", new {Id = account},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            // 取得密碼hash
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            // 取得帳號當下的Otp
            var currentOtp = "";
            using (var httpClient = new HttpClient() {BaseAddress = new Uri(apiUrl)})
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
                // 成功之後重計
                using (var httpClient = new HttpClient() {BaseAddress = new Uri(apiUrl)})
                {
                    var response = httpClient.PostAsJsonAsync("api/FailCounter/Reset", account).Result;
                    if (response.IsSuccessStatusCode == false)
                    {
                        throw new Exception($"web api error, accountId:{account}");
                    }
                }
                return true;
            }

            // Slack通知User
            var slackClient = new SlackClient("my Api token");
            slackClient.PostMessage(r => { }, "mychannel", "message");

            // 計算失敗次數
            using (var httpClient = new HttpClient() {BaseAddress = new Uri(apiUrl)})
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