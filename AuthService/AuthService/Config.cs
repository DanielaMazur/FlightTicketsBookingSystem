using System.Collections.Generic;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace AuthService
{
     public class Config
     {
          public static IEnumerable<IdentityResource> IdentityResources =>
               new List<IdentityResource>
               {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile(),
               };

          public static IEnumerable<ApiScope> ApiScopes =>
               new List<ApiScope>
               {
                    new("CustomerApi", "Customer API"),
                    new("AdminApi", "Admin API")
               };

          public static IEnumerable<Client> Clients =>
               new List<Client>
               {
                    new()
                    {
                         ClientId = "customer",

                         AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                         // secret for authentication
                         ClientSecrets =
                         {
                              new Secret("secret".Sha256())
                         },

                         // scopes that client has access to
                         AllowedScopes = { "CustomerApi" }
                    },
                    new()
                    {
                         ClientId = "admin",

                         AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                         // secret for authentication
                         ClientSecrets =
                         {
                              new Secret("secret".Sha256())
                         },

                         // scopes that client has access to
                         AllowedScopes = { "AdminApi" }
                    }
               };

          public static List<TestUser> GetUsers()
          {
               return new List<TestUser>
               {
                    new TestUser
                    {
                         SubjectId = "1",
                         Username = "alice",
                         Password = "password"
                    },
                    new TestUser
                    {
                         SubjectId = "2",
                         Username = "bob",
                         Password = "password"
                    }
               };
          }
     }
}
