using System;
using System.ComponentModel.DataAnnotations;

namespace VirtualScrap_25069_24169.Models.ViewModels
{
    public class PostCommentDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A Descrição é de escrita obrigatória")]
        [StringLength(500)]
        public string Description { get; set; } = "";

        public DateTime CommentDate { get; set; }

        // Chaves estrangeiras obrigatórias para associar á base de dados 
        public int? AutorFK { get; set; }

        [Required(ErrorMessage = "O ID do post comentado é obrigatório.")]
        public int PostFK { get; set; }

        // Propriedades auxiliares para as leituras (GET) serem legíveis em JSON
        public string AutorName { get; set; } = "";
        public string CommentedPostTitle { get; set; } = "";
    }
}