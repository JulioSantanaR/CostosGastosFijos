namespace Data.Models
{
    /// <summary>
    /// Objeto que contiene la información asociada a cada área.
    /// </summary>
    public class AreaData
    {
        /// <summary>
        /// Id asociado al área.
        /// </summary>
        public int AreaId { get; set; }

        /// <summary>
        /// Nombre del área.
        /// </summary>
        public string NameArea { get; set; }

        /// <summary>
        /// Bandera para saber si es un área default.
        /// </summary>
        public bool DefaultArea { get; set; }
    }
}