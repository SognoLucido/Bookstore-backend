
using System.ComponentModel.DataAnnotations;


namespace Database.Model.ModelsDto
{
    public class Rolechanger
    {
        [EmailAddress]
        public string? email {  get; set; }

        
        public Guid? UserID { get; set; }

        [MaxLength(20)]
        public string Role {  get; set; }


    }
}
