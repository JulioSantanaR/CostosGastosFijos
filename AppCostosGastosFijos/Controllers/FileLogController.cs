namespace AppCostosGastosFijos.Controllers
{
    using System;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;
    using AppCostosGastosFijos.Models;
    using Business;
    using Business.Services;
    using Data.Models;
    using Data.Models.Request;
    using Data.Models.Response;

    /// <summary>
    /// Controlador asociado a las operaciones sobre el historial de archivos o cargas en la aplicación.
    /// </summary>
    public class FileLogController : Controller
    {
        /// <summary>
        /// Método utilizado para recuperar la información mostrada en la tabla con el historial de cargas.
        /// </summary>
        /// <param name="dataTableInfo">Objeto que contiene los parámetros de búsqueda para la tabla en el historial de cargas.</param>
        /// <returns>Devuelve una cadena json utilizada para mostrar la tabla con la información del historial de cargas.</returns>
        [HttpPost]
        public JsonResult GetFileLog(DataTableRequest dataTableInfo)
        {
            string filesJsonFormat = string.Empty;
            int filesCount = 0;
            try
            {
                if (dataTableInfo != null && dataTableInfo.FileRequest != null && dataTableInfo.FileRequest.IsCollaborator)
                {
                    string username = HttpContext.Request.LogonUserIdentity.Name;
                    UserData userInformation = new JDVSCController().GetUserData(username);
                    if (userInformation != null)
                    {
                        dataTableInfo.FileRequest.CollaboratorId = userInformation.CollaboratorId;
                    }
                }

                FileLogTableResponse fileLogData = FileLogService.GetFileLogTable(dataTableInfo);
                if (fileLogData != null)
                {
                    if (fileLogData.FileLogList != null && fileLogData.FileLogList.Count > 0)
                    {
                        filesJsonFormat = new JavaScriptSerializer().Serialize(fileLogData.FileLogList);
                    }

                    filesCount = fileLogData.FileLogCount;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return Json(new { data = filesJsonFormat, recordsTotal = filesCount });
        }

        /// <summary>
        /// Método utilizado para mostrar la vista asociada al historial de cargas.
        /// </summary>
        /// <param name="isCollaborator">Bandera para determinar si es un colaborador o un administrador.</param>
        /// <returns>Devuelve la vista con el historial de cargas.</returns>
        [HttpPost]
        public ActionResult ShowFileLogView(bool isCollaborator = false)
        {
            try
            {
                FileLogViewModel fileLogView = new FileLogViewModel();
                ViewBag.fileLogView = fileLogView;
                ViewBag.isCollaborator = isCollaborator;
                return PartialView("Index");
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a un archivo cargado en el historial.
        /// </summary>
        /// <param name="deleteFileRequest">Objeto auxiliar en la eliminación de un archivo dentro del historial de cargas.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente o no.</returns>
        [HttpPost]
        public ActionResult DeleteFileLog(DeleteFileRequest deleteFileRequest)
        {
            bool successResponse = false;
            try
            {
                successResponse = FileLogService.DeleteFileLog(deleteFileRequest);
            }
            catch (Exception)
            {
                throw;
            }

            return Json(new { successResponse });
        }
    }
}