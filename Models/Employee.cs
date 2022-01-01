using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salton.Models
{
    public class Employee
    {
        public string Number { get; set; }
        public List<string> Names { get; set; }
        public string Section { get; set; }
    }
    public class MatchedEmployee
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public string Section { get; set; }
    }
    
}
