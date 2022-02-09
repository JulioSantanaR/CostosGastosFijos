namespace Data
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;
    using Data.Models.Request;
    using Data.Repositories;

    /// <summary>
    /// Clase que contiene los métodos asociados a la operación de eliminación de datos.
    /// </summary>
    public class DeleteDataDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public DeleteDataDAO()
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
                    sqlcmd.Connection = Connection;
                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = "DELETE [dbo].[Tbl_Presupuesto_General] WHERE anio = @anio AND cve_TipoCarga = @tipo_carga AND cve_Colaborador = @collaborator AND cve_Area = @area";
                    sqlcmd.Parameters.AddWithValue("@anio", accountsData.YearAccounts);
                    sqlcmd.Parameters.AddWithValue("@tipo_carga", accountsData.ChargeTypeAccounts);
                    sqlcmd.Parameters.AddWithValue("@collaborator", accountsData.Collaborator);
                    sqlcmd.Parameters.AddWithValue("@area", accountsData.Area);
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
        /// Método utilizado para eliminar la información asociada al volumen, de acuerdo a un año y tipo de carga específicos.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public bool DeleteVolume(int yearData, int chargeTypeData)
        {
            bool successDelete = false;
            try
            {
                string query = string.Format("DELETE [dbo].[Tbl_Volumen] WHERE anio = {0} AND cve_TipoCarga = {1}", yearData, chargeTypeData);
                Open();
                SqlConnection connectionData = GetConnection();
                SqlCommand sqlcmd = new SqlCommand()
                {
                    Connection = connectionData,
                    CommandType = CommandType.Text,
                    CommandText = query
                };
                sqlcmd.ExecuteNonQuery();
                successDelete = true;
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteVolume()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada al volumen en un BP/Rolling 0+12.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public bool DeleteVolumeBP(int yearData, int chargeTypeData)
        {
            bool successDelete = false;
            try
            {
                string query = string.Format("DELETE [dbo].[Tbl_Volumen_BP] WHERE anio = {0} AND cve_TipoCarga = {1}", yearData, chargeTypeData);
                Open();
                SqlConnection connectionData = GetConnection();
                SqlCommand sqlcmd = new SqlCommand()
                {
                    Connection = connectionData,
                    CommandType = CommandType.Text,
                    CommandText = query
                };
                sqlcmd.ExecuteNonQuery();
                successDelete = true;
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteVolumeBP()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a la promotoria, de acuerdo a un año y tipo de carga específicos.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public bool DeletePromotoria(int yearData, int chargeTypeData)
        {
            bool successDelete = false;
            try
            {
                string query = string.Format("DELETE [dbo].[Tbl_Promotoria] WHERE anio = {0} AND cve_TipoCarga = {1}", yearData, chargeTypeData);
                Open();
                SqlConnection connectionData = GetConnection();
                SqlCommand sqlcmd = new SqlCommand()
                {
                    Connection = connectionData,
                    CommandType = CommandType.Text,
                    CommandText = query
                };
                sqlcmd.ExecuteNonQuery();
                successDelete = true;
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeletePromotoria()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a los porcentajes de acuerdo al año/tipo de ejercicio.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="exerciseType">Tipo de ejercicio que se está realizando (BP/Rolling)</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public bool DeleteChannelPercentages(int yearData, string exerciseType)
        {
            bool successDelete = false;
            try
            {
                string query = string.Format("DELETE [dbo].[Tbl_Asignacion_Canales_Porcentaje] WHERE anio = {0} AND tipoEjercicio = '{1}'", yearData, exerciseType);
                Open();
                SqlConnection connectionData = GetConnection();
                SqlCommand sqlcmd = new SqlCommand()
                {
                    Connection = connectionData,
                    CommandType = CommandType.Text,
                    CommandText = query
                };
                sqlcmd.ExecuteNonQuery();
                successDelete = true;
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteChannelPercentages()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
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
                    sqlcmd.Connection = Connection;
                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = "DELETE [dbo].[Tbl_Presupuesto_Ajuste] WHERE anio = @anio AND cve_TipoCarga = @tipo_carga AND cve_Colaborador = @collaborator AND cve_Area = @area";
                    sqlcmd.Parameters.AddWithValue("@anio", accountsData.YearAccounts);
                    sqlcmd.Parameters.AddWithValue("@tipo_carga", accountsData.ChargeTypeAccounts);
                    sqlcmd.Parameters.AddWithValue("@collaborator", accountsData.Collaborator);
                    sqlcmd.Parameters.AddWithValue("@area", accountsData.Area);
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
        /// Método utilizado para eliminar la relación entre un usuario y la(s) área(s) asociadas a este.
        /// </summary>
        /// <param name="userId">Id asociado al usuario.</param>
        /// <param name="areaId">Id asociado al área.</param>
        /// <returns>Devuelve una bandera para determinar si la eliminación de la información fue correcta o no.</returns>
        public bool DeleteUserAreas(int? userId = null, int? areaId = null)
        {
            bool successDelete = false;
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = Connection;
                sqlcmd.CommandType = CommandType.Text;
                StringBuilder query = new StringBuilder();
                query.Append(" DELETE [dbo].[Cat_ColaboradorAreas] WHERE 1 = 1 ");
                if (userId.HasValue && userId.Value > 0)
                {
                    query.Append(" AND cve_Colaborador = @collaboratorId ");
                    sqlcmd.Parameters.AddWithValue("@collaboratorId", userId);
                }

                if (areaId.HasValue && areaId.Value > 0)
                {
                    query.Append(" AND cve_Area = @areaId ");
                    sqlcmd.Parameters.AddWithValue("@areaId", areaId.Value);
                }

                sqlcmd.CommandText = query.ToString();
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
        /// Método utilizado para eliminar la información general asociada a un área.
        /// </summary>
        /// <param name="areaId">Id asociado al área.</param>
        /// <returns>Devuelve una bandera para determinar si la información se eliminó correctamente.</returns>
        public bool DeleteAreaInformation(int areaId)
        {
            bool successDelete = false;
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = Connection;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = "DELETE [dbo].[Cat_Areas] WHERE cve_Area = @areaId ";
                sqlcmd.Parameters.AddWithValue("@areaId", areaId);
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
    }
}