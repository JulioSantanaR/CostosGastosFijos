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
    /// Clase intermedia entre el acceso a datos y la capa del cliente para manipular la información asociada a los porcentajes de Lácteos, usados en la proyección.
    /// </summary>
    public class LacteosPercentagesService
    {
        /// <summary>
        /// Método utilizado para realizar el proceso que guarda los porcentajes asociados al portafolio de LÁCTEOS.
        /// </summary>
        /// <param name="percentageData">Objeto tipo request que contiene la información para guardar los porcentajes.</param>
        /// <returns>Devuelve una bandera para determinar si todo el proceso fue satisfactorio o no.</returns>
        public static bool SaveLacteosPercentage(BasePercentageRequest percentageData)
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
                        FileTypeName = "Porcentajes Lácteos",
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

                            // Columnas - Asignación Subcategoría.
                            List<string> subcategoryCol = new List<string>()
                            {
                                "Cadena Suministro", "Canal", "Subcategoria", "Asignacion", "Enero", "Febrero", "Marzo", "Abril",
                                "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
                            };

                            // Columnas - Asignación Canal.
                            List<string> channelCol = ChannelPercentageService.ChannelColumnsFile;

                            // Guardar los porcentajes, dependiendo de la estructura de las hojas.
                            for (int i = 0; i < percentagesTable.Tables.Count; i++)
                            {
                                var singlePercentageTbl = percentagesTable.Tables[i];
                                var tblColumns = CommonService.ClearDataTableStructure(ref singlePercentageTbl);
                                percentageData.PercentagesTable = singlePercentageTbl;
                                if (tblColumns.All(str => subcategoryCol.Contains(str.ColumnName)))
                                {
                                    successProcess = SaveSubcategoryPercentage(percentageData); // Guardar el porcentaje Asignación Subcategoría.
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
                generalRepository.WriteLog("SaveLacteosPercentage()." + "Error: " + ex.Message);
            }

            return successProcess;
        }

        /// <summary>
        /// Método utilizado para guardar la información relacionada con los porcentajes de cada subcategoría.
        /// </summary>
        /// <param name="percentageData">Objeto tipo request que contiene la información para guardar los porcentajes.</param>
        /// <returns>Devuelve una bandera para determinar si la información se guardó correctamente.</returns>
        public static bool SaveSubcategoryPercentage(BasePercentageRequest percentageData)
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
                    successProcess = InsertSubcategoryBasePercentage(percentageData);

                    // Guardar los porcentajes de cada subcategoría.
                    if (successProcess)
                    {
                        successProcess = SaveSubcategoryManualPercentage(percentageData);
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveSubcategoryPercentage()." + "Error: " + ex.Message);
            }

            return successProcess;
        }

        /// <summary>
        /// Método utilizado para insertar la información de los porcentajes base de cada subcategoría.
        /// </summary>
        /// <param name="percentageData">Objeto tipo request que contiene la información para guardar los porcentajes.</param>
        /// <returns>Devuelve una bandera para determinar si la información se guardó correctamente.</returns>
        public static bool InsertSubcategoryBasePercentage(BasePercentageRequest percentageData)
        {
            bool successInsert = false;
            try
            {
                LacteosPercentagesDAO lacteosPercentageDao = new LacteosPercentagesDAO();
                PivotTableRequest pivotTbl = new PivotTableRequest()
                {
                    ColumnsPivot = new List<string> { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" },
                    NewColumnName = "Porcentaje",
                    IncludeMonth = true
                };
                List<PivotTableRequest> pivotRequest = new List<PivotTableRequest>() { pivotTbl };
                percentageData.PercentagesTable = CommonService.UnpivotDataTable(percentageData.PercentagesTable, pivotRequest);
                successInsert = lacteosPercentageDao.InsertSubcategoryBasePercentage(percentageData);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("InsertSubcategoryBasePercentage()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para ejecutar el stored procedure que guarda los porcentajes de cada subcategoría en el formato correspondiente.
        /// </summary>
        /// <param name="percentageData">Objeto tipo request que contiene la información para guardar los porcentajes.</param>
        /// <returns>Devuelve una bandera para determinar si la información se guardó correctamente.</returns>
        public static bool SaveSubcategoryManualPercentage(BasePercentageRequest percentageData)
        {
            bool successInsert = false;
            try
            {
                int yearData = percentageData.YearData;
                int chargeTypeId = percentageData.ChargeType;
                int fileLogId = percentageData.FileLogId;

                LacteosPercentagesDAO lacteosPercentageDao = new LacteosPercentagesDAO();
                successInsert = lacteosPercentageDao.SaveSubcategoryManualPercentage(yearData, chargeTypeId, fileLogId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveSubcategoryManualPercentage()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para eliminar la información de los porcentajes base de cada subcategoría.
        /// </summary>
        /// <param name="percentageData">Objeto tipo request que contiene la información para guardar los porcentajes.</param>
        /// <returns>Devuelve una bandera para determinar si la información se fue eliminada correctamente.</returns>
        public static bool DeleteSubcategoryBasePercentage(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                LacteosPercentagesDAO lacteosPercentageDao = new LacteosPercentagesDAO();
                successDelete = lacteosPercentageDao.DeleteSubcategoryBasePercentage(yearData, chargeTypeData, fileLogId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteSubcategoryBasePercentage()." + "Error: " + ex.Message);
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar la información de los porcentajes asociados a cada subcategoría.
        /// </summary>
        /// <param name="percentageData">Objeto tipo request que contiene la información para guardar los porcentajes.</param>
        /// <returns>Devuelve una bandera para determinar si la información se fue eliminada correctamente.</returns>
        public static bool DeleteSubcategoryManualPercentage(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                LacteosPercentagesDAO lacteosPercentageDao = new LacteosPercentagesDAO();
                successDelete = lacteosPercentageDao.DeleteSubcategoryManualPercentage(yearData, chargeTypeData, fileLogId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteSubcategoryManualPercentage()." + "Error: " + ex.Message);
            }

            return successDelete;
        }
    }
}