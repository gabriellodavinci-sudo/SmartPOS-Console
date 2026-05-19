using Microsoft.Win32;
using SMARTPOS.file_opening;
using SMARTPOS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;

namespace SMARTPOS.functions
{
    interface Iprintreceipt
    {
        void PrintReceipt(Order order, Employee employee);
    }

    interface Istorable
    {
        void SaveData();
        void LoadData();
    }

    internal class POSSYSTEM : Register, Iprintreceipt, Istorable
    {
        private List<product> products;
        private List<Employee> employees;
        private ProductFileHandler productHandler;
        private EmployeeFileHandler employeeHandler;
        private Sales sales;
        private Employee currentEmployee;

        public POSSYSTEM()
        {
            productHandler = new ProductFileHandler("products.txt");
            employeeHandler = new EmployeeFileHandler("employees.txt");
            sales = new Sales();

            RefreshData();
        }

        private void RefreshData()
        {
            products = productHandler.LoadProducts();
            employees = employeeHandler.LoadEmployees();
        }

        private void RebindCurrentEmployee()
        {
            if (currentEmployee == null) return;
            var match = employees.FirstOrDefault(e =>
                string.Equals(e.nameEmployee, currentEmployee.nameEmployee, StringComparison.OrdinalIgnoreCase));
            if (match != null) currentEmployee = match;
        }

        private void PrintHeader(string title)
        {
            // Eye-catching cyan-teal gradient style header (centered & balanced)
            Console.ForegroundColor = ConsoleColor.Cyan;

            int boxWidth = 50; // fixed width of the box (constant, fits all titles nicely)
            title = $" {title.ToUpper()} ";

            // truncate if too long
            if (title.Length > boxWidth - 2)
                title = title.Substring(0, boxWidth - 5) + "...";

            int paddingLeft = (boxWidth - title.Length) / 2;
            int paddingRight = boxWidth - title.Length - paddingLeft;

            string top = $"╔{new string('═', boxWidth)}╗";
            string middle = $"║{new string(' ', paddingLeft)}{title}{new string(' ', paddingRight)}║";
            string bottom = $"╚{new string('═', boxWidth)}╝";

            // compute center alignment in console window
            int consoleWidth = Console.WindowWidth;
            int leftMargin = Math.Max((consoleWidth - (boxWidth + 2)) / 2, 0);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(new string(' ', leftMargin) + top);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(new string(' ', leftMargin) + middle);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(new string(' ', leftMargin) + bottom);

            Console.ResetColor();
            Console.WriteLine();
        }



        private int ArrowMenu(string title, string[] options)
        {
            int index = 0;
            ConsoleKey key;

            do
            {
                Console.Clear();
                PrintHeader(title);

                for (int i = 0; i < options.Length; i++)
                {
                    string opt = options[i];
                    int badgePos = opt.IndexOf('!');
                    bool isSelected = (i == index);

                    if (isSelected)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                       
                    }

                    if (badgePos >= 0)
                    {
                        string left = opt.Substring(0, badgePos).TrimEnd();
                        string badge = opt.Substring(badgePos).TrimStart();

                        Console.Write(isSelected ? $"> {left} " : $"  {left} ");

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.BackgroundColor = isSelected ? ConsoleColor.White : ConsoleColor.Black;
                        Console.WriteLine(badge);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine(isSelected ? $"> {opt}" : $"  {opt}");
                        if (isSelected) Console.ResetColor();
                    }
                }

                key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.UpArrow)
                    index = (index == 0) ? options.Length - 1 : index - 1;
                if (key == ConsoleKey.DownArrow)
                    index = (index == options.Length - 1) ? 0 : index + 1;

            } while (key != ConsoleKey.Enter);

