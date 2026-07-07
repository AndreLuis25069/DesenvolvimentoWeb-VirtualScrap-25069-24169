using System.ComponentModel.DataAnnotations;

namespace VirtualScrap_25069_24169.Models.ViewModels
{
    public class PostDTO
    {
        [Required(ErrorMessage = "O Título é de preenchimento obrigatório!")]
        [StringLength(50)]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "A Descrição é de preenchimento obrigatório!")]
        [StringLength(150)]
        public string Description { get; set; } = "";

        [StringLength(19)]
        [RegularExpression(@"\+?[0-9]{9,18}", ErrorMessage = "Ocorreu um erro!")]
        public string CellPhone { get; set; } = "";

        public DateTime PostDate { get; set; }

        [StringLength(50)]
        public string Photo { get; set; } = "";

        [Required(ErrorMessage = "O Preço é de preenchimento obrigatório!")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "A Localização é de preenchimento obrigatório!")]
        [StringLength(100)]
        public string Localizacao { get; set; } = "";

        // Campos auxiliares para receber o texto vindo da API
        [Required(ErrorMessage = "A Categoria é obrigatória.")]
        public string CategoryName { get; set; } = "";

        public string OwnerName { get; set; } = "";
    }
}