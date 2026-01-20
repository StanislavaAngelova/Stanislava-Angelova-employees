using EmployeePair.Models;

namespace EmployeePair.ViewModels
{
    public class ResultsViewModel
    {
        public int Emp1 { get; set; }
        public int Emp2 { get; set; }
        public List<PairProjectWork> Projects { get; set; }
    }
}
