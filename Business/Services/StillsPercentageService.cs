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
        public static bool SaveBasePercentage(BasePercentageRequest percentageData)
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
                            List<string> channelCol = new List<string>()
                            {
                                "Megagestión", "Canal", "Filtro", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
                                "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
                            };

                            // Guardar los porcentajes, dependiendo de la estructura de las hojas.
                            for (int i = 0; i < percentagesTable.Tables.Count; i++)
                            {
                                var singlePercentageTbl = percentagesTable.Tables[i];
                                singlePercentageTbl = CommonService.RemoveWhiteSpaces(singlePercentageTbl);
                                CommonService.RemoveNullColumns(ref singlePercentageTbl);
                                var tblColumns = singlePercentageTbl.Columns.Cast<DataColumn>().Select(dc => dc.ColumnName).ToList();
                                percentageData.PercentagesTable = singlePercentageTbl;
                                if (tblColumns.All(s => brandVolumeCol.Contains(s)))
                                {
                                    successProcess = SaveBrandVolumePercentage(percentageData); // Guardar el porcentaje Marca-Volumen.
                                }
                                else if (tblColumns.All(s => bottlerCol.Contains(s)))
                                {
                                    successProcess = BulkInsertBottler(percentageData); // Guardar el porcentaje Asignación Embotellador.
                                }
                                else if (tblColumns.All(s => channelCol.Contains(s)))
                                {
                                    successProcess = SaveChannelPercentage(percentageData); // Guardar el porcentaje Asignación Canal.
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
                                string chargeTypeName = percentageData.ChargeTypeName.ToLower();
                                bool bpExercise = chargeTypeName == "rolling 0+12" || chargeTypeName == "business plan";
                                string exerciseType = bpExercise ? "BP" : "Rolling";
                                successProcess = LogProjectionService.SaveOrUpdateLogProjection(percentageData.YearData, percentageData.ChargeType, exerciseType);
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveBasePercentage()." + "Error: " + ex.Message);
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
                    successProcess = BulkInsertBasePercentage(percentageTbl, yearData, chargeTypeId, fileLogId);

                    // Realizar el cálculo de los porcentajes por marca.
                    if (successProcess)
                    {
                        string chargeTypeName = percentageData.ChargeTypeName;
                        chargeTypeName = chargeTypeName.ToLower();
                        chargeTypeName = chargeTypeName == "rolling 0+12" || chargeTypeName == "business plan" ? "BP" : "Rolling";
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
        /// <param name="basePercentageTbl">Objeto que contiene la información de los porcentajes base.</param>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public static bool BulkInsertBasePercentage(DataTable basePercentageTbl, int yearData, int chargeTypeData, int fileLogId)
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
                basePercentageTbl = CommonService.UnpivotDataTable(basePercentageTbl, pivotRequest);
                successInsert = stillsPercentageDao.BulkInsertBasePercentage(basePercentageTbl, yearData, chargeTypeData, fileLogId);
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
                    var bottlerPercentageTbl = percentageData.PercentagesTable;
                    int yearData = percentageData.YearData;
                    int chargeTypeId = percentageData.ChargeType;
                    int fileLogId = percentageData.FileLogId;
                    StillsPercentageDAO stillsPercentageDao = new StillsPercentageDAO();
                    PivotTableRequest pivotTbl = new PivotTableRequest()
                    {
                        ColumnsPivot = new List<string> { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" },
                        NewColumnName = "Porcentaje",
                        IncludeMonth = true
                    };
                    List<PivotTableRequest> pivotRequest = new List<PivotTableRequest>() { pivotTbl };
                    bottlerPercentageTbl = CommonService.UnpivotDataTable(bottlerPercentageTbl, pivotRequest);
                    successInsert = stillsPercentageDao.BulkInsertBottler(bottlerPercentageTbl, yearData, chargeTypeId, fileLogId);
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

                    // Guardar la información de los porcentajes base para asignación por canal.
                    successProcess = BulkInsertBaseChannel(baseChannelTbl, yearData, chargeTypeId, fileLogId);

                    // Realizar el cálculo de los porcentajes por canal/portafolio.
                    if (successProcess)
                    {
                        string chargeTypeName = percentageData.ChargeTypeName;
                        chargeTypeName = chargeTypeName.ToLower();
                        chargeTypeName = chargeTypeName == "rolling 0+12" || chargeTypeName == "business plan" ? "BP" : "Rolling";
                        successProcess = SaveManualChannelPercentage(yearData, chargeTypeId, chargeTypeName, fileLogId, "Stills");
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
                StillsPercentageDAO stillsPercentageDao = new StillsPercentageDAO();
                PivotTableRequest pivotTbl = new PivotTableRequest()
                {
                    ColumnsPivot = new List<string> { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" },
                    NewColumnName = "Porcentaje",
                    IncludeMonth = true
                };
                List<PivotTableRequest> pivotRequest = new List<PivotTableRequest>() { pivotTbl };
                baseChannelTbl = CommonService.UnpivotDataTable(baseChannelTbl, pivotRequest);
                successInsert = stillsPercentageDao.BulkInsertBaseChannel(baseChannelTbl, yearData, chargeTypeData, fileLogId);
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
                StillsPercentageDAO stillsPercentageDao = new StillsPercentageDAO();
                successInsert = stillsPercentageDao.SaveManualChannelPercentage(yearData, chargeTypeData, chargeTypeName, fileLogId, megagestion);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveManualChannelPercentage()." + "Error: " + ex.Message);
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
                StillsPercentageDAO stillsPercentageDao = new StillsPercentageDAO();
                successDelete = stillsPercentageDao.DeleteBasePercentageChannel(yearData, chargeTypeData, fileLogId);
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
                StillsPercentageDAO stillsPercentageDao = new StillsPercentageDAO();
                successDelete = stillsPercentageDao.DeleteManualPercentageChannel(yearData, chargeTypeData, fileLogId);
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