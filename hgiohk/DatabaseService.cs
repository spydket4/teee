using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace UserAuthApp
{
    public static class DatabaseService
    {
        private static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.json");

        public static List<User> LoadUsers()
        {
            if (!File.Exists(filePath))
            {
                var defaultUsers = new List<User>
                {
                    new User { Id = 1, Username = "admin", Password = "123", Role = "Администратор", IsBlocked = false, FailedAttempts = 0 }
                };
                SaveUsers(defaultUsers);
                return defaultUsers;
            }

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        public static void SaveUsers(List<User> users)
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public static User Authenticate(string login, string password)
        {
            var users = LoadUsers();
            return users.FirstOrDefault(u => u.Username == login && u.Password == password);
        }

        public static bool UserExists(string login)
        {
            var users = LoadUsers();
            return users.Any(u => u.Username == login);
        }

        public static void AddUser(string login, string password, string role)
        {
            var users = LoadUsers();
            int nextId = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
            
            users.Add(new User 
            { 
                Id = nextId, 
                Username = login, 
                Password = password, 
                Role = role, 
                IsBlocked = false, 
                FailedAttempts = 0 
            });
            SaveUsers(users);
        }

        public static void UnblockUser(int userId)
        {
            var users = LoadUsers();
            var user = users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.IsBlocked = false;
                user.FailedAttempts = 0;
                SaveUsers(users);
            }
        }

        public static void BlockUser(string login)
        {
            var users = LoadUsers();
            var user = users.FirstOrDefault(u => u.Username == login);
            if (user != null)
            {
                user.IsBlocked = true;
                SaveUsers(users);
            }
        }

        public static void UpdateFailedAttempts(string login, int attempts)
        {
            var users = LoadUsers();
            var user = users.FirstOrDefault(u => u.Username == login);
            if (user != null)
            {
                user.FailedAttempts = attempts;
                SaveUsers(users);
            }
        }
    }
}