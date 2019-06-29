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
        private static HttpClient _httpClient;

        public bool Verify(string accountId, string password, string inputOtp)
        {
            var apiUrl = "http://joey.com/";
            _httpClient = new HttpClient() {BaseAddress = new Uri(apiUrl)};

            // 檢查是否Lock
            if (AccountIsLock(accountId))
            {
                throw new FailedTooManyTimesException();
            }

            // 取得密碼hash
            var hashPassword = GetHashPassword(password);

            // 取得帳號當下的Otp
            var currentOtp = GetCurrentOtp(accountId);

            // 取得帳號的password
            var dbHashPassword = GetCurrentPasswordFromDB(accountId);

            // 比對正確性
            if (inputOtp == currentOtp && hashPassword.ToString() == dbHashPassword)
            {
                // 成功之後重計
                ResetFailCount(accountId);
                return true;
            }

            // Slack通知User
            PushMessage();

            // 計算失敗次數
            AddFailCount(accountId);

            // 在取得現在的失敗次數之後紀錄log
            LogFailCount(accountId);

            return false;
        }

        private static bool AccountIsLock(string accountId)
        {
            var isAccountLockedResponse = _httpClient.PostAsJsonAsync("api/FailCounter/IsLock", accountId).Result;
            isAccountLockedResponse.EnsureSuccessStatusCode();
            // 檢查帳號是否被lock了
            var isLock = isAccountLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLock;
        }

        private static void PushMessage()
        {
            var slackClient = new SlackClient("my Api token");
            slackClient.PostMessage(r => { }, "mychannel", "message");
        }

        private static void LogFailCount(string accountId)
        {
            var failedCount = GetFailCount(accountId);
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }

        private static int GetFailCount(string accountId)
        {
            var failedCountResponse = _httpClient.PostAsJsonAsync("api/FailCounter/Get", accountId).Result;
            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        private static void AddFailCount(string accountId)
        {
            var addResponse = _httpClient.PostAsJsonAsync("api/FailCounter/Add", accountId).Result;
            addResponse.EnsureSuccessStatusCode();
        }

        private static void ResetFailCount(string accountId)
        {
            var resetResponse = _httpClient.PostAsJsonAsync("api/FailCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        private static string GetCurrentOtp(string accountId)
        {
            var currentOtp = "";
            var otpResponse = _httpClient.PostAsJsonAsync("api/otps", accountId).Result;
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