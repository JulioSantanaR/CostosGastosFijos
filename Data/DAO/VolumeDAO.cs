namespace Data.DAO
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using Data.Repositories;

    /// <summary>
    /// Clase asociada al acceso a datos para manipular la información asociada al volumen.
    /// </summary>
    public class VolumeDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        readonly string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public VolumeDAO()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada al volumen, de acuerdo a un año y tipo de carga específicos.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public bool DeleteVolume(int yearData, int chargeTypeData)
        {
            bool successDelete = false;
            try
            {
                string query = string.Format("DELETE [dbo].[Tbl_Volumen] WHERE anio = {0} AND cve_TipoCarga = {1}", yearData, chargeTypeData);
                Open();
                SqlConnection connectionData = GetConnection();
                SqlCommand sqlcmd = new SqlCommand()
                {
                    Connection = connectionData,
                    CommandType = CommandType.Text,
                    CommandText = query
                };
                sqlcmd.ExecuteNonQuery();
                successDelete = true;
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteVolume()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada al volumen en un BP/Rolling 0+12.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public bool DeleteVolumeBP(int yearData, int chargeTypeData)
        {
            bool successDelete = false;
            try
            {
                string query = string.Format("DELETE [dbo].[Tbl_Volumen_BP] WHERE anio = {0} AND cve_TipoCarga = {1}", yearData, chargeTypeData);
                Open();
                SqlConnection connectionData = GetConnection();
                SqlCommand sqlcmd = new SqlCommand()
                {
                    Connection = connectionData,
                    CommandType = CommandType.Text,
                    CommandText = query
                };
                sqlcmd.ExecuteNonQuery();
                successDelete = true;
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeleteVolumeBP()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para guardar la base asociada al volumen.
        /// </summary>
        /// <param name="volumeTable">Objeto que contiene la información de la base del volumen.</param>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public bool BulkInsertVolumen(DataTable volumeTable, int yearData, int chargeTypeData, int fileLogId)
        {
            bool successInsert = false;
            try
            {
                Open();
                SqlConnection connectionData = GetConnection();
                SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connectionData)
                {
                    DestinationTableName = "[dbo].[Tbl_Volumen]",
                    BulkCopyTimeout = 400
                };

                // Agregar columnas pendientes.
                DataTableAddColumn(volumeTable, "anio", yearData);
                DataTableAddColumn(volumeTable, "cve_TipoCarga", chargeTypeData);
                DataTableAddColumn(volumeTable, "cve_LogArchivo", fileLogId);

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
                sqlBulkCopy.ColumnMappings.Add("cve_LogArchivo", "cve_LogArchivo");

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
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public bool BulkInsertVolumenBP(DataTable volumeTable, int yearData, int chargeTypeData, int fileLogId)
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
                DataTableAddColumn(volumeTable, "cve_LogArchivo", fileLogId);

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
                sqlBulkCopy.ColumnMappings.Add("cve_LogArchivo", "cve_LogArchivo");

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
    }
}