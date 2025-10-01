using System;
using Domain.Common;

namespace Domain.Entities;

public class OrderItem: Entity
{
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem() { }

    public OrderItem(string productName, int quantity)
    {
        ProductName = productName;
        Quantity = quantity;
    }
}