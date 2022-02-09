namespace Data.Models
{
    /// <summary>
    /// Objeto que contiene la información de un rol de usuario.
    /// </summary>
    public class UserRole
    {
        /// <summary>
        /// Id asociado al rol de usuario.
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Nombre del rol de usuario.
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// Bandera para saber si es un rol de usuario default.
        /// </summary>
        public bool DefaultRole { get; set; }
    }
}