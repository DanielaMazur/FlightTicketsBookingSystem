namespace AdminService.Models
{
     public class Airport: Entity
     {
          public string Name { get; set; }

          public string TimeZoneId { get; set; } // Use TimeZoneInfo class to get the TimeZone offset
     }
}
