namespace Business
{
    using System;
    using System.Collections.Generic;
    using Data;
    using Data.Models;
    using Data.Repositories;

    /// <summary>
    /// Clase de negocio intermedia entre el acceso a datos y la capa del cliente para recuperación de catálogos.
    /// </summary>
    public static class CatalogService
    {
        /// <summary>
        /// Método utilizado para recuperar el catálogo asociado a los tipos de carga de un ejercicio financiero.
        /// </summary>
        /// <returns>Lista de objetos que contiene los tipos de carga.</returns>
        public static List<ChargeType> GetChargeTypes()
        {
            List<ChargeType> chargeTypes = new List<ChargeType>();
            try
            {
                CatalogDAO catalogDAO = new CatalogDAO();
                chargeTypes = catalogDAO.GetChargeTypes();
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetChargeTypes()." + "Error: " + ex.Message);
            }

            return chargeTypes;
        }
    }
}