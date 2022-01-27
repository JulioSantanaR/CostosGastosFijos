namespace Data
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using Data.Repositories;

    /// <summary>
    /// Clase común en el acceso a datos, utilizada para abrir/cerrar la conexión a la Base de Datos.
    /// </summary>
    public class CommonDAO
    {
        /// <summary>
        /// Conexión SQL para acceder a la Base de Datos correspondiente.
        /// </summary>
        public SqlConnection Connection { get; set; }

        /// <summary>
        /// Cadena asociada a la conexión a la Base de Datos.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Devolver la cadena de conexión existente.
        /// </summary>
        public SqlConnection SqlConn
        {
            get { return Connection; }
        }

        /// <summary>
        /// Método utilizado para abrir la conexión a la Base de Datos.
        /// </summary>
        public void Open()
        {
            try
            {
                Connection = new SqlConnection
                {
                    ConnectionString = ConnectionString
                };
                Connection.Open();
            }
            catch (Exception err)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("Open()." + "Error: " + err.Message);
            }
        }

        /// <summary>
        /// Método utilizado para cerrar la conexión a la Base de Datos.
        /// </summary>
        public void Close()
        {
            try
            {
                if (Connection.State == ConnectionState.Open)
                {
                    Connection.Close();
                    Connection.Dispose();
                }
            }
            catch (Exception err)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("Close()." + "Error: " + err.Message);
            }
        }

        /// <summary>
        /// Método utilizado para obtener la conexión que esté abierta en ese momento.
        /// </summary>
        /// <returns>Devuelve el objeto asociado a la conexión actual.</returns>
        public SqlConnection GetConnection()
        {
            return Connection;
        }
    }
}