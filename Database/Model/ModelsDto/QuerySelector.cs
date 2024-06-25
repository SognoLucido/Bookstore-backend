using System.ComponentModel.DataAnnotations;

namespace Database.Model.Apimodels;

public class QuerySelector
{
    [MaxLength(30)]
    public string? ByAuthor { get; set ; }
    [MaxLength(30)]
    public string? ByBooktitle { get; set; }

    [MaxLength(30)]
    public string? ByCategory { get; set; }


}
