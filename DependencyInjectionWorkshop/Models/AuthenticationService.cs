using System;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ILogger _logger;
        private readonly SlackAdapter _slackAdapter;
        private readonly FailCounter _failCounter;
        private readonly OtpService _otpService;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly ProfileDao _profileDao;

        public AuthenticationService(SlackAdapter slackAdapter, ILogger logger, FailCounter failCounter, OtpService otpService, Sha256Adapter sha256Adapter, ProfileDao profileDao, ProfileDao test)
        {
            _slackAdapter = slackAdapter;
            _logger = logger;
            _failCounter = failCounter;
            _otpService = otpService;
            _sha256Adapter = sha256Adapter;
            _profileDao = profileDao;
        }

        public AuthenticationService()
        {
            _slackAdapter = new SlackAdapter();
            _logger = new NLogAdapter();
            _failCounter = new FailCounter();
            _otpService = new OtpService();
            _sha256Adapter = new Sha256Adapter();
            _profileDao = new ProfileDao();
        }


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