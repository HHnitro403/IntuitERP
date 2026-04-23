using Microsoft.Data.Sqlite;
using Dapper;
using System;
using System.IO;
using System.Linq;

namespace DBConfigurator.Avalonia.UI.Services
{
    public class AdminCredentialsService
    {
        private readonly string _dbPath;

        private string ConnectionString => $"Data Source={_dbPath}";

        public AdminCredentialsService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string folderPath = Path.Combine(appDataPath, "IntuitERP", "Config");
            
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            
            _dbPath = Path.Combine(folderPath, "AdminCreds.db");
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                db.Open();
                db.Execute(@"
                    CREATE TABLE IF NOT EXISTS admin_credentials (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT UNIQUE,
                        PasswordHash TEXT
                    )");

                var admin = db.QueryFirstOrDefault<AdminCredential>("SELECT * FROM admin_credentials LIMIT 1");
                if (admin == null)
                {
                    var defaultAdmin = new AdminCredential
                    {
                        Username = "BbAdmin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("masterkey", 12)
                    };
                    db.Execute("INSERT INTO admin_credentials (Username, PasswordHash) VALUES (@Username, @PasswordHash)", defaultAdmin);
                }
            }
        }

        public bool ValidateCredentials(string username, string password)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                var admin = db.QueryFirstOrDefault<AdminCredential>(
                    "SELECT * FROM admin_credentials WHERE Username = @Username", new { Username = username });

                if (admin == null || admin.PasswordHash == null)
                {
                    return false;
                }

                return BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash);
            }
        }

        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            using (var db = new SqliteConnection(ConnectionString))
            {
                var admin = db.QueryFirstOrDefault<AdminCredential>(
                    "SELECT * FROM admin_credentials WHERE Username = @Username", new { Username = username });

                if (admin == null || admin.PasswordHash == null)
                {
                    return false;
                }

                if (!BCrypt.Net.BCrypt.Verify(oldPassword, admin.PasswordHash))
                {
                    return false;
                }

                string newHash = BCrypt.Net.BCrypt.HashPassword(newPassword, 12);
                db.Execute("UPDATE admin_credentials SET PasswordHash = @PasswordHash WHERE Username = @Username", 
                    new { PasswordHash = newHash, Username = username });
                return true;
            }
        }
    }

    public class AdminCredential
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
    }
}
