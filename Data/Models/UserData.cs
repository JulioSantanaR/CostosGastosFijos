namespace Data.Models
{
    using System.Collections.Generic;

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
        /// Id asociado al rol de usuario.
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Areas asociadas al usuario.
        /// </summary>
        public List<AreaData> Areas { get; set; }

        /// <summary>
        /// Nombre asociado al rol del usuario.
        /// </summary>
        public string RolUsuario { get; set; }
    }
}