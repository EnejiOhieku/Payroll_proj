using System;
using System.IO;
using System.Security.AccessControl;
using System.Xml.Linq;

namespace Payroll_proj {
    public class Program {
        static void Main(string[] args) {
            Console.WriteLine("Welcome to the Payroll System!");

            List<Staff> staffs = new List<Staff>();
            FileReader fr = new FileReader();   
            int month = 0, year = 0;

            while (year == 0) {
                Console.Write("\nPlease enter the year: ");
                try {
                    year = int.Parse(Console.ReadLine());
                } catch (FormatException) {
                    Console.WriteLine("Invalid year, try again...");
                }
            }

            while (month == 0) {
                Console.Write("\nPlease enter the month: ");
                try {
                    int _month = int.Parse(Console.ReadLine());
                    if (_month < 1 || _month > 12) throw new FormatException();
                    month = _month;
                } catch (FormatException) {
                    Console.WriteLine("Invalid month, try again...");
                }
            }

            staffs = fr.ReadFile();

            for (int i = 0; i < staffs.Count; i++) {
                try {
                    Console.Write($"\nEnter Hours worked for {staffs[i].NameOfStaff}: ");
                    staffs[i].HoursWorked = int.Parse(Console.ReadLine());
                    staffs[i].CalculatePay();
                    Console.WriteLine(staffs[i]);
                } catch (Exception e) {
                    Console.WriteLine(e);
                    i--;
                }
            }

            PaySlip ps = new PaySlip(month, year);
            ps.GeneratePaySlip(staffs);
            ps.GenerateSummary(staffs);
            Console.WriteLine("Program Complete");
        }
    }

    public class Staff {
        private float hourlyRate;
        private int hWorked;

        public float TotalPay { get; protected set; }
        public float BasicPay { get; private set; }
        public string NameOfStaff { get; private set; }

        public int HoursWorked {
            get { return hWorked; }
            set {
                if (value > 0) {
                    hWorked = value;
                } else hWorked = 0;
            }
        }

        public Staff(string name, float rate) {
            NameOfStaff = name;
            hourlyRate = rate;
        }

        public virtual void CalculatePay() {
            Console.WriteLine("Calculating Pay...");
            BasicPay = hWorked*hourlyRate;
            TotalPay = BasicPay;
        }

        public override string ToString() {
            return $" Name: {NameOfStaff}\n hours worked: {hWorked}\n rate: {hourlyRate}\n Basic Pay: {BasicPay}\n Total Pay: {TotalPay}";
        }

    }

    public class Manager : Staff {
        private const float managerHourlyRate = 50;

        public int Allowance { get; private set; }

        public Manager(string name) : base (name, managerHourlyRate) { }

        public override void CalculatePay() {
            base.CalculatePay();

            Allowance = 0;
            if (HoursWorked > 160) {
                Allowance = 1000;
                TotalPay += Allowance;
            }
        }

        public override string ToString() {
            return base.ToString() + " (Manager)";
        }
    }

    public class Admin: Staff {
        private const float overtimeRate = 15.5f;
        private const float adminHourlyRate = 30;

        public float Overtime {  get; private set; }
        
        public Admin(string name) : base(name, adminHourlyRate) { }

        public override void CalculatePay() {
            base.CalculatePay();

            Overtime = 0;
            if (HoursWorked > 160) {
                Overtime = overtimeRate * (HoursWorked-160);
                TotalPay += Overtime;
            }
        }

        public override string ToString() {
            return base.ToString()+" (Admin)";
        }
    }

    public class FileReader {
        public List<Staff> ReadFile() {
            List<Staff> staffs = new List<Staff>();
            string filePath = "C:\\Users\\Admin\\Desktop\\Payroll_proj\\Payroll_proj\\staffs.txt";
            string[] seperator = new [] { ", " };

            if (File.Exists(filePath)) {
                using (StreamReader sr = new StreamReader(filePath)) {
                    while (!sr.EndOfStream) {
                        string line = sr.ReadLine();
                        string[] result = line.Split(seperator, StringSplitOptions.TrimEntries);
                        Staff staff;

                        if (result[1] == "Admin") staff = new Admin(result[0]);
                        else staff = new Manager(result[0]);

                        staffs.Add(staff);
                    }
                    sr.Close();
                }
            } else Console.WriteLine("Error: Couldn't find staffs.txt");

            return staffs;
        }
    }

    public class PaySlip {
        private int month;
        private int year;

        enum MonthsOfYear {
            JAN = 1, FEB, MAR, APR, MAY, JUN, JUL, AUG, SEP, OCT, NOV, DEC
        }

        public PaySlip(int payMonth, int payYear) {
            month = payMonth;
            year = payYear;
        }

        public void GeneratePaySlip(List<Staff> staffs) {
            string filePath = "C:\\Users\\Admin\\Desktop\\Payroll_proj\\Payroll_proj\\payslips\\";

            foreach (Staff staff in staffs) {
                string path = $"{filePath}{staff.NameOfStaff}.txt";
                string spacer = new string('=', 20);

                using (StreamWriter sw = new StreamWriter(path)) {
                    string monthStr = ((MonthsOfYear)month).ToString();

                    sw.WriteLine($"PAYSLIP FOR {monthStr} {year}");
                    sw.WriteLine(spacer);
                    sw.WriteLine($"Name of Staff: {staff.NameOfStaff}");
                    sw.WriteLine($"Hours Worked: {staff.HoursWorked}");
                    sw.WriteLine();
                    sw.WriteLine($"Basic Pay: ${staff.BasicPay:F2}");
                    if (staff is Admin) {
                        Admin admin = (Admin)staff;
                        sw.WriteLine($"Overtime: ${admin.Overtime:F2}");
                    } else {
                        Manager manager = (Manager)staff;
                        sw.WriteLine($"Allowance: ${manager.Allowance:F2}");
                    }
                    sw.WriteLine();
                    sw.WriteLine(spacer);
                    sw.WriteLine($"Total Pay: ${staff.TotalPay:F2}");
                    sw.WriteLine(spacer);
                    sw.Close();
                }
            }
        }

        public void GenerateSummary(List<Staff> staffs) {
            string filePath = "C:\\Users\\Admin\\Desktop\\Payroll_proj\\Payroll_proj\\summary.txt";
            var result =
                from staff in staffs
                where staff.HoursWorked < 10
                orderby staff.NameOfStaff ascending
                select staff;

            using (StreamWriter sw = new StreamWriter(filePath)) {
                sw.WriteLine("Staff with less than 10 working hours");
                foreach (Staff staff in result) {
                    sw.WriteLine($"Name of Staff: {staff.NameOfStaff}, Hours Worked: {staff.HoursWorked}");
                }
            }
        }

        public override string ToString() {
            return $"Payslip at {(MonthsOfYear)month} {year}";
        }
    }
}