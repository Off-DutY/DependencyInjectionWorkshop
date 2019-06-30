namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string accountId, string password, string inputOtp);
    }

    public abstract class AuthenticationBase : IAuthentication
    {
        private readonly IAuthentication _authentication;

        protected AuthenticationBase(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        public virtual bool Verify(string accountId, string password, string inputOtp)
        {
            return _authentication.Verify(accountId, password, inputOtp);
        }
    }
}