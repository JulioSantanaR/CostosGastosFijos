namespace Data.DAO
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;
    using Data.Repositories;

    /// <summary>
    /// Clase asociada al acceso a datos para manipular la información asociada a los porcentajes por marca.
    /// </summary>
    public class MixBrandPercentageDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public MixBrandPercentageDAO()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Método utilizado para guardar la información de los porcentajes por marca a partir de los porcentajes base.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="chargeTypeName">Nombre asociado al tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public bool SavePercentageByBrand(int yearData, int chargeTypeData, string chargeTypeName, int fileLogId)
        {
            bool successInsert;
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand("[dbo].[usp_InsertarPorcentaje_Marca]", Connection);
                sqlcmd.CommandType = CommandType.StoredProcedure;
                sqlcmd.Parameters.AddWithValue("@anio", yearData);
                sqlcmd.Parameters.AddWithValue("@tipo_carga", chargeTypeData);
                sqlcmd.Parameters.AddWithValue("@tipoEjercicio", chargeTypeName);
                sqlcmd.Parameters.AddWithValue("@logId", fileLogId);
                sqlcmd.CommandTimeout = 3600;
                sqlcmd.ExecuteNonQuery();
                Close();
                successInsert = true;
            }
            catch (Exception ex)
            {
                successInsert = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SavePercentageByBrand()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a los porcentajes por marca, de acuerdo a un año y tipo de carga específicos.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public bool DeleteBrandMixPercentage(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                SqlCommand sqlcmd = new SqlCommand();
                StringBuilder query = new StringBuilder();
                query.Append("DELETE [dbo].[Tbl_PorcentajeMix_Marca] WHERE 1 = 1 ");
                if (yearData.HasValue && yearData.Value > 0)
                {
                    query.Append(" AND anio = @yearData ");
                    sqlcmd.Parameters.AddWithValue("@yearData", yearData.Value);
                }

                if (chargeTypeData.HasValue && chargeTypeData.Value > 0)
                {
                    query.Append(" AND cve_TipoCarga = @chargeTypeData ");
                    sqlcmd.Parameters.AddWithValue("@chargeTypeData", chargeTypeData.Value);
                }

                if (fileLogId.HasValue && fileLogId.Value > 0)
                {
                    query.Append(" AND cve_LogArchivo = @fileLogId ");
                    sqlcmd.Parameters.AddWithValue("@fileLogId", fileLogId.Value);
                }

                Open();
                SqlConnection connectionData = GetConnection();
                sqlcmd.Connection = connectionData;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = query.ToString();
                sqlcmd.ExecuteNonQuery();
                successDelete = true;
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteBasePercentage()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successDelete;
        }
    }
}
