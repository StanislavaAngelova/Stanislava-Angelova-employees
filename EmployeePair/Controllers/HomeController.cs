using EmployeePair.Services;
using EmployeePair.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EmployeePair.Controllers;

public class HomeController : Controller
{
    private readonly ICsvLoader _loader;
    private readonly IPairWorkCalculator _calculator;
    public HomeController(ICsvLoader loader, IPairWorkCalculator calculator)
    {
        _loader = loader;
        _calculator = calculator;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ViewBag.Error = "Please select a CSV file.";
            return View("Index");
        }

        string tempPath = Path.GetTempFileName();

        using (var stream = new FileStream(tempPath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        try
        {
            // Load and validate CSV
            var records = _loader.Load(tempPath).ToList();

            // Calculate longest working pair
            var result = _calculator.GetLongestWorkingPair(records);

            var model = new ResultsViewModel
            {
                Emp1 = result.emp1,
                Emp2 = result.emp2,
                Projects = result.list
            };

            return View("Results", model);
        }
        catch (Exception ex)
        {
            // Show the error to the user
            ViewBag.Error = ex.Message;
            return View("Index");
        }
    }
}