namespace Data.DAO
{
    using Data.Models;
    using Data.Repositories;
    using FastMember;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Clase asociada al acceso a datos para manipular la información de la relación usuario-áreas.
    /// </summary>
    public class UserAreasDAO : CommonDAO
    {
        /// <summary>
        /// Cadena de conexión asociada a la Base de Datos de Costos y Gastos fijos.
        /// </summary>
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["dbCGFijos"].ToString();

        /// <summary>
        /// Constructor default de la clase.
        /// </summary>
        public UserAreasDAO()
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