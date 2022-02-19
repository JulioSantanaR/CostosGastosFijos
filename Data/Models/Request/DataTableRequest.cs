namespace Data.Models.Request
{
    /// <summary>
    /// Objeto auxiliar para obtener información desde un DataTable
    /// </summary>
    public class DataTableRequest
    {
        /// <summary>
        /// Parámetro de búsqueda de servicios (dentro de una tabla)
        /// </summary>
        public string SearchValue { get; set; }

        /// <summary>
        /// Número de elementos que deseamos saltarnos en la consulta
        /// </summary>
        public int RowsToSkip { get; set; }

        /// <summary>
        /// Número de elementos que deseamos recuperar en la consulta
        /// </summary>
        public int NumberOfRows { get; set; }

        /// <summary>
        /// Saber si la consulta es para obtener el total de resultados en la Base de Datos
        /// </summary>
        public bool GetTotalRowsCount { get; set; }

        /// <summary>
        /// Saber si se desea obtener también el conteo de los usuarios o solo la consulta de info
        /// </summary>
        public bool MakeServicesCountQuery { get; set; }

        /// <summary>
        /// Nombre de la columna que se pretende ordenar
        /// </summary>
        public string SortName { get; set; }

        /// <summary>
        /// Orden de la columna (ASC o DESC)
        /// </summary>
        public string SortOrder { get; set; }

        /// <summary>
        /// Objeto que contiene los parámetros para buscar información en el historial de archivos.
        /// </summary>
        public LogFileRequest FileRequest { get; set; }
    }
}