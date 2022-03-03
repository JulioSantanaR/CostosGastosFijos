namespace Business.Services
{
    using System;
    using Data.DAO;
    using Data.Repositories;

    /// <summary>
    /// Clase intermedia entre el acceso a datos y la capa del cliente para manipular la información asociada a los porcentajes por marca.
    /// </summary>
    public static class MixBrandPercentageService
    {
        /// <summary>
        /// Método utilizado para guardar la información de los porcentajes por marca a partir de los porcentajes base.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="chargeTypeName">Nombre asociado al tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public static bool SavePercentageByBrand(int yearData, int chargeTypeData, string chargeTypeName, int fileLogId)
        {
            bool successInsert = false;
            try
            {
                MixBrandPercentageDAO mixBrandPercentageDao = new MixBrandPercentageDAO();
                successInsert = mixBrandPercentageDao.SavePercentageByBrand(yearData, chargeTypeData, chargeTypeName, fileLogId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SavePercentageByBrand()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a los porcentajes por marca, de acuerdo a un año y tipo de carga específicos.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public static bool DeleteBrandMixPercentage(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                MixBrandPercentageDAO mixBrandPercentageDao = new MixBrandPercentageDAO();
                successDelete = mixBrandPercentageDao.DeleteBrandMixPercentage(yearData, chargeTypeData, fileLogId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteBrandMixPercentage()." + "Error: " + ex.Message);
            }

            return successDelete;
        }
    }
}