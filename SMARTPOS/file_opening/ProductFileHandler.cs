using SMARTPOS.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace SMARTPOS.file_opening
{
    class ProductFileHandler
    {
        private string filePath;

        public ProductFileHandler(string filePath)
        {
            this.filePath = filePath;
        }

        public List<product> LoadProducts()
        {
            var products = new List<product>();
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found: " + filePath);
                return products;
            }

            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split(',');
                if (parts.Length < 3) continue;

                string id = parts[0].Trim();
                string name = parts[1].Trim();
                if (!double.TryParse(parts[2].Trim(), out double price)) continue;
                double stock = 0;
                if (parts.Length >= 4)
                    double.TryParse(parts[3].Trim(), out stock);

                products.Add(new product
                {
                    id = id,
                    name = name.Replace("\r", "").Replace("\n", ""),
                    price = price,
                    stock = stock
                });
            }

            Console.WriteLine($"Products loaded: {products.Count}");
            return products;
        }

        public void SaveProducts(List<product> products)
        {
            using (var writer = new StreamWriter(filePath))
            {
                foreach (var p in products)
                {
                    writer.WriteLine($"{p.id},{p.name},{p.price},{p.stock}");
                }
            }
        }
    }
}
