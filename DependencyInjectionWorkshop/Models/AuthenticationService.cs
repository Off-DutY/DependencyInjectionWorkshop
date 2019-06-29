using System;

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string accountId, string password, string inputOtp);
    }

    public class AuthenticationService : IAuthentication
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
            // 取得密碼hash
            var hashPassword = _hash.Compute(password);

            // 取得帳號當下的Otp
            var currentOtp = _otpService.Get(accountId);

            // 取得帳號的password
            var dbHashPassword = _profile.GetPassword(accountId);

            // 比對
            return inputOtp == currentOtp && hashPassword == dbHashPassword;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}