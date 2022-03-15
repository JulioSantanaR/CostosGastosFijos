namespace AppCostosGastosFijos.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;
    using AppCostosGastosFijos.Models;
    using Business;
    using Business.Services;
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
            UserData userInformation = null;
            HomeViewModel homeView = new HomeViewModel();
            string userName = GetUserInformation(user);
            if (!string.IsNullOrEmpty(userName))
            {
                userInformation = UsersService.UserLogin(userName);
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
                UserData userInformation = GetUserData();
                if (userInformation != null)
                {
                    foreach (string item in Request.Files)
                    {
                        HttpPostedFileBase file = Request.Files[item];
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
                                    FileName = file.FileName,
                                };

                                AccountsResponse response = null;
                                if (manualBudget)
                                {
                                    response = BudgetService.ManualBudgetInformation(accountsData);
                                }
                                else
                                {
                                    response = BudgetService.AccountsInformation(accountsData);
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
                UserData userInformation = GetUserData();
                if (userInformation != null)
                {
                    foreach (string item in Request.Files)
                    {
                        HttpPostedFileBase file = Request.Files[item];
                        if (file.ContentLength > 0)
                        {
                            VolumeDataRequest volumeData = new VolumeDataRequest()
                            {
                                FileData = file,
                                YearData = yearData,
                                ChargeType = chargeTypeData,
                                ChargeTypeName = chargeTypeName,
                                Collaborator = userInformation.CollaboratorId,
                            };
                            successResponse = VolumeService.VolumenInformation(volumeData);
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
                UserData userInformation = GetUserData();
                if (userInformation != null)
                {
                    foreach (string item in Request.Files)
                    {
                        HttpPostedFileBase file = Request.Files[item];
                        if (file.ContentLength > 0)
                        {
                            PromotoriaDataRequest volumeData = new PromotoriaDataRequest()
                            {
                                FileData = file,
                                YearData = yearData,
                                ChargeType = chargeTypeData,
                                ChargeTypeName = chargeTypeName,
                                Collaborator = userInformation.CollaboratorId,
                            };
                            UploadResponse response = PromotoriaService.PromotoriaInformation(volumeData);
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
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UploadPromotoria()." + "Error: " + ex.Message);
            }

            return Json(new { successResponse, message });
        }

        /// <summary>
        /// Método utilizado para realizar el proceso que guarda los porcentajes base y los porcentajes por marca.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="chargeTypeName">Nombre asociado al tipo de carga.</param>
        /// <param name="fileTypeName">Nombre asociado al tipo de archivo a cargar.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        [HttpPost]
        public ActionResult UploadPercentages(int yearData, int chargeTypeData, string chargeTypeName, string fileTypeName)
        {
            bool successResponse = false;
            string message = string.Empty;
            try
            {
                UserData userInformation = GetUserData();
                if (userInformation != null)
                {
                    foreach (string item in Request.Files)
                    {
                        HttpPostedFileBase file = Request.Files[item];
                        if (file.ContentLength > 0)
                        {
                            BasePercentageRequest percentageData = new BasePercentageRequest()
                            {
                                FileData = file,
                                YearData = yearData,
                                ChargeType = chargeTypeData,
                                ChargeTypeName = chargeTypeName,
                                Collaborator = userInformation.CollaboratorId,
                            };
                            switch (fileTypeName)
                            {
                                case "Porcentajes Stills":
                                    successResponse = StillsPercentageService.SaveStillsPercentage(percentageData);
                                    break;
                                case "Porcentajes Ades":
                                    successResponse = AdesPercentageService.SaveAdesPercentage(percentageData);
                                    break;
                                case "Porcentajes Lácteos":
                                    successResponse = LacteosPercentagesService.SaveLacteosPercentage(percentageData);
                                    break;
                            }

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
                generalRepository.WriteLog("UploadStillsPercentages()." + "Error: " + ex.Message);
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
                successResponse = CubeService.UpdateChannelAssignTbl();
                if (successResponse)
                {
                    string jobName = "JDV_CGFijos";
                    string jobId = "B8485C8B-52FC-49A8-9EAD-24D5D24F1FE1";
                    successResponse = CubeService.UpdateCube(jobName, jobId);
                }
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
                successResponse = CubeService.UpdateProjectionTbl();
                if (successResponse)
                {
                    string jobName = "JDV_CGProyeccion";
                    string jobId = "D6C970BA-F946-43ED-859F-2254B470973E";
                    CubeService.UpdateCube(jobName, jobId);
                    successResponse = true;
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateCubeProjection()." + "Error: " + ex.Message);
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

        /// <summary>
        /// Obtener la información general asociada al usuario.
        /// </summary>
        /// <param name="user">Username del usuario (opcional).</param>
        /// <returns>Devuelve un objeto que contiene la información general asociada al usuario.</returns>
        public UserData GetUserData(string user = "")
        {
            UserData userInformation = null;
            string userName = GetUserInformation(user);
            if (!string.IsNullOrEmpty(userName))
            {
                userInformation = UsersService.CollaboratorByUsername(userName);
            }

            return userInformation;
        }
    }
}