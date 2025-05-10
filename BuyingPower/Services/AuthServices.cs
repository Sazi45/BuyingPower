using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace BuyingPower.Services
{
    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsVerified { get; set; } = false;
        public string VerificationToken { get; set; }
    }

    public class AuthService
    {
        private bool _isAuthenticated = false;
        private User _currentUser;
        private List<User> _users = new();

        public bool IsAuthenticated => _isAuthenticated;
        public string UserEmail => _currentUser?.Email;
        public string FullName => _currentUser != null ? $"{_currentUser.FirstName} {_currentUser.LastName}" : "";

        public bool Register(string firstName, string lastName, string email, string password)
        {
            if (_users.Any(u => u.Email == email))
                return false;

            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password,
                VerificationToken = Guid.NewGuid().ToString()
            };

            _users.Add(user);
            SendVerificationEmail(email); 
            return true;
        }

        public bool Login(string email, string password)
        {
            var user = _users.FirstOrDefault(u => u.Email == email && u.Password == password && u.IsVerified);
            if (user != null)
            {
                _isAuthenticated = true;
                _currentUser = user;
                return true;
            }

            return false;
        }

        public void Logout()
        {
            _isAuthenticated = false;
            _currentUser = null;
        }

        public void SendVerificationEmail(string email)
        {
            var user = _users.FirstOrDefault(u => u.Email == email);
            if (user == null) return;

            var verificationLink = $"https://yourdomain.com/verify?token={user.VerificationToken}";

            var fromAddress = new MailAddress("yourapp@example.com", "BuyingPower App");
            var toAddress = new MailAddress(email);
            const string fromPassword = "your-app-password";
            const string subject = "Verify your email";
            string body = $"Hello {user.FirstName},\n\nPlease verify your email by clicking the link:\n{verificationLink}";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 20000
            };

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };

            smtp.Send(message);
        }

        public bool VerifyEmail(string token)
        {
            var user = _users.FirstOrDefault(u => u.VerificationToken == token);
            if (user == null) return false;

            user.IsVerified = true;
            user.VerificationToken = null;
            return true;
        }
    }
}
