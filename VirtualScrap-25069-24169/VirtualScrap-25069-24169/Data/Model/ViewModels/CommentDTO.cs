using System;
using System.ComponentModel.DataAnnotations;

namespace VirtualScrap_25069_24169.Models.ViewModels
{
    public class CommentDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A Descrição é de escrita obrigatória")]
        [StringLength(500)]
        public string Description { get; set; } = "";

        public DateTime CommentDate { get; set; }

        // Chaves Estrangeiras exatas do  modelo Comment.cs
        public int? AutorFK { get; set; }

        [Required(ErrorMessage = "O ID do destinatário (RecipientFK) é obrigatório.")]
        public int RecipientFK { get; set; }

        [Required(ErrorMessage = "O Rating é de preenchimento obrigatório.")]
        [Range(1, 5, ErrorMessage = "O Rating deve ser um valor entre 1 e 5.")]
        public int Rating { get; set; } = 5;

        // Propriedades auxiliares para as leituras (GET) virem limpas no JSON
        public string AutorName { get; set; } = "";
        public string RecipientName { get; set; } = "";
    }
}