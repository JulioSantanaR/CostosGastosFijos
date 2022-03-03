namespace Business.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web;
    using Data;
    using Data.DAO;
    using Data.Models;
    using Data.Models.Request;
    using Data.Models.Response;
    using Data.Repositories;

    /// <summary>
    /// Clase intermedia entre el acceso a datos y la capa del cliente para manipular la información asociada a la promotoria.
    /// </summary>
    public static class PromotoriaService
    {
        /// <summary>
        /// Método utilizado para cargar el documento asociado al detalle de la promotoria.
        /// </summary>
        /// <param name="promotoriaData">Objeto auxiliar en el guardado de la información de la promotoria.</param>
        /// <returns>Respuesta al realizar el proceso de la carga de la promotoria.</returns>
        public static UploadResponse PromotoriaInformation(PromotoriaDataRequest promotoriaData)
        {
            bool responseFlag = false;
            string errorMsg = string.Empty;
            try
            {
                if (promotoriaData != null)
                {
                    int yearData = promotoriaData.YearData;
                    int chargeTypeData = promotoriaData.ChargeType;
                    string chargeTypeName = promotoriaData.ChargeTypeName;
                    HttpPostedFileBase fileInfo = promotoriaData.FileData;
                    string fileExtension = Path.GetExtension(fileInfo.FileName);
                    var accountsTable = CommonService.ReadFile(fileInfo.InputStream, fileExtension);
                    if (accountsTable.Tables.Count > 0)
                    {
                        // Obtener la promotoria de acuerdo al año y el ejercicio.
                        List<PromotoriaDB> promotoriaFromDB = GetPromotoria(yearData, chargeTypeData);
                        if (promotoriaFromDB != null && promotoriaFromDB.Count > 0)
                        {
                            var fileData = accountsTable.Tables[0];
                            PromotoriaValidateFile(fileData, chargeTypeName, promotoriaFromDB, ref responseFlag, ref errorMsg);
                            if (responseFlag)
                            {
                                // Guardar la información del archivo que se está cargando.
                                FileLogData fileLogData = new FileLogData()
                                {
                                    FileName = fileInfo.FileName,
                                    ChargeDate = DateTime.Now,
                                    ApprovalFlag = false,
                                    FileTypeName = "Promotoria",
                                    UserId = promotoriaData.Collaborator,
                                    ChargeTypeId = chargeTypeData,
                                    YearData = yearData,
                                    DefaultArea = true,
                                };
                                int fileLogId = FileLogService.SaveFileLog(fileLogData);
                                if (fileLogId != 0)
                                {
                                    // Insertar la información de la promotoria.
                                    responseFlag = BulkInsertPromotoria(fileData, yearData, chargeTypeData, fileLogId);
                                    if (responseFlag)
                                    {
                                        chargeTypeName = chargeTypeName.ToLower();
                                        bool bpExercise = chargeTypeName == "rolling 0+12" || chargeTypeName == "business plan";
                                        string exerciseType = bpExercise ? "BP" : "Rolling";

                                        // Actualizar la tabla de hechos de la proyección para este año y tipo de carga.
                                        responseFlag = LogProjectionService.SaveOrUpdateLogProjection(yearData, chargeTypeData, exerciseType);
                                    }
                                    else
                                    {
                                        errorMsg = "Ocurrió un error al guardar el archivo de la promotoria";
                                    }
                                }
                            }
                        }
                        else
                        {
                            errorMsg = "No se ha cargado el presupuesto para el año y tipo de carga seleccionados.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("PromotoriaInformation()." + "Error: " + ex.Message);
            }

            UploadResponse response = new UploadResponse()
            {
                ResponseFlag = responseFlag,
                ErrorMessage = errorMsg
            };
            return response;
        }

        /// <summary>
        /// Método utilizado para mapear la información del archivo de promotoria a un objeto.
        /// </summary>
        /// <param name="fileData">DataTable que contiene la información leída del archivo de promotoria.</param>
        /// <returns>Devuelve un objeto que contiene la información del archivo de promotoria.</returns>
        public static List<PromotoriaFile> PromotoriaFileMapping(DataTable fileData)
        {
            List<PromotoriaFile> promotoriaFromFile = (from rw in fileData.AsEnumerable()
                                                       select new PromotoriaFile()
                                                       {
                                                           CadenaAgrupada = rw["Cadena Agrupada"].ToString(),
                                                           FormatoCadena = rw["Formato Cadena"].ToString(),
                                                           Cuenta = rw["Cuenta"].ToString(),
                                                           CentroDeCosto = rw["Centro de Costo"].ToString(),
                                                           Enero = rw["Enero"] != DBNull.Value ? Convert.ToDouble(rw["Enero"]) : default(double),
                                                           Febrero = rw["Febrero"] != DBNull.Value ? Convert.ToDouble(rw["Febrero"]) : default(double),
                                                           Marzo = rw["Marzo"] != DBNull.Value ? Convert.ToDouble(rw["Marzo"]) : default(double),
                                                           Abril = rw["Abril"] != DBNull.Value ? Convert.ToDouble(rw["Abril"]) : default(double),
                                                           Mayo = rw["Mayo"] != DBNull.Value ? Convert.ToDouble(rw["Mayo"]) : default(double),
                                                           Junio = rw["Junio"] != DBNull.Value ? Convert.ToDouble(rw["Junio"]) : default(double),
                                                           Julio = rw["Julio"] != DBNull.Value ? Convert.ToDouble(rw["Julio"]) : default(double),
                                                           Agosto = rw["Agosto"] != DBNull.Value ? Convert.ToDouble(rw["Agosto"]) : default(double),
                                                           Septiembre = rw["Septiembre"] != DBNull.Value ? Convert.ToDouble(rw["Septiembre"]) : default(double),
                                                           Octubre = rw["Octubre"] != DBNull.Value ? Convert.ToDouble(rw["Octubre"]) : default(double),
                                                           Noviembre = rw["Noviembre"] != DBNull.Value ? Convert.ToDouble(rw["Noviembre"]) : default(double),
                                                           Diciembre = rw["Diciembre"] != DBNull.Value ? Convert.ToDouble(rw["Diciembre"]) : default(double),
                                                       }).ToList();
            return promotoriaFromFile;
        }

        /// <summary>
        /// Método utilizado para guardar la base asociada a la promotoria de cada portafolio.
        /// </summary>
        /// <param name="promotoriaTable">Objeto que contiene la información de la base de la promotoria.</param>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public static bool BulkInsertPromotoria(DataTable promotoriaTable, int yearData, int chargeTypeData, int fileLogId)
        {
            bool successInsert = false;
            try
            {
                PivotTableRequest pivotTbl = new PivotTableRequest()
                {
                    ColumnsPivot = new List<string> { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" },
                    NewColumnName = "TotalPorMes",
                    NewColumnType = typeof(string),
                    IncludeMonth = true
                };
                List<PivotTableRequest> pivotRequest = new List<PivotTableRequest>() { pivotTbl };
                promotoriaTable = CommonService.UnpivotDataTable(promotoriaTable, pivotRequest);
                PromotoriaDAO promotoriaDao = new PromotoriaDAO();
                successInsert = promotoriaDao.BulkInsertPromotoria(promotoriaTable, yearData, chargeTypeData, fileLogId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertPromotoria()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a la promotoria, de acuerdo a un año y tipo de carga específicos.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public static bool DeletePromotoria(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                PromotoriaDAO promotoriaDao = new PromotoriaDAO();
                successDelete = promotoriaDao.DeletePromotoria(yearData, chargeTypeData, fileLogId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeletePromotoria()." + "Error: " + ex.Message);
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para recuperar la información de la promotoría de acuerdo a un año y tipo de ejercicio específicos.
        /// </summary>
        /// <param name="yearData">Año del ejercicio.</param>
        /// <param name="chargeTypeData">Tipo de ejercicio.</param>
        /// <returns>Devuelve la información asociada al detalle del presupuesto correspondiente a la promotoría.</returns>
        public static List<PromotoriaDB> GetPromotoria(int yearData, int chargeTypeData)
        {
            List<PromotoriaDB> promotoria = new List<PromotoriaDB>();
            try
            {
                PromotoriaDAO promotoriaDao = new PromotoriaDAO();
                promotoria = promotoriaDao.GetPromotoria(yearData, chargeTypeData);
            }
            catch (Exception ex)
            {
            }

            return promotoria;
        }

        /// <summary>
        /// Método utilizado para validar que la información de la promotoría coincida con lo cargado en el presupuesto.
        /// </summary>
        /// <param name="fileData">Información del archivo que se está cargando.</param>
        /// <param name="chargeTypeName">Nombre del tipo de carga.</param>
        /// <param name="promotoriaFromDB">Información de la promotoría cargada en el presupuesto.</param>
        /// <param name="responseFlag">Bandera para saber si la información es válida.</param>
        /// <param name="errorMsg">Mensaje de error en caso de que la validación no sea correcta.</param>
        private static void PromotoriaValidateFile(DataTable fileData, string chargeTypeName, List<PromotoriaDB> promotoriaFromDB, ref bool responseFlag, ref string errorMsg)
        {
            List<PromotoriaFile> promotoriaFromFile = PromotoriaFileMapping(fileData);
            int firstMonth = 0;
            int lastMonth = 12;
            int errorsCount = 0;
            string monthsErrors = string.Empty;
            if (chargeTypeName.Contains("+"))
            {
                string[] onlyDigits = Regex.Split(chargeTypeName, @"\D+").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                if (onlyDigits.Length > 1)
                {
                    firstMonth = Convert.ToInt32(onlyDigits[0]);
                }
            }

            for (firstMonth++; firstMonth < lastMonth + 1; firstMonth++)
            {
                string monthName = CultureInfo.CreateSpecificCulture("es").DateTimeFormat.GetMonthName(firstMonth);
                string montTitleCase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(monthName);
                Type t = typeof(PromotoriaFile);
                PropertyInfo prop = t.GetProperty(montTitleCase);
                var singlePromotoriaFile = promotoriaFromFile.Select(file => (double)prop.GetValue(file)).Sum();
                var findPromotoriaDB = promotoriaFromDB.Where(x => x.MonthNumber == firstMonth).FirstOrDefault();
                var singlePromotoriaDB = findPromotoriaDB != null ? findPromotoriaDB.Budget : default;
                if (singlePromotoriaFile.ToString("C6") != singlePromotoriaDB.ToString("C6"))
                {
                    errorsCount++;
                    monthsErrors += string.Format("{0}{1}|{2}|{3}", "\n", montTitleCase, singlePromotoriaFile.ToString("C6"), singlePromotoriaDB.ToString("C6"));
                }

                responseFlag = errorsCount == 0;
                if (!responseFlag)
                {
                    errorMsg = string.Format("Existen diferencias entre el presupuesto existente contra lo cargado: {0}", monthsErrors);
                }
            }
        }
    }
}