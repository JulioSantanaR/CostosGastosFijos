namespace Data
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.SqlClient;
    using Data.Models;
    using Data.Repositories;

    /// <summary>
    /// Clase de acceso a datos utilizado para manipular la Base de Datos que contiene los catálogos generales.
    /// </summary>
    public class CatalogDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos que contiene catálogos generales de JDVS.
        /// </summary>
        string connectionString = ConfigurationManager.ConnectionStrings["catalogs"].ToString();

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public CatalogDAO()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Método utilizado para recuperar el catálogo asociado a los tipos de carga de un ejercicio financiero.
        /// </summary>
        /// <returns>Lista de objetos que contiene los tipos de carga.</returns>
        public List<ChargeType> GetChargeTypes()
        {
            List<ChargeType> chargeTypes = new List<ChargeType>();
            try
            {
                string query = "SELECT cve_Tipo_Carga, tipoCarga FROM [dbo].[Cat_TiposCarga] ORDER BY PATINDEX('%[0-9]%', tipoCarga) ";
                Open();
                SqlCommand sqlcmd = new SqlCommand(query, Connection);
                SqlDataReader dataReader = sqlcmd.ExecuteReader();
                while (dataReader.Read())
                {
                    ChargeType singleChargeType = new ChargeType();
                    singleChargeType.ChargeTypeId = dataReader["cve_Tipo_Carga"] != DBNull.Value ? Convert.ToInt32(dataReader["cve_Tipo_Carga"]) : 0;
                    singleChargeType.ChargeTypeName = dataReader["tipoCarga"] != DBNull.Value ? dataReader["tipoCarga"].ToString() : string.Empty;
                    chargeTypes.Add(singleChargeType);
                }

                Close();
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetChargeTypes()." + "Error: " + ex.Message);
                chargeTypes = null;
            }

            return chargeTypes;
        }
    }
}