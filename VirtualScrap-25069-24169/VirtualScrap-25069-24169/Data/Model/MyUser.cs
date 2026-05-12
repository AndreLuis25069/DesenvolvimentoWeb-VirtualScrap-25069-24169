using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualScrap_25069_24169.Data.Model
{
    public class MyUser
    {
        [Key] //Primary Key
        public int Id { get; set; }
        ///<summary>
        ///Nome do Utilizador.
        ///</summary>

        [Display(Name="Nome completo do Utilizador")]
        [Required(ErrorMessage ="O {0} é de preenchimento obrigatório!")]
        [StringLength(50)]
        public string Name { get; set; } = "";

        ///<summary>
        ///Numero de telefone do Utilizador
        /// </summary>
        [Display(Name="Numero de Telefone do Utilizador")]
        [StringLength(19)]
        [RegularExpression(@"\+?[0-9]{9,18}",ErrorMessage ="O {0} é de preenchimento obrigatório")]
        public string CellPhone { get; set; }


        ///<summary>
        ///Lista de comentários feita ao utilizador
        /// </summary>
        [InverseProperty(nameof(Comment.Autor))] 
        public ICollection<Comment> Sent_Comments { get; set; } = [];


        ///<summary>
        ///Lista de comentários feita Pelo utilizador
        /// </summary>
        [InverseProperty(nameof(Comment.Recipient))]
        public ICollection<Comment> Received_Comments { get; set; } = [];



        ///<summary>
        ///Chave Forasteira para ligar com a tabela de autenticação
        /// </summary>

        public string IdUser { get; set; } = null!;


        ///<summary>
        ///Lista de anuncios feitos pelo utilizador
        /// </summary>
        public ICollection<Post> PostsList { get; set; } = [];
        

        ///<summary>
        ///Lista de likes que este utilizador realizou
        ///</summary>
        public ICollection<Like> LikesList { get; set; } = [];
    }
}
