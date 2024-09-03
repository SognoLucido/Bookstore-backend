
using System.ComponentModel.DataAnnotations;


namespace Database.Model.Apimodels
{
    public class Pagemodel
    {

        [Required]
        [Range(1, int.MaxValue)]
        public int Page {  get; set; }

        [Required]
        [Range(1,50)]
        public int Pagesize { get; set; }
    }
}
