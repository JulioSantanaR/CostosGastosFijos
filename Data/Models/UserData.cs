namespace Data.Models
{
    /// <summary>
    /// Objeto que contiene la información asociada a cada colaborador.
    /// </summary>
    public class UserData
    {
        /// <summary>
        /// Identificador único del colaborador.
        /// </summary>
        public int CollaboratorId { get; set; }

        /// <summary>
        /// Nombre completo del colaborador.
        /// </summary>
        public string CollaboratorName { get; set; }

        /// <summary>
        /// Correo del colaborador.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Username del colaborador.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Id asociado al área.
        /// </summary>
        public int AreaId { get; set; }

        /// <summary>
        /// Nombre del área.
        /// </summary>
        public string NameArea { get; set; }

        /// <summary>
        /// Bandera para saber si es un área default.
        /// </summary>
        public bool DefaultArea { get; set; }
    }
}