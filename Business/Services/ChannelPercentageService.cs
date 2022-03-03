namespace Business.Services
{
    using System;
    using Data.DAO;
    using Data.Repositories;

    public static class ChannelPercentageService
    {
        /// <summary>
        /// Método utilizado para insertar los porcentajes de acuerdo al año.
        /// </summary>
        /// <param name="yearAccounts">Año de carga.</param>
        /// <param name="chargeTypeAccounts">Tipo de carga.</param>
        /// <param name="exerciseType">Tipo de ejercicio que se está realizando (BP/Rolling).</param>
        /// <returns>Devuelve una bandera para determinar si la información se insertó correctamente o no.</returns>
        public static bool InsertChannelPercentages(int yearAccounts, int chargeTypeAccounts, string exerciseType = "Rolling")
        {
            bool successInsert = false;
            try
            {
                ChannelPercentageDAO channelPercentageDao = new ChannelPercentageDAO();
                successInsert = channelPercentageDao.InsertChannelPercentages(yearAccounts, chargeTypeAccounts, exerciseType);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("InsertChannelPercentages()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a los porcentajes de acuerdo al año/tipo de ejercicio.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="exerciseType">Tipo de ejercicio que se está realizando (BP/Rolling)</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public static bool DeleteChannelPercentages(int yearData, string exerciseType)
        {
            bool successDelete = false;
            try
            {
                ChannelPercentageDAO channelPercentageDao = new ChannelPercentageDAO();
                successDelete = channelPercentageDao.DeleteChannelPercentages(yearData, exerciseType);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteChannelPercentages()." + "Error: " + ex.Message);
            }

            return successDelete;
        }
    }
}
