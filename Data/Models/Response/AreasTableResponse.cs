namespace Data.Models.Response
{
    using System.Collections.Generic;

    /// <summary>
    /// Objeto tipo response que contiene la información en tabla del catálogo de áreas.
    /// </summary>
    public class AreasTableResponse
    {
        /// <summary>
        /// Lista de áreas activas en la aplicación.
        /// </summary>
        public List<AreaData> AreasList { get; set; }

        /// <summary>
        /// Número total de áreas dadas de alta en la aplicación.
        /// </summary>
        public int AreasCount { get; set; }
    }
}