using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class SlackAdapter
    {
        public void PushMessage(string accountId)
        {
            var slackClient = new SlackClient("my Api token");
            slackClient.PostMessage(r => { }, "mychannel", $"message {accountId}");
        }
    }
}