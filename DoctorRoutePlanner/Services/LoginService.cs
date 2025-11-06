using Microsoft.Extensions.Options;
using DoctorRoutePlanner.Models;

namespace DoctorRoutePlanner.Services
{
    public interface ILoginService
    {
        bool Validate(string username, string password);
    }

    public class LoginService : ILoginService
    {
        private readonly LoginCredentials _credentials;
        private readonly ILogger<LoginService> _logger;

        public LoginService(IOptions<LoginCredentials> credentials, ILogger<LoginService> logger)
        {
            _credentials = credentials.Value;
            _logger = logger;
        }

        public bool Validate(string username, string password)
        {
            try
            {
                _logger.LogInformation("Validating user credentials");
                
                if(username == _credentials.Username && password == _credentials.Password)
                {
                    _logger.LogInformation("User credentials validated successfully");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Invalid user credentials provided (Username: {username} Password: {password}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during credential validation: {ex.Message}");
                return false;
            }
        }
    }
}