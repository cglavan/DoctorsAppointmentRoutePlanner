using DoctorRoutePlanner.Interfaces;
using DoctorRoutePlanner.Models;

namespace DoctorRoutePlanner.Services
{
    public class LocalRoutePlanner : IRoutePlanner
    {
        /// <summary>
        /// Plans an effective route based on start time of a list of appointments
        /// </summary>
        /// <param name="appointments">The list of appointments to include in the results</param>
        /// <param name="homeLat">The latitude coordinate of the home office location</param>
        /// <param name="homeLng">The longitude coordinate of the home office location</param>
        /// <param name="startTime">The start time of the work day</param>
        /// <returns></returns>
        public RoutePlan PlanRoute(List<Appointment> appointments, double homeLat, double homeLng, DateTime startTime)
        {
            var route = new RoutePlan();
            var currentTime = startTime;
            var currentLat = homeLat;
            var currentLng = homeLng;

            // Sort appointments by WindowStart
            var ordered = appointments.OrderBy(a => a.WindowStart).ToList();

            foreach (var appt in ordered)
            {
                var travelTime = TimeSpan.FromMinutes(GetDistance(currentLat, currentLng, appt.Latitude, appt.Longitude) * 2);
                var arrival = currentTime + travelTime;

                // Wait if arriving early
                if (arrival < appt.WindowStart)
                    arrival = appt.WindowStart;

                var departure = arrival.Add(appt.Duration);

                // Add the route point information
                route.Points.Add(new RoutePoint
                {
                    Name = appt.PatientName,
                    Latitude = appt.Latitude,
                    Longitude = appt.Longitude,
                    ArrivalTime = arrival,
                    DepartureTime = departure
                });

                currentTime = departure;
                currentLat = appt.Latitude;
                currentLng = appt.Longitude;
            }

            // Add the time to return to home office
            var returnTime = currentTime + TimeSpan.FromMinutes(GetDistance(currentLat, currentLng, homeLat, homeLng) * 2);
            route.TotalDuration = returnTime - startTime;

            // Total distance: sum of legs + return
            double totalDistance = 0;
            double lastLat = homeLat;
            double lastLng = homeLng;

            // Calculate the total distance for the route summary module
            foreach (var point in route.Points)
            {
                totalDistance += GetDistance(lastLat, lastLng, point.Latitude, point.Longitude);
                lastLat = point.Latitude;
                lastLng = point.Longitude;
            }

            // Add the distance to return to home
            totalDistance += GetDistance(lastLat, lastLng, homeLat, homeLng);
            route.TotalDistanceKm = totalDistance;

            return route;
        }

        private double GetDistance(double lat1, double lon1, double lat2, double lon2, string unit = "mi")
        {
            // Determine the radius in miles or kilometers (default is miles)
            double R = unit.ToLower() == "mi" ? 3958.8 : 6371.0; 
            double dLat = ToRad(lat2 - lat1);
            double dLon = ToRad(lon2 - lon1);
            double a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRad(double angle)
        {
            return angle * Math.PI / 180.0;
        }
    }
}
