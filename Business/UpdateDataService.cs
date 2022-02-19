namespace Business
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Business.Services;
    using Data;
    using Data.Models;
    using Data.Repositories;

    /// <summary>
    /// Clase que contiene los métodos utilizados para actualizar información dentro de la Base de Datos.
    /// </summary>
    public static class UpdateDataService
    {
        /// <summary>
        /// Método utilizado para actualizar la información asociada a un usuario de la aplicación.
        /// </summary>
        /// <param name="userInformation">Objeto que contiene la información general del usuario.</param>
        /// <returns>Devuelve una bandera para determinar si la actualización fue correcta.</returns>
        public static bool UpdateUserInformation(UserData userInformation)
        {
            bool successUpdate = false;
            try
            {
                UpdateDataDAO connection = new UpdateDataDAO();
                successUpdate = connection.UpdateUserInformation(userInformation);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateUserInformation()." + "Error: " + ex.Message);
            }

            return successUpdate;
        }

        /// <summary>
        /// Método utilizado para agregar o eliminar nuevas áreas asociadas a un usuario.
        /// </summary>
        /// <param name="areas">Lista de áreas que se asociarán a un usuario.</param>
        /// <param name="userId">Id asociado al usuario.</param>
        /// <returns>Devuelve una bandera para determinar si el proceso concluyó correctamente.</returns>
        public static bool UpdateUserAreas(List<int> areas, int userId)
        {
            bool successUpdate = false;
            try
            {
                List<int> areasToDelete = new List<int>();
                List<int> areasToAdd = new List<int>();
                ReadDataDAO readData = new ReadDataDAO();
                var existingAreas = readData.UserAreas(null, userId);
                if (existingAreas != null && existingAreas.Count > 0)
                {
                    bool allAreas = areas.Any(x => x == 0);
                    if (allAreas)
                    {
                        var findExistingAllArea = existingAreas.Where(x => x.AreaId == 0).FirstOrDefault();
                        if (findExistingAllArea == null)
                        {
                            var auxAreasToAdd = 0;
                            areasToAdd.Add(auxAreasToAdd);
                        }

                        var auxAreasToDelete = existingAreas.Where(x => x.AreaId != 0).Select(x => x.AreaId).ToList();
                        areasToDelete.AddRange(auxAreasToDelete);
                    }
                    else
                    {
                        for (int i = 0; i < existingAreas.Count; i++)
                        {
                            var singleExisting = existingAreas[i];
                            var findArea = areas.Where(x => x == singleExisting.AreaId).FirstOrDefault();
                            if (findArea != 0)
                            {
                                areas.Remove(singleExisting.AreaId);
                            }
                            else
                            {
                                areasToDelete.Add(singleExisting.AreaId);
                            }
                        }

                        // Agregamos las nuevas áreas (que aún no están guardadas).
                        if (areas != null && areas.Count > 0)
                        {
                            areasToAdd.AddRange(areas);
                        }
                    }

                    // Eliminar las áreas que ya no estén en la selección actual.
                    if (areasToDelete != null && areasToDelete.Count > 0)
                    {
                        RemoveExistingAreas(areasToDelete, userId);
                    }

                    // Agregar las áreas nuevas.
                    if (areasToAdd != null && areasToAdd.Count > 0)
                    {
                        successUpdate = SaveDataService.BulkInsertUserAreas(areasToAdd, userId);
                    }
                    else
                    {
                        successUpdate = true;
                    }
                }
                else
                {
                    successUpdate = SaveDataService.BulkInsertUserAreas(areas, userId);
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateUserAreas()." + "Error: " + ex.Message);
            }

            return successUpdate;
        }

        /// <summary>
        /// Método auxiliar en la eliminación de áreas existentes en la Base de Datos.
        /// </summary>
        /// <param name="areas">Lista de áreas que se pretende eliminar.</param>
        /// <param name="userId">Id asociado al usuario.</param>
        private static void RemoveExistingAreas(List<int> existingAreas, int userId)
        {
            if (existingAreas != null && existingAreas.Count > 0)
            {
                for (int i = 0; i < existingAreas.Count; i++)
                {
                    var singleArea = existingAreas[i];
                    AreasService.DeleteUserAreas(userId, singleArea);
                }
            }
        }
    }
}