namespace Data.Models.Response
{
    using System.Collections.Generic;

    public class UsersTableResponse
    {
        /// <summary>
        /// Lista de usuarios activos en la aplicación.
        /// </summary>
        public List<UserData> UsersList { get; set; }

        /// <summary>
        /// Número total de usuarios dados de alta en la aplicación.
        /// </summary>
        public int UsersCount { get; set; }
    }
}