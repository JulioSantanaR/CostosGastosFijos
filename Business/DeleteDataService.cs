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
    }
}