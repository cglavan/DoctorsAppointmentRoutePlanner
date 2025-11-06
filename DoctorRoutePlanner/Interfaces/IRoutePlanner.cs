using DoctorRoutePlanner.Models;

namespace DoctorRoutePlanner.Interfaces
{
    public interface IRoutePlanner
    {
        RoutePlan PlanRoute(List<Appointment> appointments, double homeLat, double homeLng, DateTime startTime);
    }
}
