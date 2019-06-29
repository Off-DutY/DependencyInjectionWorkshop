namespace DependencyInjectionWorkshop.Models
{
    public class FailCounterDecorator : IAuthentication
    {
        private readonly IFailCounter _failCounter;
        private readonly IAuthentication _authenticationService;

        public FailCounterDecorator(IAuthentication authenticationService, IFailCounter failCounter)
        {
            _failCounter = failCounter;
            _authenticationService = authenticationService;
        }


        private void CheckLock(string accountId)
        {
            if (_failCounter.IsLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }
        }

        public bool Verify(string accountId, string password, string inputOtp)
        {
            CheckLock(accountId);
            return _authenticationService.Verify(accountId, password, inputOtp);
        }
    }
}