using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Contacts
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }
        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        [Required]
        public string FullName { get; set; }
        
        [StringLength(50)]
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public DateTime DateSent {get; set;}
        public string Message {get; set;}
        [StringLength(20)]
        [Phone]
        public string Phone { get; set; }
    }
}