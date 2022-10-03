using Newtonsoft.Json;

namespace AdminService.Models
{
     public class Entity
     {
          [JsonProperty(PropertyName = "id")]
          public string Id { get; set; }
     }
}
