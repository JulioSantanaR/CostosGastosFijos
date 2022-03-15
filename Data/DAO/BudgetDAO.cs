namespace Data.DAO
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;
    using Data.Models;
    using Data.Models.Request;
    using Data.Repositories;

    /// <summary>
    /// Clase asociada al acceso a datos para manipular la información asociada al presupuesto con las cuentas/cecos por colaborador.
    /// </summary>
    public class BudgetDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        readonly string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public BudgetDAO()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Método utilizado para insertar el presupuesto general de las cuentas, de acuerdo a un año y tipo de carga específicos.
        /// </summary>
        /// <param name="saveAccountsData">Objeto auxiliar en el guardado de cuentas/Cecos desde un archivo.</param>
        /// <returns>Devuelve una bandera para determinar si la información se insertó correctamente o no.</returns>
        public bool InsertAccounts(AccountsDataRequest accountsData)
        {
            bool successInsert = false;
            try
            {
                if (accountsData != null)
                {
                    Open();
                    SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(Connection)
                    {
                        DestinationTableName = "[dbo].[Tbl_Presupuesto_General]",
                        BulkCopyTimeout = 400
                    };

                    // Agregar columnas pendientes.
                    DataTableAddColumn(accountsData.FileData, "anio", accountsData.YearAccounts);
                    DataTableAddColumn(accountsData.FileData, "cve_TipoCarga", accountsData.ChargeTypeAccounts);
                    DataTableAddColumn(accountsData.FileData, "cve_Colaborador", accountsData.Collaborator);
                    DataTableAddColumn(accountsData.FileData, "cve_Area", accountsData.Area);
                    DataTableAddColumn(accountsData.FileData, "cve_LogArchivo", accountsData.FileLogId);
                    sqlBulkCopy.ColumnMappings.Add("anio", "anio");
                    sqlBulkCopy.ColumnMappings.Add("cve_TipoCarga", "cve_TipoCarga");
                    sqlBulkCopy.ColumnMappings.Add("cve_Colaborador", "cve_Colaborador");
                    sqlBulkCopy.ColumnMappings.Add("cve_Area", "cve_Area");
                    sqlBulkCopy.ColumnMappings.Add("cve_LogArchivo", "cve_LogArchivo");

                    // Mapear columnas en el archivo hacia la tabla.
                    sqlBulkCopy.ColumnMappings.Add("Tipo de Gestion", "tipoGestion");
                    sqlBulkCopy.ColumnMappings.Add("Centro de Costo", "centroDeCosto");
                    sqlBulkCopy.ColumnMappings.Add("Cuenta", "cuenta");
                    sqlBulkCopy.ColumnMappings.Add("Segmento", "segmento");
                    sqlBulkCopy.ColumnMappings.Add("Enero", "enero");
                    sqlBulkCopy.ColumnMappings.Add("Febrero", "febrero");
                    sqlBulkCopy.ColumnMappings.Add("Marzo", "marzo");
                    sqlBulkCopy.ColumnMappings.Add("Abril", "abril");
                    sqlBulkCopy.ColumnMappings.Add("Mayo", "mayo");
                    sqlBulkCopy.ColumnMappings.Add("Junio", "junio");
                    sqlBulkCopy.ColumnMappings.Add("Julio", "julio");
                    sqlBulkCopy.ColumnMappings.Add("Agosto", "agosto");
                    sqlBulkCopy.ColumnMappings.Add("Septiembre", "septiembre");
                    sqlBulkCopy.ColumnMappings.Add("Octubre", "octubre");
                    sqlBulkCopy.ColumnMappings.Add("Noviembre", "noviembre");
                    sqlBulkCopy.ColumnMappings.Add("Diciembre", "diciembre");
                    sqlBulkCopy.WriteToServer(accountsData.FileData);
                    successInsert = true;
                }
            }
            catch (Exception ex)
            {
                successInsert = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("InsertAccounts()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para recuperar las cuentas/cecos que no tengan la información completa en el BIF.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la búsqueda de cuentas/cecos con información incompleta en el BIF.</param>
        /// <returns>Devuelve la lista de cuentas dentro del presupuesto cargado que no están completas en el BIF.</returns>
        public List<Accounts> GetNotFoundAccountsBIF(AccountsDataRequest accountsData)
        {
            List<Accounts> notFoundAccounts = new List<Accounts>();
            try
            {
                if (accountsData != null)
                {
                    Open();
                    SqlCommand sqlcmd = new SqlCommand("usp_CuentasIncompletasBIF", Connection);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.Parameters.AddWithValue("@anio", accountsData.YearAccounts);
                    sqlcmd.Parameters.AddWithValue("@tipo_carga", accountsData.ChargeTypeAccounts);
                    sqlcmd.Parameters.AddWithValue("@colaborador", accountsData.Collaborator);
                    sqlcmd.Parameters.AddWithValue("@area", accountsData.Area);
                    sqlcmd.CommandTimeout = 3600;
                    var reader = sqlcmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Accounts singleAccount = new Accounts();
                        singleAccount.Account = reader["cuenta"] != DBNull.Value ? reader["cuenta"].ToString() : string.Empty;
                        singleAccount.CostCenter = reader["centroDeCosto"] != DBNull.Value ? reader["centroDeCosto"].ToString() : string.Empty;
                        notFoundAccounts.Add(singleAccount);
                    }

                    reader.Close();
                    Close();
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetNotFoundAccountsBIF()." + "Error: " + ex.Message);
            }

            return notFoundAccounts;
        }

        /// <summary>
        /// Método utilizado para insertar las cuentas asociadas a un ajuste de presupuesto manual.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la eliminación de cuentas/Cecos.</param>
        /// <returns>Devuelve una bandera para determinar si la eliminación fue correcta o no.</returns>
        public bool InsertManualBudget(AccountsDataRequest accountsData)
        {
            bool successInsert = false;
            try
            {
                if (accountsData != null)
                {
                    Open();
                    SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(Connection)
                    {
                        DestinationTableName = "[dbo].[Tbl_Presupuesto_Ajuste]",
                        BulkCopyTimeout = 400
                    };

                    // Agregar columnas pendientes.
                    DataTableAddColumn(accountsData.FileData, "anio", accountsData.YearAccounts);
                    DataTableAddColumn(accountsData.FileData, "cve_TipoCarga", accountsData.ChargeTypeAccounts);
                    DataTableAddColumn(accountsData.FileData, "cve_Colaborador", accountsData.Collaborator);
                    DataTableAddColumn(accountsData.FileData, "cve_Area", accountsData.Area);
                    DataTableAddColumn(accountsData.FileData, "cve_LogArchivo", accountsData.FileLogId);
                    sqlBulkCopy.ColumnMappings.Add("anio", "anio");
                    sqlBulkCopy.ColumnMappings.Add("cve_TipoCarga", "cve_TipoCarga");
                    sqlBulkCopy.ColumnMappings.Add("cve_Colaborador", "cve_Colaborador");
                    sqlBulkCopy.ColumnMappings.Add("cve_Area", "cve_Area");
                    sqlBulkCopy.ColumnMappings.Add("cve_LogArchivo", "cve_LogArchivo");

                    // Mapear columnas en el archivo hacia la tabla.
                    sqlBulkCopy.ColumnMappings.Add("Tipo de Gestion", "tipoGestion");
                    sqlBulkCopy.ColumnMappings.Add("Centro de Costo", "centroDeCosto");
                    sqlBulkCopy.ColumnMappings.Add("Cuenta", "cuenta");
                    sqlBulkCopy.ColumnMappings.Add("Segmento", "segmento");
                    sqlBulkCopy.ColumnMappings.Add("Filtro", "filtro");
                    sqlBulkCopy.ColumnMappings.Add("Canal", "canal");
                    sqlBulkCopy.ColumnMappings.Add("Enero", "enero");
                    sqlBulkCopy.ColumnMappings.Add("Febrero", "febrero");
                    sqlBulkCopy.ColumnMappings.Add("Marzo", "marzo");
                    sqlBulkCopy.ColumnMappings.Add("Abril", "abril");
                    sqlBulkCopy.ColumnMappings.Add("Mayo", "mayo");
                    sqlBulkCopy.ColumnMappings.Add("Junio", "junio");
                    sqlBulkCopy.ColumnMappings.Add("Julio", "julio");
                    sqlBulkCopy.ColumnMappings.Add("Agosto", "agosto");
                    sqlBulkCopy.ColumnMappings.Add("Septiembre", "septiembre");
                    sqlBulkCopy.ColumnMappings.Add("Octubre", "octubre");
                    sqlBulkCopy.ColumnMappings.Add("Noviembre", "noviembre");
                    sqlBulkCopy.ColumnMappings.Add("Diciembre", "diciembre");

                    sqlBulkCopy.WriteToServer(accountsData.FileData);
                    successInsert = true;
                }
            }
            catch (Exception ex)
            {
                successInsert = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("InsertManualBudget()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para eliminar las cuentas asociadas a un año y tipo de carga específicos.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la eliminación de cuentas/Cecos.</param>
        /// <returns>Devuelve una bandera para determinar si la eliminación fue correcta o no.</returns>
        public bool DeleteAccounts(AccountsDataRequest accountsData)
        {
            bool successDelete = false;
            try
            {
                if (accountsData != null)
                {
                    Open();
                    SqlCommand sqlcmd = new SqlCommand();
                    StringBuilder query = new StringBuilder();
                    query.Append("DELETE [dbo].[Tbl_Presupuesto_General] WHERE 1 = 1 ");
                    if (accountsData.YearAccounts != 0)
                    {
                        query.Append(" AND anio = @anio ");
                        sqlcmd.Parameters.AddWithValue("@anio", accountsData.YearAccounts);
                    }

                    if (accountsData.ChargeTypeAccounts != 0)
                    {
                        query.Append(" AND cve_TipoCarga = @tipo_carga ");
                        sqlcmd.Parameters.AddWithValue("@tipo_carga", accountsData.ChargeTypeAccounts);
                    }

                    if (accountsData.Collaborator != 0)
                    {
                        query.Append(" AND cve_Colaborador = @collaborator ");
                        sqlcmd.Parameters.AddWithValue("@collaborator", accountsData.Collaborator);
                    }

                    if (accountsData.Area != 0)
                    {
                        query.Append(" AND cve_Area = @area ");
                        sqlcmd.Parameters.AddWithValue("@area", accountsData.Area);
                    }

                    if (accountsData.FileLogId != 0)
                    {
                        query.Append(" AND cve_LogArchivo = @fileLogId ");
                        sqlcmd.Parameters.AddWithValue("@fileLogId", accountsData.FileLogId);
                    }

                    sqlcmd.Connection = Connection;
                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = query.ToString();
                    sqlcmd.ExecuteNonQuery();
                    Close();
                    successDelete = true;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar las cuentas asociadas a un ajuste de presupuesto manual.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la eliminación de cuentas/Cecos.</param>
        /// <returns>Devuelve una bandera para determinar si la eliminación fue correcta o no.</returns>
        public bool DeleteManualBudget(AccountsDataRequest accountsData)
        {
            bool successDelete = false;
            try
            {
                if (accountsData != null)
                {
                    Open();
                    SqlCommand sqlcmd = new SqlCommand();
                    StringBuilder query = new StringBuilder();
                    query.Append("DELETE [dbo].[Tbl_Presupuesto_Ajuste] WHERE 1 = 1 ");
                    if (accountsData.YearAccounts != 0)
                    {
                        query.Append(" AND anio = @anio ");
                        sqlcmd.Parameters.AddWithValue("@anio", accountsData.YearAccounts);
                    }

                    if (accountsData.ChargeTypeAccounts != 0)
                    {
                        query.Append(" AND cve_TipoCarga = @tipo_carga ");
                        sqlcmd.Parameters.AddWithValue("@tipo_carga", accountsData.ChargeTypeAccounts);
                    }

                    if (accountsData.Collaborator != 0)
                    {
                        query.Append(" AND cve_Colaborador = @collaborator ");
                        sqlcmd.Parameters.AddWithValue("@collaborator", accountsData.Collaborator);
                    }

                    if (accountsData.Area != 0)
                    {
                        query.Append(" AND cve_Area = @area ");
                        sqlcmd.Parameters.AddWithValue("@area", accountsData.Area);
                    }

                    if (accountsData.FileLogId != 0)
                    {
                        query.Append(" AND cve_LogArchivo = @fileLogId ");
                        sqlcmd.Parameters.AddWithValue("@fileLogId", accountsData.FileLogId);
                    }

                    sqlcmd.Connection = Connection;
                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = query.ToString();
                    sqlcmd.ExecuteNonQuery();
                    Close();
                    successDelete = true;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para insertar información dentro de la tabla de hechos asociada al presupuesto general de cuentas.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en el guardado de cuentas/Cecos desde un archivo en la tabla de hechos.</param>
        /// <returns>Devuelve una bandera para determinar si la inserción de la información fue correcta o no.</returns>
        public bool SaveFactAccountsManual(AccountsDataRequest accountsData)
        {
            bool successInsert = false;
            try
            {
                if (accountsData != null)
                {
                    Open();
                    SqlCommand sqlcmd = new SqlCommand("[dbo].[usp_InsertarFact_AsignacionCanal_Manual]", Connection);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.Parameters.AddWithValue("@anio", accountsData.YearAccounts);
                    sqlcmd.Parameters.AddWithValue("@tipo_carga", accountsData.ChargeTypeAccounts);
                    sqlcmd.Parameters.AddWithValue("@colaborador", accountsData.Collaborator);
                    sqlcmd.Parameters.AddWithValue("@area", accountsData.Area);
                    sqlcmd.Parameters.AddWithValue("@logArchivo", accountsData.FileLogId);
                    sqlcmd.CommandTimeout = 3600;
                    sqlcmd.ExecuteNonQuery();
                    Close();
                    successInsert = true;
                }
            }
            catch (Exception ex)
            {
                successInsert = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("SaveFactAccountsManual()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para eliminar las cuentas/cecos dentro de la tabla de hechos de acuerdo a los parámetros enviados.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la eliminación de cuentas/Cecos.</param>
        /// <returns>Devuelve una bandera para determinar si la eliminación fue correcta o no.</returns>
        public bool FactTableDeleteAccounts(AccountsDataRequest accountsData)
        {
            bool successDelete = false;
            try
            {
                if (accountsData != null)
                {
                    Open();
                    SqlCommand sqlcmd = new SqlCommand();
                    StringBuilder query = new StringBuilder();
                    query.Append("DELETE [dbo].[Fact_CostosGastosFijos] WHERE 1 = 1 ");
                    if (accountsData.YearAccounts != 0)
                    {
                        query.Append(" AND anio = @anio ");
                        sqlcmd.Parameters.AddWithValue("@anio", accountsData.YearAccounts);
                    }

                    if (accountsData.ChargeTypeAccounts != 0)
                    {
                        query.Append(" AND cve_TipoCarga = @tipo_carga ");
                        sqlcmd.Parameters.AddWithValue("@tipo_carga", accountsData.ChargeTypeAccounts);
                    }

                    if (accountsData.Collaborator != 0)
                    {
                        query.Append(" AND cve_Colaborador = @collaborator ");
                        sqlcmd.Parameters.AddWithValue("@collaborator", accountsData.Collaborator);
                    }

                    if (accountsData.Area != 0)
                    {
                        query.Append(" AND cve_Area = @area ");
                        sqlcmd.Parameters.AddWithValue("@area", accountsData.Area);
                    }

                    if (accountsData.FileLogId != 0)
                    {
                        query.Append(" AND cve_LogArchivo = @fileLogId ");
                        sqlcmd.Parameters.AddWithValue("@fileLogId", accountsData.FileLogId);
                    }

                    sqlcmd.Connection = Connection;
                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = query.ToString();
                    sqlcmd.ExecuteNonQuery();
                    Close();
                    successDelete = true;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para actualizar la tabla de hechos asociada al presupuesto general de cuentas.
        /// </summary>
        /// <param name="accountsData">Objeto auxiliar en la actualización de cuentas/Cecos desde un archivo.</param>
        /// <returns>Devuelve una bandera para determinar si la actualización fue correcta o no.</returns>
        public bool UpdateFactTblAccounts(AccountsDataRequest accountsData)
        {
            bool successUpdate = false;
            try
            {
                if (accountsData != null)
                {
                    Open();
                    SqlCommand sqlcmd = new SqlCommand("[dbo].[usp_ActualizarFact_AsignacionCanal]", Connection);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.Parameters.AddWithValue("@anio", accountsData.YearAccounts);
                    sqlcmd.Parameters.AddWithValue("@tipo_carga", accountsData.ChargeTypeAccounts);
                    sqlcmd.Parameters.AddWithValue("@tipoEjercicio", accountsData.ExerciseType);
                    sqlcmd.CommandTimeout = 3600;
                    sqlcmd.ExecuteNonQuery();
                    Close();
                    successUpdate = true;
                }
            }
            catch (Exception ex)
            {
                successUpdate = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateFactTblAccounts()." + "Error: " + ex.Message);
            }

            return successUpdate;
        }

        /// <summary>
        /// Método utilizado para actualizar la tabla de hechos que tiene que ver con la proyección de los Costos y Gastos Fijos.
        /// </summary>
        /// <param name="yearAccounts">Año de carga.</param>
        /// <param name="chargeTypeAccounts">Tipo de carga.</param>
        /// <param name="exerciseType">Tipo de ejercicio que se está realizando (BP/Rolling).</param>
        /// <returns>Devuelve una bandera para determinar si la actualización fue correcta o no.</returns>
        public bool UpdateFactProjection(int yearAccounts, int chargeTypeAccounts, string exerciseType)
        {
            bool successUpdate = false;
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand("[dbo].[usp_ActualizarFact_Proyeccion]", Connection);
                sqlcmd.CommandType = CommandType.StoredProcedure;
                sqlcmd.Parameters.AddWithValue("@anio", yearAccounts);
                sqlcmd.Parameters.AddWithValue("@tipo_carga", chargeTypeAccounts);
                sqlcmd.Parameters.AddWithValue("@tipoEjercicio", exerciseType);
                sqlcmd.CommandTimeout = 3600;
                sqlcmd.ExecuteNonQuery();
                Close();
                successUpdate = true;
            }
            catch (Exception ex)
            {
                successUpdate = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateFactProjection()." + "Error: " + ex.Message);
            }

            return successUpdate;
        }
    }
}