namespace Data.Models
{
    using System;

    /// <summary>
    /// Objeto que contiene la información del log asociado a la tabla de hechos de la proyección.
    /// </summary>
    public class LogFactData
    {
        /// <summary>
        /// Id asociado al log de la tabla de hechos.
        /// </summary>
        public int LogFactId { get; set; }

        /// <summary>
        /// Tipo de carga.
        /// </summary>
        public string ChargeType { get; set; }

        /// <summary>
        /// Id asociado al tipo de carga.
        /// </summary>
        public int ChargeTypeId { get; set; }

        /// <summary>
        /// Año de la carga.
        /// </summary>
        public int YearData { get; set; }

        /// <summary>
        /// Estatus de la proyección (0 - pendiente, 1 - actualizada).
        /// </summary>
        public bool ProjectionStatus { get; set; }

        /// <summary>
        /// Fecha en que se ejecutó la última actualización de la proyección.
        /// </summary>
        public DateTime DateActualization { get; set; }
    }
}