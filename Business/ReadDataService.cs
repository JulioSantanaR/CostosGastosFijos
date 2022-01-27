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

        public static List<UserData> UserLogin(string username)
        {
            List<UserData> userInformation = null;
            try
            {
                ReadDataDAO connection = new ReadDataDAO();
                userInformation = connection.UserLogin(username);
                if (userInformation != null && userInformation.Count > 0)
                {
                    bool allAreasFlag = userInformation.Any(x => x.AreaId == 0);
                    if (allAreasFlag)
                    {
                        userInformation = connection.UserAllAreas(username);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return userInformation;
        }

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
    }
}