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
        private readonly ProfileDao _profileDao = new ProfileDao();
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OtpService _otpService = new OtpService();
        private readonly FailCounter _failCounter = new FailCounter();
        private readonly NLogAdapter _nLogAdapter = new NLogAdapter();
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();

        public bool Verify(string accountId, string password, string inputOtp)
        {
            // 檢查是否Lock
            if (_failCounter.IsLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }

            // 取得密碼hash
            var hashPassword = _sha256Adapter.Hash(password);

            // 取得帳號當下的Otp
            var currentOtp = _otpService.Get(accountId);

            // 取得帳號的password
            var dbHashPassword = _profileDao.GetPassword(accountId);

            // 比對正確性
            if (inputOtp == currentOtp && hashPassword.ToString() == dbHashPassword)
            {
                // 成功之後重計
                _failCounter.Reset(accountId);
                return true;
            }

            // Slack通知User
            _slackAdapter.PushMessage(accountId);

            // 計算失敗次數
            _failCounter.Add(accountId);

            // 在取得現在的失敗次數之後紀錄log
            var failCount = _failCounter.Get(accountId);
            _nLogAdapter.Info($"account={accountId}, errorCount = {failCount}");

            return false;
        }
    }

    public class SlackAdapter
    {
        public void PushMessage(string accountId)
        {
            var slackClient = new SlackClient("my Api token");
            slackClient.PostMessage(r => { }, "mychannel", $"message {accountId}");
        }
    }

    public class NLogAdapter
    {
        public void Info(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }

    public class FailCounter
    {
        public void Reset(string accountId)
        {
            var resetResponse = new HttpClient()
            {
                BaseAddress = new Uri("http://joey.com/")
            }.PostAsJsonAsync("api/FailCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public void Add(string accountId)
        {
            var addResponse = new HttpClient()
            {
                BaseAddress = new Uri("http://joey.com/")
            }.PostAsJsonAsync("api/FailCounter/Add", accountId).Result;
            addResponse.EnsureSuccessStatusCode();
        }

        public int Get(string accountId)
        {
            var failedCountResponse = new HttpClient()
            {
                BaseAddress = new Uri("http://joey.com/")
            }.PostAsJsonAsync("api/FailCounter/Get", accountId).Result;
            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        public bool IsLocked(string accountId)
        {
            var isAccountLockedResponse = new HttpClient()
            {
                BaseAddress = new Uri("http://joey.com/")
            }.PostAsJsonAsync("api/FailCounter/IsLock", accountId).Result;
            isAccountLockedResponse.EnsureSuccessStatusCode();
            // 檢查帳號是否被lock了
            var isLock = isAccountLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLock;
        }
    }

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