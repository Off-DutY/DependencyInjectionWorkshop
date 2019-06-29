using System;
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
        private const string DefaultPassword = "9487";
        private const string DefaultHashPassword = "3345678";
        private const int DefaultFailCount = 1450;
        private IAuthentication _authentication;
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

            _authentication = new AuthenticationService(_otpService, _hash, _profile);
            _authentication = new FailCounterDecorator(_authentication, _failCounter, _logger);
            _authentication = new NotifyDecorator(_authentication, _notifier);
        }

        [Test]
        public void is_valid()
        {
            PresetOtp(DefaultAccountId, DefaultOtp);
            PresetPasswordFromDb(DefaultAccountId, DefaultHashPassword);
            PresetHashPassword(DefaultPassword, DefaultHashPassword);

            var isValid = WhenVerify(DefaultAccountId, DefaultPassword, DefaultOtp);

            ShouldBeValid(isValid);
        }

        [Test]
        public void is_invalid_when_otp_is_wrong()
        {
            PresetOtp(DefaultAccountId, DefaultOtp);
            PresetPasswordFromDb(DefaultAccountId, DefaultHashPassword);
            PresetHashPassword(DefaultPassword, DefaultHashPassword);

            var isValid = WhenVerify(DefaultAccountId, DefaultPassword, "wrong otp!");

            ShouldBeInvalid(isValid);
        }

        [Test]
        public void should_Notify_when_invalid()
        {
            WhenInvalid();
            ShouldNotify(DefaultAccountId);
        }

        [Test]
        public void should_add_failCount_when_invalid()
        {
            WhenInvalid();
            ShouldAddFailCount(DefaultAccountId);
        }

        [Test]
        public void should_log_ErrorCount_when_invalid()
        {
            PresetFailCount(DefaultAccountId, DefaultFailCount);
            WhenInvalid();
            ShouldLog(DefaultAccountId, DefaultFailCount.ToString());
        }

        [Test]
        public void account_is_locked()
        {
            _failCounter.IsLocked(DefaultAccountId).ReturnsForAnyArgs(true);
            void Action() => WhenValid();
            ShouldThrow<FailedTooManyTimesException>(Action);
        }


        [Test]
        public void should_Reset_when_valid()
        {
            WhenValid();
            ShouldReset(DefaultAccountId);
        }

        private static void ShouldThrow<TException>(TestDelegate action) where TException : Exception
        {
            Assert.Throws<TException>(action);
        }


        private void ShouldLog(string accountId, string failCount)
        {
            _logger.Received().Info(Arg.Is<string>(r => r.Contains(accountId) && r.Contains(failCount)));
        }

        private void PresetFailCount(string accountId, int failCount)
        {
            _failCounter.Get(accountId).ReturnsForAnyArgs(failCount);
        }


        private void ShouldAddFailCount(string accountId)
        {
            _failCounter.Received().Add(accountId);
        }

        private void ShouldReset(string accountId)
        {
            _failCounter.Received().Reset(accountId);
        }

        private bool WhenValid()
        {
            PresetOtp(DefaultAccountId, DefaultOtp);
            PresetPasswordFromDb(DefaultAccountId, DefaultHashPassword);
            PresetHashPassword(DefaultPassword, DefaultHashPassword);

            var isValid = WhenVerify(DefaultAccountId, DefaultPassword, DefaultOtp);
            return isValid;
        }

        private void ShouldNotify(string accountId)
        {
            _notifier.Received().PushMessage(Arg.Is<string>(r => r.Contains(accountId)));
        }

        private bool WhenInvalid()
        {
            PresetOtp(DefaultAccountId, DefaultOtp);
            PresetPasswordFromDb(DefaultAccountId, DefaultHashPassword);
            PresetHashPassword(DefaultPassword, DefaultHashPassword);

            var isValid = WhenVerify(DefaultAccountId, DefaultPassword, "wrong otp!");
            return isValid;
        }

        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private bool WhenVerify(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);
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