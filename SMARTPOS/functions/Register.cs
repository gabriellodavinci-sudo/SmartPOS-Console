using SMARTPOS.Models;
using System;
using System.Text;

namespace SMARTPOS.functions
{
    public class Register
    {
        protected string ReadPassword()
        {
            var sb = new StringBuilder();
            ConsoleKeyInfo keyInfo;

            while (true)
            {
                keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                    {
                        sb.Length -= 1;
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    sb.Append(keyInfo.KeyChar);
                    Console.Write('*');
                }
            }

            return sb.ToString();
        }

        public Employee RegisterEmployee()
        {
            Console.Write("Full Name: ");
            string name = Console.ReadLine();

            string role = "";
            while (true)
            {
                Console.Write("Role (Owner / Staff): ");
                role = Console.ReadLine().Trim();

                if (!string.IsNullOrWhiteSpace(role) &&
                    (role.Equals("Owner", StringComparison.OrdinalIgnoreCase) || role.Equals("Staff", StringComparison.OrdinalIgnoreCase)))
                {
                    // Normalize formatting so it's always "Owner" or "Staff"
                    role = char.ToUpper(role[0]) + role.Substring(1).ToLower();
                    break;
                }

                Console.WriteLine("Role must be 'Owner' or 'Staff'.");
            }


            Console.Write("Username: ");
            string username = Console.ReadLine();

            string password;
            string blank;
            do
            {
                Console.Write("Password: ");
                 password = Console.ReadLine();
                Console.Write("Re enter password: ");
                 blank = Console.ReadLine();
                if(password != blank)
                {
                    Console.WriteLine("password must match...\n enter again");
                }
            } while (password != blank);
            
            Console.Write("Contact Info (Email / Phone): ");
            string contact = Console.ReadLine();

            return new Employee
            {
                nameEmployee = name,
                role = role,
                username = username,
                password = password,
                contactInfo = contact,
                sales = 0
            };
        }

    }
}
