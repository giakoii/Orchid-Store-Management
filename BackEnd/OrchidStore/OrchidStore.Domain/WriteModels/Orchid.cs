namespace OrchidStore.Domain.WriteModels;

public partial class Orchid
{
    public int OrchidId { get; set; }

    public bool IsNatural { get; set; }

    public string? OrchidDescription { get; set; }

    public string OrchidName { get; set; } = null!;

    public string? OrchidUrl { get; set; }

    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
