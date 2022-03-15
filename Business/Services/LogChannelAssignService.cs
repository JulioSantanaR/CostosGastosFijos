namespace Business.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Data.DAO;
    using Data.Models;
    using Data.Repositories;

    /// <summary>
    /// Clase intermedia entre el acceso a datos y la capa del cliente para manipular la información asociada al log auxiliar en la actualización de la asignación por canal.
    /// </summary>
    public class LogChannelAssignService
    {
        /// <summary>
        /// Columnas asociadas a la tabla "Tbl_Log_FactCanalAsignacion".
        /// </summary>
        public static string[] ChannelAssignFields = new string[] { "cve_LogFactId", "tipoCarga", "cve_TipoCarga", "anio", "estatus", "fechaActualizacion" };

        /// <summary>
        /// Nombre asociado a la tabla "Tbl_Log_FactCanalAsignacion".
        /// </summary>
        public static string ChannelAssignTable = "[dbo].[Tbl_Log_FactCanalAsignacion]";

        /// <summary>
        /// Método utilizado para guardar o actualizar la información asociada al log de la tabla de hechos de la asignación por canal.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeId">Id asociado al tipo de carga.</param>
        /// <param name="chargeType">Nombre del tipo de carga.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue guardada o actualizada correctamente.</returns>
        public static bool SaveOrUpdateLogChannel(int yearData, int chargeTypeId, string chargeType)
        {
            bool successProcess = false;
            try
            {
                LogFactData logProjection = GetLogChannelAssign(chargeTypeId, yearData);
                if (logProjection != null)
                {
                    logProjection.DateActualization = DateTime.Now;
                    logProjection.ProjectionStatus = false;
                    successProcess = UpdateLogChannelAssign(logProjection);
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
                    int projectionId = SaveLogChannelAssign(logProjection);
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
        /// Método utilizado para recuperar la información de un registro dentro del log asociado a la tabla de hechos de la asignación por canal.
        /// </summary>
        /// <param name="chargeTypeId">Id asociado al tipo de carga.</param>
        /// <param name="yearData">Año de carga.</param>
        /// <returns>Devuelve la información del log asociado a la tabla de hechos, de acuerdo a los parámetros de búsqueda.</returns>
        public static LogFactData GetLogChannelAssign(int? chargeTypeId, int? yearData)
        {
            LogFactData logProjection = null;
            try
            {
                LogFactDAO logFactDao = ChannelAssignDaoInit();
                logProjection = logFactDao.GetLogFact(chargeTypeId, yearData);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetLogChannelAssign()." + "Error: " + ex.Message);
            }

            return logProjection;
        }

        /// <summary>
        /// Método utilizado para recuperar la lista de ejercicios que no han sido actualizados en la aplicación.
        /// </summary>
        /// <returns>Devuelve la lista de ejercicios pendientes de actualizar.</returns>
        public static List<LogFactData> GetPendingChannelAssign()
        {
            List<LogFactData> pendingProjections = new List<LogFactData>();
            try
            {
                LogFactDAO logFactDao = ChannelAssignDaoInit();
                pendingProjections = logFactDao.GetPendingFact();
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetPendingChannelAssign()." + "Error: " + ex.Message);
            }

            return pendingProjections;
        }

        /// <summary>
        /// Método utilizado para guardar la información asociada al log de la tabla de hechos de la asignación por canal.
        /// </summary>
        /// <param name="logProjection">Objeto que contiene la información del log asociado a la tabla de hechos de la asignación por canal.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue guardada correctamente.</returns>
        public static int SaveLogChannelAssign(LogFactData logProjection)
        {
            int logProjectionId = 0;
            try
            {
                LogFactDAO logFactDao = ChannelAssignDaoInit();
                logProjectionId = logFactDao.SaveLogFact(logProjection);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveLogChannelAssign()." + "Error: " + ex.Message);
            }

            return logProjectionId;
        }

        /// <summary>
        /// Método utilizado para actualizar la información asociada al log de la tabla de hechos de la asignación por canal.
        /// </summary>
        /// <param name="logProjection">Objeto que contiene la información del log asociado a la tabla de hechos de la asignación por canal.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue actualizada correctamente.</returns>
        public static bool UpdateLogChannelAssign(LogFactData logProjection)
        {
            bool successUpdate = false;
            try
            {
                LogFactDAO logFactDao = ChannelAssignDaoInit();
                successUpdate = logFactDao.UpdateLogFact(logProjection);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateLogChannelAssign()." + "Error: " + ex.Message);
            }

            return successUpdate;
        }

        /// <summary>
        /// Método auxiliar en la inicialización de la clase DAO asociada al log de cada ejercicio/año.
        /// </summary>
        /// <returns>Devuelve el objeto DAO del log que guarda cada ejercicio/año para acceder a la Base de Datos.</returns>
        private static LogFactDAO ChannelAssignDaoInit()
        {
            LogFactDAO projectionDAO = new LogFactDAO()
            {
                LogTableFields = ChannelAssignFields,
                LogTableName = ChannelAssignTable,
            };
            return projectionDAO;
        }
    }
}