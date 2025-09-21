using System;

namespace CS464AE
{
    public class LoginSuccessfulEventArgs : EventArgs
    {
        public int UserId { get; }
        public string Username { get; }

        public LoginSuccessfulEventArgs(int userId, string username)
        {
            UserId = userId;
            Username = username;
        }
    }
}
