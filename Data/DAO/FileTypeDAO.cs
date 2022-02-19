namespace Data.DAO
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Data.Models;

    /// <summary>
    /// Clase asociada al acceso a datos para manipular la información del catálogo de tipos de archivos.
    /// </summary>
    public class FileTypeDAO : CommonDAO
    {
        /// <summary>
        /// Columnas asociadas al catálogo de "tipos de archivos".
        /// </summary>
        private readonly string[] FileTypeFields = new string[] { "cve_TipoArchivo", "tipoArchivo" };

        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public FileTypeDAO()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Método utilizado para recuperar el catálogo asociado a los tipos de archivos.
        /// </summary>
        /// <returns>Devuelve la lista de tipos de archivos dados de alta en la aplicación.</returns>
        public List<FileType> GetFileTypes()
        {
            List<FileType> fileTypeCatalog = new List<FileType>();
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT ").Append(string.Join(",", FileTypeFields));
                query.Append(" FROM [dbo].[Cat_TiposDeArchivos] ");
                query.Append(" ORDER BY tipoArchivo ASC ");
                Open();
                SqlCommand sqlcmd = new SqlCommand
                {
                    Connection = Connection,
                    CommandType = CommandType.Text,
                    CommandText = query.ToString()
                };
                var reader = sqlcmd.ExecuteReader();
                while (reader.Read())
                {
                    FileType singleFileType = Mapping.MapFileType(reader);
                    fileTypeCatalog.Add(singleFileType);
                }

                reader.Close();
                Close();
            }
            catch (Exception ex)
            {
                throw;
            }

            return fileTypeCatalog;
        }
    }
}