namespace Business.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Web;
    using Data.DAO;
    using Data.Models;
    using Data.Models.Request;
    using Data.Repositories;

    /// <summary>
    /// Clase intermedia entre el acceso a datos y la capa del cliente para manipular la información asociada a los porcentajes de Stills, usados en la proyección.
    /// </summary>
    public static class StillsPercentageService
    {
        /// <summary>
        /// Método utilizado para realizar el proceso que guarda los porcentajes base y los porcentajes por marca.
        /// </summary>
        /// <param name="percentageData">Objeto tipo request que contiene la información para guardar los porcentajes.</param>
        /// <returns>Devuelve una bandera para determinar si todo el proceso fue satisfactorio o no.</returns>
        public static bool SaveStillsPercentage(BasePercentageRequest percentageData)
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
                        FileTypeName = "Porcentajes Stills",
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

                            // Columnas - Asignación Marca - Volumen.
                            List<string> brandVolumeCol = new List<string>()
                            {
                                "Canal", "Criterio", "Marca", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                                "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
                            };

                            // Columnas - Asignación Embotellador.
                            List<string> bottlerCol = new List<string>()
                            {
                                "Canal", "Filtro", "Formato Cadena", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
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
                                if (tblColumns.All(str => brandVolumeCol.Contains(str.ColumnName)))
                                {
                                    successProcess = SaveBrandVolumePercentage(percentageData); // Guardar el porcentaje Marca-Volumen.
                                }
                                else if (tblColumns.All(str => bottlerCol.Contains(str.ColumnName)))
                                {
                                    successProcess = BulkInsertBottler(percentageData); // Guardar el porcentaje Asignación Embotellador.
                                }
                                else if (tblColumns.All(str => channelCol.Contains(str.ColumnName)))
                                {
                                    percentageData.Portafolio = "Stills";
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

                            string exerciseType = CommonService.GetExerciseType(percentageData.ChargeTypeName);

                            // Actualizar el log asociado a la tabla de hechos de la proyección para este año y tipo de carga.
                            if (successProcess)
                            {
                                successProcess = LogProjectionService.SaveOrUpdateLogProjection(percentageData.YearData, percentageData.ChargeType, exerciseType);
                            }

                            // Actualizar el log asociado a la tabla de hechos de la asignación por canal para este año y tipo de carga.
                            if (successProcess)
                            {
                                successProcess = LogChannelAssignService.SaveOrUpdateLogChannel(percentageData.YearData, percentageData.ChargeType, exerciseType);
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveStillsPercentage()." + "Error: " + ex.Message);
            }

            return successProcess;
        }

        /// <summary>
        /// Método utilizado para guardar los porcentajes de asignación "Volumen-Marca".
        /// </summary>
        /// <param name="percentageData">Objeto tipo request que contiene la información para guardar los porcentajes.</param>
        /// <returns>Devuelve una bandera para determinar si la información se guardó correctamente o no.</returns>
        public static bool SaveBrandVolumePercentage(BasePercentageRequest percentageData)
        {
            bool successProcess = false;
            try
            {
                if (percentageData != null)
                {
                    var percentageTbl = percentageData.PercentagesTable;
                    int yearData = percentageData.YearData;
                    int chargeTypeId = percentageData.ChargeType;
                    int fileLogId = percentageData.FileLogId;

                    // Guardar la información de los porcentajes base.
                    successProcess = BulkInsertBasePercentage(percentageData);

                    // Realizar el cálculo de los porcentajes por marca.
                    if (successProcess)
                    {
                        string chargeTypeName = CommonService.GetExerciseType(percentageData.ChargeTypeName);
                        successProcess = MixBrandPercentageService.SavePercentageByBrand(yearData, chargeTypeId, chargeTypeName, fileLogId);
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveBrandVolumePercentage()." + "Error: " + ex.Message);
            }

            return successProcess;
        }

        /// <summary>
        /// Método utilizado para guardar la información de los porcentajes base para Stills.
        /// </summary>
        /// <param name="percentageData">Objeto tipo request que contiene la información para guardar los porcentajes.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public static bool BulkInsertBasePercentage(BasePercentageRequest percentageData)
        {
            bool successInsert = false;
            try
            {
                StillsPercentageDAO stillsPercentageDao = new StillsPercentageDAO();
                PivotTableRequest pivotTbl = new PivotTableRequest()
                {
                    ColumnsPivot = new List<string> { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" },
                    NewColumnName = "Porcentaje",
                    IncludeMonth = true
                };
                List<PivotTableRequest> pivotRequest = new List<PivotTableRequest>() { pivotTbl };
                percentageData.PercentagesTable = CommonService.UnpivotDataTable(percentageData.PercentagesTable, pivotRequest);
                successInsert = stillsPercentageDao.BulkInsertBasePercentage(percentageData);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertBasePercentage()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para guardar la información asociada a los porcentajes para la asignación por embotellador.
        /// </summary>
        /// <param name="percentageData">Objeto tipo request que contiene la información para guardar los porcentajes.</param>
        /// <returns>Devuelve una bandera para determinar si los porcentajes se insertaron de manera correcta.</returns>
        public static bool BulkInsertBottler(BasePercentageRequest percentageData)
        {
            bool successInsert = false;
            try
            {
                if (percentageData != null)
                {
                    StillsPercentageDAO stillsPercentageDao = new StillsPercentageDAO();
                    PivotTableRequest pivotTbl = new PivotTableRequest()
                    {
                        ColumnsPivot = new List<string> { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" },
                        NewColumnName = "Porcentaje",
                        IncludeMonth = true
                    };
                    List<PivotTableRequest> pivotRequest = new List<PivotTableRequest>() { pivotTbl };
                    percentageData.PercentagesTable = CommonService.UnpivotDataTable(percentageData.PercentagesTable, pivotRequest);
                    successInsert = stillsPercentageDao.BulkInsertBottler(percentageData);
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertBottler()." + "Error: " + ex.Message);
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
        public static bool DeleteBasePercentage(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                StillsPercentageDAO stillsPercentageDao = new StillsPercentageDAO();
                successDelete = stillsPercentageDao.DeleteBasePercentage(yearData, chargeTypeData, fileLogId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteBasePercentage()." + "Error: " + ex.Message);
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
        public static bool DeleteBottlerPercentage(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                StillsPercentageDAO stillsPercentageDao = new StillsPercentageDAO();
                successDelete = stillsPercentageDao.DeleteBottlerPercentage(yearData, chargeTypeData, fileLogId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteBottlerPercentage()." + "Error: " + ex.Message);
            }

            return successDelete;
        }
    }
}