using SMARTPOS.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SMARTPOS.functions
{
    public class Sales
    {
        public Order CreateOrder(List<product> products)
        {
            Order order = new Order();
            while (true)
            {
                Console.Write("Enter product ID or Name (or 0 to finish): ");
                string input = Console.ReadLine();
                if (input == "0") break;

                var product = products.FirstOrDefault(p =>
                    p.id.Equals(input, StringComparison.OrdinalIgnoreCase));

                if (product == null)
                {
                    product = products.FirstOrDefault(p =>
                        p.name.Equals(input, StringComparison.OrdinalIgnoreCase));
                }

                if (product == null)
                {
                    Console.WriteLine("Product not found.");
                    continue;
                }

                Console.Write($"Enter quantity for {product.name}: ");
                if (!double.TryParse(Console.ReadLine(), out double qty))
                {
                    Console.WriteLine("Invalid quantity.");
                    continue;
                }

                if (qty > product.stock)
                {
                    Console.WriteLine("Not enough stock.");
                    continue;
                }

                double subtotal = product.price * qty;
                order.Items.Add(new OrderItem
                {
                    Product = product,
                    Quantity = qty,
                    Subtotal = subtotal
                });
                if (qty < 0)
                {
                    product.stock += -1 * qty;
                }
                if (qty >= 0) product.stock -= qty;
                Console.WriteLine($"{product.name} added to order.\n");
            }
            return order;
        }

        public double CalculateTotal(Order order)
        {
            order.Total = order.Items.Sum(i => i.Subtotal);
            return order.Total;
        }
    }

    public class Order
    {
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public double Total { get; set; }
    }

    public class OrderItem
    {
        public product Product { get; set; }
        public double Quantity { get; set; }
        public double Subtotal { get; set; }
    }
}
