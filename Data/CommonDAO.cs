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

        /// <summary>
        /// Método utilizado para determinar si la información obtenida de la Base de Datos contiene una columna específica.
        /// </summary>
        /// <param name="dr">Información leída desde la Base de Datos.</param>
        /// <param name="columnName">Nombre asociado a la columna a buscar en el data reader.</param>
        /// <returns>Devuelve una bandera para determinar si la información leída contiene la columna enviada.</returns>
        public bool HasColumn(IDataRecord dr, string columnName)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Método utilizado para agregar una columna adicional a un dataTable existente.
        /// </summary>
        /// <param name="dataTableObj">Objeto que contiene la información del dataTable.</param>
        /// <param name="columnName">Nombre de la columna a agregar.</param>
        /// <param name="defaultValue">Valor default a colocar en la nueva columna.</param>
        public void DataTableAddColumn(DataTable dataTableObj, string columnName, dynamic defaultValue = null)
        {
            DataColumn newColumn = null;
            if (defaultValue != null)
            {
                newColumn = new DataColumn(columnName, defaultValue.GetType()) { DefaultValue = defaultValue };
            }
            else
            {
                newColumn = new DataColumn(columnName);
            }

            dataTableObj.Columns.Add(newColumn);
        }
    }
}