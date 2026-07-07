using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace VirtualScrap_25069_24169.Data.Model.ViewModels
{
    
   
        // Usado no momento do GET  para listar as categorias (retorna o Id e o Nome)
        public class CategoryDTO
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }

        // Usado para receber dados da API, quando se quer criar uma categoria por exemplo.
        public class CategorySimplerDTO
        {
        [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
        [StringLength(50)]
        public string Name { get; set; } = "";
    }
    }

