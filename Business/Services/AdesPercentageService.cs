namespace Business.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using Data.DAO;
    using Data.Models;
    using Data.Models.Request;
    using Data.Repositories;

    /// <summary>
    /// Clase intermedia entre el acceso a datos y la capa del cliente para manipular la información asociada a los porcentajes de ADES.
    /// </summary>
    public class AdesPercentageService
    {
        /// <summary>
        /// Método utilizado para realizar el proceso que guarda los porcentajes asociados al portafolio de ADES.
        /// </summary>
        /// <param name="percentageData">Objeto tipo request que contiene la información para guardar los porcentajes.</param>
        /// <returns>Devuelve una bandera para determinar si todo el proceso fue satisfactorio o no.</returns>
        public static bool SaveAdesPercentage(BasePercentageRequest percentageData)
        {
            bool successProcess = false;
            try
            {
                if (percentageData != null)
                {
                    HttpPostedFileBase fileInfo = percentageData.FileData;

                    // Guardar la información del archivo que se está cargando.
                    FileLogData fileLogData = new FileLogData()
                    {
                        FileName = fileInfo.FileName,
                        ChargeDate = DateTime.Now,
                        ApprovalFlag = false,
                        FileTypeName = "Porcentajes Ades",
                        UserId = percentageData.Collaborator,
                        ChargeTypeId = percentageData.ChargeType,
                        YearData = percentageData.YearData,
                        DefaultArea = true,
                    };
                    int fileLogId = FileLogService.SaveFileLog(fileLogData);
                    if (fileLogId != 0)
                    {
                        string fileExtension = Path.GetExtension(fileInfo.FileName);
                        var percentagesTable = CommonService.ReadFile(fileInfo.InputStream, fileExtension);
                        if (percentagesTable.Tables != null && percentagesTable.Tables.Count > 0)
                        {
                            percentageData.FileLogId = fileLogId;

                            // Columnas - Unitario Convento.
                            List<string> conventCol = new List<string>()
                            {
                                "AdeS Convento", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                                "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
                            };

                            // Columnas - Asignación Frutal y Dairies.
                            List<string> frutalDairiesCol = new List<string>()
                            {
                                "Megagestion", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                                "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
                            };

                            // Columnas - Asignación Canal.
                            List<string> channelCol = ChannelPercentageService.ChannelColumnsFile;

                            // Guardar los porcentajes, dependiendo de la estructura de las hojas.
                            for (int i = 0; i < percentagesTable.Tables.Count; i++)
                            {
                                var singlePercentageTbl = percentagesTable.Tables[i];
                                var tblColumns = CommonService.ClearDataTableStructure(ref singlePercentageTbl);
                                percentageData.PercentagesTable = singlePercentageTbl;
                                if (tblColumns.All(str => frutalDairiesCol.Contains(str.ColumnName)))
                                {
                                    successProcess = SaveDairiesFrutalPercentage(percentageData); // Guardar el porcentaje Asignación Frutal y Dairies.
                                }
                                else if (tblColumns.All(str => conventCol.Contains(str.ColumnName)))
                                {
                                    successProcess = SaveAdesConventPercentage(percentageData); // Guardar el porcentaje Unitario Convento.
                                }
                                else if (tblColumns.All(str => channelCol.Contains(str.ColumnName)))
                                {
                                    successProcess = ChannelPercentageService.SaveChannelPercentage(percentageData); // Guardar el porcentaje Asignación Canal.
                                }
                                else
                                {
                                    successProcess = true;
                                }

                                if (!successProcess)
                                {
                                    break;
                                }
                            }

                            // Actualizar la tabla de hechos de la proyección para este año y tipo de carga.
                            if (successProcess)
                            {
                                string exerciseType = CommonService.GetExerciseType(percentageData.ChargeTypeName);
                                successProcess = LogProjectionService.SaveOrUpdateLogProjection(percentageData.YearData, percentageData.ChargeType, exerciseType);
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveAdesPercentage()." + "Error: " + ex.Message);
            }

            return successProcess;
        }

        /// <summary>
        /// Método utilizado para guardar los porcentajes para distribuir "Ades Dairies y Ades Frutal".
        /// </summary>
        /// <param name="percentageData">Objeto tipo request que contiene la información para guardar los porcentajes.</param>
        /// <returns>Devuelve una bandera para determinar si la información se guardó correctamente o no.</returns>
        public static bool SaveDairiesFrutalPercentage(BasePercentageRequest percentageData)
        {
            bool successInsert = false;
            try
            {
                AdesPercentageDAO adesPercentageDao = new AdesPercentageDAO();
                PivotTableRequest pivotTbl = new PivotTableRequest()
                {
                    ColumnsPivot = new List<string> { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" },
                    NewColumnName = "Porcentaje",
                    IncludeMonth = true
                };
                List<PivotTableRequest> pivotRequest = new List<PivotTableRequest>() { pivotTbl };
                percentageData.PercentagesTable = CommonService.UnpivotDataTable(percentageData.PercentagesTable, pivotRequest);
                successInsert = adesPercentageDao.SaveDairiesFrutalPercentage(percentageData);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertBasePercentage()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para guardar la información del costo unitario de Ades Convento.
        /// </summary>
        /// <param name="percentageData">Objeto tipo request que contiene la información para guardar los porcentajes.</param>
        /// <returns>Devuelve una bandera para determinar si la información se guardó correctamente o no.</returns>
        public static bool SaveAdesConventPercentage(BasePercentageRequest percentageData)
        {
            bool successInsert = false;
            try
            {
                AdesPercentageDAO adesPercentageDao = new AdesPercentageDAO();
                PivotTableRequest pivotTbl = new PivotTableRequest()
                {
                    ColumnsPivot = new List<string> { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" },
                    NewColumnName = "UnitarioConvento",
                    IncludeMonth = true
                };
                List<PivotTableRequest> pivotRequest = new List<PivotTableRequest>() { pivotTbl };
                percentageData.PercentagesTable = CommonService.UnpivotDataTable(percentageData.PercentagesTable, pivotRequest);
                successInsert = adesPercentageDao.SaveAdesConventPercentage(percentageData);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveAdesConventPercentage()." + "Error: " + ex.Message);
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
        public static bool DeleteDairiesFrutalPercentage(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                AdesPercentageDAO adesPercentageDao = new AdesPercentageDAO();
                successDelete = adesPercentageDao.DeleteDairiesFrutalPercentage(yearData, chargeTypeData, fileLogId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteDairiesFrutalPercentage()." + "Error: " + ex.Message);
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
        public static bool DeleteAdesConventPercentage(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                AdesPercentageDAO adesPercentageDao = new AdesPercentageDAO();
                successDelete = adesPercentageDao.DeleteAdesConventPercentage(yearData, chargeTypeData, fileLogId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteAdesConventPercentage()." + "Error: " + ex.Message);
            }

            return successDelete;
        }
    }
}