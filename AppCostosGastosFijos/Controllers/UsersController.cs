namespace AppCostosGastosFijos.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;
    using Business;
    using Data.Models;
    using Data.Models.Request;
    using Data.Models.Response;

    /// <summary>
    /// Controlador asociado a las operaciones sobre la administración de usuarios.
    /// </summary>
    public class UsersController : Controller
    {
        /// <summary>
        /// Método utilizado para mostrar la vista principal asociada a la administración de usuarios.
        /// </summary>
        /// <returns>Devuelve la vista principal de la administración de usuarios.</returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Método utilizado para recuperar la tabla de usuarios de acuerdo a las condiciones de búsqueda.
        /// </summary>
        /// <param name="dataTableInfo">Objeto que contiene los parámetros de búsqueda para la tabla de usuarios.</param>
        /// <returns>Devuelve la lista de usuarios de acuerdo a las condiciones de búsqueda.</returns>
        [HttpPost]
        public JsonResult GetUsers(DataTableRequest dataTableInfo)
        {
            string usersJsonFormat = string.Empty;
            int usersCount = 0;
            try
            {
                UsersTableResponse usersData = ReadDataService.GetUsersTable(dataTableInfo);
                if (usersData != null)
                {
                    if (usersData.UsersList != null && usersData.UsersList.Count > 0)
                    {
                        usersJsonFormat = new JavaScriptSerializer().Serialize(usersData.UsersList);
                    }

                    usersCount = usersData.UsersCount;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return Json(new { data = usersJsonFormat, recordsTotal = usersCount });
        }

        /// <summary>
        /// Método utilizado para mostrar el modal que contiene la información de un usuario o realizar la alta de uno nuevo.
        /// </summary>
        /// <param name="userId">Id asociado al usuario.</param>
        /// <returns>Devuelve la vista parcial con el modal para editar o agregar un usuario.</returns>
        [HttpPost]
        public ActionResult ShowUser(int? userId = null)
        {
            try
            {
                List<UserRole> userRoles = ReadDataService.GetUserRoles();
                List<AreaData> areas = ReadDataService.GetAllAreas(true);
                if(userId.HasValue && userId.Value > 0)
                {
                    UserData userInformation = ReadDataService.GetUserById(userId.Value);
                    ViewBag.userInformation = userInformation;
                }

                ViewBag.userRoles = userRoles;
                ViewBag.areas = areas;
                ViewBag.userId = userId;
                return PartialView("AddEditUser");
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Método utilizado para agregar o actualizar un usuario (según sea el caso).
        /// </summary>
        /// <param name="userInformation">Objeto que contiene la información del usuario a agregar o editar según sea el caso.</param>
        /// <returns>Devuelve una bandera para determinar si la operación se realizó correctamente.</returns>
        [HttpPost]
        public ActionResult SaveEditUser(UserData userInformation)
        {
            bool successResponse = false;
            try
            {
                List<int> areasIds = new List<int>();

                // Construir la lista de ids asociados a las áreas del usuario.
                if (userInformation.Areas != null && userInformation.Areas.Count > 0)
                {
                    var allAreas = userInformation.Areas.Where(x => x.AreaId == 0).FirstOrDefault();
                    if (allAreas != null)
                    {
                        areasIds = new List<int> { allAreas.AreaId };
                    }
                    else
                    {
                        areasIds = userInformation.Areas.Select(x => x.AreaId).ToList();
                    }
                }

                // Guardar o actualizar el usuario (según sea el caso).
                if (userInformation.CollaboratorId != 0)
                {
                    successResponse = UpdateDataService.UpdateUserInformation(userInformation);
                    if (successResponse)
                    {
                        successResponse = UpdateDataService.UpdateUserAreas(areasIds, userInformation.CollaboratorId);
                    }
                }
                else
                {
                    int collaboratorId = SaveDataService.SaveUserInformation(userInformation);
                    if (collaboratorId != 0)
                    {
                        successResponse = SaveDataService.BulkInsertUserAreas(areasIds, collaboratorId);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return Json(new { successResponse });
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a un usuario.
        /// </summary>
        /// <param name="collaboratorId">Id asociado al usuario.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        [HttpPost]
        public ActionResult DeleteUserInformation(int collaboratorId)
        {
            bool successResponse = false;
            try
            {
                // Eliminar la relación entre usuario y área(s).
                successResponse = DeleteDataService.DeleteUserAreas(collaboratorId);
                if (successResponse)
                {
                    // Eliminar la información general del usuario.
                    successResponse = DeleteDataService.DeleteUserInformation(collaboratorId);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return Json(new { successResponse });
        }
    }
}