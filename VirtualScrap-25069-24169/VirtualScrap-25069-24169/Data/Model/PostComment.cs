using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


 

namespace VirtualScrap_25069_24169.Data.Model
    {
        public class PostComment
        {
            /// <summary>
            /// Chave primária para cada comentário, será chave estrangeira no anuncio
            /// </summary>
            [Key]
            public int Id { get; set; }

            ///<summary>
            /// Conteudo do comentário/Avaliação
            /// </summary>
            [Display(Name = "Descrição")]
            [Required(ErrorMessage = "A {0} é de escrita obrigatória")]
            [StringLength(500)]
            public string Description { get; set; } = null!;

            ///<summary>
            ///Data de quando foi feito o comentário
            /// </summary>
            [DataType(DataType.Date)]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public DateTime CommentDate { get; set; } = DateTime.Now;

            ///<summary>
            ///Autor do Comentário/Avaliação deixada num Post
            /// </summary>
            [ForeignKey(nameof(Autor))]
            public int AutorFK { get; set; }
            [ValidateNever]

            ///<summary>
            ///Objeto do tipo MyUser para o autor
            ///</summary>
            public MyUser Autor { get; set; } = null!;

            /// <summary>
            /// Objeto do tipo post para referenciar onde o comentario foi feito
            /// </summary>
            public Post CommentedPost { get; set; } = null!;


            /// <summary>
            /// Chave estrangeira do post comentado
            /// </summary>
            [ForeignKey(nameof(PostFK))]
            public int PostFK { get; set; }


        }
    }


