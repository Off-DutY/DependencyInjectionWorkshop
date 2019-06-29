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
            // 檢查是否Lock
            if (IsAccountLock(accountId))
            {
                throw new FailedTooManyTimesException();
            }

            // 取得密碼hash
            var hashPassword = new Sha256Adapter().Hash(password);

            // 取得帳號當下的Otp
            var currentOtp = GetCurrentOtp(accountId);

            // 取得帳號的password
            var dbHashPassword = new ProfileDao().GetPassword(accountId);

            // 比對正確性
            if (inputOtp == currentOtp && hashPassword.ToString() == dbHashPassword)
            {
                // 成功之後重計
                ResetFailCount(accountId);
                return true;
            }

            // Slack通知User
            PushMessage(accountId);

            // 計算失敗次數
            AddFailCount(accountId);

            // 在取得現在的失敗次數之後紀錄log
            LogFailCount(accountId);

            return false;
        }

        private static bool IsAccountLock(string accountId)
        {
            var isAccountLockedResponse = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}.PostAsJsonAsync("api/FailCounter/IsLock", accountId).Result;
            isAccountLockedResponse.EnsureSuccessStatusCode();
            // 檢查帳號是否被lock了
            var isLock = isAccountLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLock;
        }

        private static void PushMessage(string accountId)
        {
            var slackClient = new SlackClient("my Api token");
            slackClient.PostMessage(r => { }, "mychannel", $"message {accountId}");
        }

        private static void LogFailCount(string accountId)
        {
            var failedCount = GetFailCount(accountId);
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }

        private static int GetFailCount(string accountId)
        {
            var failedCountResponse = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}.PostAsJsonAsync("api/FailCounter/Get", accountId).Result;
            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        private static void AddFailCount(string accountId)
        {
            var addResponse = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}.PostAsJsonAsync("api/FailCounter/Add", accountId).Result;
            addResponse.EnsureSuccessStatusCode();
        }

        private static void ResetFailCount(string accountId)
        {
            var resetResponse = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}.PostAsJsonAsync("api/FailCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        private static string GetCurrentOtp(string accountId)
        {
            var currentOtp = "";
            var otpResponse = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}.PostAsJsonAsync("api/otps", accountId).Result;
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
    }

    public class Sha256Adapter
    {
        public StringBuilder Hash(string plainText)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(plainText));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash;
        }
    }

    public class ProfileDao
    {
        public string GetPassword(string accountId)
        {
            using (var connection = new SqlConnection("my connection string"))
            {
                return SqlMapper.Query<string>(connection, "spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}