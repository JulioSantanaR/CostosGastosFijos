namespace Business
{
    using System;
    using Data;
    using Data.Repositories;

    /// <summary>
    /// Clase que contiene los métodos utilizados para eliminar información dentro de la Base de Datos.
    /// </summary>
    public class DeleteDataService
    {
        /// <summary>
        /// Método utilizado para eliminar la información general asociada a un usuario.
        /// </summary>
        /// <param name="userId">Id asociado al usuario.</param>
        /// <returns>Devuelve una bandera para determinar si la información se eliminó correctamente.</returns>
        public static bool DeleteUserInformation(int userId)
        {
            bool successDelete = false;
            try
            {
                DeleteDataDAO connection = new DeleteDataDAO();
                successDelete = connection.DeleteUserInformation(userId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteUserInformation()." + "Error: " + ex.Message);
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar la relación entre un usuario y la(s) área(s) asociadas a este.
        /// </summary>
        /// <param name="userId">Id asociado al usuario.</param>
        /// <param name="areaId">Id asociado al área.</param>
        /// <returns>Devuelve una bandera para determinar si la eliminación de la información fue correcta o no.</returns>
        public static bool DeleteUserAreas(int? userId = null, int? areaId = null)
        {
            bool successDelete = false;
            try
            {
                DeleteDataDAO connection = new DeleteDataDAO();
                successDelete = connection.DeleteUserAreas(userId, areaId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteUserAreas()." + "Error: " + ex.Message);
            }

            return successDelete;
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
                DeleteDataDAO connection = new DeleteDataDAO();
                successDelete = connection.DeleteAreaInformation(areaId);
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