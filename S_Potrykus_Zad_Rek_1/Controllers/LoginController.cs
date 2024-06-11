using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using S_Potrykus_Zad_Rek_1.Modules;
using System.Data.SqlClient;
using System.Data;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace S_Potrykus_Zad_Rek_1.Controllers
{
    public class LoginController : Controller
    {
        // Passing the configuration
        public readonly IConfiguration _configuration;

        public LoginController (IConfiguration configuration)
        {
            _configuration = configuration;
        }


        // Checking if the user exist and matches credentials
        private Contact? AuthenticateUser(Contact contact)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("S_Potrykus_Zad_Rek_1Connection".ToString()));
            SqlDataAdapter adapter = new SqlDataAdapter("SELECT Password FROM Contacts WHERE email = '" + contact.Email + "';", con);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);
            if(dataTable.Rows.Count > 0)
            {
                var check = VerifyPassword(contact.Password, Convert.ToString(dataTable.Rows[0]["password"]));
                if (check)
                {
                    return contact;
                }
            }
            return null;
        }
        // Generating a secure token for login purposes
        private string GenerateToken(Contact contact)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], null, expires: DateTime.Now.AddMinutes(15), signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        [HttpPost]
        [Route("login")]

        public IActionResult Login(Contact contact)
        {
            IActionResult response = Unauthorized();
            var contact_ = AuthenticateUser(contact);
            if (contact_ != null)
            {
                var token = GenerateToken(contact_);
                response = Ok(new { token });
            }
            return response;
        }

        private const int SaltSize = 128 / 8;
        private const int KeySize = 256 / 8;
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName _hashAlgorithName = HashAlgorithmName.SHA256;
        private const char Del = ';';
        // Hashing the password with random generated salt for good security, without knowing the size of the salt this is very hard to break
        public string EncodePassword(string password)
        {

            var salt  = RandomNumberGenerator.GetBytes(SaltSize); // Get random salt
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, _hashAlgorithName, KeySize); // Hash

            return string.Join(Del, Convert.ToBase64String(salt), Convert.ToBase64String(hash)); // Return the hashed passoword with salt attached with delimiter
        }
        // Verify if given password matches the hashed one
        public bool VerifyPassword(string password, string hashedPassword)
        {
            var elements = hashedPassword.Split(Del); // Split the hashed password to get the salt
            var salt = Convert.FromBase64String(elements[0]);
            var hash = Convert.FromBase64String(elements[1]);

            var x = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, _hashAlgorithName, KeySize);

            return CryptographicOperations.FixedTimeEquals(x, hash); // Check if passwords match
        }
    }
}
