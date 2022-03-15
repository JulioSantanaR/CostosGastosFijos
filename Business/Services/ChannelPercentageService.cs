namespace Business.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Data.DAO;
    using Data.Models.Request;
    using Data.Repositories;

    /// <summary>
    /// Clase intermedia entre el acceso a datos y la capa del cliente para manipular la información asociada a los porcentajes de asignación por canal.
    /// </summary>
    public static class ChannelPercentageService
    {
        /// <summary>
        /// Columnas asociadas al archivo que se carga manualmente para guardar los porcentajes de asignación por canal.
        /// </summary>
        public static List<string> ChannelColumnsFile = new List<string>()
        {
            "Megagestion", "Canal", "Filtro", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
            "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
        };

        /// <summary>
        /// Método utilizado para insertar los porcentajes de acuerdo al año.
        /// </summary>
        /// <param name="yearAccounts">Año de carga.</param>
        /// <param name="chargeTypeAccounts">Tipo de carga.</param>
        /// <param name="exerciseType">Tipo de ejercicio que se está realizando (BP/Rolling).</param>
        /// <returns>Devuelve una bandera para determinar si la información se insertó correctamente o no.</returns>
        public static bool InsertChannelPercentages(int yearAccounts, int chargeTypeAccounts, string exerciseType = "Rolling")
        {
            bool successInsert = false;
            try
            {
                ChannelPercentageDAO channelPercentageDao = new ChannelPercentageDAO();
                successInsert = channelPercentageDao.InsertChannelPercentages(yearAccounts, chargeTypeAccounts, exerciseType);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("InsertChannelPercentages()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a los porcentajes de acuerdo al año/tipo de ejercicio.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="exerciseType">Tipo de ejercicio que se está realizando (BP/Rolling)</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public static bool DeleteChannelPercentages(int yearData, string exerciseType)
        {
            bool successDelete = false;
            try
            {
                ChannelPercentageDAO channelPercentageDao = new ChannelPercentageDAO();
                successDelete = channelPercentageDao.DeleteChannelPercentages(yearData, exerciseType);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteChannelPercentages()." + "Error: " + ex.Message);
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para guardar los porcentajes de asignación "Canal".
        /// </summary>
        /// <param name="percentageData">Objeto tipo request que contiene la información para guardar los porcentajes.</param>
        /// <returns>Devuelve una bandera para determinar si la información se guardó correctamente o no.</returns>
        public static bool SaveChannelPercentage(BasePercentageRequest percentageData)
        {
            bool successProcess = false;
            try
            {
                if (percentageData != null)
                {
                    var baseChannelTbl = percentageData.PercentagesTable;
                    int yearData = percentageData.YearData;
                    int chargeTypeId = percentageData.ChargeType;
                    int fileLogId = percentageData.FileLogId;
                    string portafolio = percentageData.Portafolio;

                    // Guardar la información de los porcentajes base para asignación por canal.
                    successProcess = BulkInsertBaseChannel(baseChannelTbl, yearData, chargeTypeId, fileLogId);

                    // Realizar el cálculo de los porcentajes por canal/portafolio.
                    if (successProcess)
                    {
                        string chargeTypeName = CommonService.GetExerciseType(percentageData.ChargeTypeName);
                        successProcess = SaveManualChannelPercentage(yearData, chargeTypeId, chargeTypeName, fileLogId, portafolio);
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveChannelPercentage()." + "Error: " + ex.Message);
            }

            return successProcess;
        }

        /// <summary>
        /// Método utilizado para guardar la información de los porcentajes base para los porcentajes de asignación por canal.
        /// </summary>
        /// <param name="baseChannelTbl">Objeto que contiene la información de los porcentajes base.</param>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public static bool BulkInsertBaseChannel(DataTable baseChannelTbl, int yearData, int chargeTypeData, int fileLogId)
        {
            bool successInsert = false;
            try
            {
                ChannelPercentageDAO channelPercentageDAO = new ChannelPercentageDAO();
                PivotTableRequest pivotTbl = new PivotTableRequest()
                {
                    ColumnsPivot = new List<string> { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" },
                    NewColumnName = "Porcentaje",
                    IncludeMonth = true
                };
                List<PivotTableRequest> pivotRequest = new List<PivotTableRequest>() { pivotTbl };
                baseChannelTbl = CommonService.UnpivotDataTable(baseChannelTbl, pivotRequest);
                successInsert = channelPercentageDAO.BulkInsertBaseChannel(baseChannelTbl, yearData, chargeTypeData, fileLogId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertBaseChannel()." + "Error: " + ex.Message);
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
        public static bool SaveManualChannelPercentage(int yearData, int chargeTypeData, string chargeTypeName, int fileLogId, string megagestion)
        {
            bool successInsert = false;
            try
            {
                ChannelPercentageDAO channelPercentageDAO = new ChannelPercentageDAO();
                successInsert = channelPercentageDAO.SaveManualChannelPercentage(yearData, chargeTypeData, chargeTypeName, fileLogId, megagestion);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveManualChannelPercentage()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a los porcentajes base para la asignación por canal.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public static bool DeleteBasePercentageChannel(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                ChannelPercentageDAO channelPercentageDAO = new ChannelPercentageDAO();
                successDelete = channelPercentageDAO.DeleteBasePercentageChannel(yearData, chargeTypeData, fileLogId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteBasePercentageChannel()." + "Error: " + ex.Message);
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
        public static bool DeleteManualPercentageChannel(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                ChannelPercentageDAO channelPercentageDAO = new ChannelPercentageDAO();
                successDelete = channelPercentageDAO.DeleteManualPercentageChannel(yearData, chargeTypeData, fileLogId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteManualPercentageChannel()." + "Error: " + ex.Message);
            }

            return successDelete;
        }
    }
}
