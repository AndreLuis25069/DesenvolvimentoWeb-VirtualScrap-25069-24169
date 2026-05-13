using System.ComponentModel.DataAnnotations;

namespace VirtualScrap_25069_24169.Data.Model
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome da categoria
        /// </summary>
        public string Name { get; set; } = "";

    }
}
