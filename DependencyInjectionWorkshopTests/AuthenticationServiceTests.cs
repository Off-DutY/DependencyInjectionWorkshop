using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var logger = Substitute.For<ILogger>();
            var notifier = Substitute.For<INotifier>();
            var profile = Substitute.For<IProfile>();
            var failCounter = Substitute.For<IFailCounter>();
            var otpService = Substitute.For<IOtpService>();
            var hash = Substitute.For<IHash>();

            var accountId = "Ted";

            otpService.Get(accountId).ReturnsForAnyArgs("9527");
            profile.GetPassword(accountId).ReturnsForAnyArgs("3345678");
            hash.Hash("9487").ReturnsForAnyArgs("3345678");

            var authenticationService = new AuthenticationService(notifier, logger, failCounter, otpService, hash, profile);
            var isValid = authenticationService.Verify(accountId, "9487", "9527");

            Assert.IsTrue(isValid);
        }
    }
}