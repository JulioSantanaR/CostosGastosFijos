namespace Data.Models
{
    using System;

    /// <summary>
    /// Objeto que contiene la información referente al historial de carga de un archivo.
    /// </summary>
    public class FileLogData
    {
        /// <summary>
        /// Nombre asociado al archivo.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Fecha de carga del archivo.
        /// </summary>
        public DateTime ChargeDate { get; set; }

        /// <summary>
        /// Bandera para saber si el archivo ya está aprobado o no.
        /// </summary>
        public bool ApprovalFlag { get; set; }

        /// <summary>
        /// Id asociado al usuario que carga el archivo.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Id asociado al tipo de archivo que se está cargando.
        /// </summary>
        public int FileTypeId { get; set; }
    }
}