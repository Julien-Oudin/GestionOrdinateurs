using System.ComponentModel.DataAnnotations;
using GestionOrdinateurs.Shared.enums;

namespace GestionOrdinateurs.Shared.classes;





public class User
{
    public int Id { get; set; } //(clé primaire)

    [Required]
	public string Login { get; set; } = string.Empty;

    public RoleEnum Role { get; set; }
    
}