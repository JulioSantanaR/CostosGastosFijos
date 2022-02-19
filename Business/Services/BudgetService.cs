namespace Business.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Data.DAO;
    using Data.Models.Request;
    using Data.Repositories;

    /// <summary>
    /// Clase intermedia entre el acceso a datos y la capa del cliente para manipular la información asociada al presupuesto con las cuentas/cecos por colaborador.
    /// </summary>
    public static class BudgetService
    {
        /// <summary>
        /// Método utilizado para eliminar las cuentas asociadas a un año y tipo de carga específicos.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la eliminación de cuentas/Cecos.</param>
        /// <returns>Devuelve una bandera para determinar si la eliminación fue correcta o no.</returns>
        public static bool DeleteAccounts(AccountsDataRequest accountsData)
        {
            bool successDelete = false;
            try
            {
                BudgetDAO budgetDao = new BudgetDAO();
                successDelete = budgetDao.DeleteAccounts(accountsData);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteAccounts()." + "Error: " + ex.Message);
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar las cuentas asociadas a un ajuste de presupuesto manual.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la eliminación de cuentas/Cecos.</param>
        /// <returns>Devuelve una bandera para determinar si la eliminación fue correcta o no.</returns>
        public static bool DeleteManualBudget(AccountsDataRequest accountsData)
        {
            bool successDelete = false;
            try
            {
                BudgetDAO budgetDao = new BudgetDAO();
                successDelete = budgetDao.DeleteManualBudget(accountsData);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteManualBudget()." + "Error: " + ex.Message);
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar las cuentas/cecos dentro de la tabla de hechos de acuerdo a los parámetros enviados.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la eliminación de cuentas/Cecos.</param>
        /// <returns>Devuelve una bandera para determinar si la eliminación fue correcta o no.</returns>
        public static bool FactTableDeleteAccounts(AccountsDataRequest accountsData)
        {
            bool successDelete = false;
            try
            {
                BudgetDAO budgetDao = new BudgetDAO();
                successDelete = budgetDao.FactTableDeleteAccounts(accountsData);
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("FactTableDeleteAccounts()." + "Error: " + ex.Message);
            }

            return successDelete;
        }
    }
}