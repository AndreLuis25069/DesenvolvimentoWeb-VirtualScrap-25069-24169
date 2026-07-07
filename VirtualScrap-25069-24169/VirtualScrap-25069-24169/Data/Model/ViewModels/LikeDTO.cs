using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace VirtualScrap_25069_24169.Models.ViewModels
{
    public class LikeDTO
    {
        [Required(ErrorMessage = "O ID do autor do gosto é obrigatório.")]
        public int LikeAutorFK { get; set; }

        [Required(ErrorMessage = "O ID do anúncio (Post) é obrigatório.")]
        public int PostFK { get; set; }

        // Propriedades auxiliares de leitura ajudam no get para mostrar os nomes
        public string AutorName { get; set; } = "";
        public string PostTitle { get; set; } = "";
    }
}
