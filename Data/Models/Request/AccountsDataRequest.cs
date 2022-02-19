namespace Data.Models.Request
{
    using System.Data;

    /// <summary>
    /// Objeto auxiliar en el guardado de cuentas/Cecos.
    /// </summary>
    public class AccountsDataRequest
    {
        /// <summary>
        /// Información asociada al archivo.
        /// </summary>
        public DataTable FileData { get; set; }

        /// <summary>
        /// Año de carga.
        /// </summary>
        public int YearAccounts { get; set; }

        /// <summary>
        /// Tipo de carga.
        /// </summary>
        public int ChargeTypeAccounts { get; set; }

        /// <summary>
        /// Nombre asociado al tipo de carga.
        /// </summary>
        public string ChargeTypeName { get; set; }

        /// <summary>
        /// Id asociado al colaborador.
        /// </summary>
        public int Collaborator { get; set; }

        /// <summary>
        /// Id asociado al área.
        /// </summary>
        public int Area { get; set; }

        /// <summary>
        /// Tipo de ejercicio.
        /// </summary>
        public string ExerciseType { get; set; }

        /// <summary>
        /// Nombre asociado al archivo que se está cargando.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Id asociado al archivo que se está cargando.
        /// </summary>
        public int FileLogId { get; set; }
    }
}