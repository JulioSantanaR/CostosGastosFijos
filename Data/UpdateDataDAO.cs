namespace Data
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using Data.Models.Request;
    using Data.Repositories;

    /// <summary>
    /// Clase que contiene los métodos asociados a la operación de actualización de datos.
    /// </summary>
    public class UpdateDataDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public UpdateDataDAO()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Método utilizado para actualizar la tabla de hechos asociada al presupuesto general de cuentas.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la actualización de cuentas/Cecos desde un archivo.</param>
        /// <returns>Devuelve una bandera para determinar si la actualización fue correcta o no.</returns>
        public bool UpdateFactTblAccounts(AccountsDataRequest accountsData)
        {
            bool successUpdate = false;
            try
            {
                if (accountsData != null)
                {
                    Open();
                    SqlCommand sqlcmd = new SqlCommand("usp_ActualizarTblHechos", Connection);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.Parameters.AddWithValue("@anio", accountsData.YearAccounts);
                    sqlcmd.Parameters.AddWithValue("@tipo_carga", accountsData.ChargeTypeAccounts);
                    sqlcmd.Parameters.AddWithValue("@colaborador", accountsData.Collaborator);
                    sqlcmd.Parameters.AddWithValue("@area", accountsData.Area);
                    sqlcmd.Parameters.AddWithValue("@tipoEjercicio", accountsData.ExerciseType);
                    sqlcmd.CommandTimeout = 3600;
                    sqlcmd.ExecuteNonQuery();
                    Close();
                    successUpdate = true;
                }
            }
            catch (Exception ex)
            {
                successUpdate = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateFactTblAccounts()." + "Error: " + ex.Message);
            }

            return successUpdate;
        }

        /// <summary>
        /// Método utilizado para actualizar los montos debido a la carga de volumen de un BP/Rolling 0+12.
        /// </summary>
        /// <param name="yearAccounts">Año de carga.</param>
        /// <param name="chargeTypeAccounts">Tipo de carga.</param>
        /// <returns>Devuelve una bandera para determinar si la actualización fue correcta o no.</returns>
        public bool UpdateFactTblBP(int yearAccounts, int chargeTypeAccounts)
        {
            bool successUpdate = false;
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand("usp_ActualizarTblHechos_BP", Connection);
                sqlcmd.CommandType = CommandType.StoredProcedure;
                sqlcmd.Parameters.AddWithValue("@anio", yearAccounts);
                sqlcmd.Parameters.AddWithValue("@tipo_carga", chargeTypeAccounts);
                sqlcmd.CommandTimeout = 3600;
                sqlcmd.ExecuteNonQuery();
                Close();
                successUpdate = true;
            }
            catch (Exception ex)
            {
                successUpdate = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateFactTblAccounts()." + "Error: " + ex.Message);
            }

            return successUpdate;
        }

        /// <summary>
        /// Método utilizado para actualizar la tabla de hechos que tiene que ver con la proyección de los Costos y Gastos Fijos.
        /// </summary>
        /// <param name="yearAccounts">Año de carga.</param>
        /// <param name="chargeTypeAccounts">Tipo de carga.</param>
        /// <param name="exerciseType">Tipo de ejercicio que se está realizando (BP/Rolling).</param>
        /// <returns>Devuelve una bandera para determinar si la actualización fue correcta o no.</returns>
        public bool UpdateFactProjection(int yearAccounts, int chargeTypeAccounts, string exerciseType)
        {
            bool successUpdate = false;
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand("usp_ActualizarProyeccion", Connection);
                sqlcmd.CommandType = CommandType.StoredProcedure;
                sqlcmd.Parameters.AddWithValue("@anio", yearAccounts);
                sqlcmd.Parameters.AddWithValue("@tipo_carga", chargeTypeAccounts);
                sqlcmd.Parameters.AddWithValue("@tipoEjercicio", exerciseType);
                sqlcmd.CommandTimeout = 3600;
                sqlcmd.ExecuteNonQuery();
                Close();
                successUpdate = true;
            }
            catch (Exception ex)
            {
                successUpdate = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateFactProjection()." + "Error: " + ex.Message);
            }

            return successUpdate;
        }
    }
}