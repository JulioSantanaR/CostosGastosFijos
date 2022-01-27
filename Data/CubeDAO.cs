namespace Data
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using Data.Repositories;

    /// <summary>
    /// Clase de acceso a datos utilizado para manipular la información del cubo de información dentro de la Base de Datos.
    /// </summary>
    public class CubeDAO
    {
        /// <summary>
        /// Método utilizado para actualizar el cubo de información de costos y gastos fijos.
        /// </summary>
        /// <param name="jobName">Nombre asociado a la tarea programada del cubo (job).</param>
        /// <returns>Devuelve una bandera para determinar si la ejecución del proceso fue correcta o no.</returns>
        public bool CubeUpdateProcess(string jobName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MSDB"].ConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand("sp_start_job ", connection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.Parameters.Add("@job_name", SqlDbType.VarChar).Value = jobName;
                        connection.Open();
                        sqlCommand.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("CubeUpdateProcess()." + "Error: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Método utilizado para obtener el log de ejecución del job asociado al cubo de información.
        /// </summary>
        /// <param name="runDate">Fecha en que se ejecuta la actualización del cubo.</param>
        /// <param name="runTime">Hora en que se ejecuta la actualización del cubo.</param>
        /// <param name="jobId">Id asociado a la tarea programada del cubo (job).</param>
        /// <returns>Devuelve la información del log del cubo de información de acuerdo a la fecha y hora enviada.</returns>
        public DataTable JobExecutionLog(string runDate, string runTime, string jobId)
        {
            DataTable dataTable = new DataTable();
            try
            {
                string query = @"SELECT MAX(RUN_DATE) AS Fecha, MAX(RUN_TIME) AS Tiempo,
                    STEP_NAME AS [Tarea en Ejecución],
                    CASE
                        WHEN run_status = 0 THEN 'Fallo Ejecución'
                        WHEN run_status = 1 THEN 'Ejecución Correcta'
                        WHEN run_status = 2 THEN 'Reintentando'
                        WHEN run_status = 3 THEN 'Fallo Ejecución'
                    ELSE 'Desconocido'
                    END AS Estatus
                    FROM sysjobhistory
                    WHERE job_id IN ('" + jobId + "') AND run_date = " + runDate + " AND run_time >= " + runTime + " GROUP BY run_status, step_name";
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MSDB"].ConnectionString))
                {
                    using (SqlCommand selectCommand = new SqlCommand(query, connection))
                    {
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommand);
                        connection.Open();
                        sqlDataAdapter.Fill(dataTable);
                        connection.Close();
                    }

                    return dataTable;
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("JobExecutionLog()." + "Error: " + ex.Message);
                return new DataTable();
            }
        }
    }
}