namespace DependencyInjectionWorkshop.Models
{
    public class FailCounterDecorator : IAuthentication
    {
        private readonly IFailCounter _failCounter;
        private readonly IAuthentication _authenticationService;
        private ILogger _logger;

        public FailCounterDecorator(IAuthentication authenticationService, IFailCounter failCounter, ILogger logger)
        {
            _failCounter = failCounter;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        public bool Verify(string accountId, string password, string inputOtp)
        {
            CheckLock(accountId);
            var isValid = _authenticationService.Verify(accountId, password, inputOtp);
            if (isValid)
                Reset(accountId);
            else
            {
                Add(accountId);
                Log(accountId);
            }
            return isValid;
        }

        private void CheckLock(string accountId)
        {
            if (_failCounter.IsLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }
        }

        private void Reset(string accountId)
        {
            _failCounter.Reset(accountId);
        }


        private void Add(string accountId)
        {
            _failCounter.Add(accountId);
        }

        private void Log(string accountId)
        {
            var failCount = _failCounter.Get(accountId);
            _logger.Info($"account={accountId}, errorCount = {failCount}");
        }
    }
}