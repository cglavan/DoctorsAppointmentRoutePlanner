namespace DoctorRoutePlanner.Models
{
    /// <summary>
    /// Appointment class to store appointment details
    /// </summary>
    public class Appointment
    {
        public string PatientId { get; set; }
        public string PatientName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime WindowStart { get; set; }
        public DateTime WindowEnd { get; set; }
        public TimeSpan Duration { get; set; }
        public string Notes { get; set; }
        public string? DoctorNotes { get; set; }
    }
}
