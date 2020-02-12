using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CSProject
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Staff> myStaff = new List<Staff>();
            FileReader fr = new FileReader();
            int month = 0, year = 0;
            while (year < 1970)
            {
                Console.Write("\nPlease enter the year: ");
                try
                {
                    year = Convert.ToInt32(Console.ReadLine());
                }
                catch (FormatException e)
                {
                    Console.WriteLine("\nInvalid year entered. Please try again. {0}", e.Message);
                }
            }
            while (month < 1)
            {
                Console.Write("\nPlease enter the month (1-12): ");
                try
                {
                    month = Convert.ToInt32(Console.ReadLine());
                    if (month < 1 || month > 12)
                    {
                        month = 0;
                        throw new InvalidDataException("Acceptable value range exceeded.");
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine("\nInvalid month value entered. Please enter the numeric value. {0}", e.Message);
                }
                catch (InvalidDataException e)
                {
                    Console.WriteLine("\nInvalid month value entered. Please enter a value between 1 and 12, inclusive. {0}", e.Message);
                }
            }

            myStaff = fr.ReadFile();

            for (int i = 0; i < myStaff.Count; i++)
            {
                try
                {
                    Console.Write("Enter hours worked for {0}: ", myStaff[i].NameOfStaff);
                    myStaff[i].HoursWorked = Convert.ToInt32(Console.ReadLine());
                    myStaff[i].CalculatePay();
                    Console.WriteLine(myStaff[i].ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    i--;
                }
            }

            Payslip ps = new Payslip(month, year);
            ps.GeneratePaySlip(myStaff);
            ps.GenerateSummary(myStaff);
            Console.Read();
        }
    }

    class Staff
    {
        private float hourlyRate;
        private int hWorked;

        public float TotalPay { get; protected set; }
        public float BasicPay { get; private set; }
        public string NameOfStaff { get; private set; }
        public int HoursWorked
        {
            get
            {
                return hWorked;
            }
            set
            {
                if (value < 0)
                    hWorked = 0;
                else
                    hWorked = value;
            }
        }

        public Staff(string pName, float pRate)
        {
            NameOfStaff = pName;
            hourlyRate = pRate;
        }
        public virtual void CalculatePay()
        {
            Console.WriteLine("Calculating pay...");
            BasicPay = TotalPay = hWorked * hourlyRate;
        }
        public override string ToString()
        {
            return "Name: "+NameOfStaff+"\nRate: "+hourlyRate+"\nHours: "+hWorked+"\nTotal: "+TotalPay;
        }
    }
    class Manager : Staff
    {
        private const float managerHourlyRate = 50f;
        public int Allowance { get; private set; }
        public Manager(string pName) : base(pName,managerHourlyRate) { }
        public override void CalculatePay()
        {
            base.CalculatePay();
            Allowance = 1000;
            if (HoursWorked>160)
                TotalPay = TotalPay + Allowance;
        }
        public override string ToString()
        {
            return base.ToString()+"\nAllowance: "+Allowance;
        }
    }
    class Admin : Staff
    {
        private const float adminHourlyRate = 15.5f;
        private const float overtimeRate = 30f;

        public float Overtime { get; private set; }
        public Admin(string pName) : base(pName, adminHourlyRate) { }
        public override void CalculatePay()
        {
            base.CalculatePay();
            if (HoursWorked > 160)
                Overtime = overtimeRate * (HoursWorked - 160);
        }
        public override string ToString()
        {
            return base.ToString() + "\nOvertime: " + Overtime;
        }

    }
    class FileReader
    {
        public List<Staff> ReadFile()
        {
            List<Staff> myStaff = new List<Staff>();
            string[] result = new string[2];
            string path = ".\\staff.txt";
            string[] separator = { "," };

            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    while (!sr.EndOfStream)
                    {
                        result = sr.ReadLine().Split(separator, 2, System.StringSplitOptions.None);
                        if (result[1].Equals("Manager", StringComparison.OrdinalIgnoreCase))
                            myStaff.Add(new Manager(result[0]));
                        else if (result[1].Equals("Admin", StringComparison.OrdinalIgnoreCase))
                            myStaff.Add(new Admin(result[0]));
                    }
                    sr.Close();
                }
            }
            else
            {
                Console.WriteLine("Path to file, {0} does not exist", path);
            }
            return myStaff;
        }
    }
    class Payslip
    {
        private int month;
        private int year;

        enum MonthsOfYear
        {
            JAN=1,FEB,MAR,APR,MAY,JUN,JUL,AUG,SEP,OCT,NOV,DEC
        }

        public Payslip(int payMonth, int payYear)
        {
            month = payMonth;
            year = payYear;
        }
        
        public void GeneratePaySlip(List<Staff> myStaff)
        {
            string path;
            foreach(Staff f in myStaff)
            {
                path = f.NameOfStaff + ".txt";
                StreamWriter sw = new StreamWriter(path,false);

                sw.WriteLine("PAYSLIP FOR {0} {1}", (MonthsOfYear)month, year);
                sw.WriteLine("========================");
                sw.WriteLine("Name of Staff: {0}", f.NameOfStaff);
                sw.WriteLine("Hours Worked: {0}", f.HoursWorked);
                sw.WriteLine();
                sw.WriteLine("Basic Pay: {0:C}", f.BasicPay);
                //string lineSeven = null;
                ////lineSeven = (f.GetType() == typeof(Manager) ? "Allowance: "+((Manager)f).Allowance : "Overtime Pay: " + ((Admin)f).Overtime);
                //sw.WriteLine(lineSeven==null?"":lineSeven);
                if (f.GetType() == typeof(Manager))
                    sw.WriteLine("Allowance: {0:C}", ((Manager)f).Allowance);
                else
                    sw.WriteLine("Allowance: {0:C}", ((Admin)f).Overtime);
                sw.WriteLine("========================");
                sw.WriteLine("========================");
                sw.WriteLine("Total Pay: {0:C}", f.TotalPay);
                sw.WriteLine("========================");
                sw.Close();
            }
        }
        public void GenerateSummary(List<Staff> myStaff)
        {
            var result = from staff in myStaff where staff.HoursWorked < 10 orderby staff.NameOfStaff ascending select new { staff.NameOfStaff, staff.HoursWorked };
            string path = "summary.txt";
            StreamWriter sw = new StreamWriter(path);
            sw.WriteLine("Staff with less than 10 working hours");
            sw.WriteLine("");
            foreach(var s in result)
            {
                sw.WriteLine("Name of Staff: {0}, Hours Worked: {1}", s.NameOfStaff, s.HoursWorked);
            }
            sw.Close();
        }
        public override string ToString()
        {
            return "This object creates payslip and summary files.";
        }
    }
}
