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
    using Data.Models.Response;

    /// <summary>
    /// Clase asociada al acceso a datos para manipular la información asociada al historial de carga de archivos en la aplicación.
    /// </summary>
    public class FileLogDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        readonly string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public FileLogDAO()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Método utilizado para recuperar la información que será mostrada en la tabla que contiene el historial de archivos.
        /// </summary>
        /// <param name="dataTableInfo">Objeto que contiene los parámetros de búsqueda para la tabla de usuarios.</param>
        /// <returns>Devuelve un objeto que contiene la información necesaria para mostrar la tabla del historial de archivos.</returns>
        public FileLogTableResponse GetFileLogTable(DataTableRequest dataTableInfo)
        {
            List<FileLogData> fileLogList = new List<FileLogData>();
            int fileLogCount = 0;
            try
            {
                if (dataTableInfo != null)
                {
                    Open();
                    if (!dataTableInfo.GetTotalRowsCount)
                    {
                        string queryFileLogList = FileLogCommonQuery(dataTableInfo);
                        SqlCommand sqlcmdList = new SqlCommand
                        {
                            Connection = Connection,
                            CommandType = CommandType.Text,
                            CommandText = queryFileLogList
                        };
                        sqlcmdList.Parameters.AddWithValue("@rowsToSkip", dataTableInfo.RowsToSkip);
                        sqlcmdList.Parameters.AddWithValue("@numbersOfRows", dataTableInfo.NumberOfRows);
                        if (dataTableInfo.FileRequest != null)
                        {
                            if (dataTableInfo.FileRequest.FileTypeId.HasValue && dataTableInfo.FileRequest.FileTypeId.Value > 0)
                            {
                                sqlcmdList.Parameters.AddWithValue("@fileTypeId", dataTableInfo.FileRequest.FileTypeId.Value);
                            }

                            if (dataTableInfo.FileRequest.AreaId.HasValue && dataTableInfo.FileRequest.AreaId.Value > 0)
                            {
                                sqlcmdList.Parameters.AddWithValue("@areaId", dataTableInfo.FileRequest.AreaId.Value);
                            }

                            if (dataTableInfo.FileRequest.ChargeTypeId.HasValue && dataTableInfo.FileRequest.ChargeTypeId.Value > 0)
                            {
                                sqlcmdList.Parameters.AddWithValue("@chargeTypeId", dataTableInfo.FileRequest.ChargeTypeId.Value);
                            }

                            if (dataTableInfo.FileRequest.Year.HasValue && dataTableInfo.FileRequest.Year.Value > 0)
                            {
                                sqlcmdList.Parameters.AddWithValue("@year", dataTableInfo.FileRequest.Year.Value);
                            }

                            if (dataTableInfo.FileRequest.CollaboratorId.HasValue && dataTableInfo.FileRequest.CollaboratorId.Value > 0)
                            {
                                sqlcmdList.Parameters.AddWithValue("@collaboratorId", dataTableInfo.FileRequest.CollaboratorId.Value);
                            }
                        }

                        if (!string.IsNullOrEmpty(dataTableInfo.SearchValue) && dataTableInfo.SearchValue.Length >= 3)
                        {
                            sqlcmdList.Parameters.AddWithValue("@searchValue", dataTableInfo.SearchValue);
                        }

                        var reader = sqlcmdList.ExecuteReader();
                        var typeFileExists = HasColumn(reader, "tipoArchivo");
                        var collaboratorNameExists = HasColumn(reader, "nombreColaborador");
                        var areaNameExists = HasColumn(reader, "nombreArea");
                        var chargeTypeExists = HasColumn(reader, "tipoCarga");
                        while (reader.Read())
                        {
                            FileLogData singleFile = Mapping.MapFileLog(reader, typeFileExists, collaboratorNameExists, areaNameExists, chargeTypeExists);
                            fileLogList.Add(singleFile);
                        }

                        reader.Close();
                    }

                    if (dataTableInfo.MakeServicesCountQuery)
                    {
                        dataTableInfo.GetTotalRowsCount = true;
                        string queryFileLogCount = FileLogCommonQuery(dataTableInfo);
                        SqlCommand sqlcmdCount = new SqlCommand
                        {
                            Connection = Connection,
                            CommandType = CommandType.Text,
                            CommandText = queryFileLogCount
                        };
                        if (dataTableInfo.FileRequest != null)
                        {
                            if (dataTableInfo.FileRequest.FileTypeId.HasValue && dataTableInfo.FileRequest.FileTypeId.Value > 0)
                            {
                                sqlcmdCount.Parameters.AddWithValue("@fileTypeId", dataTableInfo.FileRequest.FileTypeId.Value);
                            }

                            if (dataTableInfo.FileRequest.AreaId.HasValue && dataTableInfo.FileRequest.AreaId.Value > 0)
                            {
                                sqlcmdCount.Parameters.AddWithValue("@areaId", dataTableInfo.FileRequest.AreaId.Value);
                            }

                            if (dataTableInfo.FileRequest.ChargeTypeId.HasValue && dataTableInfo.FileRequest.ChargeTypeId.Value > 0)
                            {
                                sqlcmdCount.Parameters.AddWithValue("@chargeTypeId", dataTableInfo.FileRequest.ChargeTypeId.Value);
                            }

                            if (dataTableInfo.FileRequest.Year.HasValue && dataTableInfo.FileRequest.Year.Value > 0)
                            {
                                sqlcmdCount.Parameters.AddWithValue("@year", dataTableInfo.FileRequest.Year.Value);
                            }

                            if (dataTableInfo.FileRequest.CollaboratorId.HasValue && dataTableInfo.FileRequest.CollaboratorId.Value > 0)
                            {
                                sqlcmdCount.Parameters.AddWithValue("@collaboratorId", dataTableInfo.FileRequest.CollaboratorId.Value);
                            }
                        }

                        if (!string.IsNullOrEmpty(dataTableInfo.SearchValue) && dataTableInfo.SearchValue.Length >= 3)
                        {
                            sqlcmdCount.Parameters.AddWithValue("@searchValue", dataTableInfo.SearchValue);
                        }

                        fileLogCount = Convert.ToInt32(sqlcmdCount.ExecuteScalar());
                    }

                    Close();
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            FileLogTableResponse fileLogTable = new FileLogTableResponse()
            {
                FileLogList = fileLogList,
                FileLogCount = fileLogCount
            };
            return fileLogTable;
        }

        /// <summary>
        /// Método utilizado para construir la consulta para encontrar la información del historial de archivos.
        /// </summary>
        /// <param name="dataTableInfo">Objeto que contiene los parámetros de búsqueda para la tabla de usuarios.</param>
        /// <returns>Devuelve una cadena con la consulta para buscar la información.</returns>
        public string FileLogCommonQuery(DataTableRequest dataTableInfo)
        {
            StringBuilder query = new StringBuilder();
            if (dataTableInfo != null)
            {
                if (dataTableInfo.GetTotalRowsCount)
                {
                    query.Append(" SELECT COUNT(*) AS countFiles FROM ( ");
                }
                else
                {
                    query.Append(" SELECT * FROM ( ");
                }

                query.Append("  SELECT logFiles.*, catArchivos.tipoArchivo, catColab.nombre AS nombreColaborador, ");
                query.Append("   catAreas.nombre AS nombreArea, catCharges.tipoCarga, ROW_NUMBER() OVER ( ");
                if (!string.IsNullOrEmpty(dataTableInfo.SortName))
                {
                    query.Append("ORDER BY ").Append(dataTableInfo.SortName);
                    if (!string.IsNullOrEmpty(dataTableInfo.SortOrder))
                    {
                        query.Append(" ").Append(dataTableInfo.SortOrder);
                    }
                }
                else
                {
                    query.Append(" ORDER BY cve_LogArchivo ");
                }

                query.Append(" ) AS rowNumber ");
                query.Append("  FROM [dbo].[Tbl_LogArchivos] logFiles ");
                query.Append("  INNER JOIN [dbo].[Cat_TiposDeArchivos] catArchivos ON catArchivos.cve_TipoArchivo = logFiles.cve_TipoArchivo ");
                query.Append("  INNER JOIN [dbo].[Cat_Colaboradores] catColab ON catColab.cve_Colaborador = logFiles.cve_Colaborador ");
                query.Append("  INNER JOIN [dbo].[Cat_Areas] catAreas ON catAreas.cve_Area = logFiles.cve_Area ");
                query.Append("  INNER JOIN [B20CQ-004].[JDV_SC_BIF_Consolidado].[dbo].[Cat_TiposCarga] catCharges ON catCharges.cve_Tipo_Carga = logFiles.cve_TipoCarga ");
                query.Append(" WHERE 1 = 1 ");
                if (dataTableInfo.FileRequest != null)
                {
                    if (dataTableInfo.FileRequest.FileTypeId.HasValue && dataTableInfo.FileRequest.FileTypeId.Value > 0)
                    {
                        query.Append(" AND logFiles.cve_TipoArchivo = @fileTypeId ");
                    }

                    if (dataTableInfo.FileRequest.AreaId.HasValue && dataTableInfo.FileRequest.AreaId.Value > 0)
                    {
                        query.Append(" AND logFiles.cve_Area = @areaId ");
                    }

                    if (dataTableInfo.FileRequest.ChargeTypeId.HasValue && dataTableInfo.FileRequest.ChargeTypeId.Value > 0)
                    {
                        query.Append(" AND logFiles.cve_TipoCarga = @chargeTypeId ");
                    }

                    if (dataTableInfo.FileRequest.Year.HasValue && dataTableInfo.FileRequest.Year.Value > 0)
                    {
                        query.Append(" AND logFiles.anio = @year ");
                    }

                    if (dataTableInfo.FileRequest.CollaboratorId.HasValue && dataTableInfo.FileRequest.CollaboratorId.Value > 0)
                    {
                        query.Append(" AND logFiles.cve_Colaborador = @collaboratorId ");
                    }
                }

                if (!string.IsNullOrEmpty(dataTableInfo.SearchValue) && dataTableInfo.SearchValue.Length >= 3)
                {
                    query.Append(" AND CHARINDEX(REPLACE(LTRIM(RTRIM(LOWER(@searchValue))), ' ', ''), ");
                    query.Append("  REPLACE(LTRIM(RTRIM(LOWER( ");
                    query.Append("      ISNULL(nombreArchivo, '') + ISNULL(catColab.nombre, '') + ISNULL(catAreas.nombre, '') ");
                    query.Append("      + ISNULL(tipoArchivo, '') + ISNULL(tipoCarga, '') ");
                    query.Append("  ))), ' ', '') ");
                    query.Append(" ) > 0 ");
                }

                query.Append(" ) t ");
                if (!dataTableInfo.GetTotalRowsCount)
                {
                    query.Append(" WHERE rowNumber BETWEEN (@rowsToSkip + 1) AND (@rowsToSkip + @numbersOfRows) ");
                }
            }

            string finalQuery = query.ToString();
            return finalQuery;
        }

        /// <summary>
        /// Método utilizado para eliminar la información asociada a un archivo cargado en el historial.
        /// </summary>
        /// <param name="logFileId">Id asociado al archivo.</param>
        /// <returns>Devuelve una bandera para determinar si la información fue eliminada correctamente o no.</returns>
        public bool DeleteFileLogById(int logFileId)
        {
            bool successDelete;
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand
                {
                    Connection = Connection,
                    CommandType = CommandType.Text,
                    CommandText = "DELETE [dbo].[Tbl_LogArchivos] WHERE cve_LogArchivo = @logFileId "
                };
                sqlcmd.Parameters.AddWithValue("@logFileId", logFileId);
                sqlcmd.ExecuteNonQuery();
                Close();
                successDelete = true;
            }
            catch (Exception ex)
            {
                throw;
            }

            return successDelete;
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
                    query.Append(" INSERT INTO [dbo].[Tbl_LogArchivos] ");
                    query.Append(" VALUES (@fileName, @chargeDate, @approvalFlag, @userId, ");
                    if (!string.IsNullOrEmpty(fileLogData.FileTypeName))
                    {
                        query.Append(" (SELECT cve_TipoArchivo FROM [dbo].[Cat_TiposDeArchivos] WHERE tipoArchivo = @fileTypeName ), ");
                        sqlcmd.Parameters.AddWithValue("@fileTypeName", fileLogData.FileTypeName);
                    }
                    else
                    {
                        query.Append(" @fileTypeId, ");
                        sqlcmd.Parameters.AddWithValue("@fileTypeId", fileLogData.FileTypeId);
                    }

                    if (fileLogData.DefaultArea)
                    {
                        query.Append(" (SELECT cve_Area FROM [dbo].[Cat_Areas] WHERE defaultArea = @defaultArea ), ");
                        sqlcmd.Parameters.AddWithValue("@defaultArea", fileLogData.DefaultArea);
                    }
                    else
                    {
                        query.Append(" @areaId, ");
                        sqlcmd.Parameters.AddWithValue("@areaId", fileLogData.AreaId);
                    }

                    query.Append(" @chargeTypeId, @yearData); ");
                    query.Append(" SELECT SCOPE_IDENTITY(); ");
                    sqlcmd.CommandText = query.ToString();
                    sqlcmd.Parameters.AddWithValue("@fileName", fileLogData.FileName);
                    sqlcmd.Parameters.AddWithValue("@chargeDate", fileLogData.ChargeDate);
                    sqlcmd.Parameters.AddWithValue("@approvalFlag", fileLogData.ApprovalFlag);
                    sqlcmd.Parameters.AddWithValue("@userId", fileLogData.UserId);
                    sqlcmd.Parameters.AddWithValue("@chargeTypeId", fileLogData.ChargeTypeId);
                    sqlcmd.Parameters.AddWithValue("@yearData", fileLogData.YearData);
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
    }
}
