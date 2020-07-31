using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorDataTable.Shared
{
    public class WeatherForecast
    {
        [Display(Name = "Date", Order = 1)]
        public DateTime Date { get; set; }
        [Display(Name = "Temp. (C)", Order = 2)]
        public int TemperatureC { get; set; }
        [Display(Name = "Summary", Order = 4)]
        public string Summary { get; set; }
        [Display(Name = "Temp. (F)", Order = 3)]
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
