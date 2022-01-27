namespace Business
{
    using System;
    using System.Data;
    using Data;
    using Data.Repositories;

    /// <summary>
    /// Clase de negocio intermedia entre el acceso a datos y la capa del cliente, para operaciones sobre el cubo de información.
    /// </summary>
    public static class CubeService
    {
        /// <summary>
        /// Método utilizado para actualizar el cubo de información asociado a los costos/gastos fijos.
        /// </summary>
        /// <param name="jobName">Nombre asociado a la tarea programada del cubo (job).</param>
        /// <param name="jobId">Id asociado a la tarea programada del cubo (job).</param>
        public static void UpdateCube(string jobName, string jobId)
        {
            try
            {
                string runDate = DateTime.Now.ToString("yyyyMMdd");
                string runTime = DateTime.Now.AddSeconds(-10).ToString("HHmmss");
                DataTable dataTable = new DataTable();
                CubeDAO cubeDao = new CubeDAO();
                bool cubeSuccessUpdate = cubeDao.CubeUpdateProcess(jobName);
                if (cubeSuccessUpdate)
                {
                    do
                    {
                        dataTable = cubeDao.JobExecutionLog(runDate, runTime, jobId);
                    } while (dataTable == null || dataTable.Rows.Count == 0);
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateCube()." + "Error: " + ex.Message);
            }
        }
    }
}