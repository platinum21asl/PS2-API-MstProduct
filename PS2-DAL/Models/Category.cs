using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PS2_DAL.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        [DisplayName("Category Name")]
        public string? Name { get; set; }

        [DisplayName("Display Name")]
        [Range(1, 100)]
        public int DisplayOrder { get; set; }
    }
}
