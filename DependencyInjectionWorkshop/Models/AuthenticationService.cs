using System;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IOtpService _otpService;
        private readonly IHash _hash;
        private readonly IProfile _profile;

        public AuthenticationService(IOtpService otpService, IHash hash, IProfile profile)
        {
            _otpService = otpService;
            _hash = hash;
            _profile = profile;
        }

        public AuthenticationService()
        {
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