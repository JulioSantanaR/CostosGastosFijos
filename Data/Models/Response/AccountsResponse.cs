namespace Data.Models.Response
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Objeto tipo response utilizado para saber si se cargaron correctamente las cuentas o no.
    /// </summary>
    public class AccountsResponse
    {
        /// <summary>
        /// Bandera para determinar si el proceso fue correcto o no.
        /// </summary>
        public bool SuccessProcess { get; set; }

        /// <summary>
        /// Lista de cuentas que no se encuentran dadas de alta de manera completa en el BIF.
        /// </summary>
        public List<Accounts> NotFoundAccounts { get; set; }
    }
}