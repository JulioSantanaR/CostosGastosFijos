namespace Data
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
    using FastMember;

    /// <summary>
    /// Clase de acceso a datos utilizado para manipular la información dentro de la Base de Datos.
    /// </summary>
    public class SaveDataDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public SaveDataDAO()
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
                    sqlBulkCopy.ColumnMappings.Add("anio", "anio");
                    sqlBulkCopy.ColumnMappings.Add("cve_TipoCarga", "cve_TipoCarga");
                    sqlBulkCopy.ColumnMappings.Add("cve_Colaborador", "cve_Colaborador");
                    sqlBulkCopy.ColumnMappings.Add("cve_Area", "cve_Area");

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
        /// Método utilizado para insertar los porcentajes de acuerdo al año.
        /// </summary>
        /// <param name="yearAccounts">Año de carga.</param>
        /// <param name="chargeTypeAccounts">Tipo de carga.</param>
        /// <param name="exerciseType">Tipo de ejercicio que se está realizando (BP/Rolling).</param>
        /// <returns>Devuelve una bandera para determinar si la información se insertó correctamente o no.</returns>
        public bool InsertChannelPercentages(int yearAccounts, int chargeTypeAccounts, string exerciseType = "Rolling")
        {
            bool successInsert = false;
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand("usp_InsertarPorcentajesCanales", Connection);
                sqlcmd.CommandType = CommandType.StoredProcedure;
                sqlcmd.Parameters.AddWithValue("@anio", yearAccounts);
                sqlcmd.Parameters.AddWithValue("@tipo_carga", chargeTypeAccounts);
                sqlcmd.Parameters.AddWithValue("@tipoEjercicio", exerciseType);
                sqlcmd.CommandTimeout = 3600;
                sqlcmd.ExecuteNonQuery();
                Close();
                successInsert = true;
            }
            catch (Exception ex)
            {
                successInsert = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("InsertChannelPercentages()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para guardar la información asociada a los porcentajes de cada canal.
        /// </summary>
        /// <param name="percentageTable">Objeto que contiene la información de los porcentajes.</param>
        /// <param name="yearPercentage">Año de carga.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public bool BulkInsertPercentage(DataTable percentageTable, int yearPercentage)
        {
            bool successInsert = false;
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand("usp_ActualizarPorcentaje", Connection);
                sqlcmd.CommandType = CommandType.StoredProcedure;
                sqlcmd.Parameters.AddWithValue("@Percentage", percentageTable);
                sqlcmd.Parameters.AddWithValue("@anio", yearPercentage);
                sqlcmd.ExecuteNonQuery();
                Close();
                successInsert = true;
            }
            catch (Exception ex)
            {
                successInsert = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertPercentage()." + "Error: " + ex.Message);
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para guardar la base asociada al volumen.
        /// </summary>
        /// <param name="volumeTable">Objeto que contiene la información de la base del volumen.</param>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public bool BulkInsertVolumen(DataTable volumeTable, int yearData, int chargeTypeData)
        {
            bool successInsert = false;
            try
            {
                Open();
                SqlConnection connectionData = GetConnection();
                SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connectionData)
                {
                    DestinationTableName = "dbo.Tbl_Volumen",
                    BulkCopyTimeout = 400
                };

                // Agregar columnas pendientes.
                DataTableAddColumn(volumeTable, "anio", yearData);
                DataTableAddColumn(volumeTable, "cve_TipoCarga", chargeTypeData);

                // Mapear columnas en el archivo hacia la tabla.
                sqlBulkCopy.ColumnMappings.Add("Canal", "canal");
                sqlBulkCopy.ColumnMappings.Add("Cadena Suministro", "cadenaSuministro");
                sqlBulkCopy.ColumnMappings.Add("Marca", "marca");
                sqlBulkCopy.ColumnMappings.Add("Formato Cadena", "formatoCadena");
                sqlBulkCopy.ColumnMappings.Add("SKU", "sku");
                sqlBulkCopy.ColumnMappings.Add("Descripcion SKU", "descripcionSKU");
                sqlBulkCopy.ColumnMappings.Add("Subcategoria", "subcategoria");
                sqlBulkCopy.ColumnMappings.Add("SKU Descripcion SKU", "skuDescripcionSKU");
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
                sqlBulkCopy.ColumnMappings.Add("anio", "anio");
                sqlBulkCopy.ColumnMappings.Add("cve_TipoCarga", "cve_TipoCarga");

                sqlBulkCopy.WriteToServer(volumeTable);
                successInsert = true;
            }
            catch (Exception ex)
            {
                successInsert = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertVolumen()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para guardar la base asociada al volumen para un BP/Rolling 0+12.
        /// </summary>
        /// <param name="volumeTable">Objeto que contiene la información de la base del volumen.</param>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public bool BulkInsertVolumenBP(DataTable volumeTable, int yearData, int chargeTypeData)
        {
            bool successInsert = false;
            try
            {
                Open();
                SqlConnection connectionData = GetConnection();
                SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connectionData)
                {
                    DestinationTableName = "[dbo].[Tbl_Volumen_BP]",
                    BulkCopyTimeout = 400
                };

                // Agregar columnas pendientes.
                DataTableAddColumn(volumeTable, "anio", yearData);
                DataTableAddColumn(volumeTable, "cve_TipoCarga", chargeTypeData);

                // Mapear columnas en el archivo hacia la tabla.
                sqlBulkCopy.ColumnMappings.Add("Key Tiendas", "keyTiendas");
                sqlBulkCopy.ColumnMappings.Add("Canal", "canal");
                sqlBulkCopy.ColumnMappings.Add("Cadena Suministro", "cadenaSuministro");
                sqlBulkCopy.ColumnMappings.Add("Formato", "formato");
                sqlBulkCopy.ColumnMappings.Add("Tienda", "tienda");
                sqlBulkCopy.ColumnMappings.Add("SKU", "sku");
                sqlBulkCopy.ColumnMappings.Add("Enero Volumen", "eneroVolumen");
                sqlBulkCopy.ColumnMappings.Add("Enero Ingresos", "eneroIngresos");
                sqlBulkCopy.ColumnMappings.Add("Febrero Volumen", "febreroVolumen");
                sqlBulkCopy.ColumnMappings.Add("Febrero Ingresos", "febreroIngresos");
                sqlBulkCopy.ColumnMappings.Add("Marzo Volumen", "marzoVolumen");
                sqlBulkCopy.ColumnMappings.Add("Marzo Ingresos", "marzoIngresos");
                sqlBulkCopy.ColumnMappings.Add("Abril Volumen", "abrilVolumen");
                sqlBulkCopy.ColumnMappings.Add("Abril Ingresos", "abrilIngresos");
                sqlBulkCopy.ColumnMappings.Add("Mayo Volumen", "mayoVolumen");
                sqlBulkCopy.ColumnMappings.Add("Mayo Ingresos", "mayoIngresos");
                sqlBulkCopy.ColumnMappings.Add("Junio Volumen", "junioVolumen");
                sqlBulkCopy.ColumnMappings.Add("Junio Ingresos", "junioIngresos");
                sqlBulkCopy.ColumnMappings.Add("Julio Volumen", "julioVolumen");
                sqlBulkCopy.ColumnMappings.Add("Julio Ingresos", "julioIngresos");
                sqlBulkCopy.ColumnMappings.Add("Agosto Volumen", "agostoVolumen");
                sqlBulkCopy.ColumnMappings.Add("Agosto Ingresos", "agostoIngresos");
                sqlBulkCopy.ColumnMappings.Add("Septiembre Volumen", "septiembreVolumen");
                sqlBulkCopy.ColumnMappings.Add("Septiembre Ingresos", "septiembreIngresos");
                sqlBulkCopy.ColumnMappings.Add("Octubre Volumen", "octubreVolumen");
                sqlBulkCopy.ColumnMappings.Add("Octubre Ingresos", "octubreIngresos");
                sqlBulkCopy.ColumnMappings.Add("Noviembre Volumen", "noviembreVolumen");
                sqlBulkCopy.ColumnMappings.Add("Noviembre Ingresos", "noviembreIngresos");
                sqlBulkCopy.ColumnMappings.Add("Diciembre Volumen", "diciembreVolumen");
                sqlBulkCopy.ColumnMappings.Add("Diciembre Ingresos", "diciembreIngresos");
                sqlBulkCopy.ColumnMappings.Add("anio", "anio");
                sqlBulkCopy.ColumnMappings.Add("cve_TipoCarga", "cve_TipoCarga");

                sqlBulkCopy.WriteToServer(volumeTable);
                successInsert = true;
            }
            catch (Exception ex)
            {
                successInsert = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertVolumen()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para guardar la base asociada a la promotoria de cada portafolio.
        /// </summary>
        /// <param name="promotoriaTable">Objeto que contiene la información de la base de la promotoria.</param>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public bool BulkInsertPromotoria(DataTable promotoriaTable, int yearData, int chargeTypeData)
        {
            bool successInsert = false;
            try
            {
                Open();
                SqlConnection connectionData = GetConnection();
                SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connectionData)
                {
                    DestinationTableName = "[dbo].[Tbl_Promotoria]",
                    BulkCopyTimeout = 400
                };

                // Agregar columnas pendientes.
                DataTableAddColumn(promotoriaTable, "anio", yearData);
                DataTableAddColumn(promotoriaTable, "cve_TipoCarga", chargeTypeData);

                // Mapear columnas en el archivo hacia la tabla.
                sqlBulkCopy.ColumnMappings.Add("Cadena Agrupada", "cadenaAgrupada");
                sqlBulkCopy.ColumnMappings.Add("Formato Cadena", "formatoCadena");
                sqlBulkCopy.ColumnMappings.Add("Cuenta", "cuenta");
                sqlBulkCopy.ColumnMappings.Add("Centro de Costo", "centroDeCosto");
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
                sqlBulkCopy.ColumnMappings.Add("Total", "total");
                sqlBulkCopy.ColumnMappings.Add("anio", "anio");
                sqlBulkCopy.ColumnMappings.Add("cve_TipoCarga", "cve_TipoCarga");

                sqlBulkCopy.WriteToServer(promotoriaTable);
                successInsert = true;
            }
            catch (Exception ex)
            {
                successInsert = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertPromotoria()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successInsert;
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
                    sqlBulkCopy.ColumnMappings.Add("anio", "anio");
                    sqlBulkCopy.ColumnMappings.Add("cve_TipoCarga", "cve_TipoCarga");
                    sqlBulkCopy.ColumnMappings.Add("cve_Colaborador", "cve_Colaborador");
                    sqlBulkCopy.ColumnMappings.Add("cve_Area", "cve_Area");

                    // Mapear columnas en el archivo hacia la tabla.
                    sqlBulkCopy.ColumnMappings.Add("Tipo de Gestion", "tipoGestion");
                    sqlBulkCopy.ColumnMappings.Add("Centro de Costo", "centroDeCosto");
                    sqlBulkCopy.ColumnMappings.Add("Cuenta", "cuenta");
                    sqlBulkCopy.ColumnMappings.Add("Segmento", "segmento");
                    sqlBulkCopy.ColumnMappings.Add("Filtro", "filtro");
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

                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno UHT Enero", "modernoUHTEnero");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno UHT Febrero", "modernoUHTFebrero");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno UHT Marzo", "modernoUHTMarzo");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno UHT Abril", "modernoUHTAbril");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno UHT Mayo", "modernoUHTMayo");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno UHT Junio", "modernoUHTJunio");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno UHT Julio", "modernoUHTJulio");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno UHT Agosto", "modernoUHTAgosto");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno UHT Septiembre", "modernoUHTSeptiembre");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno UHT Octubre", "modernoUHTOctubre");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno UHT Noviembre", "modernoUHTNoviembre");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno UHT Diciembre", "modernoUHTDiciembre");

                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno VAD Enero", "modernoVADEnero");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno VAD Febrero", "modernoVADFebrero");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno VAD Marzo", "modernoVADMarzo");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno VAD Abril", "modernoVADAbril");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno VAD Mayo", "modernoVADMayo");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno VAD Junio", "modernoVADJunio");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno VAD Julio", "modernoVADJulio");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno VAD Agosto", "modernoVADAgosto");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno VAD Septiembre", "modernoVADSeptiembre");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno VAD Octubre", "modernoVADOctubre");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno VAD Noviembre", "modernoVADNoviembre");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Moderno VAD Diciembre", "modernoVADDiciembre");

                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Embotellador Enero", "embotelladorEnero");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Embotellador Febrero", "embotelladorFebrero");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Embotellador Marzo", "embotelladorMarzo");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Embotellador Abril", "embotelladorAbril");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Embotellador Mayo", "embotelladorMayo");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Embotellador Junio", "embotelladorJunio");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Embotellador Julio", "embotelladorJulio");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Embotellador Agosto", "embotelladorAgosto");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Embotellador Septiembre", "embotelladorSeptiembre");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Embotellador Octubre", "embotelladorOctubre");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Embotellador Noviembre", "embotelladorNoviembre");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Embotellador Diciembre", "embotelladorDiciembre");

                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Tiendas Enero", "tiendasEnero");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Tiendas Febrero", "tiendasFebrero");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Tiendas Marzo", "tiendasMarzo");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Tiendas Abril", "tiendasAbril");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Tiendas Mayo", "tiendasMayo");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Tiendas Junio", "tiendasJunio");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Tiendas Julio", "tiendasJulio");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Tiendas Agosto", "tiendasAgosto");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Tiendas Septiembre", "tiendasSeptiembre");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Tiendas Octubre", "tiendasOctubre");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Tiendas Noviembre", "tiendasNoviembre");
                    sqlBulkCopy.ColumnMappings.Add("Porcentaje Tiendas Diciembre", "tiendasDiciembre");
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
                    SqlCommand sqlcmd = new SqlCommand("[dbo].[usp_InsertarTblHechos_Manual]", Connection);
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.Parameters.AddWithValue("@anio", accountsData.YearAccounts);
                    sqlcmd.Parameters.AddWithValue("@tipo_carga", accountsData.ChargeTypeAccounts);
                    sqlcmd.Parameters.AddWithValue("@colaborador", accountsData.Collaborator);
                    sqlcmd.Parameters.AddWithValue("@area", accountsData.Area);
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
        /// Método utilizado para guardar la información asociada a un usuario de la aplicación.
        /// </summary>
        /// <param name="userInformation">Objeto que contiene la información general del usuario.</param>
        /// <returns>Devuelve el id asociado al usuario recién insertado en la Base de Datos.</returns>
        public int SaveUserInformation(UserData userInformation)
        {
            int collaboratorId = 0;
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = Connection;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = "INSERT INTO [dbo].[Cat_Colaboradores] VALUES (@name, @email, @username, @roleId); SELECT SCOPE_IDENTITY();";
                sqlcmd.Parameters.AddWithValue("@name", userInformation.CollaboratorName);
                sqlcmd.Parameters.AddWithValue("@email", userInformation.Email);
                sqlcmd.Parameters.AddWithValue("@username", userInformation.Username);
                sqlcmd.Parameters.AddWithValue("@roleId", userInformation.RoleId);
                collaboratorId = Convert.ToInt32(sqlcmd.ExecuteScalar());
                Close();
            }
            catch (Exception ex)
            {
                throw;
            }

            return collaboratorId;
        }

        /// <summary>
        /// Método utilizado para guardar la relación entre un usuario y la(s) área(s) asociadas a este.
        /// </summary>
        /// <param name="userAreas">Lista de áreas asociadas a un usuario.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue guardada correctamente.</returns>
        public bool BulkInsertUserAreas(List<UserAreaRelation> userAreas)
        {
            bool successInsert = false;
            try
            {
                Open();
                SqlConnection connectionData = GetConnection();
                SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connectionData)
                {
                    DestinationTableName = "[dbo].[Cat_ColaboradorAreas]",
                    BulkCopyTimeout = 400
                };

                // Mapear columnas en el archivo hacia la tabla.
                sqlBulkCopy.ColumnMappings.Add(nameof(UserAreaRelation.UserId), "cve_Colaborador");
                sqlBulkCopy.ColumnMappings.Add(nameof(UserAreaRelation.AreaId), "cve_Area");

                DataTable table = new DataTable();
                using (var reader = ObjectReader.Create(userAreas))
                {
                    table.Load(reader);
                }

                sqlBulkCopy.WriteToServer(table);
                successInsert = true;
            }
            catch (Exception ex)
            {
                successInsert = false;
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("BulkInsertUserAreas()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successInsert;
        }

        /// <summary>
        /// Método utilizado para guardar la información asociada a un área dentro del catálogo.
        /// </summary>
        /// <param name="areaInformation">Objeto que contiene la información general del área.</param>
        /// <returns>Devuelve el id asociado al área recién insertada en la Base de Datos.</returns>
        public int SaveAreaInformation(AreaData areaInformation)
        {
            int collaboratorId = 0;
            try
            {
                if (areaInformation != null)
                {
                    Open();
                    SqlCommand sqlcmd = new SqlCommand();
                    sqlcmd.Connection = Connection;
                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = "INSERT INTO [dbo].[Cat_Areas] VALUES (@nameArea, @defaultArea); SELECT SCOPE_IDENTITY();";
                    sqlcmd.Parameters.AddWithValue("@nameArea", areaInformation.NameArea);
                    sqlcmd.Parameters.AddWithValue("@defaultArea", areaInformation.DefaultArea);
                    collaboratorId = Convert.ToInt32(sqlcmd.ExecuteScalar());
                    Close();
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return collaboratorId;
        }

        /// <summary>
        /// Método utilizado para almacenar un registro en el historial de carga de archivos.
        /// </summary>
        /// <param name="fileLogData">Objeto que contiene la información general del archivo que se está cargando.</param>
        /// <returns>Devuelve el id asociado al historial de carga del archivo que recién fue insertado.</returns>
        public int SaveFileLog(FileLogData fileLogData)
        {
            int fileLogId = 0;
            try
            {
                if (fileLogData != null)
                {
                    SqlCommand sqlcmd = new SqlCommand();
                    StringBuilder query = new StringBuilder();
                    Open();
                    sqlcmd.Connection = Connection;
                    sqlcmd.CommandType = CommandType.Text;
                    query.Append(" INSERT INTO [dbo].[Tbl_LogArchivos] VALUES (@fileName, @chargeDate, @approvalFlag, @userId, @fileTypeId); ");
                    query.Append(" SELECT SCOPE_IDENTITY(); ");
                    sqlcmd.CommandText = query.ToString();
                    sqlcmd.Parameters.AddWithValue("@fileName", fileLogData.FileName);
                    sqlcmd.Parameters.AddWithValue("@chargeDate", fileLogData.ChargeDate);
                    sqlcmd.Parameters.AddWithValue("@approvalFlag", fileLogData.ApprovalFlag);
                    sqlcmd.Parameters.AddWithValue("@userId", fileLogData.UserId);
                    sqlcmd.Parameters.AddWithValue("@fileTypeId", fileLogData.FileTypeId);
                    fileLogId = Convert.ToInt32(sqlcmd.ExecuteScalar());
                    Close();
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return fileLogId;
        }

        /// <summary>
        /// Método utilizado para agregar una columna adicional a un dataTable existente.
        /// </summary>
        /// <param name="dataTableObj">Objeto que contiene la información del dataTable.</param>
        /// <param name="columnName">Nombre de la columna a agregar.</param>
        /// <param name="defaultValue">Valor default a colocar en la nueva columna.</param>
        private static void DataTableAddColumn(DataTable dataTableObj, string columnName, dynamic defaultValue = null)
        {
            DataColumn newColumn = null;
            if (defaultValue != null)
            {
                newColumn = new DataColumn(columnName, defaultValue.GetType()) { DefaultValue = defaultValue };
            }
            else
            {
                newColumn = new DataColumn(columnName);
            }

            dataTableObj.Columns.Add(newColumn);
        }
    }
}