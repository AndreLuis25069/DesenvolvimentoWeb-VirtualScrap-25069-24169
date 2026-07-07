using System.ComponentModel.DataAnnotations;

namespace VirtualScrap_25069_24169.Models.ViewModels
{
    public class MyUserDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O Nome completo do Utilizador é de preenchimento obrigatório!")]
        [StringLength(50)]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "O Numero de Telefone do Utilizador é de preenchimento obrigatório.")]
        [StringLength(19)]
        [RegularExpression(@"\+?[0-9]{9,18}", ErrorMessage = "O Numero de Telefone do Utilizador não respeita o formato desejado ex:914567899.")]
        public string CellPhone { get; set; } = "";

        [StringLength(50)]
        public string Photo { get; set; } = "";

        public bool IsDeleted { get; set; }

        // Campo opcional para o GET se quiseres saber a quantos posts ou likes ele está associado
        public int TotalPosts { get; set; }
    }
}