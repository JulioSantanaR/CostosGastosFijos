namespace AppCostosGastosFijos.Models
{
    using System.Collections.Generic;
    using Business;
    using Data.Models;

    /// <summary>
    /// Objeto auxiliar en la construcción de la información de la página principal del sitio.
    /// </summary>
    public class HomeViewModel
    {
        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public HomeViewModel()
        {
            ChargeTypes = CatalogService.GetChargeTypes();
        }

        /// <summary>
        /// Lista de objetos que contiene los tipos de carga asociados a un ejercicio financiero.
        /// </summary>
        public List<ChargeType> ChargeTypes { get; set; }
    }
}