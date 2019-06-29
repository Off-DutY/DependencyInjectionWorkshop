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
        public bool Verify(string accountId, string password, string inputOtp)
        {
            var apiUrl = "http://joey.com/";
            var httpClient = new HttpClient() {BaseAddress = new Uri(apiUrl)};

            var isAccountLockedResponse = httpClient.PostAsJsonAsync("api/FailCounter/Get", accountId).Result;
            isAccountLockedResponse.EnsureSuccessStatusCode();
            // 檢查帳號是否被lock了
            if (isAccountLockedResponse.Content.ReadAsAsync<bool>().Result)
            {
                throw new FailedTooManyTimesException();
            }

            // 取得密碼hash
            var hashPassword = GetHashPassword(password);

            // 取得帳號當下的Otp
            var currentOtp = GetCurrentOtp(accountId, httpClient);

            // 取得帳號的password
            var dbHashPassword = GetCurrentPasswordFromDB(accountId);

            if (inputOtp == currentOtp && hashPassword.ToString() == dbHashPassword)
            {
                // 成功之後重計
                var resetResponse = httpClient.PostAsJsonAsync("api/FailCounter/Reset", accountId).Result;
                resetResponse.EnsureSuccessStatusCode();
                return true;
            }

            // Slack通知User
            var slackClient = new SlackClient("my Api token");
            slackClient.PostMessage(r => { }, "mychannel", "message");

            // 計算失敗次數
            var addResponse = httpClient.PostAsJsonAsync("api/FailCounter/Add", accountId).Result;
            addResponse.EnsureSuccessStatusCode();

            // 在取得現在的失敗次數之後紀錄log
            var failedCountResponse = httpClient.PostAsJsonAsync("api/FailCounter/Get", accountId).Result;
            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");

            return false;
        }

        private static string GetCurrentOtp(string accountId, HttpClient httpClient)
        {
            var currentOtp = "";
            var otpResponse = httpClient.PostAsJsonAsync("api/otps", accountId).Result;
            if (otpResponse.IsSuccessStatusCode)
            {
                currentOtp = otpResponse.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }
            return currentOtp;
        }

        private static StringBuilder GetHashPassword(string password)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash;
        }

        private static string GetCurrentPasswordFromDB(string accountId)
        {
            var hashPassword = "";
            using (var connection = new SqlConnection("my connection string"))
            {
                hashPassword = connection.Query<string>("spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
            return hashPassword;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}