namespace AppCostosGastosFijos.Models
{
    using System;
    using System.Collections.Generic;
    using Business;
    using Business.Services;
    using Data.Models;
    using Data.Repositories;

    /// <summary>
    /// Objeto auxiliar en la construcción de la información que se mostrará en la vista del historial de cargas.
    /// </summary>
    public class FileLogViewModel
    {
        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public FileLogViewModel()
        {
            try
            {
                FileTypeCatalog = FileTypeService.GetFileTypes();
                AreasCatalog = AreasService.GetAllAreas();
                ChargeTypes = CatalogService.GetChargeTypes();
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("FileLogViewModel()." + "Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Lista de objetos que contiene el catálogo de tipos de archivos.
        /// </summary>
        public List<FileType> FileTypeCatalog { get; set; }

        /// <summary>
        /// Lista de objetos que contiene el catálogo de áreas.
        /// </summary>
        public List<AreaData> AreasCatalog { get; set; }

        /// <summary>
        /// Lista de objetos que contiene los tipos de carga asociados a un ejercicio financiero.
        /// </summary>
        public List<ChargeType> ChargeTypes { get; set; }
    }
}