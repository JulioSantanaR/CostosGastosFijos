namespace Data.Models
{
    /// <summary>
    /// Objeto que contiene la información asociada a los tipos de carga para realizar un ejercicio financiero.
    /// </summary>
    public class ChargeType
    {
        /// <summary>
        /// Id asociado al tipo de carga.
        /// </summary>
        public int ChargeTypeId { get; set; }

        /// <summary>
        /// Nombre asociado al tipo de carga.
        /// </summary>
        public string ChargeTypeName { get; set; }
    }
}