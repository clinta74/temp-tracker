namespace temp_tracker.Models
{
    public class UserChangePasswordRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string OldPassword { get; set; }
    }
}