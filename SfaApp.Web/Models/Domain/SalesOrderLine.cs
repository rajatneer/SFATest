using System.ComponentModel.DataAnnotations;

namespace SfaApp.Web.Models.Domain;

public class SalesOrderLine
{
    public int Id { get; set; }

    [Required]
    public int SalesOrderId { get; set; }

    public SalesOrder? SalesOrder { get; set; }

    [Required]
    public int ProductId { get; set; }

    public Product? Product { get; set; }

    [Range(0.001, 999999)]
    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }
}