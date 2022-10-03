using System;

namespace AdminService.Models
{
     public class Ticket: Entity
     {
          public string FromAirportId { get; set; }

          public string ToAirportId { get; set; }

          public DateTime DepartureTime { get; set; }

          public TimeSpan FlightDuration { get; set; }

          public decimal Price { get; set; }
     }
}
