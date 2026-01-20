using EmployeePair.Models;

namespace EmployeePair.Services
{
    public interface IPairWorkCalculator
    {
        (List<PairProjectWork> list, int emp1, int emp2) GetLongestWorkingPair(IEnumerable<EmployeeProjectRecord> records);
    }
}

