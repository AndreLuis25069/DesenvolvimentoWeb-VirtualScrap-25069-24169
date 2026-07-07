using System.ComponentModel.DataAnnotations;

namespace VirtualScrap_25069_24169.Data.Model.ViewModels
{
    public class LoginDTO
    {

        [System.ComponentModel.DataAnnotations.Required]
        [StringLength(50)]
        public string Username { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Required]
        [StringLength(50)]
        public string Password { get; set; } = "";
    }
}
