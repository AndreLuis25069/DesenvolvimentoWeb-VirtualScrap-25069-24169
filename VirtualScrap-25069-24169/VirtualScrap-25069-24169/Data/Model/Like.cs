using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualScrap_25069_24169.Data.Model
{   

    /// <summary>
    /// Chave Primaria Composta, para cada like, pois um like é uma ponte entre um utilizador e um anuncio 
    /// </summary>
    [PrimaryKey(nameof(LikeAutorFK), nameof(PostFK))]
    public class Like
    {   
       
        ///<summary>
        ///Autor do Like deixado num anuncio
        /// </summary>
        
        public int LikeAutorFK { get; set; }

        ///<summary>
        ///Objeto do tipo MyUser para o autor do like
        ///</summary>
        [ValidateNever]
        [ForeignKey(nameof(LikeAutorFK))]
        public MyUser LikeAutor { get; set; } = null!;


        /// <summary>
        /// Referência ao post a que pertence o like
        /// </summary>
        [ForeignKey(nameof(LikedPost))]
        public int PostFK { get; set; }


        ///<summary>
        ///Objeto do tipo Post/Anuncio
        ///</summary>
        [ValidateNever]
        
        public Post LikedPost { get; set; } = null!;

    }
}
