namespace Business.Services
{
    using System;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Web;
    using Data.DAO;
    using Data.Models;
    using Data.Models.Request;
    using Data.Repositories;

    /// <summary>
    /// Clase intermedia entre el acceso a datos y la capa del cliente para manipular la información asociada al volumen.
    /// </summary>
    public static class VolumeService
    {
        /// <summary>
        /// Método utilizado para guardar la base asociada al volumen.
        /// </summary>
        /// <param name="volumeData">Objeto auxiliar en el guardado de la información del volumen.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public static bool VolumenInformation(VolumeDataRequest volumeData)
        {
            bool successProcess = false;
            try
            {
                if (volumeData != null)
                {
                    int yearData = volumeData.YearData;
                    int chargeTypeData = volumeData.ChargeType;
                    string chargeTypeName = volumeData.ChargeTypeName;
                    HttpPostedFileBase fileInfo = volumeData.FileData;
                    string fileExtension = Path.GetExtension(fileInfo.FileName);
                    var accountsTable = CommonService.ReadFile(fileInfo.InputStream, fileExtension);
                    if (accountsTable.Tables.Count > 0)
                    {
                        // Guardar la información del archivo que se está cargando.
                        FileLogData fileLogData = new FileLogData()
                        {
                            FileName = fileInfo.FileName,
                            ChargeDate = DateTime.Now,
                            ApprovalFlag = false,
                            FileTypeName = "Volumen",
                            UserId = volumeData.Collaborator,
                            ChargeTypeId = chargeTypeData,
                            YearData = yearData,
                            DefaultArea = true,
                        };
                        int fileLogId = FileLogService.SaveFileLog(fileLogData);
                        if (fileLogId != 0)
                        {
                            string exerciseType = CommonService.GetExerciseType(chargeTypeName);
                            successProcess = exerciseType == "BP" 
                                ? BulkInsertVolumenBP(accountsTable.Tables[0], yearData, chargeTypeData, fileLogId)
                                : BulkInsertVolumen(accountsTable.Tables[0], yearData, chargeTypeData, fileLogId);

                            // Actualizar el log asociado a la tabla de hechos de la proyección para este año y tipo de carga.
                            if (successProcess)
                            {
                                successProcess = LogProjectionService.SaveOrUpdateLogProjection(yearData, chargeTypeData, exerciseType);
                            }

                            // Actualizar el log asociado a la tabla de hechos de la asignación por canal para este año y tipo de carga.
                            if (successProcess)
                            {
                                successProcess = LogChannelAssignService.SaveOrUpdateLogChannel(yearData, chargeTypeData, exerciseType);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("VolumenInformation()." + "Error: " + ex.Message);
            }

            return successProcess;
        }

        /// <summary>
        /// Método utilizado para guardar la base asociada al volumen.
        /// </summary>
        /// <param name="volumeTable">Objeto que contiene la información de la base del volumen.</param>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public static bool BulkInsertVolumen(DataTable volumeTable, int yearData, int chargeTypeData, int fileLogId)
        {
            bool successInsert = false;
            try
            {
                if (volumeTable != null && volumeTable.Rows.Count > 0)
                {
                    int insertCommitSize = 5000;
                    int numberOfPages = (volumeTable.Rows.Count / insertCommitSize) + (volumeTable.Rows.Count % insertCommitSize == 0 ? 0 : 1);
                    for (int pageIndex = 0; pageIndex < numberOfPages; pageIndex++)
                    {
                        VolumeDAO volumeDao = new VolumeDAO();
                        DataTable auxDt = volumeTable.AsEnumerable().Skip(pageIndex * insertCommitSize).Take(insertCommitSize).CopyToDataTable();
                        successInsert = volumeDao.BulkInsertVolumen(auxDt, yearData, chargeTypeData, fileLogId);
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertVolumen()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para guardar la base asociada al volumen para un BP/Rolling 0+12.
        /// </summary>
        /// <param name="volumeTable">Objeto que contiene la información de la base del volumen.</param>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public static bool BulkInsertVolumenBP(DataTable volumeTable, int yearData, int chargeTypeData, int fileLogId)
        {
            bool successInsert = false;
            try
            {                

                if (volumeTable != null && volumeTable.Rows.Count > 0)
                {
                    int insertCommitSize = 5000;
                    int numberOfPages = (volumeTable.Rows.Count / insertCommitSize) + (volumeTable.Rows.Count % insertCommitSize == 0 ? 0 : 1);
                    for (int pageIndex = 0; pageIndex < numberOfPages; pageIndex++)
                    {
                        VolumeDAO volumeDao = new VolumeDAO();
                        DataTable auxDt = volumeTable.AsEnumerable().Skip(pageIndex * insertCommitSize).Take(insertCommitSize).CopyToDataTable();
                        successInsert = volumeDao.BulkInsertVolumenBP(auxDt, yearData, chargeTypeData, fileLogId);
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertVolumenBP()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada al volumen, de acuerdo a un año y tipo de carga específicos.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public static bool DeleteVolume(int yearData, int chargeTypeData)
        {
            bool successDelete = false;
            try
            {
                VolumeDAO volumeDao = new VolumeDAO();
                successDelete = volumeDao.DeleteVolume(yearData, chargeTypeData);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteVolume()." + "Error: " + ex.Message);
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada al volumen en un BP/Rolling 0+12.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public static bool DeleteVolumeBP(int yearData, int chargeTypeData)
        {
            bool successDelete = false;
            try
            {
                VolumeDAO volumeDao = new VolumeDAO();
                successDelete = volumeDao.DeleteVolumeBP(yearData, chargeTypeData);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteVolumeBP()." + "Error: " + ex.Message);
            }

            return successDelete;
        }
    }
}