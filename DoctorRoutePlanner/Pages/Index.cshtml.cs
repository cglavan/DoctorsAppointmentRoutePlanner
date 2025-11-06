using DoctorRoutePlanner.Interfaces;
using DoctorRoutePlanner.Models;
using DoctorRoutePlanner.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

public class IndexModel : PageModel
{
    private readonly IRoutePlanner _planner;

    public IndexModel(IRoutePlanner planner)
    {
        _planner = planner;
    }

    [BindProperty]
    public string JsonInput { get; set; }

    public List<Appointment> Appointments { get; set; } = new();
    public List<RoutePoint> RoutePoints { get; set; } = new();
    public static Dictionary<string, DoctorNoteEntry> DoctorNotesMemory { get; set; } = new();

    public IActionResult OnPost()
    {
        try
        {
            Appointments = JsonSerializer.Deserialize<List<Appointment>>(JsonInput);
            var result = _planner.PlanRoute(Appointments, 40.58190, -79.58980, DateTime.Today.AddHours(8));
            RoutePoints = result.Points;
            return Page();
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Invalid input or processing error.");
            return Page();
        }
    }

    public IActionResult OnPostSaveNotes([FromForm] string patientId, [FromForm] string doctorNotes, [FromForm] bool? completed)
    {
        if (!DoctorNotesMemory.ContainsKey(patientId))
        {
            DoctorNotesMemory[patientId] = new DoctorNoteEntry();
        }

        DoctorNotesMemory[patientId].Notes = doctorNotes;
        DoctorNotesMemory[patientId].Completed = completed;

        return new JsonResult(new { success = true });
    }

    public IActionResult OnPostExportNotes()
    {
        var exportData = DoctorNotesMemory.Select(kvp => new
        {
            PatientId = kvp.Key,
            DoctorNotes = kvp.Value.Notes,
            Completed = kvp.Value.Completed
        });

        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
        var bytes = Encoding.UTF8.GetBytes(json);

        return File(bytes, "application/json", "DoctorNotesExport.json");
    }
}