using EmployeePair.Models;

namespace EmployeePair.Services;

public class PairWorkCalculator : IPairWorkCalculator
{
    // Returns the pair of employees who worked together the longest and a breakdown of their shared projects.
    public (List<PairProjectWork> list, int emp1, int emp2)
      GetLongestWorkingPair(IEnumerable<EmployeeProjectRecord> records)
    {
        // Clean and normalize data: merge overlapping intervals per employee/project
        var mergedRecords = MergeIntervals(records);

        // Group by project for pairwise comparison
        var byProject = mergedRecords.GroupBy(r => r.ProjectId).ToList();

        var allPairs = new List<PairProjectWork>();
        var totals = new Dictionary<(int, int), int>();

        foreach (var group in byProject)
        {
            // Sort by start date
            var recs = group.OrderBy(r => r.DateFrom).ToList();

            for (int i = 0; i < recs.Count; i++)
            {
                for (int j = i + 1; j < recs.Count; j++)
                {
                    var a = recs[i];
                    var b = recs[j];

                    // If the next employee starts after date ends, no more overlaps possible
                    if (b.DateFrom > a.DateTo)
                        break;

                    // Compute overlap interval
                    var start = a.DateFrom >= b.DateFrom ? a.DateFrom : b.DateFrom;
                    var end = a.DateTo <= b.DateTo ? a.DateTo : b.DateTo;

                    if (end < start)
                        continue;

                    int days = (end - start).Days + 1;
                    if (days <= 0)
                        continue;

                    // Normalize employee order (smallest Id first)
                    int e1 = Math.Min(a.EmpId, b.EmpId);
                    int e2 = Math.Max(a.EmpId, b.EmpId);

                    // Store detailed breakdown
                    allPairs.Add(new PairProjectWork
                    {
                        EmpId1 = e1,
                        EmpId2 = e2,
                        ProjectId = group.Key,
                        DaysWorked = days
                    });

                    // Accumulate total days for this pair
                    var key = (e1, e2);
                    if (!totals.ContainsKey(key))
                        totals[key] = 0;

                    totals[key] += days;
                }
            }
        }

        // No pairs found
        if (!totals.Any())
            return (new List<PairProjectWork>(), 0, 0);

        // Find the pair with the maximum total days
        var best = totals.OrderByDescending(t => t.Value).First().Key;

        // Return all project breakdowns for the winning pair
        var bestList = allPairs
            .Where(p => p.EmpId1 == best.Item1 && p.EmpId2 == best.Item2)
            .OrderBy(p => p.ProjectId)
            .ToList();

        return (bestList, best.Item1, best.Item2);
    }

    // Merges overlapping or adjacent date intervals for each employee/project.
    // Ensures clean, non-overlapping data before pair calculations.
    private List<EmployeeProjectRecord> MergeIntervals(IEnumerable<EmployeeProjectRecord> records)
    {
        return records
            .GroupBy(r => new { r.EmpId, r.ProjectId })
            .SelectMany(g =>
            {
                var sorted = g.OrderBy(r => r.DateFrom).ToList();
                var merged = new List<EmployeeProjectRecord>();

                var current = new EmployeeProjectRecord
                {
                    EmpId = sorted[0].EmpId,
                    ProjectId = sorted[0].ProjectId,
                    DateFrom = sorted[0].DateFrom,
                    DateTo = sorted[0].DateTo
                };

                for (int i = 1; i < sorted.Count; i++)
                {
                    var next = sorted[i];

                    // Overlapping or adjacent intervals → merge
                    if (next.DateFrom <= current.DateTo.AddDays(1))
                    {
                        if (next.DateTo > current.DateTo)
                            current.DateTo = next.DateTo;
                    }
                    else
                    {
                        merged.Add(current);
                        current = new EmployeeProjectRecord
                        {
                            EmpId = next.EmpId,
                            ProjectId = next.ProjectId,
                            DateFrom = next.DateFrom,
                            DateTo = next.DateTo
                        };
                    }
                }

                merged.Add(current);
                return merged;
            }).ToList();
    }
}