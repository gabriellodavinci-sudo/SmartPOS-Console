using SMARTPOS.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SMARTPOS.functions
{
    public class Search
    {
        public void SearchByName(List<product> products, string name)
        {
            if (products == null || products.Count == 0)
            {
                Console.WriteLine("No products available to search.");
                return;
            }

            string searchName = name.Trim();
            var product = products.FirstOrDefault(p =>
                CleanString(p.name).Equals(searchName, StringComparison.OrdinalIgnoreCase));

            if (product != null)
                product.getproductinfo();
            else
                Console.WriteLine("Product not found");
        }

        public void SearchById(List<product> products, string id)
        {
            if (products == null || products.Count == 0)
            {
                Console.WriteLine("No products available to search.");
                return;
            }

            string searchId = id.Trim();
            var product = products.FirstOrDefault(p =>
                CleanString(p.id).Equals(searchId, StringComparison.OrdinalIgnoreCase));

            if (product != null)
                product.getproductinfo();
            else
                Console.WriteLine("Product not found");
        }

        private string CleanString(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Trim().Replace("\r", "").Replace("\n", "").Replace("\uFEFF", "");
        }
    }
}
