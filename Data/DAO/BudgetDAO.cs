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
    using Data.Models.Request;

    /// <summary>
    /// Clase asociada al acceso a datos para manipular la información asociada al presupuesto con las cuentas/cecos por colaborador.
    /// </summary>
    public class BudgetDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        readonly string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public BudgetDAO()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Método utilizado para eliminar las cuentas asociadas a un año y tipo de carga específicos.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la eliminación de cuentas/Cecos.</param>
        /// <returns>Devuelve una bandera para determinar si la eliminación fue correcta o no.</returns>
        public bool DeleteAccounts(AccountsDataRequest accountsData)
        {
            bool successDelete = false;
            try
            {
                if (accountsData != null)
                {
                    Open();
                    SqlCommand sqlcmd = new SqlCommand();
                    StringBuilder query = new StringBuilder();
                    query.Append("DELETE [dbo].[Tbl_Presupuesto_General] WHERE 1 = 1 ");
                    if (accountsData.YearAccounts != 0)
                    {
                        query.Append(" AND anio = @anio ");
                        sqlcmd.Parameters.AddWithValue("@anio", accountsData.YearAccounts);
                    }

                    if (accountsData.ChargeTypeAccounts != 0)
                    {
                        query.Append(" AND cve_TipoCarga = @tipo_carga ");
                        sqlcmd.Parameters.AddWithValue("@tipo_carga", accountsData.ChargeTypeAccounts);
                    }

                    if (accountsData.Collaborator != 0)
                    {
                        query.Append(" AND cve_Colaborador = @collaborator ");
                        sqlcmd.Parameters.AddWithValue("@collaborator", accountsData.Collaborator);
                    }

                    if (accountsData.Area != 0)
                    {
                        query.Append(" AND cve_Area = @area ");
                        sqlcmd.Parameters.AddWithValue("@area", accountsData.Area);
                    }

                    if (accountsData.FileLogId != 0)
                    {
                        query.Append(" AND cve_LogArchivo = @fileLogId ");
                        sqlcmd.Parameters.AddWithValue("@fileLogId", accountsData.FileLogId);
                    }

                    sqlcmd.Connection = Connection;
                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = query.ToString();
                    sqlcmd.ExecuteNonQuery();
                    Close();
                    successDelete = true;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar las cuentas asociadas a un ajuste de presupuesto manual.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la eliminación de cuentas/Cecos.</param>
        /// <returns>Devuelve una bandera para determinar si la eliminación fue correcta o no.</returns>
        public bool DeleteManualBudget(AccountsDataRequest accountsData)
        {
            bool successDelete = false;
            try
            {
                if (accountsData != null)
                {
                    Open();
                    SqlCommand sqlcmd = new SqlCommand();
                    StringBuilder query = new StringBuilder();
                    query.Append("DELETE [dbo].[Tbl_Presupuesto_Ajuste] WHERE 1 = 1 ");
                    if (accountsData.YearAccounts != 0)
                    {
                        query.Append(" AND anio = @anio ");
                        sqlcmd.Parameters.AddWithValue("@anio", accountsData.YearAccounts);
                    }

                    if (accountsData.ChargeTypeAccounts != 0)
                    {
                        query.Append(" AND cve_TipoCarga = @tipo_carga ");
                        sqlcmd.Parameters.AddWithValue("@tipo_carga", accountsData.ChargeTypeAccounts);
                    }

                    if (accountsData.Collaborator != 0)
                    {
                        query.Append(" AND cve_Colaborador = @collaborator ");
                        sqlcmd.Parameters.AddWithValue("@collaborator", accountsData.Collaborator);
                    }

                    if (accountsData.Area != 0)
                    {
                        query.Append(" AND cve_Area = @area ");
                        sqlcmd.Parameters.AddWithValue("@area", accountsData.Area);
                    }

                    if (accountsData.FileLogId != 0)
                    {
                        query.Append(" AND cve_LogArchivo = @fileLogId ");
                        sqlcmd.Parameters.AddWithValue("@fileLogId", accountsData.FileLogId);
                    }

                    sqlcmd.Connection = Connection;
                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = query.ToString();
                    sqlcmd.ExecuteNonQuery();
                    Close();
                    successDelete = true;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar las cuentas/cecos dentro de la tabla de hechos de acuerdo a los parámetros enviados.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la eliminación de cuentas/Cecos.</param>
        /// <returns>Devuelve una bandera para determinar si la eliminación fue correcta o no.</returns>
        public bool FactTableDeleteAccounts(AccountsDataRequest accountsData)
        {
            bool successDelete = false;
            try
            {
                if (accountsData != null)
                {
                    Open();
                    SqlCommand sqlcmd = new SqlCommand();
                    StringBuilder query = new StringBuilder();
                    query.Append("DELETE [dbo].[Fact_CostosGastosFijos] WHERE 1 = 1 ");
                    if (accountsData.YearAccounts != 0)
                    {
                        query.Append(" AND anio = @anio ");
                        sqlcmd.Parameters.AddWithValue("@anio", accountsData.YearAccounts);
                    }

                    if (accountsData.ChargeTypeAccounts != 0)
                    {
                        query.Append(" AND cve_TipoCarga = @tipo_carga ");
                        sqlcmd.Parameters.AddWithValue("@tipo_carga", accountsData.ChargeTypeAccounts);
                    }

                    if (accountsData.Collaborator != 0)
                    {
                        query.Append(" AND cve_Colaborador = @collaborator ");
                        sqlcmd.Parameters.AddWithValue("@collaborator", accountsData.Collaborator);
                    }

                    if (accountsData.Area != 0)
                    {
                        query.Append(" AND cve_Area = @area ");
                        sqlcmd.Parameters.AddWithValue("@area", accountsData.Area);
                    }

                    if (accountsData.FileLogId != 0)
                    {
                        query.Append(" AND cve_LogArchivo = @fileLogId ");
                        sqlcmd.Parameters.AddWithValue("@fileLogId", accountsData.FileLogId);
                    }

                    sqlcmd.Connection = Connection;
                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = query.ToString();
                    sqlcmd.ExecuteNonQuery();
                    Close();
                    successDelete = true;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return successDelete;
        }
    }
}