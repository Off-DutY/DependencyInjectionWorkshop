using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var otpService = new OtpService();
            var hash = new Sha256Adapter();
            var profile = new ProfileDao();
            var failCounter = new FailCounter();
            var logger = new NLogAdapter();
            var notifier = new SlackAdapter();

            var authenticationService = new AuthenticationService(otpService, hash, profile);
            var failCounterDecorator = new FailCounterDecorator(authenticationService, failCounter, logger);
            var authentication = new NotifyDecorator(failCounterDecorator, notifier);

            var accountId = "Ted";
            var password = "9527";
            var inputOtp = "3345678";
//            var accountId = args[0]; //"Ted";
//            var password = args[1]; //"9527";
//            var inputOtp = args[2]; //"3345678";

            Console.WriteLine(authentication.Verify(accountId, password, inputOtp));
        }
    }
}