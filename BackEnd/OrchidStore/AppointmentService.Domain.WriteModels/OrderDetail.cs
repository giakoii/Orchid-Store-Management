using System;
using System.Collections.Generic;

namespace OrchidStore.Infrastructure;

public partial class OrderDetail
{
    public int Id { get; set; }

    public int OrchidId { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public int OrderId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string UpdatedBy { get; set; } = null!;

    public virtual Orchid Orchid { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
