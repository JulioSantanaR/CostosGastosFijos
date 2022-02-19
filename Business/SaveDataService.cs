namespace Business
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
    using Business.Services;
    using Data;
    using Data.Models;
    using Data.Models.Request;
    using Data.Models.Response;
    using Data.Repositories;

    /// <summary>
    /// Clase de negocio intermedia entre el acceso a datos y la capa del cliente.
    /// </summary>
    public static class SaveDataService
    {
        /// <summary>
        /// Método utilizado para guardar la información asociada a las cuentas/cecos.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en el guardado de cuentas/Cecos desde un archivo.</param>
        /// <returns>Objeto tipo response utilizado para saber si se cargaron correctamente las cuentas o no.</returns>
        public static AccountsResponse AccountsInformation(AccountsDataRequest accountsData)
        {
            bool successProcess = false;
            List<Accounts> notFoundAccounts = null;
            try
            {
                if (accountsData != null)
                {
                    string chargeTypeName = accountsData.ChargeTypeName;
                    int yearAccounts = accountsData.YearAccounts;
                    int chargeTypeAccounts = accountsData.ChargeTypeAccounts;

                    // Guardar la información del archivo que se está cargando.
                    FileLogData fileLogData = new FileLogData()
                    {
                        FileName = accountsData.FileName,
                        ChargeDate = DateTime.Now,
                        ApprovalFlag = false,
                        FileTypeName = "Presupuesto",
                        UserId = accountsData.Collaborator,
                        AreaId = accountsData.Area,
                        ChargeTypeId = accountsData.ChargeTypeAccounts,
                        YearData = accountsData.YearAccounts,
                    };
                    int fileLogId = FileLogService.SaveFileLog(fileLogData);
                    if (fileLogId != 0)
                    {
                        accountsData.FileLogId = fileLogId;

                        // Insertar el presupuesto anual.
                        SaveDataDAO saveData = new SaveDataDAO();
                        successProcess = saveData.InsertAccounts(accountsData);
                        if (successProcess)
                        {
                            chargeTypeName = chargeTypeName.ToLower();
                            chargeTypeName = chargeTypeName == "rolling 0+12" || chargeTypeName == "business plan" ? "BP" : "Rolling";

                            // Revisar si existen cuentas que no estén completas en el BIF.
                            notFoundAccounts = ReadDataService.GetNotFoundAccountsBIF(accountsData);
                            if (notFoundAccounts != null && notFoundAccounts.Count > 0)
                            {
                                BudgetService.DeleteAccounts(accountsData);
                                successProcess = false;
                            }
                            else
                            {
                                // Insertar los porcentajes para cada canal.
                                successProcess = saveData.InsertChannelPercentages(yearAccounts, chargeTypeAccounts, chargeTypeName);
                                if (successProcess)
                                {
                                    UpdateDataDAO updateData = new UpdateDataDAO();

                                    // Actualizar la tabla de hechos de las cuentas/cecos.
                                    accountsData.ExerciseType = chargeTypeName;
                                    successProcess = updateData.UpdateFactTblAccounts(accountsData);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("AccountsInformation()." + "Error: " + ex.Message);
            }

            AccountsResponse response = new AccountsResponse()
            {
                SuccessProcess = successProcess,
                NotFoundAccounts = notFoundAccounts,
            };
            return response;
        }

        /// <summary>
        /// Método utilizado para guardar la información asociada al ajuste manual en las cuentas/cecos.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en el guardado de cuentas/Cecos desde un archivo.</param>
        /// <returns>Objeto tipo response utilizado para saber si se cargaron correctamente las cuentas o no.</returns>
        public static AccountsResponse ManualBudgetInformation(AccountsDataRequest accountsData)
        {
            bool successProcess = false;
            try
            {
                if (accountsData != null)
                {
                    SaveDataDAO saveData = new SaveDataDAO();

                    // Guardar la información del archivo que se está cargando.
                    FileLogData fileLogData = new FileLogData()
                    {
                        FileName = accountsData.FileName,
                        ChargeDate = DateTime.Now,
                        ApprovalFlag = false,
                        FileTypeName = "Ajuste manual",
                        UserId = accountsData.Collaborator,
                        AreaId = accountsData.Area,
                        ChargeTypeId = accountsData.ChargeTypeAccounts,
                        YearData = accountsData.YearAccounts,
                    };
                    int fileLogId = FileLogService.SaveFileLog(fileLogData);
                    if (fileLogId != 0)
                    {
                        accountsData.FileLogId = fileLogId;

                        // Insertar el presupuesto/ajuste manual.
                        successProcess = saveData.InsertManualBudget(accountsData);
                        if (successProcess)
                        {
                            // Insertar la información en la tabla de hechos de las cuentas/cecos.
                            successProcess = saveData.SaveFactAccountsManual(accountsData);

                            // Actualizar la tabla de hechos de la proyección.
                            if (successProcess)
                            {
                                UpdateDataDAO updateData = new UpdateDataDAO();
                                int yearAccounts = accountsData.YearAccounts;
                                int chargeTypeAccounts = accountsData.ChargeTypeAccounts;
                                string chargeTypeName = accountsData.ChargeTypeName;
                                chargeTypeName = chargeTypeName.ToLower();
                                chargeTypeName = chargeTypeName == "rolling 0+12" || chargeTypeName == "business plan" ? "BP" : "Rolling";
                                successProcess = updateData.UpdateFactProjection(yearAccounts, chargeTypeAccounts, chargeTypeName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("ManualBudgetInformation()." + "Error: " + ex.Message);
            }

            AccountsResponse response = new AccountsResponse() { SuccessProcess = successProcess };
            return response;
        }

        /// <summary>
        /// Método utilizado para guardar la información asociada a los porcentajes de cada canal de venta.
        /// </summary>
        /// <param name="fileStream">Información asociada al archivo.</param>
        /// <param name="fileExtension">Extensión del archivo.</param>
        /// <param name="yearPercentage">Año de carga.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public static bool PercentagesInformation(Stream fileStream, string fileExtension, int yearPercentage)
        {
            bool successInsert = false;
            try
            {
                var percentageTable = CommonService.ReadFile(fileStream, fileExtension);
                if (percentageTable.Tables.Count > 0)
                {
                    SaveDataDAO connection = new SaveDataDAO();
                    successInsert = connection.BulkInsertPercentage(percentageTable.Tables[0], yearPercentage);
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("PercentagesInformation()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

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
                        string exerciseType = string.Empty;
                        DeleteDataDAO deleteData = new DeleteDataDAO();
                        SaveDataDAO saveData = new SaveDataDAO();
                        UpdateDataDAO updateData = new UpdateDataDAO();

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
                            chargeTypeName = chargeTypeName.ToLower();
                            bool bpExercise = chargeTypeName == "rolling 0+12" || chargeTypeName == "business plan";
                            if (bpExercise)
                            {
                                exerciseType = "BP";
                                successProcess = deleteData.DeleteVolumeBP(yearData, chargeTypeData);
                                if (successProcess)
                                {
                                    successProcess = saveData.BulkInsertVolumenBP(accountsTable.Tables[0], yearData, chargeTypeData, fileLogId);
                                    if (successProcess)
                                    {
                                        successProcess = deleteData.DeleteChannelPercentages(yearData, exerciseType);
                                        if (successProcess)
                                        {
                                            successProcess = saveData.InsertChannelPercentages(yearData, chargeTypeData, exerciseType);
                                            if (successProcess)
                                            {
                                                successProcess = updateData.UpdateFactTblBP(yearData, chargeTypeData);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                exerciseType = "Rolling";
                                successProcess = deleteData.DeleteVolume(yearData, chargeTypeData);
                                if (successProcess)
                                {
                                    successProcess = saveData.BulkInsertVolumen(accountsTable.Tables[0], yearData, chargeTypeData, fileLogId);
                                }
                            }

                            // Actualizar la tabla de hechos de la proyección.
                            if (successProcess)
                            {
                                successProcess = updateData.UpdateFactProjection(yearData, chargeTypeData, exerciseType);
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
                        List<PromotoriaDB> promotoriaFromDB = PromotoriaService.GetPromotoria(yearData, chargeTypeData);
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
                                    responseFlag = PromotoriaService.BulkInsertPromotoria(fileData, yearData, chargeTypeData, fileLogId);
                                    if (responseFlag)
                                    {
                                        chargeTypeName = chargeTypeName.ToLower();
                                        bool bpExercise = chargeTypeName == "rolling 0+12" || chargeTypeName == "business plan";
                                        string exerciseType = bpExercise ? "BP" : "Rolling";

                                        // Actualizar la tabla de hechos de la proyección para este año y tipo de carga.
                                        UpdateDataDAO updateData = new UpdateDataDAO();
                                        responseFlag = updateData.UpdateFactProjection(yearData, chargeTypeData, exerciseType);
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
        /// Método utilizado para guardar la información asociada a un usuario de la aplicación.
        /// </summary>
        /// <param name="userInformation">Objeto que contiene la información general del usuario.</param>
        /// <returns>Devuelve el id asociado al usuario recién insertado en la Base de Datos.</returns>
        public static int SaveUserInformation(UserData userInformation)
        {
            int collaboratorId = 0;
            try
            {
                SaveDataDAO connection = new SaveDataDAO();
                collaboratorId = connection.SaveUserInformation(userInformation);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveUserInformation()." + "Error: " + ex.Message);
            }

            return collaboratorId;
        }

        /// <summary>
        /// Método utilizado para guardar la relación entre un usuario y la(s) área(s) asociadas a este.
        /// </summary>
        /// <param name="userAreas">Lista de áreas asociadas a un usuario.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue guardada correctamente.</returns>
        public static bool BulkInsertUserAreas(List<int> areas, int userId)
        {
            bool successInsert = false;
            try
            {
                List<UserAreaRelation> userAreas = new List<UserAreaRelation>();
                if (areas != null && areas.Count > 0)
                {
                    for (int i = 0; i < areas.Count; i++)
                    {
                        UserAreaRelation singleUserArea = new UserAreaRelation();
                        singleUserArea.AreaId = areas[i];
                        singleUserArea.UserId = userId;
                        userAreas.Add(singleUserArea);
                    }
                }

                if (userAreas != null && userAreas.Count > 0)
                {
                    SaveDataDAO connection = new SaveDataDAO();
                    successInsert = connection.BulkInsertUserAreas(userAreas);
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertUserAreas()." + "Error: " + ex.Message);
            }

            return successInsert;
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