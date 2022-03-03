namespace AppCostosGastosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using Business;
    using Business.Services;
    using Data.Models;
    using Data.Repositories;

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
            try
            {
                ChargeTypes = CatalogService.GetChargeTypes();
                FileTypeCatalog = FileTypeService.GetFileTypes(true);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("HomeViewModel()." + "Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Lista de objetos que contiene los tipos de carga asociados a un ejercicio financiero.
        /// </summary>
        public List<ChargeType> ChargeTypes { get; set; }

        /// <summary>
        /// Lista de objetos que contiene el catálogo de tipos de archivos.
        /// </summary>
        public List<FileType> FileTypeCatalog { get; set; }
    }
}