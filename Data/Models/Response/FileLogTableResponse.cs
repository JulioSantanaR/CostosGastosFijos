namespace Data.Models.Response
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Objeto tipo response que contiene la información que se mostrará en la tabla de "Historial de cargas".
    /// </summary>
    public class FileLogTableResponse
    {
        /// <summary>
        /// Lista de archivos cargados en la aplicación.
        /// </summary>
        public List<FileLogData> FileLogList { get; set; }

        /// <summary>
        /// Número total de archivos cargados en la aplicación.
        /// </summary>
        public int FileLogCount { get; set; }
    }
}
