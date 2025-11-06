namespace DoctorRoutePlanner.Models
{
    /// <summary>
    /// RoutePoint class to store route point data and arrival/departure time
    /// </summary>
    public class RoutePoint
    {
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime ArrivalTime { get; set; }
        public DateTime DepartureTime { get; set; }
    }
}
