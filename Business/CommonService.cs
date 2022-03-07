namespace Business
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Data.Models.Request;
    using Data.Repositories;
    using ExcelDataReader;

    /// <summary>
    /// Clase que contiene métodos en común utilizados en las distintas clases de la aplicación.
    /// </summary>
    public class CommonService
    {
        /// <summary>
        /// Método utilizado para leer un archivo de excel y convertirlo en un DataTable.
        /// </summary>
        /// <param name="fileStream">Información asociada al archivo.</param>
        /// <param name="fileExtension">Extensión del archivo.</param>
        /// <returns>Devuelve la información del archivo en formato DataTable.</returns>
        public static DataSet ReadFile(Stream fileStream, string fileExtension)
        {
            try
            {
                IExcelDataReader excelDataReader = (IExcelDataReader)null;
                if (fileExtension == ".xls")
                {
                    excelDataReader = ExcelReaderFactory.CreateBinaryReader(fileStream);
                }
                else if (fileExtension == ".xlsx")
                {
                    excelDataReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);
                }

                if (excelDataReader == null)
                {
                    return new DataSet();
                }

                DataSet dataSet = excelDataReader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true
                    }
                });

                excelDataReader.Close();
                var result = dataSet.Tables.Count > 0 ? dataSet : new DataSet();
                return result;
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("PercentagesInformation()." + "Error: " + ex.Message);
                return new DataSet();
            }
            finally
            {
                fileStream.Close();
            }
        }

        /// <summary>
        /// Método utilizado para remover renglones en blanco, columnas nulas y acentos dentro de un dataTable.
        /// </summary>
        /// <param name="dataTable">DataTable que se pretende limpiar.</param>
        /// <returns>Devuelve las columnas "limpias" asociadas al DataTable.</returns>
        public static List<DataColumn> ClearDataTableStructure(ref DataTable dataTable)
        {
            dataTable = RemoveWhiteSpaces(dataTable);
            RemoveNullColumns(ref dataTable);
            var tblColumns = dataTable.Columns.Cast<DataColumn>()
                .Select(dc => { dc.ColumnName = CommonService.RemoveDiacritics(dc.ColumnName); return dc; })
                .ToList();
            return tblColumns;
        }

        /// <summary>
        /// Método utilizado para realizar el proceso de conversión de columnas a renglones en un DataTable.
        /// </summary>
        /// <param name="dt">DataTable que se pretende transformar.</param>
        /// <param name="pivotRequest">Objeto tipo request auxiliar en la conversión de columnas a renglones.</param>
        /// <returns>Devuelve el nuevo DataTable transformado, de acuerdo a la solicitud.</returns>
        public static DataTable UnpivotDataTable(DataTable dt, List<PivotTableRequest> pivotRequest)
        {
            DataTable unpivotTbl = null;
            if (pivotRequest != null && pivotRequest.Count > 0)
            {
                var newColumns = pivotRequest.SelectMany(x => x.ColumnsPivot).ToList();
                string[] actualColumns = dt.Columns.Cast<DataColumn>()
                    .Where(x => !newColumns.Contains(x.ColumnName))
                    .Select(x => x.ColumnName).ToArray();
                unpivotTbl = new DataTable("unpivot");
                for (int i = 0; i < actualColumns.Length; i++)
                {
                    var singleColumnName = actualColumns[i];
                    unpivotTbl.Columns.Add(singleColumnName, typeof(string));
                }

                int colCount = 0;
                for (int pvR = 0; pvR < pivotRequest.Count; pvR++)
                {
                    var singlePivot = pivotRequest[pvR];
                    var columnsPivot = singlePivot.ColumnsPivot;
                    unpivotTbl.Columns.Add(singlePivot.NewColumnName, singlePivot.NewColumnType);
                    if (singlePivot.IncludeMonth)
                    {
                        unpivotTbl.Columns.Add("Mes", typeof(int));
                    }

                    for (int rowCount = 0; rowCount < dt.Rows.Count; rowCount++)
                    {
                        for (int pivot = 0; pivot < columnsPivot.Count; pivot++)
                        {
                            DataRow rowToAdd = unpivotTbl.NewRow();

                            // Columnas actuales.
                            for (colCount = 0; colCount < actualColumns.Length; colCount++)
                            {
                                var singleActualColumn = actualColumns[colCount];
                                int actualColIndex = dt.Rows[rowCount].Table.Columns[singleActualColumn].Ordinal;
                                rowToAdd[singleActualColumn] = dt.Rows[rowCount].ItemArray[actualColIndex];
                            }

                            // Nuevas columnas.
                            var singlePivotColumn = columnsPivot[pivot];
                            int oldColIndex = dt.Rows[rowCount].Table.Columns[singlePivotColumn].Ordinal;
                            rowToAdd[singlePivot.NewColumnName] = dt.Rows[rowCount].ItemArray[oldColIndex];
                            if (singlePivot.IncludeMonth)
                            {
                                rowToAdd["Mes"] = GetMonthNumber(columnsPivot[pivot]);
                            }
                            
                            unpivotTbl.Rows.Add(rowToAdd);
                        }
                    }
                }
            }

            return unpivotTbl;
        }

        /// <summary>
        /// Método utilizado para remover espacios o renglones blancos en un DataTable.
        /// </summary>
        /// <param name="dataTable">DataTable que se pretende limpiar.</param>
        /// <returns>Devuelve el DataTable sin espacios o renglones blancos.</returns>
        public static DataTable RemoveWhiteSpaces(DataTable dataTable)
        {
            dataTable = dataTable.Rows.Cast<DataRow>()
                .Where(row => !row.ItemArray.All(field => field is DBNull || string.IsNullOrWhiteSpace(field as string)))
                .CopyToDataTable();
            return dataTable;
        }

        /// <summary>
        /// Método utilizado para eliminar aquellas columnas que no tengan ningún renglón con información.
        /// </summary>
        /// <param name="tbl">Tabla que se desea revisar, para limpiar las columnas.</param>
        public static void RemoveNullColumns(ref DataTable tbl)
        {
            var columns = tbl.Columns.Cast<DataColumn>();
            var rows = tbl.AsEnumerable();
            var nullColumns = columns.Where(col => rows.All(r => r.IsNull(col))).ToList();
            foreach (DataColumn colToRemove in nullColumns)
            {
                tbl.Columns.Remove(colToRemove);
            }
        }

        /// <summary>
        /// Método utilizado para remover acentos en una cadena específica.
        /// </summary>
        /// <param name="text">Cadena a la que se le desean remover los acentos.</param>
        /// <returns>Devuelve la cadena sin acentos.</returns>
        public static string RemoveDiacritics(string text)
        {
            string formD = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            foreach (char ch in formD)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Método utilizado para recuperar el número asociado a un mes de acuerdo a su nombre.
        /// </summary>
        /// <param name="monthName">Nombre asociado al mes.</param>
        /// <returns>Devuelve el número de mes, de acuerdo al nombre.</returns>
        public static int GetMonthNumber(string monthName)
        {
            int monthNumber = DateTime.ParseExact(monthName, "MMMM", new CultureInfo("es-ES", false)).Month;
            return monthNumber;
        }

        /// <summary>
        /// Método utilizado para devolver el tipo de ejercicio de acuerdo al nombre del tipo de carga.
        /// </summary>
        /// <param name="chargeTypeName">Nombre asociado al tipo de carga.</param>
        /// <returns>Devuelve el tipo de ejercicio de acuerdo al tipo de carga.</returns>
        public static string GetExerciseType(string chargeTypeName)
        {
            chargeTypeName = chargeTypeName.ToLower();
            bool bpExercise = chargeTypeName == "rolling 0+12" || chargeTypeName == "business plan";
            string exerciseType = bpExercise ? "BP" : "Rolling";
            return exerciseType;
        }
    }
}