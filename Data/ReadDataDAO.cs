namespace Data
{
    using Data.Models;
    using Data.Models.Request;
    using Data.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;

    public class ReadDataDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        private string[] CollaboratorFields = new string[] { "cve_Colaborador", "nombre", "correo", "usuario", "cve_Area" };

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public ReadDataDAO()
        {
            ConnectionString = connectionString;
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
                Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = Connection;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = "SELECT cve_Colaborador, nombre, correo, usuario FROM Cat_Colaboradores WHERE usuario = @username ";
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

        public List<UserData> UserLogin(string username)
        {
            List<UserData> userInformation = new List<UserData>();
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT colab.*, area.nombre AS nameArea, area.cve_Area, area.defaultArea FROM [dbo].[Cat_Colaboradores] AS colab ");
                query.Append("INNER JOIN [dbo].[Cat_ColaboradorAreas] colabArea ON colabArea.cve_Colaborador = colab.cve_colaborador ");
                query.Append("INNER JOIN [dbo].[Cat_Areas] area ON colabArea.cve_Area = area.cve_Area ");
                query.Append(" WHERE usuario = @username ");
                query.Append("ORDER BY area.nombre ASC ");
                userInformation = GetUserData(username, query.ToString());
            }
            catch (Exception ex)
            {
                throw;
            }

            return userInformation;
        }

        public List<UserData> UserAllAreas(string username)
        {
            List<UserData> userInformation = new List<UserData>();
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT colab.*, area.nombre AS nameArea, area.cve_Area, area.defaultArea FROM [dbo].[Cat_Colaboradores] AS colab, ");
                query.Append("[dbo].[Cat_Areas] AS area WHERE usuario = @username AND cve_Area <> 0 ");
                query.Append("ORDER BY area.nombre ASC ");
                userInformation = GetUserData(username, query.ToString());
            }
            catch (Exception ex)
            {
                throw;
            }

            return userInformation;
        }

        public List<UserData> GetUserData(string username, string query)
        {
            List<UserData> userInformation = new List<UserData>();
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = Connection;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = query;
                sqlcmd.Parameters.AddWithValue("@username", username);
                var reader = sqlcmd.ExecuteReader();
                while (reader.Read())
                {
                    UserData singleData = new UserData();
                    singleData.CollaboratorId = reader["cve_Colaborador"] != DBNull.Value ? Convert.ToInt32(reader["cve_Colaborador"]) : 0;
                    singleData.CollaboratorName = reader["nombre"] != DBNull.Value ? reader["nombre"].ToString() : string.Empty;
                    singleData.Email = reader["correo"] != DBNull.Value ? reader["correo"].ToString() : string.Empty;
                    singleData.Username = reader["usuario"] != DBNull.Value ? reader["usuario"].ToString() : string.Empty;
                    singleData.AreaId = reader["cve_Area"] != DBNull.Value ? Convert.ToInt32(reader["cve_Area"]) : 0;
                    singleData.NameArea = reader["nameArea"] != DBNull.Value ? reader["nameArea"].ToString() : string.Empty;
                    singleData.DefaultArea = reader["defaultArea"] != DBNull.Value ? Convert.ToBoolean(reader["defaultArea"]) : false;
                    userInformation.Add(singleData);
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

        public List<PromotoriaDB> GetPromotoria(int yearData, int chargeTypeData)
        {
            List<PromotoriaDB> promotoria = new List<PromotoriaDB>();
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT mes, SUM(presupuesto) AS presupuesto FROM [dbo].[Fact_CostosGastosFijos] ");
                query.Append("WHERE (filtro COLLATE Latin1_general_CI_AI) LIKE '%promotoria%' ");
                query.Append("AND cve_TipoCarga = @chargeTypeData AND anio = @yearData GROUP BY mes ");
                Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = Connection;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = query.ToString();
                sqlcmd.Parameters.AddWithValue("@chargeTypeData", chargeTypeData);
                sqlcmd.Parameters.AddWithValue("@yearData", yearData);
                var reader = sqlcmd.ExecuteReader();
                while (reader.Read())
                {
                    PromotoriaDB singleBudget = new PromotoriaDB();
                    singleBudget.MonthNumber = reader["mes"] != DBNull.Value ? Convert.ToInt32(reader["mes"]) : default(int);
                    singleBudget.Budget = reader["presupuesto"] != DBNull.Value ? Convert.ToDouble(reader["presupuesto"]) : default(double);
                    promotoria.Add(singleBudget);
                }

                reader.Close();
                Close();
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetPromotoria()." + "Error: " + ex.Message);
            }

            return promotoria;
        }

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