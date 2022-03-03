namespace Business.Services
{
    using System;
    using System.Collections.Generic;
    using Data.DAO;
    using Data.Models;
    using Data.Models.Request;
    using Data.Models.Response;
    using Data.Repositories;

    /// <summary>
    /// Clase intermedia entre el acceso a datos y la capa del cliente para manipular la información asociada al catálogo de áreas.
    /// </summary>
    public class AreasService
    {
        /// <summary>
        /// Método utilizado para recuperar todo el catálogo de áreas.
        /// </summary>
        /// <param name="includeAllAreas">Bandera para saber si incluir "Todas las áreas" en la consulta.</param>
        /// <returns>Devuelve la lista de todas las áreas dadas de alta en el catálogo.</returns>
        public static List<AreaData> GetAllAreas(bool includeAllAreas = false)
        {
            List<AreaData> areas = null;
            try
            {
                AreasDAO areasDao = new AreasDAO();
                areas = areasDao.GetAllAreas(includeAllAreas);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetAllAreas()." + "Error: " + ex.Message);
            }

            return areas;
        }

        /// <summary>
        /// Método utilizado para recuperar la información de un área de acuerdo al id asociado a esta.
        /// </summary>
        /// <param name="areaId">Id asociado al área.</param>
        /// <returns>Devuelve la información general del área.</returns>
        public static AreaData GetAreaById(int areaId)
        {
            AreaData areaInformation = null;
            try
            {
                AreasDAO areasDao = new AreasDAO();
                areaInformation = areasDao.GetAreaById(areaId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetAreaById()." + "Error: " + ex.Message);
            }

            return areaInformation;
        }

        /// <summary>
        /// Método utilizado para guardar la información asociada a un área dentro del catálogo.
        /// </summary>
        /// <param name="areaInformation">Objeto que contiene la información general del área.</param>
        /// <returns>Devuelve el id asociado al área recién insertada en la Base de Datos.</returns>
        public static int SaveAreaInformation(AreaData areaInformation)
        {
            int areaId = 0;
            try
            {
                AreasDAO areasDao = new AreasDAO();
                areaId = areasDao.SaveAreaInformation(areaInformation);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveAreaInformation()." + "Error: " + ex.Message);
            }

            return areaId;
        }

        /// <summary>
        /// Método utilizado para actualizar la información asociada a un área en el catálogo de la aplicación.
        /// </summary>
        /// <param name="areaInformation">Objeto que contiene la información general del área.</param>
        /// <returns>Devuelve una bandera para determinar si la actualización fue correcta.</returns>
        public static bool UpdateAreaInformation(AreaData areaInformation)
        {
            bool successUpdate = false;
            try
            {
                AreasDAO areasDao = new AreasDAO();
                successUpdate = areasDao.UpdateAreaInformation(areaInformation);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateAreaInformation()." + "Error: " + ex.Message);
            }

            return successUpdate;
        }

        /// <summary>
        /// Método utilizado para recuperar la información que será mostrada en la tabla del catálogo de áreas.
        /// </summary>
        /// <param name="dataTableInfo">Objeto que contiene los parámetros de búsqueda para la tabla de áreas.</param>
        /// <returns>Devuelve un objeto que contiene la información necesaria para mostrar la tabla de áreas.</returns>
        public static AreasTableResponse GetAreasTable(DataTableRequest dataTableInfo)
        {
            AreasTableResponse areasTable = null;
            try
            {
                AreasDAO areasDao = new AreasDAO();
                areasTable = areasDao.GetAreasTable(dataTableInfo);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetAreasTable()." + "Error: " + ex.Message);
            }

            return areasTable;
        }

        /// <summary>
        /// Método utilizado para eliminar la información general asociada a un área.
        /// </summary>
        /// <param name="areaId">Id asociado al área.</param>
        /// <returns>Devuelve una bandera para determinar si la información se eliminó correctamente.</returns>
        public static bool DeleteAreaInformation(int areaId)
        {
            bool successDelete = false;
            try
            {
                AreasDAO areasDao = new AreasDAO();
                successDelete = areasDao.DeleteAreaInformation(areaId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteAreaInformation()." + "Error: " + ex.Message);
            }

            return successDelete;
        }
    }
}