namespace Data.Models.Request
{
    /// <summary>
    /// Objeto tipo request utilizado para eliminar la información asociada a un archivo dentro del historial de cargas.
    /// </summary>
    public class DeleteFileRequest
    {
        /// <summary>
        /// Id asociado al archivo.
        /// </summary>
        public int LogFileId { get; set; }

        /// <summary>
        /// Nombre asociado al tipo de archivo.
        /// </summary>
        public string LogFileType { get; set; }

        /// <summary>
        /// Año del ejercicio.
        /// </summary>
        public int YearData { get; set; }

        /// <summary>
        /// Id asociado al tipo de carga del ejercicio.
        /// </summary>
        public int ChargeTypeData { get; set; }

        /// <summary>
        /// Nombre del tipo de carga.
        /// </summary>
        public string ChargeTypeName { get; set; }
    }
}