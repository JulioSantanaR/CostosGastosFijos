namespace Business.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Data.DAO;
    using Data.Models;
    using Data.Models.Request;
    using Data.Models.Response;
    using Data.Repositories;

    /// <summary>
    /// Clase intermedia entre el acceso a datos y la capa del cliente para manipular la información asociada a cada usuario.
    /// </summary>
    public static class UsersService
    {
        /// <summary>
        /// Método utilizado para recuperar la información de un colaborador de acuerdo a su username.
        /// </summary>
        /// <param name="username">Nombre de usuario del colaborador.</param>
        /// <returns>Devuelve un objeto con la información general del colaborador.</returns>
        public static UserData CollaboratorByUsername(string username)
        {
            UserData collaborator = null;
            try
            {
                UsersDAO usersDao = new UsersDAO();
                collaborator = usersDao.CollaboratorByUsername(username);
            }
            catch (Exception ex)
            {
            }

            return collaborator;
        }

        /// <summary>
        /// Método utilizado para recuperar los datos generales de un usuario.
        /// </summary>
        /// <param name="username">Nombre de usuario.</param>
        /// <returns>Devuelve un objeto que contiene la información general de un usuario.</returns>
        public static UserData UserLogin(string username)
        {
            UserData userInformation = null;
            try
            {
                UsersDAO usersDao = new UsersDAO();
                userInformation = usersDao.UserLogin(username);
                if (userInformation != null)
                {
                    userInformation.Areas = UserAreasService.UserAreas(userInformation.Username);
                    if (userInformation.Areas != null && userInformation.Areas.Count > 0)
                    {
                        bool allAreasFlag = userInformation.Areas.Any(x => x.AreaId == 0);
                        if (allAreasFlag)
                        {
                            userInformation.Areas = AreasService.GetAllAreas();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return userInformation;
        }

        /// <summary>
        /// Método utilizado para recuperar la información de un usuario de acuerdo al id asociado a este.
        /// </summary>
        /// <param name="userId">Id asociado al usuario.</param>
        /// <returns>Devuelve la información general del usuario.</returns>
        public static UserData GetUserById(int userId)
        {
            UserData userInformation = null;
            try
            {
                UsersDAO usersDao = new UsersDAO();
                userInformation = usersDao.GetUserById(userId);
                if (userInformation != null)
                {
                    userInformation.Areas = UserAreasService.UserAreas(userInformation.Username);
                    if (userInformation.Areas != null && userInformation.Areas.Count > 0)
                    {
                        var allAreasData = userInformation.Areas.Where(x => x.AreaId == 0).FirstOrDefault();
                        if (allAreasData != null)
                        {
                            userInformation.Areas = new List<AreaData>() { allAreasData };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetUserById()." + "Error: " + ex.Message);
            }

            return userInformation;
        }

        /// <summary>
        /// Método utilizado para recuperar la información que será mostrada en la tabla de usuarios.
        /// </summary>
        /// <param name="dataTableInfo">Objeto que contiene los parámetros de búsqueda para la tabla de usuarios.</param>
        /// <returns>Devuelve un objeto que contiene la información necesaria para mostrar la tabla de usuarios.</returns>
        public static UsersTableResponse GetUsersTable(DataTableRequest dataTableInfo)
        {
            UsersTableResponse usersTable = null;
            try
            {
                UsersDAO usersDao = new UsersDAO();
                usersTable = usersDao.GetUsersTable(dataTableInfo);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetUsersTable()." + "Error: " + ex.Message);
            }

            return usersTable;
        }

        /// <summary>
        /// Método utilizado para guardar la información asociada a un usuario de la aplicación.
        /// </summary>
        /// <param name="userInformation">Objeto que contiene la información general del usuario.</param>
        /// <returns>Devuelve el id asociado al usuario recién insertado en la Base de Datos.</returns>
        public static int SaveUserInformation(UserData userInformation)
        {
            int collaboratorId = 0;
            try
            {
                UsersDAO usersDao = new UsersDAO();
                collaboratorId = usersDao.SaveUserInformation(userInformation);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveUserInformation()." + "Error: " + ex.Message);
            }

            return collaboratorId;
        }

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
                UsersDAO usersDao = new UsersDAO();
                successUpdate = usersDao.UpdateUserInformation(userInformation);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateUserInformation()." + "Error: " + ex.Message);
            }

            return successUpdate;
        }

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
                UsersDAO usersDao = new UsersDAO();
                successDelete = usersDao.DeleteUserInformation(userId);
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