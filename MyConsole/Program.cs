using System;
using System.Security.Policy;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var otpService = new FakeOtp();
            var hash = new FakeHash();
            var profile = new FakeProfile();
            var failCounter = new FakeFailedCounter();
            var logger = new ConsoleLogger();
            var notifier = new FakeSlack();

            var authenticationService = new AuthenticationService(otpService, hash, profile);
            var failCounterDecorator = new FailCounterDecorator(authenticationService, failCounter, logger);
            var authentication = new NotifyDecorator(failCounterDecorator, notifier);

            var accountId = "Ted";
            var password = "123456";
            var inputOtp = "3334567";
//            var accountId = args[0]; //"Ted";
//            var password = args[1]; //"9527";
//            var inputOtp = args[2]; //"3334567";

            Console.WriteLine(authentication.Verify(accountId, password, inputOtp));
        }
    }

    internal class ConsoleLogger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine($"{nameof(ConsoleLogger)}{nameof(Info)}{message}");
        }
    }

    internal class FakeSlack : INotifier
    {
        public void PushMessage(string message)
        {
            Console.WriteLine($"{nameof(FakeSlack)}.{nameof(PushMessage)}({message})");
        }
    }

    internal class FakeFailedCounter : IFailCounter
    {
        public void Reset(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Reset)}({accountId})");
        }

        public void Add(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Add)}({accountId})");
        }

        public int Get(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Get)}({accountId})");
            return 91;
        }

        public bool IsLocked(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(IsLocked)}({accountId})");
            return false;
        }
    }

    internal class FakeOtp : IOtpService
    {
        public string Get(string accountId)
        {
            Console.WriteLine($"{nameof(FakeOtp)}.{nameof(Get)}({accountId})");
            return "3334567";
        }
    }

    internal class FakeHash : IHash
    {
        public string Compute(string plainText)
        {
            Console.WriteLine($"{nameof(FakeHash)}.{nameof(Hash)}({plainText})");
            return "123456";
        }
    }

    internal class FakeProfile : IProfile
    {
        public string GetPassword(string accountId)
        {
            Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPassword)}({accountId})");
            return "123456";
        }
    }
}