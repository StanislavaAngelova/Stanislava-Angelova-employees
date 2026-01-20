using EmployeePair.Models;

namespace EmployeePair.Services
{
    public interface ICsvLoader
    {
        IEnumerable<EmployeeProjectRecord> Load(string path);
    }
}
