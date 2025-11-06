namespace DoctorRoutePlanner.Models
{
    /// <summary>
    /// DoctorNoteEntry class to store doctor's notes and completion status
    /// </summary>
    public class DoctorNoteEntry
    {
        public string Notes { get; set; } = string.Empty;
        public bool? Completed { get; set; }
    }
}
