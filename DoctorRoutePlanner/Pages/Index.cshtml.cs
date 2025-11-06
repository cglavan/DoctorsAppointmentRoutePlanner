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
    private readonly ILogger<IndexModel> _logger;


    public IndexModel(IRoutePlanner planner, ILogger<IndexModel> logger)
    {
        _planner = planner;
        _logger = logger;

    }

    [BindProperty]
    public string JsonInput { get; set; }

    public List<Appointment> Appointments { get; set; } = new();
    public List<RoutePoint> RoutePoints { get; set; } = new();
    public static Dictionary<string, DoctorNoteEntry> DoctorNotesMemory { get; set; } = new();

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("User") == null)
        {
            _logger.LogWarning("Unauthorized access attempt to Index page. Redirecting to login");
            return RedirectToPage("/Login");
        }

        _logger.LogInformation("Authenticated user has requested Index page");

        return Page();
    }

    public IActionResult OnPost()
    {
        try
        {
            _logger.LogInformation("Processing route planning request");

            Appointments = JsonSerializer.Deserialize<List<Appointment>>(JsonInput);
            var result = _planner.PlanRoute(Appointments, 40.58190, -79.58980, DateTime.Today.AddHours(8));
            RoutePoints = result.Points;

            _logger.LogInformation("Route planning completed successfully");

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error occurred during route planning: {ex.Message}");
            ModelState.AddModelError(string.Empty, "Invalid input or processing error.");
            return Page();
        }
    }

    public IActionResult OnPostSaveNotes([FromForm] string patientId, [FromForm] string doctorNotes, [FromForm] bool? completed)
    {
        try
        {
            _logger.LogInformation($"Saving doctor notes for patient {patientId}");

            if (!DoctorNotesMemory.ContainsKey(patientId))
            {
                DoctorNotesMemory[patientId] = new DoctorNoteEntry();
            }

            DoctorNotesMemory[patientId].Notes = doctorNotes;
            DoctorNotesMemory[patientId].Completed = completed;

            _logger.LogInformation($"Doctor notes for patient {patientId} saved successfully");

            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error saving doctor notes for patient {patientId}: {ex.Message}");
            return new JsonResult(new { success = false, message = "Error saving notes." });
        }
    }

    public IActionResult OnPostExportNotes()
    {
        try
        {
            var exportData = DoctorNotesMemory.Select(kvp => new
            {
                PatientId = kvp.Key,
                DoctorNotes = kvp.Value.Notes,
                Completed = kvp.Value.Completed
            });

            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
            var bytes = Encoding.UTF8.GetBytes(json);

            _logger.LogInformation("Doctor notes exported successfully");

            return File(bytes, "application/json", "DoctorNotesExport.json");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error exporting doctor notes: {ex.Message}");
            return new JsonResult(new { success = false, message = "Error exporting notes." });
        }
    }
}