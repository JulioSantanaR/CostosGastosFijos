namespace Data
{
    using System;
    using System.Data.SqlClient;
    using Data.Models;

    /// <summary>
    /// Clase utilizada para mapear la información obtenida desde la Base de Datos.
    /// </summary>
    public static class Mapping
    {
        /// <summary>
        /// Método utilizado para mapear la información relacionada a la consulta de un colaborador.
        /// </summary>
        /// <param name="reader">Información obtenida desde la Base de datos.</param>
        /// <param name="userRoleExists">Bandera para determinar si se debe agregar la información del rol de usuario.</param>
        /// <returns>Devuelve un objeto que contiene la información mapeada del colaborador.</returns>
        public static UserData MapCollaborator(SqlDataReader reader, bool userRoleExists = false)
        {
            UserData userInformation = new UserData();
            userInformation.CollaboratorId = reader["cve_Colaborador"] != DBNull.Value ? Convert.ToInt32(reader["cve_Colaborador"]) : 0;
            userInformation.CollaboratorName = reader["nombre"] != DBNull.Value ? reader["nombre"].ToString() : string.Empty;
            userInformation.Email = reader["correo"] != DBNull.Value ? reader["correo"].ToString() : string.Empty;
            userInformation.Username = reader["usuario"] != DBNull.Value ? reader["usuario"].ToString() : string.Empty;
            userInformation.RoleId = reader["cve_RolUsuario"] != DBNull.Value ? Convert.ToInt32(reader["cve_RolUsuario"]) : 0;
            if (userRoleExists)
            {
                userInformation.RolUsuario = reader["rolUsuario"] != DBNull.Value ? reader["rolUsuario"].ToString() : string.Empty;
            }

            return userInformation;
        }

        /// <summary>
        /// Método utilizado para mapear la información relacionada a los roles de usuario.
        /// </summary>
        /// <param name="reader">Información obtenida desde la Base de datos.</param>
        /// <returns>Devuelve la información de los roles de usuario.</returns>
        public static UserRole MapRole(SqlDataReader reader)
        {
            UserRole roleInformation = new UserRole();
            roleInformation.RoleId = reader["cve_RolUsuario"] != DBNull.Value ? Convert.ToInt32(reader["cve_RolUsuario"]) : 0;
            roleInformation.RoleName = reader["rolUsuario"] != DBNull.Value ? reader["rolUsuario"].ToString() : string.Empty;
            roleInformation.DefaultRole = reader["defaultRole"] != DBNull.Value ? Convert.ToBoolean(reader["defaultRole"]) : false;
            return roleInformation;
        }

        /// <summary>
        /// Método utilizado para mapear la información relacionada al catálogo de áreas.
        /// </summary>
        /// <param name="reader">Información obtenida desde la Base de datos.</param>
        /// <returns>Devuelve la información asociada al catálogo de áreas.</returns>
        public static AreaData MapArea(SqlDataReader reader)
        {
            AreaData areaInformation = new AreaData();
            areaInformation.AreaId = reader["cve_Area"] != DBNull.Value ? Convert.ToInt32(reader["cve_Area"]) : 0;
            areaInformation.NameArea = reader["nombre"] != DBNull.Value ? reader["nombre"].ToString() : string.Empty;
            areaInformation.DefaultArea = reader["defaultArea"] != DBNull.Value ? Convert.ToBoolean(reader["defaultArea"]) : false;
            return areaInformation;
        }

        /// <summary>
        /// Método utilizado para mapear la información relacionada al historial de carga de archivos.
        /// </summary>
        /// <param name="reader">Información obtenida desde la Base de datos.</param>
        /// <param name="typeFileExists">Bandera para saber si mapear la columna de tipo de archivo o no.</param>
        /// <param name="collaboratorNameExists">Bandera para saber si mapear la columna asociada al nombre del colaborador.</param>
        /// <param name="areaNameExists">Bandera para saber si mapear la columna asociada al nombre del área.</param>
        /// <param name="chargeTypeExists">Bandera para saber si mapear la columna asociada al tipo de carga.</param>
        /// <returns>Devuelve la información asociada al historial de carga de archivos.</returns>
        public static FileLogData MapFileLog(SqlDataReader reader, bool typeFileExists = false, bool collaboratorNameExists = false, bool areaNameExists = false, bool chargeTypeExists = false)
        {
            FileLogData logInformation = new FileLogData();
            logInformation.FileLogId = reader["cve_LogArchivo"] != DBNull.Value ? Convert.ToInt32(reader["cve_LogArchivo"]) : 0;
            logInformation.FileName = reader["nombreArchivo"] != DBNull.Value ? reader["nombreArchivo"].ToString() : string.Empty;
            logInformation.ChargeDate = reader["fechaDeCarga"] != DBNull.Value ? Convert.ToDateTime(reader["fechaDeCarga"]) : DateTime.MinValue;
            logInformation.ApprovalFlag = reader["aprobado"] != DBNull.Value ? Convert.ToBoolean(reader["aprobado"]) : false;
            logInformation.UserId = reader["cve_Colaborador"] != DBNull.Value ? Convert.ToInt32(reader["cve_Colaborador"]) : 0;
            logInformation.FileTypeId = reader["cve_TipoArchivo"] != DBNull.Value ? Convert.ToInt32(reader["cve_TipoArchivo"]) : 0;
            logInformation.AreaId = reader["cve_Area"] != DBNull.Value ? Convert.ToInt32(reader["cve_Area"]) : 0;
            logInformation.ChargeTypeId = reader["cve_TipoCarga"] != DBNull.Value ? Convert.ToInt32(reader["cve_TipoCarga"]) : 0;
            logInformation.YearData = reader["anio"] != DBNull.Value ? Convert.ToInt32(reader["anio"]) : 0;
            if (typeFileExists)
            {
                logInformation.FileTypeName = reader["tipoArchivo"] != DBNull.Value ? reader["tipoArchivo"].ToString() : string.Empty;
            }

            if (collaboratorNameExists)
            {
                logInformation.CollaboratorName = reader["nombreColaborador"] != DBNull.Value ? reader["nombreColaborador"].ToString() : string.Empty;
            }

            if (areaNameExists)
            {
                logInformation.AreaName = reader["nombreArea"] != DBNull.Value ? reader["nombreArea"].ToString() : string.Empty;
            }

            if (chargeTypeExists)
            {
                logInformation.ChargeTypeName = reader["tipoCarga"] != DBNull.Value ? reader["tipoCarga"].ToString() : string.Empty;
            }

            return logInformation;
        }

        /// <summary>
        /// Método utilizado para mapear la información relacionada con el catálogo de tipos de archivos.
        /// </summary>
        /// <param name="reader">Información obtenida desde la Base de datos.</param>
        /// <returns>Devuelve la información asociada al catálogo de tipos de archivos.</returns>
        public static FileType MapFileType(SqlDataReader reader)
        {
            FileType fileType = new FileType()
            {
                FileTypeId = reader["cve_TipoArchivo"] != DBNull.Value ? Convert.ToInt32(reader["cve_TipoArchivo"]) : 0,
                FileTypeName = reader["tipoArchivo"] != DBNull.Value ? reader["tipoArchivo"].ToString() : string.Empty,
            };
            return fileType;
        }

        /// <summary>
        /// Método utilizado para mapear la información relacionada con el log referente a la tabla de hechos.
        /// </summary>
        /// <param name="reader">Información obtenida desde la Base de datos.</param>
        /// <returns>Devuelve la información asociada al log de la tabla de hechos.</returns>
        public static LogFactData MapLogFact(SqlDataReader reader)
        {
            LogFactData singleProjection = new LogFactData();
            singleProjection.LogFactId = reader["cve_LogFactId"] != DBNull.Value ? Convert.ToInt32(reader["cve_LogFactId"]) : 0;
            singleProjection.ChargeType = reader["tipoCarga"] != DBNull.Value ? reader["tipoCarga"].ToString() : string.Empty;
            singleProjection.ChargeTypeId = reader["cve_TipoCarga"] != DBNull.Value ? Convert.ToInt32(reader["cve_TipoCarga"]) : 0;
            singleProjection.YearData = reader["anio"] != DBNull.Value ? Convert.ToInt32(reader["anio"]) : 0;
            singleProjection.ProjectionStatus = reader["estatus"] != DBNull.Value ? Convert.ToBoolean(reader["estatus"]) : false;
            singleProjection.DateActualization = reader["fechaActualizacion"] != DBNull.Value ? Convert.ToDateTime(reader["fechaActualizacion"]) : DateTime.MinValue;
            return singleProjection;
        }
    }
}