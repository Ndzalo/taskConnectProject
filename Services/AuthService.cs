using Firebase.Auth;
using System;
using System.Threading.Tasks;

namespace taskConnectProject.Services
{
    internal class AuthService
    {
        private readonly FirebaseAuthProvider _authProvider;

        public AuthService()
        {
            _authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyCDUjGoGt8py3ok6DxPLlf1YtbrotXwZPU"));
        }

        // Method to sign in a user
        public async Task<FirebaseAuthLink> SignInAsync(string email, string password)
        {
            try
            {
                var authLink = await _authProvider.SignInWithEmailAndPasswordAsync(email, password);
                return authLink;  // Returns the authentication link
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return null;
            }
        }

        // Method to register a new user
        public async Task<string> RegisterAsync(string email, string password)
        {
            try
            {
                await _authProvider.CreateUserWithEmailAndPasswordAsync(email, password);
                return "User registered successfully!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                return null;
            }
        }

        // Method to get the email of the currently logged-in user
        public async Task<string> GetCurrentUserEmailAsync(string firebaseToken)
        {
            try
            {
                var user = await _authProvider.GetUserAsync(firebaseToken);
                return user.Email;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user email: {ex.Message}");
                return null;
            }
        }

        // Method to reset password
        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                await _authProvider.SendPasswordResetEmailAsync(email);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Password reset error: {ex.Message}");
                return false;
            }
        }
    }
}
