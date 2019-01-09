using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace IdentityResourceClient
{
    class Program
    {
        private const string Authority = "http://localhost:5000";
        private const string ClientId = "IdentityResourceClient";
        private const string ClientSecret = "secretKey";
        private const string ClientScope = "openid profile";

        private const string UserName = "admin@example.com";
        private const string UserPassword = "AwesomePassword4U!";

        public static void Main(string[] args)
        {
            Console.Title = "Identity Resource Console Client";

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
            var tokenRequest = new PasswordTokenRequest
            {
                Address = discoveryResponse.TokenEndpoint,
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                Scope = ClientScope,
                UserName = UserName,
                Password = UserPassword
            };
            var tokenResponse = await client.RequestPasswordTokenAsync(tokenRequest);
            if (tokenResponse.IsError) throw new ApplicationException(tokenResponse.Error);
            Console.WriteLine($"Identity Response Code: {(int)tokenResponse.HttpStatusCode} {tokenResponse.HttpStatusCode}");
            Console.WriteLine($"Token Response:\n{tokenResponse.Json}\n\n");

            // request userInfo
            var userInfoRequest = new UserInfoRequest
            {
                Address = discoveryResponse.UserInfoEndpoint,
                Token = tokenResponse.AccessToken
            };
            var userInfoResponse = await client.GetUserInfoAsync(userInfoRequest);

            Console.WriteLine($"UserInfo Response Code: {(int)userInfoResponse.HttpStatusCode} {userInfoResponse.HttpStatusCode}");
            if (userInfoResponse.IsError)
            {
                Console.WriteLine($"UserInfo Error Response: {userInfoResponse.Error}");
                throw new Exception(userInfoResponse.Error);
            }

            Console.WriteLine("User Claims:");
            foreach (var claim in userInfoResponse.Claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }
        }
    }
}
