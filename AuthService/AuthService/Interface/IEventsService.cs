using System.Collections.Generic;

namespace AuthService.Interface
{
     public interface IEventsService
     {
          public Dictionary<string, int> Events { get; set; }
     }
}
