using Salton.Bank;
using Salton.Inventory;
using Salton.PayRoll;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Salton
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void bank_UploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newMDIChild = new BankCreateUpload
            {
                MdiParent = this,
                BankType = Salton.Models.Bank.TDNY
            };
            newMDIChild.Show();
        }

        private void tool_OTR_CSV_StripMenuItem_Click(object sender, EventArgs e)
        {
            var newMDIChild = new BankCreateUpload
            {
                MdiParent = this,
                BankType = Salton.Models.Bank.OTR
            };
            newMDIChild.Show();

        }

        private void matchStripMenuItem_Click(object sender, EventArgs e)
        {
            var newMDIChild = new MatchEmployeeNumber
            {
                MdiParent = this,
            };
            newMDIChild.Show();

        }

        private void bankReconciliationStripMenuItem_Click(object sender, EventArgs e)
        {
            //double[] numbers = { -0.47, -0.35, -0.19, 0.23, 0.36, 0.47, 0.51, 0.59, 0.63, 0.79, 0.85,
            //    0.91, 0.99, 1.02, 1.17, 1.25, 1.39, 1.44, 1.59, 1.60, 1.79, 1.88, 1.99, 2.14, 2.31 };

            //double target = 24.16;

            //DisplaySubsetsThatSumTo(target, numbers);

            var newMDIChild = new BankReconciliation
            {
                MdiParent = this,
            };
            newMDIChild.Show();
            

        }

        private static void DisplaySubsetsThatSumTo(double Target, double[] numbers)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();

            bool[] wheel = new bool[numbers.Length];
            int resultsCount = 0;
            double? sum = 0;

            stopwatch.Start();

            do
            {
                sum = IncrementWheel(0, sum, numbers, wheel);
                //Use subtraction comparison due to double type imprecision
                if (sum.HasValue && Math.Abs(sum.Value - Target) < 0.000001F)
                {
                    //Found a subset. Display the result.
                    Console.WriteLine(string.Join(" + ", numbers.Where((n, idx) => wheel[idx])) + " = " + Target);
                    resultsCount++;
                }
            } while (sum != null);

            stopwatch.Stop();

            Console.WriteLine("--------------------------");
            Console.WriteLine($"Processed {numbers.Length} numbers in {stopwatch.ElapsedMilliseconds / 1000.0} seconds ({resultsCount} results). Press any key to exit.");
            Console.ReadKey();
        }

        private static double? IncrementWheel(int Position, double? Sum, double[] numbers, bool[] wheel)
        {
            if (Position == numbers.Length || !Sum.HasValue)
            {
                return null;
            }
            wheel[Position] = !wheel[Position];
            if (!wheel[Position])
            {
                Sum -= numbers[Position];
                Sum = IncrementWheel(Position + 1, Sum, numbers, wheel);
            }
            else
            {
                Sum += numbers[Position];
            }
            return Sum;
        }

        private void inventoryAdjustment_Click(object sender, EventArgs e)
        {
            var newMDIChild = new InventoryAdjustment
            {
                MdiParent = this,
            };
            newMDIChild.Show();

        }
    }
}
