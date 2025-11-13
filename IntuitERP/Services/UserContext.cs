using IntuitERP.models;

namespace IntuitERP.Services
{
    /// <summary>
    /// Manages the current logged-in user session and provides access to user information
    /// throughout the application lifecycle
    /// </summary>
    public class UserContext
    {
        private static UserContext? _instance;
        private static readonly object _lock = new object();

        private UsuarioModel? _currentUser;

        /// <summary>
        /// Gets the singleton instance of UserContext
        /// </summary>
        public static UserContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new UserContext();
                        }
                    }
                }
                return _instance;
            }
        }

        private UserContext()
        {
            // Private constructor for singleton pattern
        }

        /// <summary>
        /// Gets the currently logged-in user
        /// </summary>
        public UsuarioModel? CurrentUser
        {
            get => _currentUser;
            private set => _currentUser = value;
        }

        /// <summary>
        /// Checks if a user is currently logged in
        /// </summary>
        public bool IsAuthenticated => _currentUser != null;

        /// <summary>
        /// Sets the current user after successful login
        /// </summary>
        /// <param name="user">The authenticated user</param>
        public void SetCurrentUser(UsuarioModel user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _currentUser = user;
        }

        /// <summary>
        /// Clears the current user (logout)
        /// </summary>
        public void ClearCurrentUser()
        {
            _currentUser = null;
        }

        /// <summary>
        /// Gets the current user's ID
        /// </summary>
        /// <returns>User ID or null if not authenticated</returns>
        public int? GetCurrentUserId()
        {
            return _currentUser?.CodUsuarios;
        }

        /// <summary>
        /// Gets the current user's username
        /// </summary>
        /// <returns>Username or null if not authenticated</returns>
        public string? GetCurrentUsername()
        {
            return _currentUser?.Usuario;
        }

        /// <summary>
        /// Checks if the current user has a specific permission
        /// </summary>
        /// <param name="permissionProperty">Permission property name (e.g., "PermissaoProdutosCreate")</param>
        /// <returns>True if user has the permission</returns>
        public bool HasPermission(string permissionProperty)
        {
            if (!IsAuthenticated || _currentUser == null)
            {
                return false;
            }

            // Use reflection to get the permission value
            var property = typeof(UsuarioModel).GetProperty(permissionProperty);
            if (property == null)
            {
                return false;
            }

            var value = property.GetValue(_currentUser);
            if (value is int intValue)
            {
                return intValue > 0;
            }

            return false;
        }

        /// <summary>
        /// Ensures a user is authenticated, throws exception if not
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated</exception>
        public void RequireAuthentication()
        {
            if (!IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User must be logged in to perform this action");
            }
        }

        /// <summary>
        /// Ensures a user has a specific permission, throws exception if not
        /// </summary>
        /// <param name="permissionProperty">Permission property name</param>
        /// <exception cref="UnauthorizedAccessException">Thrown when user lacks permission</exception>
        public void RequirePermission(string permissionProperty)
        {
            RequireAuthentication();

            if (!HasPermission(permissionProperty))
            {
                throw new UnauthorizedAccessException($"User does not have permission: {permissionProperty}");
            }
        }
    }
}
