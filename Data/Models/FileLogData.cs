namespace Data.Models
{
    using System;

    /// <summary>
    /// Objeto que contiene la información referente al historial de carga de un archivo.
    /// </summary>
    public class FileLogData
    {
        /// <summary>
        /// Id asociado al log del archivo.
        /// </summary>
        public int FileLogId { get; set; }

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

        /// <summary>
        /// Id asociada al área.
        /// </summary>
        public int AreaId { get; set; }

        /// <summary>
        /// Id asociado al tipo de carga.
        /// </summary>
        public int ChargeTypeId { get; set; }

        /// <summary>
        /// Año asociado al ejercicio.
        /// </summary>
        public int YearData { get; set; }

        /// <summary>
        /// Bandera para saber si guardar la información del área default.
        /// </summary>
        public bool DefaultArea { get; set; }

        /// <summary>
        /// Nombre asociado al tipo de archivo.
        /// </summary>
        public string FileTypeName { get; set; }

        /// <summary>
        /// Nombre asociado al colaborador que carga el archivo.
        /// </summary>
        public string CollaboratorName { get; set; }

        /// <summary>
        /// Nombre asociado al área donde se carga el archivo.
        /// </summary>
        public string AreaName { get; set; }

        /// <summary>
        /// Nombre asociado al tipo de carga.
        /// </summary>
        public string ChargeTypeName { get; set; }
    }
}