namespace Data.Models.Request
{
    using System.Data;
    using System.Web;

    /// <summary>
    /// Objeto auxiliar en el guardado de la información de los porcentajes base.
    /// </summary>
    public class BasePercentageRequest
    {
        /// <summary>
        /// Información asociada al archivo que se está cargando.
        /// </summary>
        public HttpPostedFileBase FileData { get; set; }

        /// <summary>
        /// Id asociado al colaborador.
        /// </summary>
        public int Collaborator { get; set; }

        /// <summary>
        /// Año de carga.
        /// </summary>
        public int YearData { get; set; }

        /// <summary>
        /// Tipo de carga.
        /// </summary>
        public int ChargeType { get; set; }

        /// <summary>
        /// Nombre asociado al tipo de carga.
        /// </summary>
        public string ChargeTypeName { get; set; }

        /// <summary>
        /// DataTable con la información de los porcentajes.
        /// </summary>
        public DataTable PercentagesTable { get; set; }

        /// <summary>
        /// Id asociado al archivo cargado.
        /// </summary>
        public int FileLogId { get; set; }

        /// <summary>
        /// Portafolio/clasificación del porcentaje.
        /// </summary>
        public string Portafolio { get; set; }
    }
}