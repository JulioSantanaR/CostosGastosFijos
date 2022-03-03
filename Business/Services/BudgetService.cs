namespace Business.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Data.DAO;
    using Data.Models;
    using Data.Models.Request;
    using Data.Models.Response;
    using Data.Repositories;

    /// <summary>
    /// Clase intermedia entre el acceso a datos y la capa del cliente para manipular la información asociada al presupuesto con las cuentas/cecos por colaborador.
    /// </summary>
    public static class BudgetService
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
                        successProcess = InsertAccounts(accountsData);
                        if (successProcess)
                        {
                            chargeTypeName = chargeTypeName.ToLower();
                            chargeTypeName = chargeTypeName == "rolling 0+12" || chargeTypeName == "business plan" ? "BP" : "Rolling";

                            // Revisar si existen cuentas que no estén completas en el BIF.
                            notFoundAccounts = GetNotFoundAccountsBIF(accountsData);
                            if (notFoundAccounts != null && notFoundAccounts.Count > 0)
                            {
                                DeleteAccounts(accountsData);
                                successProcess = false;
                            }
                            else
                            {
                                // Insertar los porcentajes para cada canal.
                                successProcess = ChannelPercentageService.InsertChannelPercentages(yearAccounts, chargeTypeAccounts, chargeTypeName);

                                // Actualizar la tabla de hechos de las cuentas/cecos.
                                if (successProcess)
                                {
                                    accountsData.ExerciseType = chargeTypeName;
                                    successProcess = UpdateFactTblAccounts(accountsData);
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

                    // Guardar la información del archivo que se está cargando.
                    int fileLogId = FileLogService.SaveFileLog(fileLogData);
                    if (fileLogId != 0)
                    {
                        accountsData.FileLogId = fileLogId;

                        // Insertar el presupuesto/ajuste manual.
                        successProcess = InsertManualBudget(accountsData);
                        if (successProcess)
                        {
                            // Insertar la información en la tabla de hechos de las cuentas/cecos.
                            successProcess = SaveFactAccountsManual(accountsData);

                            // Actualizar la tabla de hechos de la proyección.
                            if (successProcess)
                            {
                                int yearAccounts = accountsData.YearAccounts;
                                int chargeTypeAccounts = accountsData.ChargeTypeAccounts;
                                string chargeTypeName = accountsData.ChargeTypeName;
                                chargeTypeName = chargeTypeName.ToLower();
                                chargeTypeName = chargeTypeName == "rolling 0+12" || chargeTypeName == "business plan" ? "BP" : "Rolling";
                                successProcess = LogProjectionService.SaveOrUpdateLogProjection(yearAccounts, chargeTypeAccounts, chargeTypeName);
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
        /// Método utilizado para insertar el presupuesto general de las cuentas, de acuerdo a un año y tipo de carga específicos.
        /// </summary>
        /// <param name="saveAccountsData">Objeto auxiliar en el guardado de cuentas/Cecos desde un archivo.</param>
        /// <returns>Devuelve una bandera para determinar si la información se insertó correctamente o no.</returns>
        public static bool InsertAccounts(AccountsDataRequest accountsData)
        {
            bool successInsert = false;
            try
            {
                BudgetDAO budgetDao = new BudgetDAO();
                successInsert = budgetDao.InsertAccounts(accountsData);
            }
            catch (Exception ex)
            {
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para recuperar las cuentas/cecos que no tengan la información completa en el BIF.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la búsqueda de cuentas/cecos con información incompleta en el BIF.</param>
        /// <returns>Devuelve la lista de cuentas dentro del presupuesto cargado que no están completas en el BIF.</returns>
        public static List<Accounts> GetNotFoundAccountsBIF(AccountsDataRequest accountsData)
        {
            List<Accounts> notFoundAccounts = new List<Accounts>();
            try
            {
                BudgetDAO budgetDao = new BudgetDAO();
                notFoundAccounts = budgetDao.GetNotFoundAccountsBIF(accountsData);
            }
            catch (Exception ex)
            {
            }

            return notFoundAccounts;
        }

        /// <summary>
        /// Método utilizado para insertar las cuentas asociadas a un ajuste de presupuesto manual.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la eliminación de cuentas/Cecos.</param>
        /// <returns>Devuelve una bandera para determinar si la eliminación fue correcta o no.</returns>
        public static bool InsertManualBudget(AccountsDataRequest accountsData)
        {
            bool successInsert = false;
            try
            {
                BudgetDAO budgetDao = new BudgetDAO();
                successInsert = budgetDao.InsertManualBudget(accountsData);
            }
            catch (Exception ex)
            {
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para eliminar las cuentas asociadas a un año y tipo de carga específicos.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la eliminación de cuentas/Cecos.</param>
        /// <returns>Devuelve una bandera para determinar si la eliminación fue correcta o no.</returns>
        public static bool DeleteAccounts(AccountsDataRequest accountsData)
        {
            bool successDelete = false;
            try
            {
                BudgetDAO budgetDao = new BudgetDAO();
                successDelete = budgetDao.DeleteAccounts(accountsData);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteAccounts()." + "Error: " + ex.Message);
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar las cuentas asociadas a un ajuste de presupuesto manual.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la eliminación de cuentas/Cecos.</param>
        /// <returns>Devuelve una bandera para determinar si la eliminación fue correcta o no.</returns>
        public static bool DeleteManualBudget(AccountsDataRequest accountsData)
        {
            bool successDelete = false;
            try
            {
                BudgetDAO budgetDao = new BudgetDAO();
                successDelete = budgetDao.DeleteManualBudget(accountsData);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteManualBudget()." + "Error: " + ex.Message);
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para insertar información dentro de la tabla de hechos asociada al presupuesto general de cuentas.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en el guardado de cuentas/Cecos desde un archivo en la tabla de hechos.</param>
        /// <returns>Devuelve una bandera para determinar si la inserción de la información fue correcta o no.</returns>
        public static bool SaveFactAccountsManual(AccountsDataRequest accountsData)
        {
            bool successInsert = false;
            try
            {
                BudgetDAO budgetDao = new BudgetDAO();
                successInsert = budgetDao.SaveFactAccountsManual(accountsData);
            }
            catch (Exception ex)
            {
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para eliminar las cuentas/cecos dentro de la tabla de hechos de acuerdo a los parámetros enviados.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la eliminación de cuentas/Cecos.</param>
        /// <returns>Devuelve una bandera para determinar si la eliminación fue correcta o no.</returns>
        public static bool FactTableDeleteAccounts(AccountsDataRequest accountsData)
        {
            bool successDelete = false;
            try
            {
                BudgetDAO budgetDao = new BudgetDAO();
                successDelete = budgetDao.FactTableDeleteAccounts(accountsData);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("FactTableDeleteAccounts()." + "Error: " + ex.Message);
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para actualizar la tabla de hechos asociada al presupuesto general de cuentas.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la actualización de cuentas/Cecos desde un archivo.</param>
        /// <returns>Devuelve una bandera para determinar si la actualización fue correcta o no.</returns>
        public static bool UpdateFactTblAccounts(AccountsDataRequest accountsData)
        {
            bool successUpdate = false;
            try
            {
                BudgetDAO budgetDao = new BudgetDAO();
                successUpdate = budgetDao.UpdateFactTblAccounts(accountsData);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateFactTblAccounts()." + "Error: " + ex.Message);
            }

            return successUpdate;
        }

        /// <summary>
        /// Método utilizado para actualizar los montos debido a la carga de volumen de un BP/Rolling 0+12.
        /// </summary>
        /// <param name="yearAccounts">Año de carga.</param>
        /// <param name="chargeTypeAccounts">Tipo de carga.</param>
        /// <returns>Devuelve una bandera para determinar si la actualización fue correcta o no.</returns>
        public static bool UpdateFactTblBP(int yearAccounts, int chargeTypeAccounts)
        {
            bool successUpdate = false;
            try
            {
                BudgetDAO budgetDao = new BudgetDAO();
                successUpdate = budgetDao.UpdateFactTblBP(yearAccounts, chargeTypeAccounts);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateFactTblBP()." + "Error: " + ex.Message);
            }

            return successUpdate;
        }

        /// <summary>
        /// Método utilizado para actualizar la tabla de hechos que tiene que ver con la proyección de los Costos y Gastos Fijos.
        /// </summary>
        /// <param name="yearAccounts">Año de carga.</param>
        /// <param name="chargeTypeAccounts">Tipo de carga.</param>
        /// <param name="exerciseType">Tipo de ejercicio que se está realizando (BP/Rolling).</param>
        /// <returns>Devuelve una bandera para determinar si la actualización fue correcta o no.</returns>
        public static bool UpdateFactProjection(int yearAccounts, int chargeTypeAccounts, string exerciseType)
        {
            bool successUpdate = false;
            try
            {
                BudgetDAO budgetDao = new BudgetDAO();
                successUpdate = budgetDao.UpdateFactProjection(yearAccounts, chargeTypeAccounts, exerciseType);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateFactProjection()." + "Error: " + ex.Message);
            }

            return successUpdate;
        }
    }
}