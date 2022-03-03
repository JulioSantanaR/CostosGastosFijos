namespace Data.DAO
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;
    using Data.Repositories;

    /// <summary>
    /// Clase asociada al acceso a datos para manipular la información asociada a los porcentajes de Stills, usados en la proyección.
    /// </summary>
    public class StillsPercentageDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public StillsPercentageDAO()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Método utilizado para guardar la información de los porcentajes base para Stills.
        /// </summary>
        /// <param name="basePercentageTbl">Objeto que contiene la información de los porcentajes base.</param>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public bool BulkInsertBasePercentage(DataTable basePercentageTbl, int yearData, int chargeTypeData, int fileLogId)
        {
            bool successInsert = false;
            try
            {
                Open();
                SqlConnection connectionData = GetConnection();
                SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connectionData)
                {
                    DestinationTableName = "[dbo].[Tbl_PorcentajeBase]",
                    BulkCopyTimeout = 400
                };

                // Agregar columnas pendientes.
                DataTableAddColumn(basePercentageTbl, "anio", yearData);
                DataTableAddColumn(basePercentageTbl, "cve_TipoCarga", chargeTypeData);
                DataTableAddColumn(basePercentageTbl, "cve_LogArchivo", fileLogId);

                // Mapear columnas en el archivo hacia la tabla.
                sqlBulkCopy.ColumnMappings.Add("Canal", "canal");
                sqlBulkCopy.ColumnMappings.Add("Criterio", "criterio");
                if (basePercentageTbl.Columns.Contains("marca"))
                {
                    sqlBulkCopy.ColumnMappings.Add("Marca", "marca");
                }
                else
                {
                    DataTableAddColumn(basePercentageTbl, "Marca", "");
                    sqlBulkCopy.ColumnMappings.Add("Marca", "marca");
                }

                sqlBulkCopy.ColumnMappings.Add("Mes", "mes");
                sqlBulkCopy.ColumnMappings.Add("Porcentaje", "porcentaje");
                sqlBulkCopy.ColumnMappings.Add("anio", "anio");
                sqlBulkCopy.ColumnMappings.Add("cve_TipoCarga", "cve_TipoCarga");
                sqlBulkCopy.ColumnMappings.Add("cve_LogArchivo", "cve_LogArchivo");

                sqlBulkCopy.WriteToServer(basePercentageTbl);
                successInsert = true;
            }
            catch (Exception ex)
            {
                successInsert = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertBasePercentage()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para guardar la información asociada a los porcentajes para la asignación por embotellador.
        /// </summary>
        /// <param name="bottlerPercentageTbl">Objeto que contiene la información de los porcentajes por embotellador.</param>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Devuelve una bandera para determinar si los porcentajes se insertaron de manera correcta.</returns>
        public bool BulkInsertBottler(DataTable bottlerPercentageTbl, int yearData, int chargeTypeData, int fileLogId)
        {
            bool successInsert = false;
            try
            {
                Open();
                SqlConnection connectionData = GetConnection();
                SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connectionData)
                {
                    DestinationTableName = "[dbo].[Tbl_PorcentajeMix_FormatoCadena]",
                    BulkCopyTimeout = 400
                };

                // Agregar columnas pendientes.
                DataTableAddColumn(bottlerPercentageTbl, "anio", yearData);
                DataTableAddColumn(bottlerPercentageTbl, "cve_TipoCarga", chargeTypeData);
                DataTableAddColumn(bottlerPercentageTbl, "cve_LogArchivo", fileLogId);

                // Mapear columnas en el archivo hacia la tabla.
                sqlBulkCopy.ColumnMappings.Add("Canal", "canal");
                sqlBulkCopy.ColumnMappings.Add("Filtro", "filtro");
                sqlBulkCopy.ColumnMappings.Add("Formato Cadena", "formatoCadena");
                sqlBulkCopy.ColumnMappings.Add("Mes", "mes");
                sqlBulkCopy.ColumnMappings.Add("Porcentaje", "porcentaje");
                sqlBulkCopy.ColumnMappings.Add("anio", "anio");
                sqlBulkCopy.ColumnMappings.Add("cve_TipoCarga", "cve_TipoCarga");
                sqlBulkCopy.ColumnMappings.Add("cve_LogArchivo", "cve_LogArchivo");

                sqlBulkCopy.WriteToServer(bottlerPercentageTbl);
                successInsert = true;
            }
            catch (Exception ex)
            {
                successInsert = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertBottler()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para guardar la información de los porcentajes base para los porcentajes de asignación por canal.
        /// </summary>
        /// <param name="baseChannelTbl">Objeto que contiene la información de los porcentajes base.</param>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public bool BulkInsertBaseChannel(DataTable baseChannelTbl, int yearData, int chargeTypeData, int fileLogId)
        {
            bool successInsert = false;
            try
            {
                Open();
                SqlConnection connectionData = GetConnection();
                SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connectionData)
                {
                    DestinationTableName = "[dbo].[Tbl_PorcentajeBase_Canal]",
                    BulkCopyTimeout = 400
                };

                // Agregar columnas pendientes.
                DataTableAddColumn(baseChannelTbl, "anio", yearData);
                DataTableAddColumn(baseChannelTbl, "cve_TipoCarga", chargeTypeData);
                DataTableAddColumn(baseChannelTbl, "cve_LogArchivo", fileLogId);

                // Mapear columnas en el archivo hacia la tabla.
                sqlBulkCopy.ColumnMappings.Add("Megagestión", "megagestion");
                sqlBulkCopy.ColumnMappings.Add("Canal", "canal");
                sqlBulkCopy.ColumnMappings.Add("Filtro", "filtro");
                sqlBulkCopy.ColumnMappings.Add("Mes", "mes");
                sqlBulkCopy.ColumnMappings.Add("Porcentaje", "porcentaje");
                sqlBulkCopy.ColumnMappings.Add("anio", "anio");
                sqlBulkCopy.ColumnMappings.Add("cve_TipoCarga", "cve_TipoCarga");
                sqlBulkCopy.ColumnMappings.Add("cve_LogArchivo", "cve_LogArchivo");

                sqlBulkCopy.WriteToServer(baseChannelTbl);
                successInsert = true;
            }
            catch (Exception ex)
            {
                successInsert = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertBaseChannel()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para guardar la información de los porcentajes por canal/portafolio según sea el caso.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="chargeTypeName">Nombre asociado al tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public bool SaveManualChannelPercentage(int yearData, int chargeTypeData, string chargeTypeName, int fileLogId, string megagestion)
        {
            bool successInsert;
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand("[dbo].[usp_InsertarPorcentaje_Canal_Manual]", Connection);
                sqlcmd.CommandType = CommandType.StoredProcedure;
                sqlcmd.Parameters.AddWithValue("@anio", yearData);
                sqlcmd.Parameters.AddWithValue("@tipo_carga", chargeTypeData);
                sqlcmd.Parameters.AddWithValue("@tipoEjercicio", chargeTypeName);
                sqlcmd.Parameters.AddWithValue("@logId", fileLogId);
                sqlcmd.Parameters.AddWithValue("@portafolio", megagestion);
                sqlcmd.CommandTimeout = 3600;
                sqlcmd.ExecuteNonQuery();
                Close();
                successInsert = true;
            }
            catch (Exception ex)
            {
                successInsert = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveManualChannelPercentage()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a los porcentajes base, de acuerdo a un año y tipo de carga específicos.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public bool DeleteBasePercentage(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                SqlCommand sqlcmd = new SqlCommand();
                StringBuilder query = new StringBuilder();
                query.Append("DELETE [dbo].[Tbl_PorcentajeBase] WHERE 1 = 1 ");
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
                generalRepository.WriteLog("DeleteBasePercentage()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a los porcentajes de asignación por embotellador.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public bool DeleteBottlerPercentage(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                SqlCommand sqlcmd = new SqlCommand();
                StringBuilder query = new StringBuilder();
                query.Append("DELETE [dbo].[Tbl_PorcentajeMix_FormatoCadena] WHERE 1 = 1 ");
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
                generalRepository.WriteLog("DeleteBottlerPercentage()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a los porcentajes base para la asignación por canal.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public bool DeleteBasePercentageChannel(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                SqlCommand sqlcmd = new SqlCommand();
                StringBuilder query = new StringBuilder();
                query.Append("DELETE [dbo].[Tbl_PorcentajeBase_Canal] WHERE 1 = 1 ");
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
                generalRepository.WriteLog("DeleteBasePercentageChannel()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a los porcentajes de asignación por canal.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public bool DeleteManualPercentageChannel(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                SqlCommand sqlcmd = new SqlCommand();
                StringBuilder query = new StringBuilder();
                query.Append("DELETE [dbo].[Tbl_Asignacion_Canales_Porcentaje] WHERE 1 = 1 ");
                if (yearData.HasValue && yearData.Value > 0)
                {
                    query.Append(" AND anio = @yearData ");
                    sqlcmd.Parameters.AddWithValue("@yearData", yearData.Value);
                }

                if (chargeTypeData.HasValue && chargeTypeData.Value > 0)
                {
                    query.Append(" AND tipoEjercicio = @chargeTypeData ");
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
                generalRepository.WriteLog("DeleteManualPercentageChannel()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successDelete;
        }
    }
}