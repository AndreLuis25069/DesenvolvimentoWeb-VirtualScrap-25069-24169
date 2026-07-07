using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace VirtualScrap_25069_24169.Models.ViewModels
{
    public class LikeDTO
    {
        [Required(ErrorMessage = "O ID do autor do gosto é obrigatório.")]
        public int AutorId { get; set; }

        [Required(ErrorMessage = "O ID do anúncio (Post) é obrigatório.")]
        public int PostId { get; set; }

        // Propriedades auxiliares de leitura ajudam no get para mostrar os nomes
        public string AutorName { get; set; } = "";
        public string PostTitle { get; set; } = "";
    }
}
