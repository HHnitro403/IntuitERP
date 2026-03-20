using SQLite;

namespace DBconfigurator.Services
{
    /// <summary>
    /// Secure admin credentials management for DB Configurator
    /// </summary>
    public class AdminCredentialsService
    {
        private readonly string _dbPath;

        public AdminCredentialsService()
        {
            _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AdminCreds.db");
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                db.CreateTable<AdminCredential>();

                // Check if admin user exists
                var admin = db.Table<AdminCredential>().FirstOrDefault();
                if (admin == null)
                {
                    // Create default admin with hashed password
                    // IMPORTANT: User must change this on first run in production!
                    var defaultAdmin = new AdminCredential
                    {
                        Username = "admin",
                        // Hash of "ChangeMe123!" - MUST BE CHANGED ON FIRST RUN
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMe123!", 12)
                    };
                    db.Insert(defaultAdmin);
                }
            }
        }

        public bool ValidateCredentials(string username, string password)
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                var admin = db.Table<AdminCredential>()
                    .FirstOrDefault(a => a.Username == username);

                if (admin == null)
                {
                    return false;
                }

                return BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash);
            }
        }

        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                var admin = db.Table<AdminCredential>()
                    .FirstOrDefault(a => a.Username == username);

                if (admin == null)
                {
                    return false;
                }

                // Verify old password
                if (!BCrypt.Net.BCrypt.Verify(oldPassword, admin.PasswordHash))
                {
                    return false;
                }

                // Update with new hashed password
                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, 12);
                db.Update(admin);
                return true;
            }
        }

        public bool IsDefaultPassword(string username)
        {
            using (var db = new SQLiteConnection(_dbPath))
            {
                var admin = db.Table<AdminCredential>()
                    .FirstOrDefault(a => a.Username == username);

                if (admin == null)
                {
                    return false;
                }

                // Check if still using default password
                return BCrypt.Net.BCrypt.Verify("ChangeMe123!", admin.PasswordHash);
            }
        }
    }

    [Table("admin_credentials")]
    public class AdminCredential
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed, Unique]
        public string Username { get; set; }

        public string PasswordHash { get; set; }
    }
}
