using System;

namespace SMARTPOS.Models
{
    public class product
    {
        public string id { get; set; }
        public string name { get; set; }
        public double price { get; set; }
        public double stock { get; set; }

        public void getproductinfo()
        {
            Console.WriteLine($"PRODUCT INFO:\nName: {name}\nID: {id}\nPrice: {price}\nStock: {stock}");
        }

        public double Calculateprice(double quantity)
        {
            double subtotal = price * quantity;
            stock -= quantity;
            return subtotal;
        }
    }
}
