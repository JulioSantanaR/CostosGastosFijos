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
    /// Clase asociada al acceso a datos para manipular la información asociada al catálogo de áreas.
    /// </summary>
    public class AreasDAO : CommonDAO
    {
        /// <summary>
        /// Columnas asociadas al catálogo de "áreas".
        /// </summary>
        private readonly string[] AreaFields = new string[] { "cve_Area", "nombre", "defaultArea" };

        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public AreasDAO()
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Método utilizado para obtener las áreas asociadas a un usuario.
        /// </summary>
        /// <param name="username">Nombre de usuario.</param>
        /// <param name="userId">Id asociado al usuario.</param>
        /// <returns>Devuelve la lista de áreas asociadas a un usuario/colaborador.</returns>
        public List<AreaData> UserAreas(string username = "", int? userId = null)
        {
            List<AreaData> userAreas = null;
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT colab.*, area.nombre AS nameArea, area.cve_Area, area.defaultArea FROM [dbo].[Cat_Colaboradores] AS colab ");
                query.Append("INNER JOIN [dbo].[Cat_ColaboradorAreas] colabArea ON colabArea.cve_Colaborador = colab.cve_colaborador ");
                query.Append("INNER JOIN [dbo].[Cat_Areas] area ON colabArea.cve_Area = area.cve_Area ");
                query.Append(" WHERE 1 = 1 ");
                if (!string.IsNullOrEmpty(username))
                {
                    query.Append(" AND usuario = @username ");
                }

                if (userId.HasValue && userId.Value > 0)
                {
                    query.Append(" AND colab.cve_Colaborador = @userId ");
                }

                query.Append("ORDER BY area.nombre ASC ");
                userAreas = GetUserAreas(query.ToString(), username, userId);
            }
            catch (Exception ex)
            {
                throw;
            }

            return userAreas;
        }

        /// <summary>
        /// Método utilizado para recuperar todo el catálogo de áreas.
        /// </summary>
        /// <param name="includeAllAreas">Bandera para saber si incluir "Todas las áreas" en la consulta.</param>
        /// <returns>Devuelve la lista de todas las áreas dadas de alta en el catálogo.</returns>
        public List<AreaData> GetAllAreas(bool includeAllAreas = false)
        {
            List<AreaData> allAreas = new List<AreaData>();
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT ").Append(string.Join(",", AreaFields));
                query.Append(" FROM [dbo].[Cat_Areas] ");
                query.Append(" WHERE 1 = 1 ");
                if (!includeAllAreas)
                {
                    query.Append(" AND cve_Area <> 0 ");
                }

                query.Append(" ORDER BY CASE WHEN cve_Area = 0 THEN 0 ELSE 1 END, nombre, cve_Area ASC ");
                Open();
                SqlCommand sqlcmd = new SqlCommand
                {
                    Connection = Connection,
                    CommandType = CommandType.Text,
                    CommandText = query.ToString()
                };
                var reader = sqlcmd.ExecuteReader();
                while (reader.Read())
                {
                    AreaData singleArea = new AreaData();
                    singleArea.AreaId = reader["cve_Area"] != DBNull.Value ? Convert.ToInt32(reader["cve_Area"]) : 0;
                    singleArea.NameArea = reader["nombre"] != DBNull.Value ? reader["nombre"].ToString() : string.Empty;
                    singleArea.DefaultArea = reader["defaultArea"] != DBNull.Value ? Convert.ToBoolean(reader["defaultArea"]) : false;
                    allAreas.Add(singleArea);
                }

                reader.Close();
                Close();
            }
            catch (Exception ex)
            {
                throw;
            }

            return allAreas;
        }

        /// <summary>
        /// Método utilizado para recuperar la información de un área de acuerdo al id asociado a esta.
        /// </summary>
        /// <param name="areaId">Id asociado al área.</param>
        /// <returns>Devuelve la información general del área.</returns>
        public AreaData GetAreaById(int areaId)
        {
            AreaData areaInformation = null;
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("SELECT ").Append(string.Join(",", AreaFields));
                query.Append(" FROM [dbo].[Cat_Areas] ");
                query.Append(" WHERE cve_Area = @areaId");
                Open();
                SqlCommand sqlcmd = new SqlCommand
                {
                    Connection = Connection,
                    CommandType = CommandType.Text,
                    CommandText = query.ToString()
                };
                sqlcmd.Parameters.AddWithValue("@areaId", areaId);
                var reader = sqlcmd.ExecuteReader();
                while (reader.Read())
                {
                    areaInformation = Mapping.MapArea(reader);
                }

                reader.Close();
                Close();
            }
            catch (Exception ex)
            {
                throw;
            }

            return areaInformation;
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
                    StringBuilder query = new StringBuilder();
                    query.Append("INSERT INTO [dbo].[Cat_Areas] VALUES (@nameArea, @defaultArea); ");
                    query.Append(" SELECT SCOPE_IDENTITY(); ");
                    Open();
                    SqlCommand sqlcmd = new SqlCommand
                    {
                        Connection = Connection,
                        CommandType = CommandType.Text,
                        CommandText = query.ToString()
                    };
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
        /// Método utilizado para actualizar la información asociada a un área en el catálogo de la aplicación.
        /// </summary>
        /// <param name="areaInformation">Objeto que contiene la información general del área.</param>
        /// <returns>Devuelve una bandera para determinar si la actualización fue correcta.</returns>
        public bool UpdateAreaInformation(AreaData areaInformation)
        {
            bool successUpdate = false;
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append("UPDATE [dbo].[Cat_Areas] SET nombre = @name, defaultArea = @defaultArea WHERE cve_Area = @areaId");
                Open();
                SqlCommand sqlcmd = new SqlCommand
                {
                    Connection = Connection,
                    CommandType = CommandType.Text,
                    CommandText = query.ToString()
                };
                sqlcmd.Parameters.AddWithValue("@name", areaInformation.NameArea);
                sqlcmd.Parameters.AddWithValue("@defaultArea", areaInformation.DefaultArea);
                sqlcmd.Parameters.AddWithValue("@areaId", areaInformation.AreaId);
                sqlcmd.ExecuteNonQuery();
                Close();
                successUpdate = true;
            }
            catch (Exception ex)
            {
                throw;
            }

            return successUpdate;
        }

        /// <summary>
        /// Método utilizado para recuperar la información que será mostrada en la tabla del catálogo de áreas.
        /// </summary>
        /// <param name="dataTableInfo">Objeto que contiene los parámetros de búsqueda para la tabla de áreas.</param>
        /// <returns>Devuelve un objeto que contiene la información necesaria para mostrar la tabla de áreas.</returns>
        public AreasTableResponse GetAreasTable(DataTableRequest dataTableInfo)
        {
            List<AreaData> areasList = new List<AreaData>();
            int areasCount = 0;
            try
            {
                if (dataTableInfo != null)
                {
                    Open();
                    if (!dataTableInfo.GetTotalRowsCount)
                    {
                        string queryAreasList = AreasCommonQuery(dataTableInfo);
                        SqlCommand sqlcmdList = new SqlCommand
                        {
                            Connection = Connection,
                            CommandType = CommandType.Text,
                            CommandText = queryAreasList
                        };
                        sqlcmdList.Parameters.AddWithValue("@rowsToSkip", dataTableInfo.RowsToSkip);
                        sqlcmdList.Parameters.AddWithValue("@numbersOfRows", dataTableInfo.NumberOfRows);
                        if (!string.IsNullOrEmpty(dataTableInfo.SearchValue) && dataTableInfo.SearchValue.Length >= 3)
                        {
                            sqlcmdList.Parameters.AddWithValue("@searchValue", dataTableInfo.SearchValue);
                        }

                        var reader = sqlcmdList.ExecuteReader();
                        while (reader.Read())
                        {
                            AreaData singleArea = Mapping.MapArea(reader);
                            areasList.Add(singleArea);
                        }

                        reader.Close();
                    }

                    if (dataTableInfo.MakeServicesCountQuery)
                    {
                        dataTableInfo.GetTotalRowsCount = true;
                        string queryAreasCount = AreasCommonQuery(dataTableInfo);
                        SqlCommand sqlcmdCount = new SqlCommand
                        {
                            Connection = Connection,
                            CommandType = CommandType.Text,
                            CommandText = queryAreasCount
                        };
                        if (!string.IsNullOrEmpty(dataTableInfo.SearchValue) && dataTableInfo.SearchValue.Length >= 3)
                        {
                            sqlcmdCount.Parameters.AddWithValue("@searchValue", dataTableInfo.SearchValue);
                        }

                        areasCount = Convert.ToInt32(sqlcmdCount.ExecuteScalar());
                    }

                    Close();
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            AreasTableResponse areasTable = new AreasTableResponse()
            {
                AreasList = areasList,
                AreasCount = areasCount
            };
            return areasTable;
        }

        /// <summary>
        /// Método utilizado para construir la consulta para encontrar la información en la tabla de áreas.
        /// </summary>
        /// <param name="dataTableInfo">Objeto que contiene los parámetros de búsqueda para la tabla de áreas.</param>
        /// <returns>Devuelve una cadena con la consulta para buscar la información.</returns>
        public string AreasCommonQuery(DataTableRequest dataTableInfo)
        {
            StringBuilder query = new StringBuilder();
            if (dataTableInfo != null)
            {
                if (dataTableInfo.GetTotalRowsCount)
                {
                    query.Append(" SELECT COUNT(*) AS countAreas FROM ( ");
                }
                else
                {
                    query.Append(" SELECT * FROM ( ");
                }

                query.Append("  SELECT *, ROW_NUMBER() OVER (");
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
                    query.Append(" ORDER BY cve_Area ");
                }

                query.Append(" ) AS rowNumber ");
                query.Append("  FROM [dbo].[Cat_Areas] WHERE cve_Area <> 0 ");
                if (!string.IsNullOrEmpty(dataTableInfo.SearchValue) && dataTableInfo.SearchValue.Length >= 3)
                {
                    query.Append(" AND CHARINDEX(REPLACE(LTRIM(RTRIM(LOWER(@searchValue))), ' ', ''), ");
                    query.Append("  REPLACE(LTRIM(RTRIM(LOWER( ");
                    query.Append("      ISNULL(nombre, '') ");
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
        /// Método utilizado para eliminar la información general asociada a un área.
        /// </summary>
        /// <param name="areaId">Id asociado al área.</param>
        /// <returns>Devuelve una bandera para determinar si la información se eliminó correctamente.</returns>
        public bool DeleteAreaInformation(int areaId)
        {
            bool successDelete = false;
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand
                {
                    Connection = Connection,
                    CommandType = CommandType.Text,
                    CommandText = "DELETE [dbo].[Cat_Areas] WHERE cve_Area = @areaId "
                };
                sqlcmd.Parameters.AddWithValue("@areaId", areaId);
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
        /// Método utilizado para eliminar la relación entre un usuario y la(s) área(s) asociadas a este.
        /// </summary>
        /// <param name="userId">Id asociado al usuario.</param>
        /// <param name="areaId">Id asociado al área.</param>
        /// <returns>Devuelve una bandera para determinar si la eliminación de la información fue correcta o no.</returns>
        public bool DeleteUserAreas(int? userId = null, int? areaId = null)
        {
            bool successDelete = false;
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = Connection;
                sqlcmd.CommandType = CommandType.Text;
                StringBuilder query = new StringBuilder();
                query.Append(" DELETE [dbo].[Cat_ColaboradorAreas] WHERE 1 = 1 ");
                if (userId.HasValue && userId.Value > 0)
                {
                    query.Append(" AND cve_Colaborador = @collaboratorId ");
                    sqlcmd.Parameters.AddWithValue("@collaboratorId", userId);
                }

                if (areaId.HasValue && areaId.Value > 0)
                {
                    query.Append(" AND cve_Area = @areaId ");
                    sqlcmd.Parameters.AddWithValue("@areaId", areaId.Value);
                }

                sqlcmd.CommandText = query.ToString();
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
        /// Método utilizado para mapear la información del catálogo de áreas.
        /// </summary>
        /// <param name="query">Consulta SQL para recuperar el catálogo de áreas.</param>
        /// <param name="username">Nombre de usuario.</param>
        /// <param name="userId">Id asociado al usuario.</param>
        /// <returns>Devuelve una lista que contiene la información de las áreas asociadas a un usuario.</returns>
        private List<AreaData> GetUserAreas(string query, string username = "", int? userId = null)
        {
            List<AreaData> userAreas = new List<AreaData>();
            try
            {
                Open();
                SqlCommand sqlcmd = new SqlCommand
                {
                    Connection = Connection,
                    CommandType = CommandType.Text,
                    CommandText = query
                };
                if (!string.IsNullOrEmpty(username))
                {
                    sqlcmd.Parameters.AddWithValue("@username", username);
                }

                if (userId.HasValue && userId.Value > 0)
                {
                    sqlcmd.Parameters.AddWithValue("@userId", userId);
                }

                var reader = sqlcmd.ExecuteReader();
                while (reader.Read())
                {
                    AreaData singleArea = new AreaData
                    {
                        AreaId = reader["cve_Area"] != DBNull.Value ? Convert.ToInt32(reader["cve_Area"]) : 0,
                        NameArea = reader["nameArea"] != DBNull.Value ? reader["nameArea"].ToString() : string.Empty,
                        DefaultArea = reader["defaultArea"] != DBNull.Value ? Convert.ToBoolean(reader["defaultArea"]) : false
                    };
                    userAreas.Add(singleArea);
                }

                reader.Close();
                Close();
            }
            catch (Exception ex)
            {
                throw;
            }

            return userAreas;
        }
    }
}
