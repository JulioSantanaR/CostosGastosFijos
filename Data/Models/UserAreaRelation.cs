namespace Data.Models
{
    /// <summary>
    /// Objeto que contiene la relación entre un usuario y un área específica.
    /// </summary>
    public class UserAreaRelation
    {
        /// <summary>
        /// Id asociado al usuario.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Id asociado al área.
        /// </summary>
        public int AreaId { get; set; }
    }
}