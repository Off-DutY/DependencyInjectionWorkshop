using System;
using System.Security.Policy;
using Autofac;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    class Program
    {
        private static IContainer _containers;


        static void Main(string[] args)
        {
            RegistContains();

//            var otpService = new FakeOtp();
//            var hash = new FakeHash();
//            var profile = new FakeProfile();
//            var failCounter = new FakeFailedCounter();
//            var logger = new ConsoleLogger();
//            var notifier = new FakeSlack();
//
//            var authenticationService = new AuthenticationService(otpService, hash, profile);
//            var failCounterDecorator = new FailCounterDecorator(authenticationService, failCounter, logger);
//            var authentication = new NotifyDecorator(failCounterDecorator, notifier);

            var authentication = _containers.Resolve<IAuthentication>();

            var accountId = "Ted";
            var password = "123456";
//            var inputOtp = "3334567";
            var inputOtp = "wrongOtp";

//            var accountId = args[0]; //"Ted";
//            var password = args[1]; //"9527";
//            var inputOtp = args[2]; //"3334567";

            Console.WriteLine(authentication.Verify(accountId, password, inputOtp));
        }

        private static void RegistContains()
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<FakeHash>().As<IHash>();
            containerBuilder.RegisterType<FakeOtp>().As<IOtpService>();
            containerBuilder.RegisterType<FakeProfile>().As<IProfile>();
            containerBuilder.RegisterType<FakeSlack>().As<INotifier>();
            containerBuilder.RegisterType<FakeFailedCounter>().As<IFailCounter>();
            containerBuilder.RegisterType<ConsoleLogger>().As<ILogger>();

            containerBuilder.RegisterType<AuthenticationService>().As<IAuthentication>();
            containerBuilder.RegisterDecorator<NotifyDecorator, IAuthentication>();
            containerBuilder.RegisterDecorator<FailCounterDecorator, IAuthentication>();

            _containers = containerBuilder.Build();
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