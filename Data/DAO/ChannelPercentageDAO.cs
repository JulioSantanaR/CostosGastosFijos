namespace Data.DAO
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using Data.Repositories;

    public class ChannelPercentageDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        readonly string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public ChannelPercentageDAO()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Método utilizado para insertar los porcentajes de acuerdo al año.
        /// </summary>
        /// <param name="yearAccounts">Año de carga.</param>
        /// <param name="chargeTypeAccounts">Tipo de carga.</param>
        /// <param name="exerciseType">Tipo de ejercicio que se está realizando (BP/Rolling).</param>
        /// <returns>Devuelve una bandera para determinar si la información se insertó correctamente o no.</returns>
        public bool InsertChannelPercentages(int yearAccounts, int chargeTypeAccounts, string exerciseType = "Rolling")
        {
            bool successInsert = false;
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand("usp_InsertarPorcentajesCanales", Connection);
                sqlcmd.CommandType = CommandType.StoredProcedure;
                sqlcmd.Parameters.AddWithValue("@anio", yearAccounts);
                sqlcmd.Parameters.AddWithValue("@tipo_carga", chargeTypeAccounts);
                sqlcmd.Parameters.AddWithValue("@tipoEjercicio", exerciseType);
                sqlcmd.CommandTimeout = 3600;
                sqlcmd.ExecuteNonQuery();
                Close();
                successInsert = true;
            }
            catch (Exception ex)
            {
                successInsert = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("InsertChannelPercentages()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a los porcentajes de acuerdo al año/tipo de ejercicio.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="exerciseType">Tipo de ejercicio que se está realizando (BP/Rolling)</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public bool DeleteChannelPercentages(int yearData, string exerciseType)
        {
            bool successDelete = false;
            try
            {
                string query = string.Format("DELETE [dbo].[Tbl_Asignacion_Canales_Porcentaje] WHERE anio = {0} AND tipoEjercicio = '{1}'", yearData, exerciseType);
                Open();
                SqlConnection connectionData = GetConnection();
                SqlCommand sqlcmd = new SqlCommand()
                {
                    Connection = connectionData,
                    CommandType = CommandType.Text,
                    CommandText = query
                };
                sqlcmd.ExecuteNonQuery();
                successDelete = true;
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteChannelPercentages()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successDelete;
        }
    }
}