            return index;
        }

        public void Start()
        {
            string[] startMenu = { "Login" };
            int choice = ArrowMenu("SMART POS", startMenu);

            if (choice == 1)
            {
                Console.Clear();
                PrintHeader("Register Employee");
                Employee newEmp = RegisterEmployee();
                employees.Add(newEmp);
                employeeHandler.SaveEmployee(employees);
                Console.WriteLine("Employee registered successfully!");
                Thread.Sleep(1000);
                return;
            }
            else
            {
                Console.Clear();
                currentEmployee = Login();
                if (currentEmployee == null)
                {
                    Console.WriteLine("Login failed. Exiting...");
                    return;
                }
            }

            MainMenu();
        }

        private Employee Login()
        {
            // Ensure all employees have a username (fallback for old employees)
            foreach (var e in employees)
            {
                if (string.IsNullOrWhiteSpace(e.username))
                    e.username = e.nameEmployee; // fallback
            }

            PrintHeader("LOGIN");

            Console.Write("Enter Username: ");
            string username = Console.ReadLine();
            Console.Write("Enter Password: ");
            string password = ReadPassword();  // Make sure ReadPassword() is protected in Register

            // Safe login check
            Employee found = employees.FirstOrDefault(e =>
                !string.IsNullOrWhiteSpace(e.username) &&
                e.username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                e.password == password
            );

            if (found != null)
            {
                Console.Clear();
                PrintHeader($"Welcome, {found.nameEmployee} ({found.role})");
                Console.WriteLine($"Username: {found.username}");
                Console.WriteLine($"Contact Info: {found.contactInfo}");
                Console.WriteLine($"Your total sales: P{found.sales:F2}");
                Thread.Sleep(2000);
                return found;
            }

            Console.WriteLine("Invalid username or password.");
            Thread.Sleep(1000);
            return null;
        }


        private void MainMenu()
        {
            while (true)
            {
                int lowStockCount = products.Count(p => p.stock < 10);
                string lowStockBadge = lowStockCount > 0 ? $" ! Low Stock: {lowStockCount}" : "";

                string[] options = currentEmployee.role.Trim().Equals("Owner", StringComparison.OrdinalIgnoreCase)

                    ? new string[]
                    {
        "Search Product",
        "Process Sale",
        "Check Stock" + lowStockBadge,
        "Add Product",
        "Edit Product Info",
        "View Employee Sales",
        "View Sales Report",
        "Register employee",// ✅ NEW
        "Logout / Exit"
    }
                    : new string[]
                    {
                        "Search Product",
                        "Process Sale",
                        "Check Stock" + lowStockBadge,
                        "Logout / Exit"
                    };

                int choice = ArrowMenu("POS SYSTEM MENU", options);

                if (currentEmployee.role.Trim().Equals("Owner", StringComparison.OrdinalIgnoreCase))
                {
                    switch (choice)
                    {
                        case 0: Console.Clear(); SearchProductRealtime(); break;
                        case 1: Console.Clear(); ProcessSale(); break;
                        case 2: Console.Clear(); CheckStock(); break;
                        case 3: Console.Clear(); AddProduct(); break;
                        case 4: Console.Clear(); EditProductInfo(); break;
                        case 5: Console.Clear(); ViewEmployeeSales(); break;
                        case 6: Console.Clear(); ViewSalesReport(); break;  // ✅ NEW
                        case 7: Console.Clear();
                            {
                                Console.Clear();
                                PrintHeader("Register Employee");
                                Employee newEmp = RegisterEmployee();
                                employees.Add(newEmp);
                                employeeHandler.SaveEmployee(employees);
                                Console.WriteLine("Employee registered successfully!");
                                Thread.Sleep(1000);
                                return;
                            }
                        case 8: Console.Clear(); SaveData(); return;

                    }

                }
                else
                {
                    switch (choice)
                    {
                        case 0: Console.Clear(); SearchProductRealtime(); break;
                        case 1: Console.Clear(); ProcessSale(); break;
                        case 2: Console.Clear(); CheckStock(); break;
                        case 3: Console.Clear(); SaveData(); return;
                    }
                }
            }
        }

        private void SearchProductRealtime()
        {
            if (products.Count == 0)
            {
                Console.WriteLine("No products available.");
                Thread.Sleep(1000);
                return;
            }

            string query = "";
            int index = 0;
            ConsoleKey key;

            while (true)
            {
                var filtered = products
                    .Where(p => p.id.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                p.name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                if (filtered.Count == 0) index = 0;
                else index = Math.Clamp(index, 0, filtered.Count - 1);

                Console.Clear();
                PrintHeader("SEARCH PRODUCTS");
                Console.WriteLine("\nTYPE DIRECTLY TO SEARCH");
                Console.WriteLine("ESC = Cancel\n");
                Console.WriteLine($"Query: {query}");
                Console.WriteLine(new string('-', 40));

                int toShow = Math.Min(filtered.Count, 25);
                for (int i = 0; i < toShow; i++)
                {
                    var p = filtered[i];
                    bool sel = (i == index);

                    if (sel) { Console.BackgroundColor = ConsoleColor.White; Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("> "); }
                    else Console.Write("  ");

                    Console.Write($"{p.id} | {p.name} | P{p.price:F2} | ");
                    if (p.stock <= 0) { Console.BackgroundColor = sel ? ConsoleColor.White : ConsoleColor.DarkRed; Console.ForegroundColor = sel ? ConsoleColor.Black : ConsoleColor.White; Console.Write("OUT"); }
                    else if (p.stock < 5) { Console.ForegroundColor = ConsoleColor.Red; Console.Write($"{p.stock}"); }
                    else if (p.stock < 10) { Console.ForegroundColor = ConsoleColor.Yellow; Console.Write($"{p.stock}"); }
                    else { Console.ResetColor(); Console.Write($"{p.stock}"); }
                    Console.ResetColor();
                    Console.WriteLine();
                }

                

                var keyInfo = Console.ReadKey(true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Escape) return;
                else if (key == ConsoleKey.Backspace && query.Length > 0) query = query.Substring(0, query.Length - 1);
                else if (key == ConsoleKey.Enter && filtered.Count > 0) ShowProductDetails(filtered[index]);
                else if (key == ConsoleKey.UpArrow && filtered.Count > 0) index = (index == 0) ? filtered.Count - 1 : index - 1;
                else if (key == ConsoleKey.DownArrow && filtered.Count > 0) index = (index == filtered.Count - 1) ? 0 : index + 1;
                else if (!char.IsControl(keyInfo.KeyChar)) query += keyInfo.KeyChar;
            }
        }

        private void ShowProductDetails(product p)
        {
            Console.Clear();
            PrintHeader("PRODUCT DETAILS");
            Console.WriteLine($"ID: {p.id}");
            Console.WriteLine($"Name: {p.name}");
            Console.WriteLine($"Price: P{p.price:F2}");
            Console.WriteLine($"Stock: {p.stock}");

            if (p.stock < 10 && p.stock > 0) { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine($"⚠ Low stock: {p.stock} left"); Console.ResetColor(); }
            else if (p.stock <= 0) { Console.BackgroundColor = ConsoleColor.DarkRed; Console.ForegroundColor = ConsoleColor.White; Console.WriteLine("OUT OF STOCK"); Console.ResetColor(); }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
        }

        private void CheckStock()
        {
            Console.Clear();
            PrintHeader("CURRENT STOCK");

            foreach (var p in products)
            {
                // Format: align name to 20 chars wide for consistency
                Console.Write($"{p.id,-6} - {p.name,-20} | Stock: ");

                if (p.stock <= 0)
                {
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{p.stock}");
                    Console.ResetColor();
                }
                else if (p.stock < 5)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"{p.stock}");
                    Console.ResetColor();
                }
                else if (p.stock < 10)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"{p.stock}");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write($"{p.stock}");
                }

                Console.WriteLine();
            }


            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
        }

        private void AddProduct()
        {
            Console.Clear();
            PrintHeader("ADD PRODUCT");
            Console.WriteLine("Press ESC anytime to cancel.\n");

            string id = "";
            string name = "";
            double price = 0;
            int stockInt = 0;
            ConsoleKey key;

            // Product ID input (ESC to cancel)
            Console.Write("Product ID: ");
            while (true)
            {
                var input = ReadLineOrEscape();
                if (input == null) return; // ESC pressed
                id = input.Trim();
                if (!string.IsNullOrEmpty(id)) break;
                Console.Write("Product ID cannot be empty. Try again: ");
            }

            // Product Name input
            Console.Write("Product Name: ");
            var nameInput = ReadLineOrEscape();
            if (nameInput == null) return;
            name = nameInput.Trim();

            // Price input
            while (true)
            {
                Console.Write("Price: ");
                var input = ReadLineOrEscape();
                if (input == null) return;
                if (double.TryParse(input, out price) && price >= 0) break;
                Console.WriteLine("Enter a valid price (numeric).");
            }

            // Stock input
            while (true)
            {
                Console.Write("Stock (integer): ");
                var input = ReadLineOrEscape();
                if (input == null) return;
                if (int.TryParse(input, out stockInt) && stockInt >= 0) break;
                Console.WriteLine("Enter a valid non-negative integer for stock.");
            }

            // Save product
            products.Add(new product { id = id, name = name, price = price, stock = stockInt });
            productHandler.SaveProducts(products);

            Console.WriteLine($"\nProduct '{name}' added successfully!");
            Thread.Sleep(1000);
        }


        private void EditProductInfo()
        {
            product prod = ProductSelector("SELECT PRODUCT TO EDIT");
            if (prod == null) return;

            Console.Clear();
            PrintHeader($"EDIT {prod.name}");

            Console.Write("New Name (leave blank to keep): ");
            string newName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newName)) prod.name = newName;

            Console.Write("New Price (leave blank to keep): ");
            string priceInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(priceInput) && double.TryParse(priceInput, out double newPrice)) prod.price = newPrice;

            Console.Write("New Stock (leave blank to keep): ");
            string stockInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(stockInput) && int.TryParse(stockInput, out int newStock)) prod.stock = newStock;

            productHandler.SaveProducts(products);
            Console.WriteLine("Product updated successfully!");
            Thread.Sleep(1000);
        }

        private void ViewEmployeeSales()
        {
            if (employees.Count == 0)
            {
                Console.WriteLine("No employees available.");
                Thread.Sleep(1000);
                return;
            }

            int index = 0;
            ConsoleKey key;

            while (true)
            {
                Console.Clear();
                PrintHeader("EMPLOYEE SALES REPORT");

                for (int i = 0; i < employees.Count; i++)
                {
                    var e = employees[i];
                    bool sel = (i == index);
                    if (sel)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write("> ");
                    }
                    else
                    {
                        Console.Write("  ");
                    }

                    Console.WriteLine($"{e.nameEmployee} ({e.role}) - Total Sales: P{e.sales:F2}");
                    Console.ResetColor();
                }

                Console.WriteLine("\nEnter = View details, Esc = Back");
                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow) index = (index == 0) ? employees.Count - 1 : index - 1;
                else if (key == ConsoleKey.DownArrow) index = (index == employees.Count - 1) ? 0 : index + 1;
                else if (key == ConsoleKey.Enter) ShowEmployeeDetails(employees[index]);
                else if (key == ConsoleKey.Escape) break;
            }
        }

        private void ShowEmployeeDetails(Employee e)
        {
            Console.Clear();
            PrintHeader($"EMPLOYEE: {e.nameEmployee}");

            Console.WriteLine($"Name: {e.nameEmployee}");
            Console.WriteLine($"Role: {e.role}");
            Console.WriteLine($"Username: {e.username}");
            Console.WriteLine($"Contact Info: {e.contactInfo}");
            Console.WriteLine($"Total Sales: P{e.sales:F2}");

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
        }




        private int ReadIntInput(string prompt, int min = 1, int max = int.MaxValue)
        {
            int val;
            while (true)
            {
                Console.Write(prompt);
                string s = Console.ReadLine();
                if (int.TryParse(s, out val) && val >= min && val <= max)
                    return val;
                Console.WriteLine($"Please enter a number between {min} and {max}.");
            }
        }

        private void ProcessSale()
        {
            Order order = new Order { Items = new List<OrderItem>() };
            Console.Clear();
            PrintHeader("NEW SALE");

            while (true)
            {
                string query = "";
                int index = 0;
                product selected = null;

                while (true)
                {
                    var filtered = products
                        .Where(p => p.id.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                    p.name.Contains(query, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (filtered.Count == 0) index = 0;
                    else index = Math.Clamp(index, 0, filtered.Count - 1);

                    Console.Clear();

                    PrintHeader("ADD ITEM TO ORDER");
                    Console.WriteLine("\nPRESS TAB TO OPEN CART");
                    Console.WriteLine($"Query: {query}");
                    Console.WriteLine(new string('-', 40));

                    int toShow = Math.Min(filtered.Count, 25);
                    for (int i = 0; i < toShow; i++)
                    {
                        var p = filtered[i];
                        bool sel = (i == index);
                        if (sel) { Console.BackgroundColor = ConsoleColor.White; Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("> "); }
                        else Console.Write("  ");

                        Console.Write($"{p.id} | {p.name} | P{p.price:F2} | ");
                        if (p.stock <= 0) { Console.BackgroundColor = sel ? ConsoleColor.White : ConsoleColor.DarkRed; Console.ForegroundColor = sel ? ConsoleColor.Black : ConsoleColor.White; Console.Write("OUT"); }
                        else if (p.stock < 5) { Console.ForegroundColor = ConsoleColor.Red; Console.Write($"{p.stock}"); }
                        else if (p.stock < 10) { Console.ForegroundColor = ConsoleColor.Yellow; Console.Write($"{p.stock}"); }
                        else { Console.ResetColor(); Console.Write($"{p.stock}"); }
                        Console.ResetColor();
                        Console.WriteLine();
                    }

                    var keyInfo = Console.ReadKey(true);
                    var key = keyInfo.Key;

                    if (key == ConsoleKey.Escape)
                    {
                        SaveAllData();
                        CancelSale(order);

                        RefreshData();
                        RebindCurrentEmployee();

                        return;
                    }
                    else if (key == ConsoleKey.Backspace && query.Length > 0) query = query.Substring(0, query.Length - 1);
                    else if (key == ConsoleKey.Enter && filtered.Count > 0) { selected = filtered[index]; break; }
                    else if (key == ConsoleKey.Tab) { ManageCart(order); continue; }
                    else if (key == ConsoleKey.UpArrow && filtered.Count > 0) index = (index == 0) ? filtered.Count - 1 : index - 1;
                    else if (key == ConsoleKey.DownArrow && filtered.Count > 0) index = (index == filtered.Count - 1) ? 0 : index + 1;
                    else if (!char.IsControl(keyInfo.KeyChar)) query += keyInfo.KeyChar;
                }

                if (selected == null || selected.stock <= 0)
                {
                    Console.WriteLine($"\n'{selected?.name}' is out of stock. Press any key...");
                    Console.ReadKey(true);
                    continue;
                }

                int qty = ReadIntInput($"Enter quantity (1-{(int)selected.stock}): ", 1, (int)selected.stock);

                var existingItem = order.Items.FirstOrDefault(it => it.Product.id == selected.id);
                if (existingItem != null)
                {
                    existingItem.Quantity += qty;
                    existingItem.Subtotal = existingItem.Quantity * existingItem.Product.price;
                }
                else
                {
                    order.Items.Add(new OrderItem { Product = selected, Quantity = qty, Subtotal = qty * selected.price });
                }

                selected.stock -= qty;
                productHandler.SaveProducts(products);

                Console.WriteLine($"\nAdded {qty} x {selected.name} to order.");
                Console.Write("\nAdd another? Y/N: ");
                var ch = Console.ReadKey(true).Key;

                if (ch == ConsoleKey.Y)
                    continue;             // add another item
                else if (ch == ConsoleKey.Tab)
                {
                    ManageCart(order);    // open cart
                    continue;
                }
                else if (ch == ConsoleKey.N)
                {
                    FinalizeOrder(order); // finish adding items
                    break;
                }
                else
                {
                    // if any other key pressed, just ask again
                    continue;
                }

            }
        }

        private void ManageCart(Order order)
        {
            if (order.Items.Count == 0)
            {
                Console.WriteLine("\nCart is empty. Press any key...");
                Console.ReadKey(true);
                return;
            }

            int index = 0;
            ConsoleKey key;

            while (true)
            {
                Console.Clear();
                PrintHeader("CURRENT CART");

                for (int i = 0; i < order.Items.Count; i++)
                {
                    var item = order.Items[i];
                    bool sel = (i == index);
                    if (sel) { Console.BackgroundColor = ConsoleColor.White; Console.ForegroundColor = ConsoleColor.Black; Console.Write("> "); }
                    else Console.Write("  ");
                    Console.WriteLine($"{item.Product.name} x {item.Quantity} = P{item.Subtotal:F2}");
                    Console.ResetColor();
                }

                Console.WriteLine("\nDel = remove item, Esc = return, P = finalize order");
                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow) index = (index == 0) ? order.Items.Count - 1 : index - 1;
                else if (key == ConsoleKey.DownArrow) index = (index == order.Items.Count - 1) ? 0 : index + 1;
                else if (key == ConsoleKey.Delete)
                {
                    var item = order.Items[index];

                    // Add the quantity back to the product stock
                    var productInList = products.FirstOrDefault(p => p.id == item.Product.id);
                    if (productInList != null)
                    {
                        productInList.stock += item.Quantity;
                    }

                    // Remove item from the order
                    order.Items.RemoveAt(index);

                    // Save updated product list
                    productHandler.SaveProducts(products);

                    // Adjust index safely
                    if (order.Items.Count == 0) return;
                    if (index >= order.Items.Count) index = order.Items.Count - 1;
                }
                else if (key == ConsoleKey.Escape)
                {

                    RebindCurrentEmployee();
                    return;
                }
                else if (key == ConsoleKey.P)
                {
                    
                    FinalizeOrder(order);
                    return;
                }
            }
        }

        private void FinalizeOrder(Order order)
        {
            if (order.Items.Count == 0) return;

            order.Total = sales.CalculateTotal(order);

            Console.Clear();
            PrintHeader("ORDER SUMMARY");

            foreach (var it in order.Items)
                Console.WriteLine($"{it.Product.name} x {it.Quantity} = P{it.Subtotal:F2}");

            Console.WriteLine(new string('-', 25));
            Console.WriteLine($"TOTAL: P{order.Total:F2}\n");

            double payment;

            while (true)
            {
                Console.Write("Enter payment (Press ESC to cancel): ");

                string input = "";
                ConsoleKeyInfo keyInfo;

                // Read input key by key
                while (true)
                {
                    keyInfo = Console.ReadKey(true);

                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        // Cancel the sale immediately
                        CancelSale(order);
                        RefreshData();
                        RebindCurrentEmployee();
                        return;
                    }
                    else if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        break; // done entering payment
                    }
                    else if (char.IsDigit(keyInfo.KeyChar) || keyInfo.KeyChar == '.')
                    {
                        input += keyInfo.KeyChar;
                        Console.Write(keyInfo.KeyChar); // show typed char
                    }
                    else if (keyInfo.Key == ConsoleKey.Backspace && input.Length > 0)
                    {
                        input = input.Substring(0, input.Length - 1);
                        Console.Write("\b \b");
                    }
                }

                if (double.TryParse(input, out payment) && payment >= order.Total) break;
                Console.WriteLine("Invalid or insufficient payment.");
            }

            double change = payment - order.Total;

            PrintReceipt(order, currentEmployee);
            SaveReceiptToFile(order, currentEmployee);

            Console.WriteLine($"\nChange: P{change:F2}");

            currentEmployee.sales += order.Total;
            productHandler.SaveProducts(products);
            employeeHandler.SaveEmployee(employees);

            RefreshData();
            RebindCurrentEmployee();
            Console.WriteLine("Sale recorded. Press any key to continue...");
            Console.ReadKey(true);
        }


        private void CancelSale(Order order)
        {
            foreach (var it in order.Items)
                it.Product.stock += it.Quantity;

            productHandler.SaveProducts(products);
            Console.WriteLine("\nSale cancelled. Returning...");
            Thread.Sleep(500);
        }

        public void PrintReceipt(Order order, Employee employee)
        {
            Console.Clear();
            PrintHeader("RECEIPT");
            Console.WriteLine($"Cashier: {employee.nameEmployee}");
            Console.WriteLine($"Date: {DateTime.Now}\n");

            foreach (var item in order.Items)
                Console.WriteLine($"{item.Product.name} x {item.Quantity} = P{item.Subtotal:F2}");

            Console.WriteLine(new string('-', 25));
            Console.WriteLine($"TOTAL: {order.Total:F2}");
            Console.WriteLine(new string('=', 25));
            Console.WriteLine();
        }

        public void SaveData()
        {
            productHandler.SaveProducts(products);
            employeeHandler.SaveEmployee(employees);
            Console.WriteLine("Data saved successfully.");
            Thread.Sleep(500);
        }

        private void SaveAllData()
        {
            productHandler.SaveProducts(products);
            employeeHandler.SaveEmployee(employees);
        }

        public void LoadData()
        {
            RefreshData();
            Console.WriteLine("Data loaded successfully.");
            Thread.Sleep(500);
        }

        private product ProductSelector(string title)
        {
            if (products.Count == 0) return null;

            int index = 0;
            ConsoleKey key;

            while (true)
            {
                Console.Clear();
                PrintHeader(title);

                for (int i = 0; i < products.Count; i++)
                {
                    var p = products[i];
                    bool sel = (i == index);
                    if (sel) { Console.BackgroundColor = ConsoleColor.White; Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("> "); }
                    else Console.Write("  ");

                    Console.Write($"{p.id} | {p.name} | P{p.price:F2} | ");
                    if (p.stock <= 0) { Console.BackgroundColor = sel ? ConsoleColor.White : ConsoleColor.DarkRed; Console.ForegroundColor = sel ? ConsoleColor.Black : ConsoleColor.White; Console.Write("OUT"); }
                    else if (p.stock < 5) { Console.ForegroundColor = ConsoleColor.Red; Console.Write($"{p.stock}"); }
                    else if (p.stock < 10) { Console.ForegroundColor = ConsoleColor.Yellow; Console.Write($"{p.stock}"); }
                    else { Console.ResetColor(); Console.Write($"{p.stock}"); }

                    Console.ResetColor();
                    Console.WriteLine();
                }

                Console.WriteLine("\nDel = remove product");
                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow) index = (index == 0) ? products.Count - 1 : index - 1;
                else if (key == ConsoleKey.DownArrow) index = (index == products.Count - 1) ? 0 : index + 1;
                else if (key == ConsoleKey.Enter) return products[index];
                else if (key == ConsoleKey.Escape) return null;
                else if (key == ConsoleKey.Delete)
                {
                    var toRemove = products[index];
                    Console.WriteLine($"\nRemove '{toRemove.name}'? (Y/N)");
                    if (Console.ReadKey(true).Key == ConsoleKey.Y)
                    {
                        products.RemoveAt(index);
                        productHandler.SaveProducts(products);
                        if (index >= products.Count) index = products.Count - 1;
                        if (products.Count == 0) return null;
                    }
                }
            }
        }
        private string ReadLineOrEscape()
        {
            var sb = new StringBuilder();
            ConsoleKeyInfo key;

            while (true)
            {
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return sb.ToString();
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("\nCancelled. Returning...");
                    Thread.Sleep(500);
                    return null; // ESC = cancel
                }
                else if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Length -= 1;
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                    Console.Write(key.KeyChar);
                }
            }
        }
        private void SaveReceiptToFile(Order order, Employee employee)
        {
            string filePath = "receipts.txt";
            using (StreamWriter sw = new StreamWriter(filePath, append: true))
            {
                sw.WriteLine("========================================");
                sw.WriteLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sw.WriteLine($"Cashier: {employee.nameEmployee} ({employee.role})");
                sw.WriteLine("----------------------------------------");
                foreach (var item in order.Items)
                {
                    sw.WriteLine($"{item.Product.name} x {item.Quantity} = P{item.Subtotal:F2}");
                }
                sw.WriteLine("----------------------------------------");
                sw.WriteLine($"TOTAL: P{order.Total:F2}");
                sw.WriteLine("========================================\n");
            }
        }

        private void ViewSalesReport()
        {
            string filePath = "receipts.txt";
            if (!File.Exists(filePath))
            {
                Console.WriteLine("No receipts found yet.");
                Thread.Sleep(1000);
                return;
            }

            string[] lines = File.ReadAllLines(filePath);
            var sales = new List<(DateTime date, double total)>();

            // Parse totals from receipts
            DateTime currentDate = DateTime.MinValue;
            foreach (var line in lines)
            {
                if (line.StartsWith("Date:"))
                {
                    if (DateTime.TryParse(line.Substring(5).Trim(), out DateTime parsedDate))
                        currentDate = parsedDate;
                }
                else if (line.StartsWith("TOTAL:"))
                {
                    string amountStr = line.Substring(6).Trim().Replace("P", "");
                    if (double.TryParse(amountStr, out double total))
                        sales.Add((currentDate, total));
                }
            }

            // If no data
            if (sales.Count == 0)
            {
                Console.WriteLine("No sales data available.");
                Thread.Sleep(1000);
                return;
            }

            string[] options = { "Weekly", "Monthly", "Yearly", "Back" };
            int choice = ArrowMenu("SALES REPORT", options);

            DateTime now = DateTime.Now;
            DateTime startDate = DateTime.MinValue;
            string reportTitle = "";

            switch (choice)
            {
                case 0:
                    startDate = now.AddDays(-7);
                    reportTitle = "WEEKLY SALES REPORT";
                    break;
                case 1:
                    startDate = now.AddMonths(-1);
                    reportTitle = "MONTHLY SALES REPORT";
                    break;
                case 2:
                    startDate = now.AddYears(-1);
                    reportTitle = "YEARLY SALES REPORT";
                    break;
                case 3:
                    return;
            }

            double totalSales = sales.Where(s => s.date >= startDate).Sum(s => s.total);

            Console.Clear();
            PrintHeader(reportTitle);
            Console.WriteLine($"Period starting from: {startDate:yyyy-MM-dd}");
            Console.WriteLine($"Total Sales: P{totalSales:F2}");
            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
        }

    }
}
