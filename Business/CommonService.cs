namespace Business
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Data.Repositories;
    using ExcelDataReader;

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
    }
}