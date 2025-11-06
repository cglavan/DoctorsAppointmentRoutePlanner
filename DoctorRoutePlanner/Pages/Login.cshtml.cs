using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DoctorRoutePlanner.Services;
using DoctorRoutePlanner.Common;

public class LoginModel : PageModel
{
    private readonly ILoginService _loginService;

    public LoginModel(ILoginService loginService)
    {
        _loginService = loginService;
    }

    [BindProperty]
    public string Username { get; set; }

    [BindProperty]
    public string Password { get; set; }

    public string ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("User") != null)
            return RedirectToPage("/Index");

        return Page();
    }

    public IActionResult OnPost()
    {
        if (_loginService.Validate(Encryption.Encrypt(Username), Encryption.Encrypt(Password)))
        {
            HttpContext.Session.SetString("User", Username);
            return RedirectToPage("/Index");
        }

        ErrorMessage = "Invalid username or password.";
        return Page();
    }
}