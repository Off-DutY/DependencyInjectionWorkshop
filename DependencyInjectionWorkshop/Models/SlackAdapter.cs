using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface INotifier
    {
        void PushMessage(string accountId);
    }

    public class Notifier : INotifier
    {
        public void PushMessage(string accountId)
        {
            var slackClient = new SlackClient("my Api token");
            slackClient.PostMessage(r => { }, "mychannel", $"message {accountId}");
        }
    }
}