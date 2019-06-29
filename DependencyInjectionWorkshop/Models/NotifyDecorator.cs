namespace DependencyInjectionWorkshop.Models
{
    public class NotifyDecorator : IAuthentication
    {
        private readonly IAuthentication _authenticationService;
        private readonly INotifier _notifier;

        public NotifyDecorator(IAuthentication authenticationService, INotifier notifier)
        {
            _authenticationService = authenticationService;
            _notifier = notifier;
        }

        private void PushMessage(string accountId)
        {
            _notifier.PushMessage(accountId);
        }

        public bool Verify(string accountId, string password, string inputOtp)
        {
            var isValid = _authenticationService.Verify(accountId, password, inputOtp);
            if (isValid == false)
            {
                PushMessage(accountId);
            }
            return isValid;
        }
    }
}