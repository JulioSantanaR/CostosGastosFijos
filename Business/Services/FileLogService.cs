namespace Business.Services
{
    using System;
    using Data.DAO;
    using Data.Models;
    using Data.Models.Request;
    using Data.Models.Response;
    using Data.Repositories;

    /// <summary>
    /// Clase intermedia entre el acceso a datos y la capa del cliente para manipular la información asociada al historial de cargas.
    /// </summary>
    public static class FileLogService
    {
        /// <summary>
        /// Método utilizado para recuperar la información que será mostrada en la tabla que contiene el historial de archivos.
        /// </summary>
        /// <param name="dataTableInfo">Objeto que contiene los parámetros de búsqueda para la tabla de usuarios.</param>
        /// <returns>Devuelve un objeto que contiene la información necesaria para mostrar la tabla del historial de archivos.</returns>
        public static FileLogTableResponse GetFileLogTable(DataTableRequest dataTableInfo)
        {
            FileLogTableResponse fileLogTable = null;
            try
            {
                FileLogDAO fileLogDao = new FileLogDAO();
                fileLogTable = fileLogDao.GetFileLogTable(dataTableInfo);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetFileLogTable()." + "Error: " + ex.Message);
            }

            return fileLogTable;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a un archivo cargado en el historial.
        /// </summary>
        /// <param name="deleteFileRequest">Objeto auxiliar en la eliminación de un archivo dentro del historial de cargas.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente o no.</returns>
        public static bool DeleteFileLog(DeleteFileRequest deleteFileRequest)
        {
            bool successDelete = false;
            bool deleteChannelPercent = false;
            try
            {
                if (deleteFileRequest != null)
                {
                    int logFileId = deleteFileRequest.LogFileId;
                    string logFileType = deleteFileRequest.LogFileType;
                    string chargeTypeName = CommonService.GetExerciseType(deleteFileRequest.ChargeTypeName);
                    AccountsDataRequest accountsData = new AccountsDataRequest() { FileLogId = logFileId };
                    switch (logFileType)
                    {
                        case "Volumen":
                            if (chargeTypeName == "BP")
                            {
                                successDelete = VolumeService.DeleteVolumeBP(deleteFileRequest.YearData, deleteFileRequest.ChargeTypeData);
                            }
                            else if (chargeTypeName == "Rolling")
                            {
                                successDelete = VolumeService.DeleteVolume(deleteFileRequest.YearData, deleteFileRequest.ChargeTypeData);
                            }

                            break;

                        case "Presupuesto":
                            successDelete = BudgetService.FactTableDeleteAccounts(accountsData);
                            if (successDelete)
                            {
                                successDelete = BudgetService.DeleteAccounts(accountsData);
                            }

                            break;
                        case "Ajuste manual":
                            successDelete = BudgetService.FactTableDeleteAccounts(accountsData);
                            if (successDelete)
                            {
                                successDelete = BudgetService.DeleteManualBudget(accountsData);
                            }

                            break;

                        case "Promotoria":
                            successDelete = PromotoriaService.DeletePromotoria(null, null, logFileId);
                            break;

                        case "Porcentajes Stills":
                            successDelete = MixBrandPercentageService.DeleteBrandMixPercentage(null, null, logFileId);
                            if (successDelete)
                            {
                                successDelete = StillsPercentageService.DeleteBasePercentage(null, null, logFileId);
                                if (successDelete)
                                {
                                    successDelete = StillsPercentageService.DeleteBottlerPercentage(null, null, logFileId);
                                    if (successDelete)
                                    {
                                        deleteChannelPercent = true;
                                    }
                                }
                            }

                            break;

                        case "Porcentajes Ades":
                            successDelete = AdesPercentageService.DeleteAdesConventPercentage(null, null, logFileId);
                            if (successDelete)
                            {
                                successDelete = AdesPercentageService.DeleteDairiesFrutalPercentage(null, null, logFileId);
                                if (successDelete)
                                {
                                    deleteChannelPercent = true;
                                }
                            }

                            break;

                        case "Porcentajes Lácteos":
                            successDelete = LacteosPercentagesService.DeleteSubcategoryBasePercentage(null, null, logFileId);
                            if (successDelete)
                            {
                                successDelete = LacteosPercentagesService.DeleteSubcategoryManualPercentage(null, null, logFileId);
                                if (successDelete)
                                {
                                    deleteChannelPercent = true;
                                }
                            }

                            break;
                    }

                    // Eliminar los porcentajes por canal.
                    if (successDelete && deleteChannelPercent)
                    {
                        successDelete = ChannelPercentageService.DeleteManualPercentageChannel(null, null, logFileId);
                        if (successDelete)
                        {
                            successDelete = ChannelPercentageService.DeleteBasePercentageChannel(null, null, logFileId);
                        }
                    }

                    // Eliminar la información general del archivo en el historial.
                    if (successDelete)
                    {
                        successDelete = DeleteFileLogById(logFileId);

                        // Actualizar la tabla de hechos de la proyección para este año y tipo de carga.
                        if (successDelete)
                        {
                            successDelete = LogProjectionService.SaveOrUpdateLogProjection(deleteFileRequest.YearData, deleteFileRequest.ChargeTypeData, chargeTypeName);
                        }

                        // Actualizar el log asociado a la tabla de hechos de la asignación por canal para este año y tipo de carga.
                        if (successDelete)
                        {
                            successDelete = LogChannelAssignService.SaveOrUpdateLogChannel(deleteFileRequest.YearData, deleteFileRequest.ChargeTypeData, chargeTypeName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteLogFile()." + "Error: " + ex.Message);
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a un archivo cargado en el historial.
        /// </summary>
        /// <param name="logFileId">Id asociado al archivo.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente o no.</returns>
        public static bool DeleteFileLogById(int logFileId)
        {
            bool successDelete = false;
            try
            {
                FileLogDAO fileLogDao = new FileLogDAO();
                successDelete = fileLogDao.DeleteFileLogById(logFileId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteFileLogById()." + "Error: " + ex.Message);
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para almacenar un registro en el historial de carga de archivos.
        /// </summary>
        /// <param name="fileLogData">Objeto que contiene la información general del archivo que se está cargando.</param>
        /// <returns>Devuelve el id asociado al historial de carga del archivo que recién fue insertado.</returns>
        public static int SaveFileLog(FileLogData fileLogData)
        {
            int fileLogId = 0;
            try
            {
                FileLogDAO fileLogDao = new FileLogDAO();
                fileLogId = fileLogDao.SaveFileLog(fileLogData);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveFileLog()." + "Error: " + ex.Message);
            }

            return fileLogId;
        }
    }
}