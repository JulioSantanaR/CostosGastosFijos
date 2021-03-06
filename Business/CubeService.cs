namespace Business
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Business.Services;
    using Data;
    using Data.Models;
    using Data.Models.Request;
    using Data.Repositories;

    /// <summary>
    /// Clase de negocio intermedia entre el acceso a datos y la capa del cliente, para operaciones sobre el cubo de información.
    /// </summary>
    public static class CubeService
    {
        /// <summary>
        /// Método utilizado para actualizar la información de la tabla de hechos, de acuerdo a los ejercicios que estén pendientes de actualizarse.
        /// </summary>
        /// <returns>Devuelve una bandera para determinar si la actualización fue correcta o no.</returns>
        public static bool UpdateProjectionTbl()
        {
            bool successUpdate = false;
            try
            {
                List<LogFactData> pendingProjections = LogProjectionService.GetPendingProjections();
                if (pendingProjections != null && pendingProjections.Count > 0)
                {
                    for (int i = 0; i < pendingProjections.Count; i++)
                    {
                        var singleProjection = pendingProjections[i];

                        // Actualizar la tabla de hechos de la proyección.
                        successUpdate = BudgetService.UpdateFactProjection(singleProjection.YearData, singleProjection.ChargeTypeId, singleProjection.ChargeType);
                        if (!successUpdate)
                        {
                            break;
                        }

                        // Actualizar la fecha y el estatus asociado al log de la tabla de hechos.
                        if (successUpdate)
                        {
                            singleProjection.ProjectionStatus = true;
                            singleProjection.DateActualization = DateTime.Now;
                            successUpdate = LogProjectionService.UpdateLogProjection(singleProjection);
                        }
                    }
                }
                else
                {
                    successUpdate = true;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return successUpdate;
        }

        /// <summary>
        /// Método utilizado para actualizar la información de la tabla de hechos asociada a cubo de asignación por canal.
        /// </summary>
        /// <returns>Devuelve una bandera para determinar si la actualización fue correcta o no.</returns>
        public static bool UpdateChannelAssignTbl()
        {
            bool successUpdate = false;
            try
            {
                List<LogFactData> pendingChannelAssign = LogChannelAssignService.GetPendingChannelAssign();
                if (pendingChannelAssign != null && pendingChannelAssign.Count > 0)
                {
                    for (int i = 0; i < pendingChannelAssign.Count; i++)
                    {
                        var singleAssign = pendingChannelAssign[i];

                        // Actualizar la tabla de hechos de la asignación por canal.
                        AccountsDataRequest accountsData = new AccountsDataRequest()
                        {
                            YearAccounts = singleAssign.YearData,
                            ChargeTypeAccounts = singleAssign.ChargeTypeId,
                            ExerciseType = singleAssign.ChargeType
                        };
                        successUpdate = BudgetService.UpdateFactTblAccounts(accountsData);
                        if (!successUpdate)
                        {
                            break;
                        }

                        // Actualizar la fecha y el estatus asociado al log de la tabla de hechos.
                        if (successUpdate)
                        {
                            singleAssign.ProjectionStatus = true;
                            singleAssign.DateActualization = DateTime.Now;
                            successUpdate = LogChannelAssignService.UpdateLogChannelAssign(singleAssign);
                        }
                    }
                }
                else
                {
                    successUpdate = true;
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return successUpdate;
        }

        /// <summary>
        /// Método utilizado para actualizar el cubo de información asociado a los costos/gastos fijos.
        /// </summary>
        /// <param name="jobName">Nombre asociado a la tarea programada del cubo (job).</param>
        /// <param name="jobId">Id asociado a la tarea programada del cubo (job).</param>
        /// <returns>Bandera para saber si el cubo se actualizó correctamente o no.</returns>
        public static bool UpdateCube(string jobName, string jobId)
        {
            bool successUpdate = false;
            try
            {
                string runDate = DateTime.Now.ToString("yyyyMMdd");
                string runTime = DateTime.Now.AddSeconds(-10).ToString("HHmmss");
                CubeDAO cubeDao = new CubeDAO();
                bool cubeSuccessUpdate = cubeDao.CubeUpdateProcess(jobName);
                if (cubeSuccessUpdate)
                {
                    DataTable dataTable = null;
                    do
                    {
                        dataTable = cubeDao.JobExecutionLog(runDate, runTime, jobId);
                    } while (dataTable == null || dataTable.Rows.Count == 0);

                    if (dataTable != null && dataTable.Rows.Count > 0)
                    {
                        var rows = dataTable.AsEnumerable().Where(r => r.Field<string>("Estatus") == "Fallo Ejecución").ToList();
                        if (rows.Count > 0)
                        {
                            successUpdate = false;
                        }
                        else
                        {
                            successUpdate = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralRepository generalRepository = new GeneralRepository();
                generalRepository.WriteLog("UpdateCube()." + "Error: " + ex.Message);
            }

            return successUpdate;
        }
    }
}