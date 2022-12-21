using DataLayer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ServiceLayer
{
    public class LoginOperations
    {
        private readonly HermesRepository _repository;

        public LoginOperations(HermesRepository repository)
        {
            _repository = repository;
        }

        public User? GetUserById(int id)
        {
            var userList = _repository.GetListOfUsers();
            var foundUser = userList.FirstOrDefault(u => u.UserId == id);
            return foundUser;
        }
        public User? GetUserByUsername(string user)
        {
            var userList = _repository.GetListOfUsers();
            var foundUserByUsername = userList.FirstOrDefault(u => u.Username == user);
            return foundUserByUsername;
        }

        // Check if user with such username & password exists; return user
        public User? GetValidUser(User user)
        {
            var userList = _repository.GetListOfUsers();
            var validUser = userList.FirstOrDefault(
                u => u.Username == user.Username && BCrypt.Net.BCrypt.Verify(user.Password, u.Password));
            return validUser;
        }

        // Add new registered user to db
        public void Register(User user)
        {
            var hashedUser = new User()
            {
                Username = user.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password),
                Email = user.Email
            };
            _repository.Register(hashedUser);
        }

        // Method to update password
        public User UpdateUserPassword(User user, string newPassword)
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            return _repository.UpdateUserPassword(user);
        }

        // Update user password with random password (update password - check by e-mail & username)
        public User? UpdateUserPasswordByEmail(User user, string newPassword)
        {
            var userList = _repository.GetListOfUsers();
            var userFromDb = userList.FirstOrDefault(
                u => u.Username == user.Username && u.Email == user.Email);

            if (userFromDb == null)
            {
                return null;
            }

            return UpdateUserPassword(userFromDb, newPassword);
        }

        // Method that generates random secure password
        public string GenerateNewPassword()
        {
            var randomString = "";
            Random random = new Random();

            var upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            for (int i = 0; i < 2; i++)
            {
                int a = random.Next(26);
                randomString += upperChars.ElementAt(a);
            }

            var lowerChars = "abcdefghijklmnopqrstuvwxy";
            for (int i = 0; i < 3; i++)
            {
                int a = random.Next(26);
                randomString += lowerChars.ElementAt(a);
            }

            var numbers = "0123456789";
            for (int i = 0; i < 3; i++)
            {
                int a = random.Next(10);
                randomString += numbers.ElementAt(a);
            }

            var specialChars = "!@#$%^&*()";
            int b = random.Next(10);
            randomString += specialChars.ElementAt(b);

            //to shuffle string charss and generate random password
            var shuffeledString = new string(randomString.ToCharArray().OrderBy(x => Guid.NewGuid()).ToArray());

            return shuffeledString;
        }

        // Check if inputted password is secure: one uppercase, lowercase letter, special character, digit, not less than 8 characters.
        public bool IsSecurePassword(string password)
        {
            //regex check that password should have at least one lowercase, uppercase letter, digit and special character and it is not less than 8 char
            var regex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";
            var match = Regex.Match(password, regex, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return false;
            }

            return true;
        } 
        
        // Method to sign in user, asign claims
        public async Task CreateAuthentication(User user, HttpContext context)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                };

            var identity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var props = new AuthenticationProperties();

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                props);
        }

        // Method to get user that is authenticated by id stored in claims
        public User? GetUserByClaim(ClaimsPrincipal principal)
        {
            //get user claim(by id)
            var userClaim = principal.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier)
                   .Select(c => c.Value).SingleOrDefault();
            int userId;

            if (!Int32.TryParse(userClaim, out userId))
            {
                return null;
            }

            return GetUserById(userId);
        }
    }
}