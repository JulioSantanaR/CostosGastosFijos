namespace Data.Models.Request
{
    /// <summary>
    /// Objeto que contiene los parámetros para buscar información en el historial de archivos.
    /// </summary>
    public class LogFileRequest
    {
        /// <summary>
        /// Id asociado al tipo de archivo.
        /// </summary>
        public int? FileTypeId { get; set; }

        /// <summary>
        /// Id asociado al área.
        /// </summary>
        public int? AreaId { get; set; }

        /// <summary>
        /// Año del ejercicio.
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Id asociado al tipo de carga.
        /// </summary>
        public int? ChargeTypeId { get; set; }

        /// <summary>
        /// Bandera para saber si el usuario es un colaborador o un administrador.
        /// </summary>
        public bool IsCollaborator { get; set; }

        /// <summary>
        /// Id asociado al colaborador.
        /// </summary>
        public int? CollaboratorId { get; set; }
    }
}