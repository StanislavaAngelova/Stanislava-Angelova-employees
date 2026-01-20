using EmployeePair.Models;
using System.Globalization;

namespace EmployeePair.Services;

public class CsvLoader: ICsvLoader
{
    private static readonly string[] DateFormats =
    {
        "yyyy-MM-dd", "dd-MM-yyyy", "MM/dd/yyyy", "dd/MM/yyyy",
        "yyyy/MM/dd", "dd.MM.yyyy", "M/d/yyyy", "d.M.yyyy"
    };

    public IEnumerable<EmployeeProjectRecord> Load(string path)
    {
        var lines = File.ReadAllLines(path)
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .ToList();

        var seenRows = new HashSet<string>();

        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            int lineNumber = i + 1;

            var parts = line.Split(',', StringSplitOptions.TrimEntries);

            // Skip header
            if (parts[0].Equals("EmpID", StringComparison.OrdinalIgnoreCase))
                continue;

            // Check for correct number of columns
            if (parts.Length != 4)
                throw new Exception($"Line {lineNumber}: CSV row must contain exactly 4 columns.");

            // Check for missing fields
            if (string.IsNullOrWhiteSpace(parts[0]))
                throw new Exception($"Line {lineNumber}: EmpID is missing.");

            if (string.IsNullOrWhiteSpace(parts[1]))
                throw new Exception($"Line {lineNumber}: ProjectID is missing.");

            if (string.IsNullOrWhiteSpace(parts[2]))
                throw new Exception($"Line {lineNumber}: DateFrom is missing.");

            if (string.IsNullOrWhiteSpace(parts[3]))
                throw new Exception($"Line {lineNumber}: DateTo is missing.");

            // Check for Numeric Ids
            if (!int.TryParse(parts[0], out int empId))
                throw new Exception($"Line {lineNumber}: EmpID must be a number.");

            if (!int.TryParse(parts[1], out int projectId))
                throw new Exception($"Line {lineNumber}: ProjectID must be a number.");

            // Check for NULL usage
            if (parts[2].Equals("NULL", StringComparison.OrdinalIgnoreCase))
                throw new Exception($"Line {lineNumber}: DateFrom cannot be NULL.");

            // Parse dates
            DateTime from = ParseDate(parts[2], lineNumber);

            DateTime to = parts[3].Equals("NULL", StringComparison.OrdinalIgnoreCase)
                ? DateTime.Today
                : ParseDate(parts[3], lineNumber);

            // Check for date range sanity
            if (from.Year < 1900 || from.Year > DateTime.Today.Year + 1)
                throw new Exception($"Line {lineNumber}: DateFrom '{from}' is outside allowed range.");

            if (to.Year < 1900 || to.Year > DateTime.Today.Year + 1)
                throw new Exception($"Line {lineNumber}: DateTo '{to}' is outside allowed range.");

            // Check for DateFrom <= DateTo
            if (from > to)
                throw new Exception(
                    $"Line {lineNumber}: Invalid date range: {from:yyyy-MM-dd} > {to:yyyy-MM-dd}");

            // Check for duplicate rows
            string rowKey = $"{empId}-{projectId}-{from:yyyy-MM-dd}-{to:yyyy-MM-dd}";
            if (!seenRows.Add(rowKey))
                throw new Exception($"Line {lineNumber}: Duplicate row detected.");

            // Construct record
            yield return new EmployeeProjectRecord
            {
                EmpId = empId,
                ProjectId = projectId,
                DateFrom = from,
                DateTo = to
            };
        }
    }


    private DateTime ParseDate(string input, int lineNumber)
    {
        if (TryParseFlexible(input, out var date, out _))
            return date;

        throw new Exception($"Line {lineNumber}: Unsupported date format '{input}'.");
    }

    public static bool TryParseFlexible(
        string input,
        out DateTime result,
        out bool usedFallback)
    {
        usedFallback = false;

        // Try exact formats
        foreach (var format in DateFormats)
        {
            if (DateTime.TryParseExact(
                    input,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out result))
            {
                return true;
            }
        }
        // Fallback
        try
        {
            result = DateTime.Parse(
                input,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces);

            usedFallback = true;
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }
}