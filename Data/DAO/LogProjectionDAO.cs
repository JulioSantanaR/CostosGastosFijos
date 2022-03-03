namespace Data.DAO
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;
    using Data.Models;

    /// <summary>
    /// Clase asociada al acceso a datos para manipular la información asociada al log auxiliar en la actualización de las proyecciones.
    /// </summary>
    public class LogProjectionDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Columnas asociadas a la tabla "Tbl_Log_FactProyeccion".
        /// </summary>
        private readonly string[] ProjectionFields = new string[] { "cve_LogFactProyeccion", "tipoCarga", "cve_TipoCarga", "anio", "estatus", "fechaActualizacion" };

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public LogProjectionDAO()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Método utilizado para recuperar la información de un registro dentro del log asociado a la tabla de hechos de la proyección.
        /// </summary>
        /// <param name="chargeTypeId">Id asociado al tipo de carga.</param>
        /// <param name="yearData">Año de carga.</param>
        /// <returns>Devuelve la información del log asociado a la tabla de hechos, de acuerdo a los parámetros de búsqueda.</returns>
        public LogProjectionData GetLogProjection(int? chargeTypeId, int? yearData)
        {
            LogProjectionData logProjection = null;
            try
            {
                SqlCommand sqlcmd = new SqlCommand();
                StringBuilder query = new StringBuilder();
                query.Append("SELECT ").Append(string.Join(",", ProjectionFields));
                query.Append(" FROM [dbo].[Tbl_Log_FactProyeccion] ");
                query.Append(" WHERE 1 = 1 ");
                if (chargeTypeId.HasValue && chargeTypeId.Value > 0)
                {
                    query.Append(" AND cve_TipoCarga = @chargeTypeId ");
                    sqlcmd.Parameters.AddWithValue("@chargeTypeId", chargeTypeId.Value);
                }

                if (yearData.HasValue && yearData.Value > 0)
                {
                    query.Append(" AND anio = @yearData ");
                    sqlcmd.Parameters.AddWithValue("@yearData", yearData.Value);
                }

                Open();
                sqlcmd.Connection = Connection;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = query.ToString();
                var reader = sqlcmd.ExecuteReader();
                while (reader.Read())
                {
                    logProjection = Mapping.MapLogProjection(reader);
                }

                reader.Close();
                Close();
            }
            catch (Exception ex)
            {
                throw;
            }

            return logProjection;
        }

        /// <summary>
        /// Método utilizado para recuperar la lista de proyecciones o ejercicios que no han sido actualizados en la aplicación.
        /// </summary>
        /// <returns>Devuelve la lista de proyecciones pendientes de actualizar.</returns>
        public List<LogProjectionData> GetPendingProjections()
        {
            List<LogProjectionData> pendingProjections = new List<LogProjectionData>();
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT ").Append(string.Join(",", ProjectionFields));
                query.Append(" FROM [dbo].[Tbl_Log_FactProyeccion] ");
                query.Append(" WHERE estatus = 0 ");
                Open();
                SqlCommand sqlcmd = new SqlCommand
                {
                    Connection = Connection,
                    CommandType = CommandType.Text,
                    CommandText = query.ToString()
                };
                var reader = sqlcmd.ExecuteReader();
                while (reader.Read())
                {
                    LogProjectionData singleProjection = Mapping.MapLogProjection(reader);
                    pendingProjections.Add(singleProjection);
                }

                reader.Close();
                Close();
            }
            catch (Exception ex)
            {
                throw;
            }

            return pendingProjections;
        }

        /// <summary>
        /// Método utilizado para guardar la información asociada al log de la tabla de hechos de la proyección.
        /// </summary>
        /// <param name="logProjection">Objeto que contiene la información del log asociado a la tabla de hechos de la proyección.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue guardada correctamente.</returns>
        public int SaveLogProjection(LogProjectionData logProjection)
        {
            int logProjectionId = 0;
            try
            {
                if (logProjection != null)
                {
                    StringBuilder query = new StringBuilder();
                    query.Append("INSERT INTO [dbo].[Tbl_Log_FactProyeccion] VALUES (@chargeType, @chargeTypeId, @yearData, @status, @date); ");
                    query.Append(" SELECT SCOPE_IDENTITY(); ");
                    Open();
                    SqlCommand sqlcmd = new SqlCommand
                    {
                        Connection = Connection,
                        CommandType = CommandType.Text,
                        CommandText = query.ToString()
                    };
                    sqlcmd.Parameters.AddWithValue("@chargeType", logProjection.ChargeType);
                    sqlcmd.Parameters.AddWithValue("@chargeTypeId", logProjection.ChargeTypeId);
                    sqlcmd.Parameters.AddWithValue("@yearData", logProjection.YearData);
                    sqlcmd.Parameters.AddWithValue("@status", logProjection.ProjectionStatus);
                    sqlcmd.Parameters.AddWithValue("@date", logProjection.DateActualization);
                    logProjectionId = Convert.ToInt32(sqlcmd.ExecuteScalar());
                    Close();
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return logProjectionId;
        }

        /// <summary>
        /// Método utilizado para actualizar la información asociada al log de la tabla de hechos de la proyección.
        /// </summary>
        /// <param name="logProjection">Objeto que contiene la información del log asociado a la tabla de hechos de la proyección.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue actualizada correctamente.</returns>
        public bool UpdateLogProjection(LogProjectionData logProjection)
        {
            bool successUpdate = false;
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("UPDATE [dbo].[Tbl_Log_FactProyeccion] SET tipoCarga = @chargeType, cve_TipoCarga = @chargeTypeId, ");
                query.Append(" anio = @yearData, estatus = @status, fechaActualizacion = @date ");
                query.Append(" WHERE cve_LogFactProyeccion = @logProjectionId");
                Open();
                SqlCommand sqlcmd = new SqlCommand
                {
                    Connection = Connection,
                    CommandType = CommandType.Text,
                    CommandText = query.ToString()
                };
                sqlcmd.Parameters.AddWithValue("@chargeType", logProjection.ChargeType);
                sqlcmd.Parameters.AddWithValue("@chargeTypeId", logProjection.ChargeTypeId);
                sqlcmd.Parameters.AddWithValue("@yearData", logProjection.YearData);
                sqlcmd.Parameters.AddWithValue("@status", logProjection.ProjectionStatus);
                sqlcmd.Parameters.AddWithValue("@date", logProjection.DateActualization);
                sqlcmd.Parameters.AddWithValue("@logProjectionId", logProjection.LogFactProjectionId);
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
    }
}