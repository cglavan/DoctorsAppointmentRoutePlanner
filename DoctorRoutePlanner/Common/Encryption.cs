using System.Security.Cryptography;
using System.Text;

namespace DoctorRoutePlanner.Common
{
    public class Encryption
    {
        private static readonly IConfiguration _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        private static readonly int _iterations = 10000;
        private static readonly int _keySize = 256;

        private static readonly string _salt = _config["EncryptionSettings:Salt"] ?? string.Empty;
        private static readonly string _vector = _config["EncryptionSettings:Vector"] ?? string.Empty;
        private static readonly string _password = _config["EncryptionSettings:EncKey"] ?? string.Empty;

        public static string ErrorMessage = string.Empty;

        public static string Encrypt(string value) => Encrypt(value, _password);
        public static string Encrypt(string value, string password) => EncryptInternal(value, password);
        public static string Decrypt(string value) => Decrypt(value, _password);
        public static string Decrypt(string value, string password) => DecryptInternal(value, password);

        private static string EncryptInternal(string value, string password)
        {
            byte[] vectorBytes = Encoding.UTF8.GetBytes(_vector);
            byte[] saltBytes = Encoding.UTF8.GetBytes(_salt);
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);

            using var aes = Aes.Create();
            using var keyDerivation = new Rfc2898DeriveBytes(password, saltBytes, _iterations, HashAlgorithmName.SHA256);
            byte[] keyBytes = keyDerivation.GetBytes(_keySize / 8);

            aes.Mode = CipherMode.CBC;
            aes.Key = keyBytes;
            aes.IV = vectorBytes;

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cs.Write(valueBytes, 0, valueBytes.Length);
            cs.FlushFinalBlock();

            return Convert.ToBase64String(ms.ToArray());
        }

        private static string DecryptInternal(string value, string password)
        {
            byte[] vectorBytes = Encoding.UTF8.GetBytes(_vector);
            byte[] saltBytes = Encoding.UTF8.GetBytes(_salt);
            byte[] valueBytes = Convert.FromBase64String(value);

            using var aes = Aes.Create();
            using var keyDerivation = new Rfc2898DeriveBytes(password, saltBytes, _iterations, HashAlgorithmName.SHA256);
            byte[] keyBytes = keyDerivation.GetBytes(_keySize / 8);

            aes.Mode = CipherMode.CBC;
            aes.Key = keyBytes;
            aes.IV = vectorBytes;

            try
            {
                using var decryptor = aes.CreateDecryptor();
                using var ms = new MemoryStream(valueBytes);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var reader = new StreamReader(cs, Encoding.UTF8);
                return reader.ReadToEnd();
            }
            catch (CryptographicException)
            {
                ErrorMessage = "Invalid key for encrypted text";
                return string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return string.Empty;
            }
        }
    }

    public static class Hashing
    {
        public static string GenerateSHA256String(string inputString)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha256.ComputeHash(bytes);
            return ConvertToHex(hash);
        }

        public static string GenerateSHA512String(string inputString)
        {
            using var sha512 = SHA512.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha512.ComputeHash(bytes);
            return ConvertToHex(hash);
        }

        private static string ConvertToHex(byte[] hash)
        {
            StringBuilder result = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
            {
                result.Append(b.ToString("X2"));
            }
            return result.ToString();
        }
    }
}
