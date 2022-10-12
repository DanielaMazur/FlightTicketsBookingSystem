using System.Collections.Generic;
using System.Threading.Tasks;
using AuthService.Interface;
using IdentityServer4.Events;
using IdentityServer4.Services;

namespace AuthService.Services
{
     public class EventsService: IEventSink, IEventsService
     {
          public Dictionary<string, int> Events { get; set; } =  new();

          public EventsService()
          {
               Events.Add("Status", 1);
          }
          
          public Task PersistAsync(Event evt)
          {
               if (Events.ContainsKey(evt.Name))
               {
                    Events[evt.Name] += 1;
               }
               else
               {
                    Events.Add(evt.Name, 1);
               }

               return Task.CompletedTask;
          }
     }
}
