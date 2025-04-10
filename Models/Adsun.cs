using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ERP.Models
{
    public class Adsun : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Id_Adsun { get; set; }
        public string Plate { get; set; }
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string Lat { get; set; }
        public string Lng { get; set; }
        public string Speed { get; set; }
        public string Km { get; set; }
        public bool Gsm { get; set; }
        public bool Gps { get; set; }
        public bool Key { get; set; }
        public bool Door { get; set; }

        public string Temper { get; set; }
        public string Temper2 { get; set; }
        public string Fuel { get; set; }
        public string DriverName { get; set; }
        public string Liciense { get; set; }
        public string TimeUpdate { get; set; }
        public string Address { get; set; }
        public string InputPower { get; set; }
        public string TripKm { get; set; }
        public bool IsStop { get; set; }
        public string StopTime { get; set; }
        public string StopCounter { get; set; }
        public string Angle { get; set; }
        public bool ACOnOff { get; set; }
        public bool IsOverSpeed { get; set; }
        public string OverSpeedCount { get; set; }
        public string BeginStop { get; set; }
        public string DayDrivingTime { get; set; }
        public string DrivingTime { get; set; }
        public string Over10h { get; set; }
        public string Over4h { get; set; }
        public string VehicleType { get; set; }
        public string SheeatsOrTons { get; set; }

    }
}