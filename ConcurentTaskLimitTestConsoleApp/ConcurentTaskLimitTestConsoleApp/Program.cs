using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConcurrentTaskLimitTestConsoleApp
{
     class Program
     {
          static async Task Main(string[] args)
          {
               var tasks = new List<Task>();
               tasks.Add(Task.Run(MakeLoginRequest));
               tasks.Add(Task.Run(MakeLoginRequest));
               tasks.Add(Task.Run(MakeLoginRequest));

               await Task.WhenAll(tasks);
          }

          static Task MakeLoginRequest()
          {
               var dict = new Dictionary<string, string>
               {
                    { "grant_type", "password" },
                    { "username", "bob" },
                    { "password", "password" },
                    { "client_id", "admin" },
                    { "client_secret", "secret" }
               };

               HttpClient client = new();

               var req = new HttpRequestMessage(HttpMethod.Post, "http://authService:6000/connect/token")
                    { Content = new FormUrlEncodedContent(dict) };
               return client.SendAsync(req).ContinueWith(response =>
               {
                    Console.WriteLine(response.Result.StatusCode);
               });
          }
     }
}
