using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExpenseTracker.Models.Constants;

namespace ExpenseTracker.Models;

public class Category
{
    [Key]
    public int? CategoryId { get; set; }

    [Column(TypeName = "nvarchar(50)")]
    public string Title { get; set; }

    [Column(TypeName = "nvarchar(5)")]
    public string Icon { get; set; } = "";

    [Column(TypeName = "nvarchar(10)")]
    public ExpenseTypes Type { get; set; } = ExpenseTypes.Expense;
    
    [NotMapped]
    public string TypeName => Type.ToString();

    [NotMapped]
    public string? TitleWithIcon {
        get
        {
            return this.Icon + " " + this.Title;
        }
    }
}
