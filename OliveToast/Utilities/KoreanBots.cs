using Newtonsoft.Json;
using OliveToast.Managements.Data;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OliveToast.Utilities
{
    public class KoreanBots
    {
        public static async Task UpdateServerCountAsync(int count)
        {
            if (Program.Client.CurrentUser.Id == 515688863474253824)
            {
                return;
            }

            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), "https://koreanbots.dev/api/v2/bots/495209098929766400/stats");

            request.Headers.TryAddWithoutValidation("content-type", "application/json");
            request.Headers.TryAddWithoutValidation("Authorization", ConfigManager.Get("KOREANBOTS_TOKEN"));

            request.Content = new StringContent("{\"servers\":" + count + "}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            await httpClient.SendAsync(request);
        }

        public static async Task<bool> IsVotedAsync(ulong userId)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://koreanbots.dev/api/v2/bots/495209098929766400/vote?userID={userId}");

            request.Headers.TryAddWithoutValidation("content-type", "application/json");
            request.Headers.TryAddWithoutValidation("Authorization", ConfigManager.Get("KOREANBOTS_TOKEN"));

            var response = await httpClient.SendAsync(request);

            var result = JsonConvert.DeserializeObject<Voted>(await response.Content.ReadAsStringAsync());

            return (DateTimeOffset.Now.ToKST() - Utility.TimestampToDateTime(result.data.lastVote)).Days > 7;
        }
    }
}
