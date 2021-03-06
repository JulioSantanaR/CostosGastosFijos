namespace Business.Services
{
    using System;
    using System.Collections.Generic;
    using Data.DAO;
    using Data.Models;
    using Data.Repositories;

    /// <summary>
    /// Clase intermedia entre el acceso a datos y la capa del cliente para manipular la información asociada al log auxiliar en la actualización de las proyecciones.
    /// </summary>
    public static class LogProjectionService
    {
        /// <summary>
        /// Columnas asociadas a la tabla "Tbl_Log_FactProyeccion".
        /// </summary>
        public static string[] ProjectionFields = new string[] { "cve_LogFactId", "tipoCarga", "cve_TipoCarga", "anio", "estatus", "fechaActualizacion" };

        /// <summary>
        /// Nombre asociado a la tabla "Tbl_Log_FactProyeccion".
        /// </summary>
        public static string ProjectionTable = "[dbo].[Tbl_Log_FactProyeccion]";

        /// <summary>
        /// Método utilizado para guardar o actualizar la información asociada al log de la tabla de hechos de la proyección.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeId">Id asociado al tipo de carga.</param>
        /// <param name="chargeType">Nombre del tipo de carga.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue guardada o actualizada correctamente.</returns>
        public static bool SaveOrUpdateLogProjection(int yearData, int chargeTypeId, string chargeType)
        {
            bool successProcess = false;
            try
            {
                LogFactData logProjection = GetLogProjection(chargeTypeId, yearData);
                if (logProjection != null)
                {
                    logProjection.DateActualization = DateTime.Now;
                    logProjection.ProjectionStatus = false;
                    successProcess = UpdateLogProjection(logProjection);
                }
                else
                {
                    logProjection = new LogFactData()
                    {
                        ChargeType = chargeType,
                        ChargeTypeId = chargeTypeId,
                        YearData = yearData,
                        DateActualization = DateTime.Now,
                        ProjectionStatus = false
                    };
                    int projectionId = SaveLogProjection(logProjection);
                    successProcess = projectionId != 0;
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            return successProcess;
        }

        /// <summary>
        /// Método utilizado para recuperar la información de un registro dentro del log asociado a la tabla de hechos de la proyección.
        /// </summary>
        /// <param name="chargeTypeId">Id asociado al tipo de carga.</param>
        /// <param name="yearData">Año de carga.</param>
        /// <returns>Devuelve la información del log asociado a la tabla de hechos, de acuerdo a los parámetros de búsqueda.</returns>
        public static LogFactData GetLogProjection(int? chargeTypeId, int? yearData)
        {
            LogFactData logProjection = null;
            try
            {
                LogFactDAO logFactDao = ProjectionDaoInit();
                logProjection = logFactDao.GetLogFact(chargeTypeId, yearData);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetLogProjection()." + "Error: " + ex.Message);
            }

            return logProjection;
        }

        /// <summary>
        /// Método utilizado para recuperar la lista de proyecciones o ejercicios que no han sido actualizados en la aplicación.
        /// </summary>
        /// <returns>Devuelve la lista de proyecciones pendientes de actualizar.</returns>
        public static List<LogFactData> GetPendingProjections()
        {
            List<LogFactData> pendingProjections = new List<LogFactData>();
            try
            {
                LogFactDAO logFactDao = ProjectionDaoInit();
                pendingProjections = logFactDao.GetPendingFact();
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetPendingProjections()." + "Error: " + ex.Message);
            }

            return pendingProjections;
        }

        /// <summary>
        /// Método utilizado para guardar la información asociada al log de la tabla de hechos de la proyección.
        /// </summary>
        /// <param name="logProjection">Objeto que contiene la información del log asociado a la tabla de hechos de la proyección.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue guardada correctamente.</returns>
        public static int SaveLogProjection(LogFactData logProjection)
        {
            int logProjectionId = 0;
            try
            {
                LogFactDAO logFactDao = ProjectionDaoInit();
                logProjectionId = logFactDao.SaveLogFact(logProjection);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveLogProjection()." + "Error: " + ex.Message);
            }

            return logProjectionId;
        }

        /// <summary>
        /// Método utilizado para actualizar la información asociada al log de la tabla de hechos de la proyección.
        /// </summary>
        /// <param name="logProjection">Objeto que contiene la información del log asociado a la tabla de hechos de la proyección.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue actualizada correctamente.</returns>
        public static bool UpdateLogProjection(LogFactData logProjection)
        {
            bool successUpdate = false;
            try
            {
                LogFactDAO logFactDao = ProjectionDaoInit();
                successUpdate = logFactDao.UpdateLogFact(logProjection);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateLogProjection()." + "Error: " + ex.Message);
            }

            return successUpdate;
        }

        /// <summary>
        /// Método auxiliar en la inicialización de la clase DAO asociada al log de cada ejercicio/año.
        /// </summary>
        /// <returns>Devuelve el objeto DAO del log que guarda cada ejercicio/año para acceder a la Base de Datos.</returns>
        private static LogFactDAO ProjectionDaoInit()
        {
            LogFactDAO projectionDAO = new LogFactDAO()
            {
                LogTableFields = ProjectionFields,
                LogTableName = ProjectionTable,
            };
            return projectionDAO;
        }
    }
}