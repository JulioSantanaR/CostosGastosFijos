namespace Data.Models.Request
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Objeto tipo request auxiliar en la conversión de columnas a renglones en un DataTable.
    /// </summary>
    public class PivotTableRequest
    {
        /// <summary>
        /// Columnas que se desean convertir a renglones.
        /// </summary>
        public List<string> ColumnsPivot { get; set; }

        /// <summary>
        /// Nombre en común para las columnas que ahora serán renglones.
        /// </summary>
        public string NewColumnName { get; set; }

        /// <summary>
        /// Tipo de dato asociado a la nueva columna.
        /// </summary>
        public Type NewColumnType { get; set; } = typeof(string);

        /// <summary>
        /// Bandera para saber si incluir o no el valor del MES.
        /// </summary>
        public bool IncludeMonth { get; set; }
    }
}