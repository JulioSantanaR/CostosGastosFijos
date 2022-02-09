namespace Business
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ClosedXML.Excel;
    using Data;
    using Data.Models;
    using Data.Models.Request;
    using Data.Models.Response;
    using Data.Repositories;

    /// <summary>
    /// Clase de negocio intermedia entre el acceso a datos y la capa del cliente para leer distinta información en la Base de Datos.
    /// </summary>
    public class ReadDataService
    {
        public static List<string> ReviewAccounts(DataTable fileData, string fileExtension, int areaId)
        {
            List<string> notAllowedAccount = null;
            try
            {
                var accounts = (from rw in fileData.AsEnumerable()
                                select new Accounts()
                                {
                                    Account = rw["Cuenta"].ToString(),
                                    CostCenter = rw["Centro de Costo"].ToString()
                                }).ToList();
                var uniqueAccounts = accounts.Select(x => x.Account).Distinct().ToList();

                List<int> areas = areaId != 0 ? new List<int>() { areaId } : null;
                ReadDataDAO connection = new ReadDataDAO();
                List<AccountArea> reviewAccount = connection.ReviewAccounts(uniqueAccounts, areas);
                if (reviewAccount != null && reviewAccount.Count > 0)
                {
                    notAllowedAccount = uniqueAccounts.Except(reviewAccount.Select(x => x.Account).Distinct().ToList()).ToList();
                }
                else
                {
                    notAllowedAccount = uniqueAccounts;
                }
            }
            catch (Exception ex)
            {
            }

            return notAllowedAccount;
        }

        public static string GenerateProjection()
        {
            string fileResult = string.Empty;
            try
            {
                XLWorkbook excelWorkbook = new XLWorkbook();

                // Recuperar la proyección de ADES.
                DataTable adesProjection = AdesProjection();
                if (adesProjection.Rows.Count > 0)
                {
                    excelWorkbook.Worksheets.Add(adesProjection, "ADES");
                }

                // Recuperar la proyección de Stills.
                DataTable stillsMod = StillsProjection("moderno");
                DataTable stillsEmb = StillsProjection("embotellador");
                if (stillsMod.Rows.Count > 0 || stillsEmb.Rows.Count > 0)
                {
                    stillsMod.Merge(stillsEmb);
                    excelWorkbook.Worksheets.Add(stillsMod, "STILLS");
                }

                // Recuperar la proyección de Lácteos.
                DataTable lacteos = LacteosProjection();
                if (lacteos.Rows.Count > 0)
                {
                    excelWorkbook.Worksheets.Add(lacteos, "LACTEOS");
                }

                // Obtener la información del archivo.
                Stream fileStream = new MemoryStream();
                excelWorkbook.SaveAs(fileStream);
                fileStream.Position = 0;
                if (fileStream.Length > 0)
                {
                    MemoryStream ms = new MemoryStream();
                    fileStream.CopyTo(ms);
                    byte[] bytes = ms.ToArray();
                    fileResult = Convert.ToBase64String(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
            }

            return fileResult;
        }

        public static DataTable AdesProjection()
        {
            DataTable adesData = new DataTable();
            try
            {
                ReadDataDAO connection = new ReadDataDAO();
                adesData = connection.AdesProjection();
            }
            catch (Exception ex)
            {
            }

            return adesData;
        }

        public static DataTable StillsProjection(string channel)
        {
            DataTable stillsData = new DataTable();
            try
            {
                ReadDataDAO connection = new ReadDataDAO();
                stillsData = connection.StillsProjection(channel);
            }
            catch (Exception ex)
            {
                throw;
            }

            return stillsData;
        }

        public static DataTable LacteosProjection()
        {
            DataTable lacteosData = new DataTable();
            try
            {
                ReadDataDAO connection = new ReadDataDAO();
                lacteosData = connection.LacteosProjection();
            }
            catch (Exception ex)
            {
                throw;
            }

            return lacteosData;
        }

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
                ReadDataDAO connection = new ReadDataDAO();
                collaborator = connection.CollaboratorByUsername(username);
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
                ReadDataDAO connection = new ReadDataDAO();
                userInformation = connection.UserLogin(username);
                if (userInformation != null)
                {
                    userInformation.Areas = connection.UserAreas(userInformation.Username);
                    if (userInformation.Areas != null && userInformation.Areas.Count > 0)
                    {
                        bool allAreasFlag = userInformation.Areas.Any(x => x.AreaId == 0);
                        if (allAreasFlag)
                        {
                            userInformation.Areas = connection.GetAllAreas();
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
        /// Método utilizado para recuperar la información de la promotoría de acuerdo a un año y tipo de ejercicio específicos.
        /// </summary>
        /// <param name="yearData">Año del ejercicio.</param>
        /// <param name="chargeTypeData">Tipo de ejercicio.</param>
        /// <returns>Devuelve la información asociada al detalle del presupuesto correspondiente a la promotoría.</returns>
        public static List<PromotoriaDB> GetPromotoria(int yearData, int chargeTypeData)
        {
            List<PromotoriaDB> promotoria = new List<PromotoriaDB>();
            try
            {
                ReadDataDAO connection = new ReadDataDAO();
                promotoria = connection.GetPromotoria(yearData, chargeTypeData);
            }
            catch (Exception ex)
            {
            }

            return promotoria;
        }

        /// <summary>
        /// Método utilizado para recuperar las cuentas/cecos que no tengan la información completa en el BIF.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la búsqueda de cuentas/cecos con información incompleta en el BIF.</param>
        /// <returns>Devuelve la lista de cuentas dentro del presupuesto cargado que no están completas en el BIF.</returns>
        public static List<Accounts> GetNotFoundAccountsBIF(AccountsDataRequest accountsData)
        {
            List<Accounts> notFoundAccounts = new List<Accounts>();
            try
            {
                ReadDataDAO connection = new ReadDataDAO();
                notFoundAccounts = connection.GetNotFoundAccountsBIF(accountsData);
            }
            catch (Exception ex)
            {
            }

            return notFoundAccounts;
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
                ReadDataDAO connection = new ReadDataDAO();
                usersTable = connection.GetUsersTable(dataTableInfo);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetUsersTable()." + "Error: " + ex.Message);
            }

            return usersTable;
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
                ReadDataDAO connection = new ReadDataDAO();
                areasTable = connection.GetAreasTable(dataTableInfo);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetAreasTable()." + "Error: " + ex.Message);
            }

            return areasTable;
        }

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
                ReadDataDAO connection = new ReadDataDAO();
                areas = connection.GetAllAreas(includeAllAreas);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetAllAreas()." + "Error: " + ex.Message);
            }

            return areas;
        }

        /// <summary>
        /// Método utilizado para recuperar el catálogo de roles de usuario.
        /// </summary>
        /// <returns>Devuelve el catálogo de roles de usuario disponibles.</returns>
        public static List<UserRole> GetUserRoles()
        {
            List<UserRole> userRoles = null;
            try
            {
                ReadDataDAO connection = new ReadDataDAO();
                userRoles = connection.GetUserRoles();
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetUserRoles()." + "Error: " + ex.Message);
            }

            return userRoles;
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
                ReadDataDAO connection = new ReadDataDAO();
                userInformation = connection.GetUserById(userId);
                if (userInformation != null)
                {
                    userInformation.Areas = connection.UserAreas(userInformation.Username);
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
        /// Método utilizado para recuperar la información de un área de acuerdo al id asociado a esta.
        /// </summary>
        /// <param name="areaId">Id asociado al área.</param>
        /// <returns>Devuelve la información general del área.</returns>
        public static AreaData GetAreaById(int areaId)
        {
            AreaData areaInformation = null;
            try
            {
                ReadDataDAO connection = new ReadDataDAO();
                areaInformation = connection.GetAreaById(areaId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetAreaById()." + "Error: " + ex.Message);
            }

            return areaInformation;
        }
    }
}