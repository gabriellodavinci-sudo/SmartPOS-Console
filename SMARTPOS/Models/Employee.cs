namespace SMARTPOS.Models
{
    public class Employee
    {
        public string username { get; set; }        // new
        public string contactInfo { get; set; }     // new
        public string nameEmployee { get; set; }
        public string password { get; set; }
        public string mobileNO { get; set; }
        public double sales { get; set; }
        public string role { get; set; } // "Owner" or "Employee"

        public Employee() { }
    }
}
