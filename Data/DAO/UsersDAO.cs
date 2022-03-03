namespace Data.DAO
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Data.Models;
    using Data.Models.Request;
    using Data.Models.Response;

    /// <summary>
    /// Clase asociada al acceso a datos para manipular la información asociada a los usuarios.
    /// </summary>
    public class UsersDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Columnas asociadas al catálogo de "colaboradores".
        /// </summary>
        private readonly string[] CollaboratorFields = new string[] { "cve_Colaborador", "nombre", "correo", "usuario", "cve_RolUsuario" };

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public UsersDAO()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Método utilizado para recuperar la información de un colaborador de acuerdo a su username.
        /// </summary>
        /// <param name="username">Nombre de usuario del colaborador.</param>
        /// <returns>Devuelve un objeto con la información general del colaborador.</returns>
        public UserData CollaboratorByUsername(string username)
        {
            UserData collaborator = new UserData();
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT ").Append(string.Join(",", CollaboratorFields));
                query.Append(" FROM Cat_Colaboradores WHERE usuario = @username ");
                Open();
                SqlCommand sqlcmd = new SqlCommand
                {
                    Connection = Connection,
                    CommandType = CommandType.Text,
                    CommandText = query.ToString()
                };
                sqlcmd.Parameters.AddWithValue("@username", username);
                var reader = sqlcmd.ExecuteReader();
                while (reader.Read())
                {
                    collaborator.CollaboratorId = reader["cve_Colaborador"] != DBNull.Value ? Convert.ToInt32(reader["cve_Colaborador"]) : 0;
                    collaborator.CollaboratorName = reader["nombre"] != DBNull.Value ? reader["nombre"].ToString() : string.Empty;
                    collaborator.Email = reader["correo"] != DBNull.Value ? reader["correo"].ToString() : string.Empty;
                    collaborator.Username = reader["usuario"] != DBNull.Value ? reader["usuario"].ToString() : string.Empty;
                }

                reader.Close();
                Close();
            }
            catch (Exception ex)
            {
                throw;
            }

            return collaborator;
        }

        /// <summary>
        /// Método utilizado para recuperar los datos generales de un usuario.
        /// </summary>
        /// <param name="username">Nombre de usuario.</param>
        /// <returns>Devuelve un objeto que contiene la información general de un usuario.</returns>
        public UserData UserLogin(string username)
        {
            UserData userInformation = null;
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT colab.*, roles.rolUsuario ");
                query.Append(" FROM [dbo].[Cat_Colaboradores] colab ");
                query.Append(" INNER JOIN [dbo].[Cat_RolesDeUsuario] roles ON roles.cve_RolUsuario = colab.cve_RolUsuario ");
                query.Append(" WHERE usuario = @username");
                userInformation = GetUserData(query.ToString(), username);
            }
            catch (Exception ex)
            {
                throw;
            }

            return userInformation;
        }

        /// <summary>
        /// Método utilizado para recuperar la información de un usuario de acuerdo al id asociado a este.
        /// </summary>
        /// <param name="userId">Id asociado al usuario.</param>
        /// <returns>Devuelve la información general del usuario.</returns>
        public UserData GetUserById(int userId)
        {
            UserData userInformation = null;
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT colab.*, roles.rolUsuario ");
                query.Append(" FROM [dbo].[Cat_Colaboradores] colab ");
                query.Append(" INNER JOIN [dbo].[Cat_RolesDeUsuario] roles ON roles.cve_RolUsuario = colab.cve_RolUsuario ");
                query.Append(" WHERE cve_Colaborador = @userId");
                userInformation = GetUserData(query.ToString(), null, userId);
            }
            catch (Exception ex)
            {
                throw;
            }

            return userInformation;
        }

        /// <summary>
        /// Método utilizado para recuperar la información que será mostrada en la tabla de usuarios.
        /// </summary>
        /// <param name="dataTableInfo">Objeto que contiene los parámetros de búsqueda para la tabla de usuarios.</param>
        /// <returns>Devuelve un objeto que contiene la información necesaria para mostrar la tabla de usuarios.</returns>
        public UsersTableResponse GetUsersTable(DataTableRequest dataTableInfo)
        {
            List<UserData> usersList = new List<UserData>();
            int usersCount = 0;
            try
            {
                if (dataTableInfo != null)
                {
                    Open();
                    if (!dataTableInfo.GetTotalRowsCount)
                    {
                        string queryUsersList = UsersCommonQuery(dataTableInfo);
                        SqlCommand sqlcmdList = new SqlCommand
                        {
                            Connection = Connection,
                            CommandType = CommandType.Text,
                            CommandText = queryUsersList
                        };
                        sqlcmdList.Parameters.AddWithValue("@rowsToSkip", dataTableInfo.RowsToSkip);
                        sqlcmdList.Parameters.AddWithValue("@numbersOfRows", dataTableInfo.NumberOfRows);
                        if (!string.IsNullOrEmpty(dataTableInfo.SearchValue) && dataTableInfo.SearchValue.Length >= 3)
                        {
                            sqlcmdList.Parameters.AddWithValue("@searchValue", dataTableInfo.SearchValue);
                        }

                        var reader = sqlcmdList.ExecuteReader();
                        while (reader.Read())
                        {
                            var userRoleExists = HasColumn(reader, "rolUsuario");
                            UserData singleUser = Mapping.MapCollaborator(reader, userRoleExists);
                            usersList.Add(singleUser);
                        }

                        reader.Close();
                    }

                    if (dataTableInfo.MakeServicesCountQuery)
                    {
                        dataTableInfo.GetTotalRowsCount = true;
                        string queryUsersCount = UsersCommonQuery(dataTableInfo);
                        SqlCommand sqlcmdCount = new SqlCommand
                        {
                            Connection = Connection,
                            CommandType = CommandType.Text,
                            CommandText = queryUsersCount
                        };
                        if (!string.IsNullOrEmpty(dataTableInfo.SearchValue) && dataTableInfo.SearchValue.Length >= 3)
                        {
                            sqlcmdCount.Parameters.AddWithValue("@searchValue", dataTableInfo.SearchValue);
                        }

                        usersCount = Convert.ToInt32(sqlcmdCount.ExecuteScalar());
                    }

                    Close();
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            UsersTableResponse usersTable = new UsersTableResponse()
            {
                UsersList = usersList,
                UsersCount = usersCount
            };
            return usersTable;
        }

        /// <summary>
        /// Método utilizado para construir la consulta para encontrar la información en la tabla de usuarios.
        /// </summary>
        /// <param name="dataTableInfo">Objeto que contiene los parámetros de búsqueda para la tabla de usuarios.</param>
        /// <returns>Devuelve una cadena con la consulta para buscar la información.</returns>
        public string UsersCommonQuery(DataTableRequest dataTableInfo)
        {
            StringBuilder query = new StringBuilder();
            if (dataTableInfo != null)
            {
                if (dataTableInfo.GetTotalRowsCount)
                {
                    query.Append(" SELECT COUNT(*) AS countUsers FROM ( ");
                }
                else
                {
                    query.Append(" SELECT * FROM ( ");
                }

                query.Append("  SELECT catColab.*, catRol.rolUsuario, ROW_NUMBER() OVER (");
                if (!string.IsNullOrEmpty(dataTableInfo.SortName))
                {
                    query.Append("ORDER BY ").Append(dataTableInfo.SortName);
                    if (!string.IsNullOrEmpty(dataTableInfo.SortOrder))
                    {
                        query.Append(" ").Append(dataTableInfo.SortOrder);
                    }
                }
                else
                {
                    query.Append(" ORDER BY cve_Colaborador ");
                }

                query.Append(" ) AS rowNumber ");
                query.Append("  FROM Cat_Colaboradores catColab ");
                query.Append("  INNER JOIN Cat_RolesDeUsuario catRol ");
                query.Append("  ON catColab.cve_RolUsuario = catRol.cve_RolUsuario ");
                if (!string.IsNullOrEmpty(dataTableInfo.SearchValue) && dataTableInfo.SearchValue.Length >= 3)
                {
                    query.Append(" WHERE CHARINDEX(REPLACE(LTRIM(RTRIM(LOWER(@searchValue))), ' ', ''), ");
                    query.Append("  REPLACE(LTRIM(RTRIM(LOWER( ");
                    query.Append("      ISNULL(nombre, '') + ISNULL(correo, '') + ISNULL(usuario, '') + ISNULL(catRol.rolUsuario, '') ");
                    query.Append("  ))), ' ', '') ");
                    query.Append(" ) > 0 ");
                }

                query.Append(" ) t ");
                if (!dataTableInfo.GetTotalRowsCount)
                {
                    query.Append(" WHERE rowNumber BETWEEN (@rowsToSkip + 1) AND (@rowsToSkip + @numbersOfRows) ");
                }
            }

            string finalQuery = query.ToString();
            return finalQuery;
        }

        /// <summary>
        /// Método utilizado para guardar la información asociada a un usuario de la aplicación.
        /// </summary>
        /// <param name="userInformation">Objeto que contiene la información general del usuario.</param>
        /// <returns>Devuelve el id asociado al usuario recién insertado en la Base de Datos.</returns>
        public int SaveUserInformation(UserData userInformation)
        {
            int collaboratorId = 0;
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = Connection;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = "INSERT INTO [dbo].[Cat_Colaboradores] VALUES (@name, @email, @username, @roleId); SELECT SCOPE_IDENTITY();";
                sqlcmd.Parameters.AddWithValue("@name", userInformation.CollaboratorName);
                sqlcmd.Parameters.AddWithValue("@email", userInformation.Email);
                sqlcmd.Parameters.AddWithValue("@username", userInformation.Username);
                sqlcmd.Parameters.AddWithValue("@roleId", userInformation.RoleId);
                collaboratorId = Convert.ToInt32(sqlcmd.ExecuteScalar());
                Close();
            }
            catch (Exception ex)
            {
                throw;
            }

            return collaboratorId;
        }

        /// <summary>
        /// Método utilizado para actualizar la información asociada a un usuario de la aplicación.
        /// </summary>
        /// <param name="userInformation">Objeto que contiene la información general del usuario.</param>
        /// <returns>Devuelve una bandera para determinar si la actualización fue correcta.</returns>
        public bool UpdateUserInformation(UserData userInformation)
        {
            bool successUpdate = false;
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = Connection;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = "UPDATE [dbo].[Cat_Colaboradores] SET nombre = @name, correo = @email, usuario = @username, cve_RolUsuario = @role WHERE cve_Colaborador = @userId ";
                sqlcmd.Parameters.AddWithValue("@name", userInformation.CollaboratorName);
                sqlcmd.Parameters.AddWithValue("@email", userInformation.Email);
                sqlcmd.Parameters.AddWithValue("@username", userInformation.Username);
                sqlcmd.Parameters.AddWithValue("@role", userInformation.RoleId);
                sqlcmd.Parameters.AddWithValue("@userId", userInformation.CollaboratorId);
                sqlcmd.ExecuteNonQuery();
                Close();
                successUpdate = true;
            }
            catch (Exception ex)
            {
                throw;
            }

            return successUpdate;
        }

        /// <summary>
        /// Método utilizado para eliminar la información general asociada a un usuario.
        /// </summary>
        /// <param name="userId">Id asociado al usuario.</param>
        /// <returns>Devuelve una bandera para determinar si la información se eliminó correctamente.</returns>
        public bool DeleteUserInformation(int userId)
        {
            bool successDelete = false;
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = Connection;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = "DELETE [dbo].[Cat_Colaboradores] WHERE cve_Colaborador = @collaboratorId ";
                sqlcmd.Parameters.AddWithValue("@collaboratorId", userId);
                sqlcmd.ExecuteNonQuery();
                Close();
                successDelete = true;
            }
            catch (Exception ex)
            {
                throw;
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para mapear la información general de un usuario.
        /// </summary>
        /// <param name="query">Consulta SQL para recuperar la información de un usuario.</param>
        /// <param name="username">Nombre de usuario.</param>
        /// <returns>Devuelve un objeto que contiene la información general de un usuario.</returns>
        private UserData GetUserData(string query, string username = "", int? userId = null)
        {
            UserData userInformation = new UserData();
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = Connection;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = query;
                if (!string.IsNullOrEmpty(username))
                {
                    sqlcmd.Parameters.AddWithValue("@username", username);
                }

                if (userId.HasValue && userId.Value > 0)
                {
                    sqlcmd.Parameters.AddWithValue("@userId", userId);
                }

                var reader = sqlcmd.ExecuteReader();
                var userRoleExists = HasColumn(reader, "rolUsuario");
                while (reader.Read())
                {
                    userInformation = Mapping.MapCollaborator(reader, userRoleExists);
                }

                reader.Close();
                Close();
            }
            catch (Exception ex)
            {
                throw;
            }

            return userInformation;
        }
    }
}