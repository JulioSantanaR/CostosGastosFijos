namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;
    using Data.Models;
    using Data.Models.Request;
    using Data.Models.Response;
    using Data.Repositories;

    /// <summary>
    /// Clase utilizada para leer información desde la Base de Datos.
    /// </summary>
    public class ReadDataDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Columnas asociadas al catálogo de "colaboradores".
        /// </summary>
        private readonly string[] CollaboratorFields = new string[] { "cve_Colaborador", "nombre", "correo", "usuario", "cve_RolUsuario" };

        private readonly string[] UserRoleFields = new string[] { "cve_RolUsuario", "rolUsuario", "defaultRole" };

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public ReadDataDAO()
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
        /// Método utilizado para obtener las áreas asociadas a un usuario.
        /// </summary>
        /// <param name="username">Nombre de usuario.</param>
        /// <param name="userId">Id asociado al usuario.</param>
        /// <returns>Devuelve la lista de áreas asociadas a un usuario/colaborador.</returns>
        public List<AreaData> UserAreas(string username = "", int? userId = null)
        {
            List<AreaData> userAreas = null;
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT colab.*, area.nombre AS nameArea, area.cve_Area, area.defaultArea FROM [dbo].[Cat_Colaboradores] AS colab ");
                query.Append("INNER JOIN [dbo].[Cat_ColaboradorAreas] colabArea ON colabArea.cve_Colaborador = colab.cve_colaborador ");
                query.Append("INNER JOIN [dbo].[Cat_Areas] area ON colabArea.cve_Area = area.cve_Area ");
                query.Append(" WHERE 1 = 1 ");
                if (!string.IsNullOrEmpty(username))
                {
                    query.Append(" AND usuario = @username ");
                }

                if (userId.HasValue && userId.Value > 0)
                {
                    query.Append(" AND colab.cve_Colaborador = @userId ");
                }

                query.Append("ORDER BY area.nombre ASC ");
                userAreas = GetUserAreas(query.ToString(), username, userId);
            }
            catch (Exception ex)
            {
                throw;
            }

            return userAreas;
        }

        /// <summary>
        /// Método utilizado para recuperar las cuentas/cecos que no tengan la información completa en el BIF.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la búsqueda de cuentas/cecos con información incompleta en el BIF.</param>
        /// <returns>Devuelve la lista de cuentas dentro del presupuesto cargado que no están completas en el BIF.</returns>
        public List<Accounts> GetNotFoundAccountsBIF(AccountsDataRequest accountsData)
        {
            List<Accounts> notFoundAccounts = new List<Accounts>();
            try
            {
                if (accountsData != null)
                {
                    Open();
                    SqlCommand sqlcmd = new SqlCommand("usp_CuentasIncompletasBIF", Connection);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.Parameters.AddWithValue("@anio", accountsData.YearAccounts);
                    sqlcmd.Parameters.AddWithValue("@tipo_carga", accountsData.ChargeTypeAccounts);
                    sqlcmd.Parameters.AddWithValue("@colaborador", accountsData.Collaborator);
                    sqlcmd.Parameters.AddWithValue("@area", accountsData.Area);
                    sqlcmd.CommandTimeout = 3600;
                    var reader = sqlcmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Accounts singleAccount = new Accounts();
                        singleAccount.Account = reader["cuenta"] != DBNull.Value ? reader["cuenta"].ToString() : string.Empty;
                        singleAccount.CostCenter = reader["centroDeCosto"] != DBNull.Value ? reader["centroDeCosto"].ToString() : string.Empty;
                        notFoundAccounts.Add(singleAccount);
                    }

                    reader.Close();
                    Close();
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetNotFoundAccountsBIF()." + "Error: " + ex.Message);
            }

            return notFoundAccounts;
        }

        public List<AccountArea> ReviewAccounts(List<string> accounts, List<int> areas)
        {
            List<AccountArea> reviewAccount = new List<AccountArea>();
            try
            {
                Open();
                StringBuilder query = new StringBuilder();
                query.Append("SELECT * FROM [dbo].[Cat_CuentasAreas] ");
                query.Append("WHERE 1 = 1 ");
                SqlCommand sqlcmd = new SqlCommand();
                if (accounts != null && accounts.Count > 0)
                {
                    string accountParameters = BuildParameters("@account", accounts.Cast<dynamic>().ToList(), sqlcmd);
                    query.Append("AND cuenta IN (").Append(accountParameters).Append(") ");
                }

                if (areas != null && areas.Count > 0)
                {
                    string areaParameters = BuildParameters("@area", areas.Cast<dynamic>().ToList(), sqlcmd);
                    query.Append("AND cve_Area IN (").Append(areaParameters).Append(") ");
                }

                sqlcmd.Connection = Connection;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = query.ToString();
                var reader = sqlcmd.ExecuteReader();
                while (reader.Read())
                {
                    AccountArea singleAccountArea = new AccountArea();
                    singleAccountArea.Account = reader["cuenta"] != DBNull.Value ? reader["cuenta"].ToString() : string.Empty;
                    singleAccountArea.AreaId = reader["cve_Area"] != DBNull.Value ? Convert.ToInt32(reader["cve_Area"]) : 0;
                    reviewAccount.Add(singleAccountArea);
                }

                reader.Close();
                Close();
            }
            catch (Exception ex)
            {
                throw;
            }

            return reviewAccount;
        }

        public DataTable AdesProjection()
        {
            DataTable adesData = new DataTable();
            try
            {
                Open();
                SqlConnection connectionData = GetConnection();
                using (var sqlcmd = new SqlCommand("usp_InsertarProyeccionAdes", Connection))
                using (var adapter = new SqlDataAdapter(sqlcmd))
                {
                    sqlcmd.CommandTimeout = 3600;
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    adapter.Fill(adesData);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return adesData;
        }

        public DataTable StillsProjection(string channel)
        {
            DataTable stillsData = new DataTable();
            try
            {
                Open();
                SqlConnection connectionData = GetConnection();
                using (var sqlcmd = new SqlCommand("usp_InsertarProyeccionStills", Connection))
                using (var adapter = new SqlDataAdapter(sqlcmd))
                {
                    sqlcmd.CommandTimeout = 3600;
                    sqlcmd.Parameters.AddWithValue("@canal", channel);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    adapter.Fill(stillsData);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return stillsData;
        }

        public DataTable LacteosProjection()
        {
            DataTable lacteosData = new DataTable();
            try
            {
                Open();
                SqlConnection connectionData = GetConnection();
                using (var sqlcmd = new SqlCommand("usp_InsertarProyeccionLacteos", Connection))
                using (var adapter = new SqlDataAdapter(sqlcmd))
                {
                    sqlcmd.CommandTimeout = 3600;
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    adapter.Fill(lacteosData);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return lacteosData;
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
                    SqlCommand sqlcmd = new SqlCommand();
                    sqlcmd.Connection = Connection;
                    sqlcmd.CommandType = CommandType.Text;
                    if (!dataTableInfo.GetTotalRowsCount)
                    {
                        string queryUsersList = UsersCommonQuery(dataTableInfo);
                        sqlcmd.CommandText = queryUsersList;
                        sqlcmd.Parameters.AddWithValue("@rowsToSkip", dataTableInfo.RowsToSkip);
                        sqlcmd.Parameters.AddWithValue("@numbersOfRows", dataTableInfo.NumberOfRows);
                        if (!string.IsNullOrEmpty(dataTableInfo.SearchValue) && dataTableInfo.SearchValue.Length >= 3)
                        {
                            sqlcmd.Parameters.AddWithValue("@searchValue", dataTableInfo.SearchValue);
                        }
                        
                        var reader = sqlcmd.ExecuteReader();
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
                        sqlcmd.CommandText = queryUsersCount;
                        usersCount = Convert.ToInt32(sqlcmd.ExecuteScalar());
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
                    query.Append(" SELECT COUNT(*) AS countUsers FROM Cat_Colaboradores ");
                }
                else
                {
                    query.Append(" SELECT * FROM ( ");
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
                        query.Append("      ISNULL(nombre, '') + ISNULL(correo, '') + ISNULL(usuario, '') ");
                        query.Append("  ))), ' ', '') ");
                        query.Append(" ) > 0 ");
                    }

                    query.Append(" ) t WHERE rowNumber BETWEEN (@rowsToSkip + 1) AND (@rowsToSkip + @numbersOfRows) ");
                }
            }

            string finalQuery = query.ToString();
            return finalQuery;
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

        /// <summary>
        /// Método utilizado para mapear la información del catálogo de áreas.
        /// </summary>
        /// <param name="query">Consulta SQL para recuperar el catálogo de áreas.</param>
        /// <param name="username">Nombre de usuario.</param>
        /// <param name="userId">Id asociado al usuario.</param>
        /// <returns>Devuelve una lista que contiene la información de las áreas asociadas a un usuario.</returns>
        private List<AreaData> GetUserAreas(string query, string username = "", int? userId = null)
        {
            List<AreaData> userAreas = new List<AreaData>();
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
                while (reader.Read())
                {
                    AreaData singleArea = new AreaData();
                    singleArea.AreaId = reader["cve_Area"] != DBNull.Value ? Convert.ToInt32(reader["cve_Area"]) : 0;
                    singleArea.NameArea = reader["nameArea"] != DBNull.Value ? reader["nameArea"].ToString() : string.Empty;
                    singleArea.DefaultArea = reader["defaultArea"] != DBNull.Value ? Convert.ToBoolean(reader["defaultArea"]) : false;
                    userAreas.Add(singleArea);
                }

                reader.Close();
                Close();
            }
            catch (Exception ex)
            {
                throw;
            }

            return userAreas;
        }

        /// <summary>
        /// Método utilizado para construir los parámetros que se colocarán en una consulta (query).
        /// </summary>
        /// <param name="parameterName">Nombre del parámetro.</param>
        /// <param name="listData">Lista de datos.</param>
        /// <param name="sqlcmd">Objeto asociado a la consulta SQL.</param>
        /// <returns>Devuelve una cadena que contiene la información de los parámetros.</returns>
        private string BuildParameters(string parameterName, List<dynamic> listData, SqlCommand sqlcmd)
        {
            var parameters = new List<string>();
            var counter = 0;
            foreach (var singleData in listData)
            {
                var paramName = parameterName + counter;
                sqlcmd.Parameters.AddWithValue(paramName, singleData);
                parameters.Add(paramName);
                counter++;
            }

            string parameterResult = string.Join(",", parameters);
            return parameterResult;
        }
    }
}