using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultAccountId = "Ted";
        private const string DefaultOtp = "9527";
        private const string defaultPassword = "9487";
        private const string defaultHashPassword = "3345678";
        private AuthenticationService _authenticationService;
        private ILogger _logger;
        private INotifier _notifier;
        private IProfile _profile;
        private IFailCounter _failCounter;
        private IOtpService _otpService;
        private IHash _hash;


        [SetUp]
        public void SetUp()
        {
            _logger = Substitute.For<ILogger>();
            _notifier = Substitute.For<INotifier>();
            _profile = Substitute.For<IProfile>();
            _failCounter = Substitute.For<IFailCounter>();
            _otpService = Substitute.For<IOtpService>();
            _hash = Substitute.For<IHash>();

            _authenticationService = new AuthenticationService(_notifier, _logger, _failCounter, _otpService, _hash, _profile);
        }

        [Test]
        public void is_valid()
        {
            PresetOtp(DefaultAccountId, DefaultOtp);
            PresetPasswordFromDb(DefaultAccountId, defaultHashPassword);
            PresetHashPassword(defaultPassword, defaultHashPassword);

            var isValid = WhenVerify(DefaultAccountId, defaultPassword, DefaultOtp);

            ShouldBeValid(isValid);
        }

        [Test]
        public void is_invalid_when_otp_is_wrong()
        {
            PresetOtp(DefaultAccountId, DefaultOtp);
            PresetPasswordFromDb(DefaultAccountId, defaultHashPassword);
            PresetHashPassword(defaultPassword, defaultHashPassword);

            var isValid = WhenVerify(DefaultAccountId, defaultPassword, "wrong otp!");

            Assert.IsFalse(isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private bool WhenVerify(string accountId, string password, string otp)
        {
            var isValid = _authenticationService.Verify(accountId, password, otp);
            return isValid;
        }

        private void PresetHashPassword(string password, string hashPassword)
        {
            _hash.Compute(password).ReturnsForAnyArgs(hashPassword);
        }

        private void PresetPasswordFromDb(string accountId, string hashPassword)
        {
            _profile.GetPassword(accountId).ReturnsForAnyArgs(hashPassword);
        }

        private void PresetOtp(string accountId, string otp)
        {
            _otpService.Get(accountId).ReturnsForAnyArgs(otp);
        }
    }
}