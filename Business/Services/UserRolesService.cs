namespace Business
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using Business.Services;
    using ClosedXML.Excel;
    using Data;
    using Data.Models;
    using Data.Models.Request;
    using Data.Models.Response;
    using Data.Repositories;

    /// <summary>
    /// Clase de negocio intermedia entre el acceso a datos y la capa del cliente para manipular información del catálogo de roles de usuario.
    /// </summary>
    public class UserRolesService
    {
        /// <summary>
        /// Método utilizado para recuperar el catálogo de roles de usuario.
        /// </summary>
        /// <returns>Devuelve el catálogo de roles de usuario disponibles.</returns>
        public static List<UserRole> GetUserRoles()
        {
            List<UserRole> userRoles = null;
            try
            {
                UserRolesDAO userRolesDao = new UserRolesDAO();
                userRoles = userRolesDao.GetUserRoles();
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetUserRoles()." + "Error: " + ex.Message);
            }

            return userRoles;
        }
    }
}