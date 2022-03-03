namespace Business.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data.DAO;
    using Data.Models;
    using Data.Repositories;

    /// <summary>
    /// Clase intermedia entre el acceso a datos y la capa del cliente para manipular la información asociada a la relación usuario-áreas.
    /// </summary>
    public static class UserAreasService
    {
        /// <summary>
        /// Método utilizado para obtener las áreas asociadas a un usuario.
        /// </summary>
        /// <param name="username">Nombre de usuario.</param>
        /// <param name="userId">Id asociado al usuario.</param>
        /// <returns>Devuelve la lista de áreas asociadas a un usuario/colaborador.</returns>
        public static List<AreaData> UserAreas(string username = "", int? userId = null)
        {
            List<AreaData> userAreas = null;
            try
            {
                UserAreasDAO userAreasDao = new UserAreasDAO();
                userAreas = userAreasDao.UserAreas(username, userId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UserAreas()." + "Error: " + ex.Message);
            }

            return userAreas;
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
                UserAreasDAO userAreasDao = new UserAreasDAO();
                successDelete = userAreasDao.DeleteUserAreas(userId, areaId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteUserAreas()." + "Error: " + ex.Message);
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para guardar la relación entre un usuario y la(s) área(s) asociadas a este.
        /// </summary>
        /// <param name="userAreas">Lista de áreas asociadas a un usuario.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue guardada correctamente.</returns>
        public static bool BulkInsertUserAreas(List<int> areas, int userId)
        {
            bool successInsert = false;
            try
            {
                List<UserAreaRelation> userAreas = new List<UserAreaRelation>();
                if (areas != null && areas.Count > 0)
                {
                    for (int i = 0; i < areas.Count; i++)
                    {
                        UserAreaRelation singleUserArea = new UserAreaRelation
                        {
                            AreaId = areas[i],
                            UserId = userId
                        };
                        userAreas.Add(singleUserArea);
                    }
                }

                if (userAreas != null && userAreas.Count > 0)
                {
                    UserAreasDAO UserAreasDao = new UserAreasDAO();
                    successInsert = UserAreasDao.BulkInsertUserAreas(userAreas);
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertUserAreas()." + "Error: " + ex.Message);
            }

            return successInsert;
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
                var existingAreas = UserAreas(null, userId);
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
                        successUpdate = BulkInsertUserAreas(areasToAdd, userId);
                    }
                    else
                    {
                        successUpdate = true;
                    }
                }
                else
                {
                    successUpdate = BulkInsertUserAreas(areas, userId);
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
                    DeleteUserAreas(userId, singleArea);
                }
            }
        }
    }
}