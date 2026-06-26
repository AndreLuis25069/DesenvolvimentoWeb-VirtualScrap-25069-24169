using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualScrap_25069_24169.Data.Model
{
    public class Post
    {
        [Key] //Primary Key
        public int Id { get; set; }

        ///<summary>
        ///Título do post (ou nome do produto a ser vendido)
        ///</summary>
        [Display(Name = "Título")]
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório!")]
        [StringLength(50)]
        public string Title { get; set; } = "";


        ///<summary>
        ///Descrição do post
        ///</summary>
        [Display(Name = "Descrição")]
        [Required(ErrorMessage = "A {0} é de preenchimento obrigatório!")]
        [StringLength(150)]
        public string Description { get; set; } = "";

        ///<summary>
        ///Lista de likes referentes ao post
        /// </summary>
        public ICollection<Like> LikesList { get; set; } = [];

        /// <summary>
        /// Lista de comentarios 
        /// </summary>
        public ICollection<PostComment> Commentaries { get; set; } = [];

        ///<summary>
        ///Contacto do vendedor
        /// </summary>
        [StringLength(19)]
        [RegularExpression(@"\+?[0-9]{9,18}", ErrorMessage = "Ocorreu um erro!")]
        public string CellPhone { get; set; } = "";

        ///<summary>
        ///Data em que o post foi publicado
        /// </summary>
        [DataType(DataType.Date)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime PostDate { get; set; } = DateTime.Now;

        ///<summary>
        ///Foto do post
        /// </summary>
        [Display(Name = "Foto")]
        [Required(ErrorMessage = "A {0} é de preenchimento obrigatório!")]
        [StringLength(50)]
        [ValidateNever]
        public string Photo { get; set; } = "";



        ///<summary>
        ///Preço relativo ao produto do post
        ///</summary>
        [Precision(9, 2)]
        public decimal Price { get; set; }

        /// <summary>
        /// Variavel para a localização do anuncio
        /// </summary>
        [Display(Name = "Localização")]
        [Required(ErrorMessage = "A {0} é de preenchimento obrigatório!")]
        [StringLength(100)]
        public string Localizacao { get; set; }


        /// <summary>
        /// Variavel auxiliar para a conversão do preço recebido em string, para um valor decimal face as regras da lingua portuguesa
        /// </summary>
        [NotMapped]
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório!")]
        [Display(Name = "Preço")]
        [StringLength(10)]
        [RegularExpression("[0-9]{1,7}([,.][0-9]{1,2})?",ErrorMessage = "O {0} deve ser um número com até 2 casas decimais")]
        public string AuxPrice { get; set; } = "";


       ///<summary>
       ///Categoria do post
       /// </summary>
       [ValidateNever]
        public Category PostCategory { get; set; } = null!;

        /// <summary>
        /// Chave estrangeira para referenciar a categoria
        /// </summary>
        [Display(Name = "Categoria")]
        [ForeignKey(nameof(PostCategory))]
        public int? CategoryFK { get; set; }




        ///<summary>
        ///Dono do post anunciado
        /// </summary>
        [ValidateNever]
        [Display(Name = "Autor")]
        [ForeignKey(nameof(OwnerFK))]
        public MyUser PostOwner { get; set; }

        
   
        ///<summary>
        ///Chave que liga my user com post
        /// </summary>
        public int OwnerFK { get; set; }
    }
}
