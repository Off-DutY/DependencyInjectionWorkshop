namespace DependencyInjectionWorkshop.Models
{
    public class NotifyDecorator : AuthenticationBase
    {
        private readonly INotifier _notifier;

        public NotifyDecorator(IAuthentication authentication, INotifier notifier) : base(authentication)
        {
            _notifier = notifier;
        }

        private void PushMessage(string accountId)
        {
            _notifier.PushMessage(accountId);
        }

        public override bool Verify(string accountId, string password, string inputOtp)
        {
            var isValid = base.Verify(accountId, password, inputOtp);
            if (isValid == false)
            {
                PushMessage(accountId);
            }
            return isValid;
        }
    }
}