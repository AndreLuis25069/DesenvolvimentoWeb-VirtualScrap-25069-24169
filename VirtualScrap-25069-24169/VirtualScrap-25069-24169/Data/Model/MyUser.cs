using System.ComponentModel.DataAnnotations;

namespace VirtualScrap_25069_24169.Data.Model
{
    public class MyUser
    {
        [Key] //Primary Key

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
        ///Chave Forasteira para ligar com a tabela de autenticação
        /// </summary>

        public string IdUser { get; set; } = null!;
       
    }
}
