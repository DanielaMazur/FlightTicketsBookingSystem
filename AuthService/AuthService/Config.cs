using System.Collections.Generic;
using IdentityServer4.Models;

namespace AuthService
{
     public class Config
     {
          public static IEnumerable<ApiScope> ApiScopes =>
               new List<ApiScope>
               {
                    new ApiScope("CustomerApi", "Customer API"),
                    new ApiScope("AdminApi", "Admin API")
               };

          public static IEnumerable<Client> Clients =>
               new List<Client>
               {
               };
     }
}
