using System;
using SMARTPOS.functions;

namespace SMARTPOS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "SMART POS SYSTEM";
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            POSSYSTEM pos = new POSSYSTEM();
            pos.Start();

            Console.WriteLine("\nThank you for using SMARTPOS!");
        }
    }
}


