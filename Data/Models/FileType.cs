namespace Data.Models
{
    /// <summary>
    /// Objeto que contiene la información asociada al catálogo de tipo de archivos.
    /// </summary>
    public class FileType
    {
        /// <summary>
        /// Id asociado al tipo de archivo.
        /// </summary>
        public int FileTypeId { get; set; }

        /// <summary>
        /// Nombre asociado al tipo de archivo.
        /// </summary>
        public string FileTypeName { get; set; }
    }
}