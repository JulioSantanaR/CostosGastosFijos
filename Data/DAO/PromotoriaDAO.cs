namespace Data.DAO
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Data.Models;
    using Data.Repositories;

    public class PromotoriaDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        readonly string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public PromotoriaDAO()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Método utilizado para guardar la base asociada a la promotoria de cada portafolio.
        /// </summary>
        /// <param name="promotoriaTable">Objeto que contiene la información de la base de la promotoria.</param>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Bandera para determinar si la inserción fue correcta o no.</returns>
        public bool BulkInsertPromotoria(DataTable promotoriaTable, int yearData, int chargeTypeData, int fileLogId)
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
                DataTableAddColumn(promotoriaTable, "cve_LogArchivo", fileLogId);

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
                sqlBulkCopy.ColumnMappings.Add("cve_LogArchivo", "cve_LogArchivo");

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
        /// Método utilizado para eliminar la información asociada a la promotoria, de acuerdo a un año y tipo de carga específicos.
        /// </summary>
        /// <param name="yearData">Año de carga.</param>
        /// <param name="chargeTypeData">Tipo de carga.</param>
        /// <param name="fileLogId">Id asociado al archivo que se está cargando.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente.</returns>
        public bool DeletePromotoria(int? yearData = null, int? chargeTypeData = null, int? fileLogId = null)
        {
            bool successDelete = false;
            try
            {
                SqlCommand sqlcmd = new SqlCommand();
                StringBuilder query = new StringBuilder();
                query.Append("DELETE [dbo].[Tbl_Promotoria] WHERE 1 = 1 ");
                if (yearData.HasValue && yearData.Value > 0)
                {
                    query.Append(" AND anio = @yearData ");
                    sqlcmd.Parameters.AddWithValue("@yearData", yearData.Value);
                }

                if (chargeTypeData.HasValue && chargeTypeData.Value > 0)
                {
                    query.Append(" AND cve_TipoCarga = @chargeTypeData ");
                    sqlcmd.Parameters.AddWithValue("@chargeTypeData", chargeTypeData.Value);
                }

                if (fileLogId.HasValue && fileLogId.Value > 0)
                {
                    query.Append(" AND cve_LogArchivo = @fileLogId ");
                    sqlcmd.Parameters.AddWithValue("@fileLogId", fileLogId.Value);
                }

                Open();
                SqlConnection connectionData = GetConnection();
                sqlcmd.Connection = connectionData;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = query.ToString();
                sqlcmd.ExecuteNonQuery();
                successDelete = true;
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("DeletePromotoria()." + "Error: " + ex.Message);
            }
            finally
            {
                Close();
            }

            return successDelete;
        }

        /// <summary>
        /// Método utilizado para recuperar la información de la promotoría de acuerdo a un año y tipo de ejercicio específicos.
        /// </summary>
        /// <param name="yearData">Año del ejercicio.</param>
        /// <param name="chargeTypeData">Tipo de ejercicio.</param>
        /// <returns>Devuelve la información asociada al detalle del presupuesto correspondiente a la promotoría.</returns>
        public List<PromotoriaDB> GetPromotoria(int yearData, int chargeTypeData)
        {
            List<PromotoriaDB> promotoria = new List<PromotoriaDB>();
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT mes, SUM(presupuesto) AS presupuesto FROM [dbo].[Fact_CostosGastosFijos] ");
                query.Append("WHERE (filtro COLLATE Latin1_general_CI_AI) LIKE '%promotoria%' ");
                query.Append("AND cve_TipoCarga = @chargeTypeData AND anio = @yearData GROUP BY mes ");
                Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = Connection;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = query.ToString();
                sqlcmd.Parameters.AddWithValue("@chargeTypeData", chargeTypeData);
                sqlcmd.Parameters.AddWithValue("@yearData", yearData);
                var reader = sqlcmd.ExecuteReader();
                while (reader.Read())
                {
                    PromotoriaDB singleBudget = new PromotoriaDB();
                    singleBudget.MonthNumber = reader["mes"] != DBNull.Value ? Convert.ToInt32(reader["mes"]) : default(int);
                    singleBudget.Budget = reader["presupuesto"] != DBNull.Value ? Convert.ToDouble(reader["presupuesto"]) : default(double);
                    promotoria.Add(singleBudget);
                }

                reader.Close();
                Close();
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("GetPromotoria()." + "Error: " + ex.Message);
            }

            return promotoria;
        }
    }
}
