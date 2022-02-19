namespace Data.Models.Request
{
    using System.Web;

    /// <summary>
    /// Objeto auxiliar en el guardado de la información de la promotoria.
    /// </summary>
    public class PromotoriaDataRequest
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
    }
}