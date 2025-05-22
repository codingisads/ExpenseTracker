using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models;

public class Transaction
{
    [Key]
    public int? TransactionId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "You must specify a category")]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Amount should be greater than 0.")]
    public double Amount { get; set; }

    [Column(TypeName = "nvarchar(500)")]
    public string? Note { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;

    [NotMapped]
    public string? CategoryTitleWithIcon
    {
        get
        {
            if (Category != null)
            {
                return Category.Icon + " " + Category.Title;
            }
            return null;
        }
    }

    [NotMapped]
    public string? AmountWithCurrency
    {
        get
        {
            
            return Category?.Type == Constants.ExpenseTypes.Expense ? "-$" + Amount : "+$" + Amount;
        }
    }
}
