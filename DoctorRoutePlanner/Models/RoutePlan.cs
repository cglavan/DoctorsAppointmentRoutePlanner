namespace DoctorRoutePlanner.Models
{
    /// <summary>
    /// RoutePlan class to store route points and total distance and time data
    /// </summary>
    public class RoutePlan
    {
        public List<RoutePoint> Points { get; set; } = new();
        public double TotalDistanceKm { get; set; }
        public TimeSpan TotalDuration { get; set; }
    }
}
