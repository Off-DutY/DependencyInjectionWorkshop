using System;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ILogger _logger;
        private readonly INotifier _notifier;
        private readonly IFailCounter _failCounter;
        private readonly IOtpService _otpService;
        private readonly IHash _hash;
        private readonly IProfile _profile;

        public AuthenticationService(INotifier notifier, ILogger logger, IFailCounter failCounter, IOtpService otpService, IHash hash, IProfile profile)
        {
            _notifier = notifier;
            _logger = logger;
            _failCounter = failCounter;
            _otpService = otpService;
            _hash = hash;
            _profile = profile;
        }

        public AuthenticationService()
        {
            _notifier = new SlackAdapter();
            _logger = new NLogAdapter();
            _failCounter = new FailCounter();
            _otpService = new OtpService();
            _hash = new Sha256Adapter();
            _profile = new ProfileDao();
        }


        public bool Verify(string accountId, string password, string inputOtp)
        {
            // 檢查是否Lock
            if (_failCounter.IsLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }

            // 取得密碼hash
            var hashPassword = _hash.Hash(password);

            // 取得帳號當下的Otp
            var currentOtp = _otpService.Get(accountId);

            // 取得帳號的password
            var dbHashPassword = _profile.GetPassword(accountId);

            // 比對
            if (inputOtp == currentOtp && hashPassword == dbHashPassword)
            {
                // 成功之後重計
                _failCounter.Reset(accountId);
                return true;
            }

            // 失敗
            // Slack通知User
            _notifier.PushMessage(accountId);

            // 計算失敗次數
            _failCounter.Add(accountId);

            // 取得現在的失敗次數之後紀錄log
            var failCount = _failCounter.Get(accountId);
            _logger.Info($"account={accountId}, errorCount = {failCount}");

            return false;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}