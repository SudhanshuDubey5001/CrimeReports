﻿using System;
namespace CrimeReports.Models
{
    public class Report
    {
        public string? category { get; set; } = "Unspecified";
        public string? location_type { get; set; }
        public Location? location { get; set; }
        public string? context { get; set; }
        public object? outcome_status { get; set; }
        public string? persistent_id { get; set; }
        public int? id { get; set; }
        public string? location_subtype { get; set; }
        public string? month { get; set; }
    }

    public class Location
    {
        public string? latitude { get; set; }
        public Street? street { get; set; }
        public string? longitude { get; set; }
    }


    public class Street
    {
        public int id { get; set; }
        public string? name { get; set; } = "Unspecified";
    }
}

