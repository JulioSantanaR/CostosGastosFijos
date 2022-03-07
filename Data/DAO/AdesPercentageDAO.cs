namespace Data.DAO
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;
    using Data.Models.Request;
    using Data.Repositories;

    /// <summary>
    /// Clase asociada al acceso a datos para manipular la información asociada a los porcentajes de Ades.
    /// </summary>
    public class AdesPercentageDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public AdesPercentageDAO()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Método utilizado para guardar los porcentajes para distribuir "Ades Dairies y Ades Frutal".
        /// </summary>
        /// <param name="percentageData">Objeto tipo request que contiene la información para guardar los porcentajes.</param>
        /// <returns>Devuelve una bandera para determinar si la información se guardó correctamente o no.</returns>
        public bool SaveDairiesFrutalPercentage(BasePercentageRequest percentageData)
        {
            bool successInsert = false;
            try
            {
                if (percentageData != null)
                {
                    Open();
                    SqlConnection connectionData = GetConnection();
                    SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connectionData)
                    {
                        DestinationTableName = "[dbo].[Tbl_Porcentaje_FrutalDairies]",
                        BulkCopyTimeout = 400
                    };

                    // Agregar columnas pendientes.
                    DataTableAddColumn(percentageData.PercentagesTable, "anio", percentageData.YearData);
                    DataTableAddColumn(percentageData.PercentagesTable, "cve_TipoCarga", percentageData.ChargeType);
                    DataTableAddColumn(percentageData.PercentagesTable, "cve_LogArchivo", percentageData.FileLogId);

                    sqlBulkCopy.ColumnMappings.Add("Megagestion", "megagestion");
                    sqlBulkCopy.ColumnMappings.Add("Mes", "mes");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje", "porcentaje");
                    sqlBulkCopy.ColumnMappings.Add("anio", "anio");
                    sqlBulkCopy.ColumnMappings.Add("cve_TipoCarga", "cve_TipoCarga");
                    sqlBulkCopy.ColumnMappings.Add("cve_LogArchivo", "cve_LogArchivo");

                    sqlBulkCopy.WriteToServer(percentageData.PercentagesTable);
                    successInsert = true;
                }
            }
            catch (Exception ex)
            {
                successInsert = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveDairiesFrutalPercentage()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para guardar la información del costo unitario de Ades Convento.
        /// </summary>
        /// <param name="percentageData">Objeto tipo request que contiene la información para guardar los porcentajes.</param>
        /// <returns>Devuelve una bandera para determinar si la información se guardó correctamente o no.</returns>
        public bool SaveAdesConventPercentage(BasePercentageRequest percentageData)
        {
            bool successInsert = false;
            try
            {
                if (percentageData != null)
                {
                    Open();
                    SqlConnection connectionData = GetConnection();
                    SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connectionData)
                    {
                        DestinationTableName = "[dbo].[Tbl_Porcentaje_AdesConvento]",
                        BulkCopyTimeout = 400
                    };

                    // Agregar columnas pendientes.
                    DataTableAddColumn(percentageData.PercentagesTable, "anio", percentageData.YearData);
                    DataTableAddColumn(percentageData.PercentagesTable, "cve_TipoCarga", percentageData.ChargeType);
                    DataTableAddColumn(percentageData.PercentagesTable, "cve_LogArchivo", percentageData.FileLogId);

                    sqlBulkCopy.ColumnMappings.Add("AdeS Convento", "adesConvento");
                    sqlBulkCopy.ColumnMappings.Add("Mes", "mes");
                    sqlBulkCopy.ColumnMappings.Add("UnitarioConvento", "unitarioConvento");
                    sqlBulkCopy.ColumnMappings.Add("anio", "anio");
                    sqlBulkCopy.ColumnMappings.Add("cve_TipoCarga", "cve_TipoCarga");
                    sqlBulkCopy.ColumnMappings.Add("cve_LogArchivo", "cve_LogArchivo");

                    sqlBulkCopy.WriteToServer(percentageData.PercentagesTable);
                    successInsert = true;
                }
            }
            catch (Exception ex)
            {
                successInsert = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveAdesConventPercentage()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a los porcentajes para distribuir "Ades Dairies" y "Ades Frutal".
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public bool DeleteDairiesFrutalPercentage(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                SqlCommand sqlcmd = new SqlCommand();
                StringBuilder query = new StringBuilder();
                query.Append("DELETE [dbo].[Tbl_Porcentaje_FrutalDairies] WHERE 1 = 1 ");
                if (yearData.HasValue && yearData.Value > 0)
                {
                    query.Append(" AND anio = @yearData ");
                    sqlcmd.Parameters.AddWithValue("@yearData", yearData.Value);
                }

                if (chargeTypeData.HasValue && chargeTypeData.Value > 0)
                {
                    query.Append(" AND cve_TipoCarga = @chargeTypeData ");
                    sqlcmd.Parameters.AddWithValue("@chargeTypeData", chargeTypeData.Value);
                }

                if (fileLogId.HasValue && fileLogId.Value > 0)
                {
                    query.Append(" AND cve_LogArchivo = @fileLogId ");
                    sqlcmd.Parameters.AddWithValue("@fileLogId", fileLogId.Value);
                }

                Open();
                SqlConnection connectionData = GetConnection();
                sqlcmd.Connection = connectionData;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = query.ToString();
                sqlcmd.ExecuteNonQuery();
                successDelete = true;
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteDairiesFrutalPercentage()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar la información del costo unitario de Ades Convento.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public bool DeleteAdesConventPercentage(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                SqlCommand sqlcmd = new SqlCommand();
                StringBuilder query = new StringBuilder();
                query.Append("DELETE [dbo].[Tbl_Porcentaje_AdesConvento] WHERE 1 = 1 ");
                if (yearData.HasValue && yearData.Value > 0)
                {
                    query.Append(" AND anio = @yearData ");
                    sqlcmd.Parameters.AddWithValue("@yearData", yearData.Value);
                }

                if (chargeTypeData.HasValue && chargeTypeData.Value > 0)
                {
                    query.Append(" AND cve_TipoCarga = @chargeTypeData ");
                    sqlcmd.Parameters.AddWithValue("@chargeTypeData", chargeTypeData.Value);
                }

                if (fileLogId.HasValue && fileLogId.Value > 0)
                {
                    query.Append(" AND cve_LogArchivo = @fileLogId ");
                    sqlcmd.Parameters.AddWithValue("@fileLogId", fileLogId.Value);
                }

                Open();
                SqlConnection connectionData = GetConnection();
                sqlcmd.Connection = connectionData;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = query.ToString();
                sqlcmd.ExecuteNonQuery();
                successDelete = true;
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteAdesConventPercentage()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successDelete;
        }
    }
}