namespace AppCostosGastosFijos.Controllers
{
    using System;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;
    using Business.Services;
    using Data.Models;
    using Data.Models.Request;
    using Data.Models.Response;

    /// <summary>
    /// Controlador asociado a las operaciones sobre la administración de áreas.
    /// </summary>
    public class AreasController : Controller
    {
        /// <summary>
        /// Método utilizado para mostrar la vista principal asociada a la administración de áreas.
        /// </summary>
        /// <returns>Devuelve la vista principal de la administración de áreas.</returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Método utilizado para recuperar la tabla de áreas de acuerdo a las condiciones de búsqueda.
        /// </summary>
        /// <param name="dataTableInfo">Objeto que contiene los parámetros de búsqueda para la tabla de áreas.</param>
        /// <returns>Devuelve la lista de áreas de acuerdo a las condiciones de búsqueda.</returns>
        [HttpPost]
        public JsonResult GetAreas(DataTableRequest dataTableInfo)
        {
            string areasJsonFormat = string.Empty;
            int areasCount = 0;
            try
            {
                AreasTableResponse areasData = AreasService.GetAreasTable(dataTableInfo);
                if (areasData != null)
                {
                    if (areasData.AreasList != null && areasData.AreasList.Count > 0)
                    {
                        areasJsonFormat = new JavaScriptSerializer().Serialize(areasData.AreasList);
                    }

                    areasCount = areasData.AreasCount;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return Json(new { data = areasJsonFormat, recordsTotal = areasCount });
        }

        /// <summary>
        /// Método utilizado para mostrar el modal que contiene la información de un área o realizar la alta de una nuevo.
        /// </summary>
        /// <param name="areaId">Id asociado al área.</param>
        /// <returns>Devuelve la vista parcial con el modal para editar o agregar un área dentro del catálogo.</returns>
        [HttpPost]
        public ActionResult ShowArea(int? areaId = null)
        {
            try
            {
                if (areaId.HasValue && areaId.Value > 0)
                {
                    AreaData areaInformation = AreasService.GetAreaById(areaId.Value);
                    ViewBag.areaInformation = areaInformation;
                }

                ViewBag.areaId = areaId;
                return PartialView("AddEditArea");
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Método utilizado para agregar o actualizar un área en el catálogo (según sea el caso).
        /// </summary>
        /// <param name="userInformation">Objeto que contiene la información de un área para agregar o editar según sea el caso.</param>
        /// <returns>Devuelve una bandera para determinar si la operación se realizó correctamente.</returns>
        [HttpPost]
        public ActionResult SaveEditArea(AreaData areaInformation)
        {
            bool successResponse = false;
            try
            {
                if (areaInformation.AreaId != 0)
                {
                    successResponse = AreasService.UpdateAreaInformation(areaInformation);
                }
                else
                {
                    int areaId = AreasService.SaveAreaInformation(areaInformation);
                    successResponse = areaId != 0;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return Json(new { successResponse });
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada al área.
        /// </summary>
        /// <param name="areaId">Id asociado al área.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        [HttpPost]
        public ActionResult DeleteAreaInformation(int areaId)
        {
            bool successResponse = false;
            try
            {
                // Eliminar el área de la relación entre usuario/área.
                successResponse = UserAreasService.DeleteUserAreas(null, areaId);
                if (successResponse)
                {
                    // Eliminar la información general del área.
                    successResponse = AreasService.DeleteAreaInformation(areaId);
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