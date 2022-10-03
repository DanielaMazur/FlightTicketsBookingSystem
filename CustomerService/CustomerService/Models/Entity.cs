using Newtonsoft.Json;

namespace CustomerService.Models
{
     public class Entity
     {
          [JsonProperty(PropertyName = "id")]
          public string Id { get; set; }
     }
}
