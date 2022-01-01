using Newtonsoft.Json;
using Salton.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Salton.Helpers
{
    public static class PayRollHelpers
    {
        private static string PayRollPath = "C:\\Store\\Payroll";
        private static string EmployeeFile = "Employees.JSON";
        internal static void LoadEmployeeNumberFile(string[] fileNames)
        {
            var employeeData = LoadEmployeeNumberData();
            var list = new List<Employee>();
            foreach (var file in fileNames)
            {
                var lines = Utilities.ReadCSVLines(file);
                foreach (var line in lines.Where(p=>p.Length>=2))
                {
                    list.Add(new Employee { 
                        Number = line[0],
                        Section = line[1],
                        Names = new List<string> { line[2].Trim()}
                    });
                }
                employeeData = UpdateEmployeeData(employeeData,list);
            }
            SaveEmplyeeData(employeeData);
        }

        private static void SaveEmplyeeData(IEnumerable<Employee> employeeData)
        {
            employeeData = CleanEmployeeData(employeeData);
            var path = Path.Combine(PayRollPath, EmployeeFile);
            var json = JsonConvert.SerializeObject(employeeData);

            if (!Directory.Exists(PayRollPath))
            {
                Directory.CreateDirectory(PayRollPath);
            }
            File.WriteAllText(path, json);
        }

        internal static IEnumerable<MatchedEmployee> GetMatchEmployeeData(string[] fileNames)
        {
            var list = new List<MatchedEmployee>();
            var employeeData = LoadEmployeeNumberData();
            foreach (var file in fileNames)
            {
                var names = Utilities.ReadCSVLines(file).Select(p=>p[0].Trim());
                foreach (var name in names)
                {
                    var matchEmployee = employeeData.FirstOrDefault(p => p.Names.Any(q=>q.Equals(name,StringComparison.InvariantCultureIgnoreCase)));
                    if (matchEmployee != null)
                    {
                        list.Add(new MatchedEmployee
                        {
                            Name = name,
                            Number = matchEmployee.Number,
                            Section = matchEmployee.Section
                        });
                    }
                    else {
                        list.Add(new MatchedEmployee
                        {
                            Name = name,
                        });
                    }

                }
            }
            return list;

        }

        private static IEnumerable<Employee> CleanEmployeeData(IEnumerable<Employee> employeeData)
        {
            foreach (var employee in employeeData)
            {
                employee.Number = employee.Number.Trim();
                employee.Section = employee.Section.Trim();
                employee.Names = employee.Names.Select(p => p.Trim()).ToList();
            }
            return employeeData;
        }

        private static List<Employee> UpdateEmployeeData(List<Employee> employeeData, IEnumerable<Employee> list)
        {
            foreach (var newEmployee in list)
            {
                if (newEmployee.Names.Any())
                {
                    var name = newEmployee.Names.FirstOrDefault();
                    var matchEmployee = employeeData.FirstOrDefault(p => p.Number.Equals(newEmployee.Number, StringComparison.InvariantCultureIgnoreCase));
                    if (matchEmployee!=null)
                    {
                        if (!matchEmployee.Names.Any(p => p.Trim().Equals(name.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                        {
                            matchEmployee.Names.Add(name.Trim());
                        }

                    }
                    else {
                        employeeData.Add(newEmployee);
                    }
                }
            }
            return employeeData;
        }

        internal static void UpdateEmployeeData(List<MatchedEmployee> list)
        {
            var employeeData = LoadEmployeeNumberData();
            foreach (var employee in list)
            {
                var match = employeeData.FirstOrDefault(p=>p.Number.Equals(employee.Number,StringComparison.InvariantCultureIgnoreCase));
                if (match != null)
                {
                    match.Names.Add(employee.Name);
                }
            }
            SaveEmplyeeData(employeeData);

        }

        private static List<Employee> LoadEmployeeNumberData()
        {
            var list = new List<Employee>();
            try
            {
                var path = Path.Combine(PayRollPath, EmployeeFile);
                var json = File.ReadAllText(path);
                list = JsonConvert.DeserializeObject<List<Employee>>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list; 
        }
    }
}
