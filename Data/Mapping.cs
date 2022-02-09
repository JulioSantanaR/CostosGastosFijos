namespace Data
{
    using System;
    using System.Data.SqlClient;
    using Data.Models;

    /// <summary>
    /// Clase utilizada para mapear la información obtenida desde la Base de Datos.
    /// </summary>
    public static class Mapping
    {
        /// <summary>
        /// Método utilizado para mapear la información relacionada a la consulta de un colaborador.
        /// </summary>
        /// <param name="reader">Información obtenida desde la Base de datos.</param>
        /// <param name="userRoleExists">Bandera para determinar si se debe agregar la información del rol de usuario.</param>
        /// <returns>Devuelve un objeto que contiene la información mapeada del colaborador.</returns>
        public static UserData MapCollaborator(SqlDataReader reader, bool userRoleExists = false)
        {
            UserData userInformation = new UserData();
            userInformation.CollaboratorId = reader["cve_Colaborador"] != DBNull.Value ? Convert.ToInt32(reader["cve_Colaborador"]) : 0;
            userInformation.CollaboratorName = reader["nombre"] != DBNull.Value ? reader["nombre"].ToString() : string.Empty;
            userInformation.Email = reader["correo"] != DBNull.Value ? reader["correo"].ToString() : string.Empty;
            userInformation.Username = reader["usuario"] != DBNull.Value ? reader["usuario"].ToString() : string.Empty;
            userInformation.RoleId = reader["cve_RolUsuario"] != DBNull.Value ? Convert.ToInt32(reader["cve_RolUsuario"]) : 0;
            if (userRoleExists)
            {
                userInformation.RolUsuario = reader["rolUsuario"] != DBNull.Value ? reader["rolUsuario"].ToString() : string.Empty;
            }

            return userInformation;
        }

        /// <summary>
        /// Método utilizado para mapear la información relacionada a los roles de usuario.
        /// </summary>
        /// <param name="reader">Información obtenida desde la Base de datos.</param>
        /// <returns>Devuelve la información de los roles de usuario.</returns>
        public static UserRole MapRole(SqlDataReader reader)
        {
            UserRole roleInformation = new UserRole();
            roleInformation.RoleId = reader["cve_RolUsuario"] != DBNull.Value ? Convert.ToInt32(reader["cve_RolUsuario"]) : 0;
            roleInformation.RoleName = reader["rolUsuario"] != DBNull.Value ? reader["rolUsuario"].ToString() : string.Empty;
            roleInformation.DefaultRole = reader["defaultRole"] != DBNull.Value ? Convert.ToBoolean(reader["defaultRole"]) : false;
            return roleInformation;
        }

        /// <summary>
        /// Método utilizado para mapear la información relacionada al catálogo de áreas.
        /// </summary>
        /// <param name="reader">Información obtenida desde la Base de datos.</param>
        /// <returns>Devuelve la información asociada al catálogo de áreas.</returns>
        public static AreaData MapArea(SqlDataReader reader)
        {
            AreaData areaInformation = new AreaData();
            areaInformation.AreaId = reader["cve_Area"] != DBNull.Value ? Convert.ToInt32(reader["cve_Area"]) : 0;
            areaInformation.NameArea = reader["nombre"] != DBNull.Value ? reader["nombre"].ToString() : string.Empty;
            areaInformation.DefaultArea = reader["defaultArea"] != DBNull.Value ? Convert.ToBoolean(reader["defaultArea"]) : false;
            return areaInformation;
        }
    }
}