﻿namespace AppCostosGastosFijos.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;
    using AppCostosGastosFijos.Models;
    using Business;
    using Data.Models;
    using Data.Models.Request;
    using Data.Models.Response;
    using Data.Repositories;

    public class JDVSCController : Controller
    {
        /// <summary>
        /// Método utilizado para mostrar la vista principal de la aplicación.
        /// </summary>
        /// <returns>Devuelve la página principal de la aplicación.</returns>
        public ActionResult Index(string user = "")
        {
            List<UserData> userInformation = null;
            HomeViewModel homeView = new HomeViewModel();
            string userName = GetUserInformation(user);
            if (!string.IsNullOrEmpty(userName))
            {
                userInformation = ReadDataService.UserLogin(userName);
            }

            ViewBag.homeView = homeView;
            ViewBag.userInformation = userInformation;
            return View();
        }

        /// <summary>
        /// Método utilizado para cargar el documento asociado a las cuentas/cecos relacionados con los costos-gastos fijos.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="chargeTypeName">Nombre asociado al tipo de carga.</param>
        /// <param name="areaData">Identificador del área a cargar.</param>
        /// <param name="manualBudget">Bandera para indicar si se está realizando un ajuste manual al presupuesto.</param>
        /// <returns>Devuelve la respuesta resultado de cargar las cuentas/cecos.</returns>
        [HttpPost]
        public ActionResult UploadAccounts(int yearData, int chargeTypeData, string chargeTypeName, int areaData = 0, bool manualBudget = false)
        {
            bool successResponse = false;
            string message = string.Empty;
            try
            {
                string userName = GetUserInformation();
                if (!string.IsNullOrEmpty(userName))
                {
                    UserData userInformation = ReadDataService.CollaboratorByUsername(userName);
                    if (userInformation != null)
                    {
                        foreach (string item in Request.Files)
                        {
                            HttpPostedFileBase file = Request.Files[item] as HttpPostedFileBase;
                            if (file.ContentLength > 0)
                            {
                                string extension = Path.GetExtension(file.FileName);

                                // Leer el archivo.
                                var accountsTable = CommonService.ReadFile(file.InputStream, extension);
                                if (accountsTable.Tables.Count > 0)
                                {
                                    AccountsDataRequest accountsData = new AccountsDataRequest()
                                    {
                                        FileData = accountsTable.Tables[0],
                                        YearAccounts = yearData,
                                        ChargeTypeAccounts = chargeTypeData,
                                        ChargeTypeName = chargeTypeName,
                                        Collaborator = userInformation.CollaboratorId,
                                        Area = areaData,
                                    };

                                    AccountsResponse response = null;
                                    if (manualBudget)
                                    {
                                        response = SaveDataService.ManualBudgetInformation(accountsData);
                                    }
                                    else
                                    {
                                        response = SaveDataService.AccountsInformation(accountsData);
                                    }
                                    
                                    if (response != null)
                                    {
                                        List<Accounts> notFoundAccounts = response.NotFoundAccounts;
                                        successResponse = response.SuccessProcess;
                                        if (notFoundAccounts != null && notFoundAccounts.Count > 0)
                                        {
                                            message = "Las siguientes cuentas/CECOS no se encontraron en el BIF";
                                            for (int i = 0; i < notFoundAccounts.Count; i++)
                                            {
                                                var singleAccount = notFoundAccounts[i];
                                                message += string.Format("{0}{1}|{2}", "\n", singleAccount.Account, singleAccount.CostCenter);
                                            }
                                        }

                                        if (!successResponse)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        message = "El usuario no tiene permisos para cargar esta información";
                    }
                }
                else
                {
                    message = "No se pudo recuperar el nombre del usuario actual.";
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UploadAccounts()." + "Error: " + ex.Message);
            }

            return Json(new { successResponse, message });
        }

        /// <summary>
        /// Método utilizado para cargar el documento asociado a la base asociada al volumen.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="chargeTypeName">Nombre asociado al tipo de carga.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        [HttpPost]
        public ActionResult UploadVolumen(int yearData, int chargeTypeData, string chargeTypeName)
        {
            bool successResponse = false;
            try
            {
                foreach (string item in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[item] as HttpPostedFileBase;
                    if (file.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(file.FileName);
                        successResponse = SaveDataService.VolumenInformation(file.InputStream, extension, yearData, chargeTypeData, chargeTypeName);
                        if (!successResponse)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UploadVolumen()." + "Error: " + ex.Message);
            }

            return Json(new { successResponse });
        }

        /// <summary>
        /// Método utilizado para cargar el documento asociado al detalle de la promotoria.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="chargeTypeName">Nombre asociado al tipo de carga.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        [HttpPost]
        public ActionResult UploadPromotoria(int yearData, int chargeTypeData, string chargeTypeName)
        {
            bool successResponse = false;
            string message = string.Empty;
            try
            {
                foreach (string item in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[item] as HttpPostedFileBase;
                    if (file.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(file.FileName);
                        UploadResponse response = SaveDataService.PromotoriaInformation(file.InputStream, extension, yearData, chargeTypeData, chargeTypeName);
                        if (response != null)
                        {
                            successResponse = response.ResponseFlag;
                            message = response.ErrorMessage;
                            if (!successResponse)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UploadPromotoria()." + "Error: " + ex.Message);
            }

            return Json(new { successResponse, message });
        }

        /// <summary>
        /// Método utilizado para actualizar el cubo de información asociado a los Costos/Gastos fijos.
        /// </summary>
        /// <returns>Devuelve una respuesta para determinar si la actualización del cubo fue correcta o no.</returns>
        [HttpPost]
        public ActionResult UpdateCubeCGFijos()
        {
            bool successResponse = false;
            try
            {
                string jobName = "JDV_CGFijos";
                string jobId = "B8485C8B-52FC-49A8-9EAD-24D5D24F1FE1";
                CubeService.UpdateCube(jobName, jobId);
                successResponse = true;
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateCubeCGFijos()." + "Error: " + ex.Message);
            }

            return Json(new { successResponse });
        }

        /// <summary>
        /// Método utilizado para actualizar el cubo de información asociado a la proyección de C&G Fijos.
        /// </summary>
        /// <returns>Devuelve una respuesta para determinar si la actualización del cubo fue correcta o no.</returns>
        [HttpPost]
        public ActionResult UpdateCubeProjection()
        {
            bool successResponse = false;
            try
            {
                string jobName = "JDV_CGProyeccion";
                string jobId = "D6C970BA-F946-43ED-859F-2254B470973E";
                CubeService.UpdateCube(jobName, jobId);
                successResponse = true;
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateCubeProjection()." + "Error: " + ex.Message);
            }

            return Json(new { successResponse });
        }

        /// <summary>
        /// Método utilizado para cargar el documento asociado a los porcentajes relacionados a cada canal de venta.
        /// </summary>
        /// <param name="yearPercentage">Año de carga.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadPercentage(int yearPercentage)
        {
            bool successResponse = false;
            try
            {
                foreach (string item in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[item] as HttpPostedFileBase;
                    if (file.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(file.FileName);
                        successResponse = SaveDataService.PercentagesInformation(file.InputStream, extension, yearPercentage);
                        if (!successResponse)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UploadPercentage()." + "Error: " + ex.Message);
            }

            return Json(new { successResponse });
        }

        /// <summary>
        /// Método utilizado para descargar el archivo asociado al log de errores del sitio.
        /// </summary>
        /// <returns>Devuelve el archivo asociado al log de errores en base64.</returns>
        [HttpPost]
        public ActionResult DownloadFile()
        {
            string fileResult = string.Empty;
            try
            {
                string logFilePath = @"D:\CGFijos\Log\Log.txt";
                FileInfo logFileInfo = new FileInfo(logFilePath);
                if (logFileInfo.Exists)
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(logFilePath);
                    fileResult = Convert.ToBase64String(bytes, 0, bytes.Length);
                }
                else
                {
                    GeneralRepository generalRepository = new GeneralRepository();
                    FileStream fileStream = generalRepository.CreateLogFile();
                    fileStream.Close();
                    return DownloadFile();
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DownloadFile()." + "Error: " + ex.Message);
            }

            return new JsonResult()
            {
                Data = fileResult,
                MaxJsonLength = 500000000
            };
        }

        /// <summary>
        /// Método utilizado para descargar el archivo asociado a la proyección a UAFIR.
        /// </summary>
        /// <returns>Devuelve una cadena con la información del archivo en base64.</returns>
        [HttpPost]
        public ActionResult DownloadFileProjection()
        {
            string fileResult = string.Empty;
            try
            {
                fileResult = ReadDataService.GenerateProjection();
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DownloadFileProjection()." + "Error: " + ex.Message);
            }

            return new JsonResult()
            {
                Data = fileResult,
                MaxJsonLength = 500000000
            };
        }

        /// <summary>
        /// Método utilizado para recuperar el username del usuario actual de la aplicación
        /// </summary>
        /// <param name="user">Username del usuario (opcional).</param>
        /// <returns>Devuelve una cadena con el nombre de usuario.</returns>
        public string GetUserInformation(string user = "")
        {
            string userName = string.Empty;
            try
            {
                userName = string.IsNullOrEmpty(user) ? HttpContext.Request.LogonUserIdentity.Name : user;
                if (!string.IsNullOrEmpty(userName) && userName.Contains("\\"))
                {
                    string[] userArray = userName.Split('\\');
                    userName = userArray[1].ToString();
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetUserInformation()." + "Error: " + ex.Message);
            }

            return userName;
        }
    }
}