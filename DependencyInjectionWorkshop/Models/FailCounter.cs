using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailCounter
    {
        void Reset(string accountId);
        void Add(string accountId);
        int Get(string accountId);
        bool IsLocked(string accountId);
    }

    public class FailCounter : IFailCounter
    {
        public void Reset(string accountId)
        {
            var resetResponse = new HttpClient()
            {
                BaseAddress = new Uri("http://joey.com/")
            }.PostAsJsonAsync("api/FailCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public void Add(string accountId)
        {
            var addResponse = new HttpClient()
            {
                BaseAddress = new Uri("http://joey.com/")
            }.PostAsJsonAsync("api/FailCounter/Add", accountId).Result;
            addResponse.EnsureSuccessStatusCode();
        }

        public int Get(string accountId)
        {
            var failedCountResponse = new HttpClient()
            {
                BaseAddress = new Uri("http://joey.com/")
            }.PostAsJsonAsync("api/FailCounter/Get", accountId).Result;
            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        public bool IsLocked(string accountId)
        {
            var isAccountLockedResponse = new HttpClient()
            {
                BaseAddress = new Uri("http://joey.com/")
            }.PostAsJsonAsync("api/FailCounter/IsLock", accountId).Result;
            isAccountLockedResponse.EnsureSuccessStatusCode();
            // 檢查帳號是否被lock了
            var isLock = isAccountLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLock;
        }
    }
}