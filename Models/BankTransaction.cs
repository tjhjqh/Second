using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salton.Models
{
    public class BankTransaction
    {
        public DateTime? Date { get; set; }
        public string Description { get; set; }

        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }

    }
}
