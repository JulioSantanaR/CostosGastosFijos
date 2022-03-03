namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;
    using Data.Models;

    /// <summary>
    /// Clase utilizada para leer información asociada a los roles de usuario.
    /// </summary>
    public class UserRolesDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Columnas asociadas al catálogo de "roles de usuario".
        /// </summary>
        private readonly string[] UserRoleFields = new string[] { "cve_RolUsuario", "rolUsuario", "defaultRole" };

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public UserRolesDAO()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Método utilizado para recuperar el catálogo de roles de usuario.
        /// </summary>
        /// <returns>Devuelve el catálogo de roles de usuario disponibles.</returns>
        public List<UserRole> GetUserRoles()
        {
            List<UserRole> userRoles = new List<UserRole>();
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT ").Append(string.Join(",", UserRoleFields));
                query.Append(" FROM [dbo].[Cat_RolesDeUsuario] ");
                query.Append(" ORDER BY rolUsuario ASC ");
                Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = Connection;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = query.ToString();
                var reader = sqlcmd.ExecuteReader();
                while (reader.Read())
                {
                    UserRole singleRole = Mapping.MapRole(reader);
                    userRoles.Add(singleRole);
                }

                reader.Close();
                Close();
            }
            catch (Exception ex)
            {
                throw;
            }

            return userRoles;
        }
    }
}