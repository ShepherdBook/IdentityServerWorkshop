using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace ConsoleClient
{
    class Program
    {
        private const string Authority = "http://localhost:5000";
        private const string ClientId = "ConsoleClient";
        private const string ClientSecret = "secretKey";
        private const string ClientScope = "apiResource";
        private const string ApiResource = "http://localhost:5010";


        public static void Main(string[] args)
        {
            Console.Title = "Identity Server Console Client";

            MainAsync().GetAwaiter().GetResult();

            Console.Write("\nPress any key to continue... ");
            Console.ReadKey();
        }

        private static async Task MainAsync()
        {
            var client = new HttpClient();
            var discoveryResponse = await client.GetDiscoveryDocumentAsync(Authority);
            if (discoveryResponse.IsError) throw new ApplicationException(discoveryResponse.Error);
            Console.WriteLine("Successful endpoint discovery");

            // request token
            var tokenRequest = new ClientCredentialsTokenRequest
            {
                Address = discoveryResponse.TokenEndpoint,
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                Scope = ClientScope
            };
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(tokenRequest);
            if (tokenResponse.IsError) throw new ApplicationException(tokenResponse.Error);
            Console.WriteLine($"Identity Response Code: {(int)tokenResponse.HttpStatusCode} {tokenResponse.HttpStatusCode}");
            Console.WriteLine($"Token Response:\n{tokenResponse.Json}\n\n");

            // call api
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(tokenResponse.AccessToken);

            var apiResponse = await apiClient.GetAsync($"{ApiResource}/api/values");
            Console.WriteLine($"API Response Code: {(int) apiResponse.StatusCode} {apiResponse.StatusCode}");
            if (!apiResponse.IsSuccessStatusCode) return;
            var identityResponseContent = await apiResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"API Response:\n{JArray.Parse(identityResponseContent)}");
        }
    }
}
