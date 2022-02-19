namespace Business.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Data.DAO;
    using Data.Models;
    using Data.Repositories;

    public static class PromotoriaService
    {
        /// <summary>
        /// Método utilizado para guardar la base asociada a la promotoria de cada portafolio.
        /// </summary>
        /// <param name="promotoriaTable">Objeto que contiene la información de la base de la promotoria.</param>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public static bool BulkInsertPromotoria(DataTable promotoriaTable, int yearData, int chargeTypeData, int fileLogId)
        {
            bool successInsert = false;
            try
            {
                PromotoriaDAO promotoriaDao = new PromotoriaDAO();
                successInsert = promotoriaDao.BulkInsertPromotoria(promotoriaTable, yearData, chargeTypeData, fileLogId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertPromotoria()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a la promotoria, de acuerdo a un año y tipo de carga específicos.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public static bool DeletePromotoria(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                PromotoriaDAO promotoriaDao = new PromotoriaDAO();
                successDelete = promotoriaDao.DeletePromotoria(yearData, chargeTypeData, fileLogId);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeletePromotoria()." + "Error: " + ex.Message);
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para recuperar la información de la promotoría de acuerdo a un año y tipo de ejercicio específicos.
        /// </summary>
        /// <param name="yearData">Año del ejercicio.</param>
        /// <param name="chargeTypeData">Tipo de ejercicio.</param>
        /// <returns>Devuelve la información asociada al detalle del presupuesto correspondiente a la promotoría.</returns>
        public static List<PromotoriaDB> GetPromotoria(int yearData, int chargeTypeData)
        {
            List<PromotoriaDB> promotoria = new List<PromotoriaDB>();
            try
            {
                PromotoriaDAO promotoriaDao = new PromotoriaDAO();
                promotoria = promotoriaDao.GetPromotoria(yearData, chargeTypeData);
            }
            catch (Exception ex)
            {
            }

            return promotoria;
        }
    }
}