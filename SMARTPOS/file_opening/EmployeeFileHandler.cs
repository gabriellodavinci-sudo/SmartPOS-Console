using SMARTPOS.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace SMARTPOS.file_opening
{
    class EmployeeFileHandler
    {
        private string filePath;

        public EmployeeFileHandler(string filePath)
        {
            this.filePath = filePath;
        }

        public List<Employee> LoadEmployees()
        {
            var employees = new List<Employee>();
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found: " + filePath);
                return employees;
            }

            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] parts = line.Split(',');

                // New format with username and contactInfo
                if (parts.Length >= 7 && double.TryParse(parts[5], out double sales))
                {
                    employees.Add(new Employee
                    {
                        nameEmployee = parts[0].Trim(),
                        password = parts[1].Trim(),
                        username = parts[2].Trim(),
                        contactInfo = parts[3].Trim(),
                        mobileNO = parts[4].Trim(),
                        sales = sales,
                        role = parts[6].Trim()
                    });
                }
                // Old format fallback
                else if (parts.Length >= 5 && double.TryParse(parts[3], out sales))
                {
                    employees.Add(new Employee
                    {
                        nameEmployee = parts[0].Trim(),
                        password = parts[1].Trim(),
                        mobileNO = parts[2].Trim(),
                        sales = sales,
                        role = parts[4].Trim(),
                        username = parts[0].Trim(),          // fallback
                        contactInfo = parts[2].Trim()       // fallback using mobileNO
                    });
                }
            }

            return employees;
        }

        public void SaveEmployee(List<Employee> employees)
        {
            using (var writer = new StreamWriter(filePath))
            {
                foreach (var e in employees)
                {
                    writer.WriteLine($"{e.nameEmployee},{e.password},{e.username},{e.contactInfo},{e.mobileNO},{e.sales},{e.role}");
                }
            }
        }
    }
}
