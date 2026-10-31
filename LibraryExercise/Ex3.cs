using System;
using System.Collections.Generic;
using System.Linq;

namespace Ex3;

public record Grade(string Course, int Score);

public class Gradebook
{
    private readonly Dictionary<string, List<Grade>> students = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> originalNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> studentOrder = new();
    private readonly HashSet<string> allowedCourses = new(StringComparer.OrdinalIgnoreCase)
    {
        ".NET", "DB", "Math"
    };
    private readonly SortedSet<string> sortedCourses = new(StringComparer.OrdinalIgnoreCase)
    {
        ".NET", "DB", "Math"
    };
    private readonly Stack<(string Name, Grade Grade)> undoStack = new();
    private readonly Queue<string> history = new();

    public string AddScore(string name, string course, int score)
    {
        if (!allowedCourses.Contains(course))
            return $"Unknown course: {course}";

        if (score < 0 || score > 100)
            return $"Invalid score: {score}";

        string canonicalCourse = sortedCourses.First(c =>
            string.Equals(c, course, StringComparison.OrdinalIgnoreCase));

        if (!students.TryGetValue(name, out List<Grade>? grades))
        {
            grades = new List<Grade>();
            students[name] = grades;
            originalNames[name] = name;
            studentOrder.Add(name);
        }

        string displayName = originalNames[name];
        Grade grade = new(canonicalCourse, score);
        grades.Add(grade);
        undoStack.Push((displayName, grade));
        RecordHistory($"add {displayName} {canonicalCourse} {score}");

        return $"Added: {displayName} {canonicalCourse} {score}";
    }

    public string Undo()
    {
        if (undoStack.Count == 0)
            return "Nothing to undo";

        (string name, Grade grade) = undoStack.Pop();
        students[name].RemoveAt(students[name].Count - 1);
        RecordHistory($"undo {name} {grade.Course} {grade.Score}");

        return $"Undone: {name} {grade.Course} {grade.Score}";
    }

    private void RecordHistory(string action)
    {
        if (history.Count == 5)
            history.Dequeue();

        history.Enqueue(action);
    }

    public void PrintTable()
    {
        Console.WriteLine("Name     Count  Average  Max  Last 3");

        foreach (string name in studentOrder)
        {
            List<Grade> grades = students[name];
            if (grades.Count == 0)
                continue;

            decimal average = grades.Average(g => (decimal)g.Score);
            int maximum = grades.Max(g => g.Score);
            int[] scores = grades.Select(g => g.Score).ToArray();
            int[] lastThree = scores.Length >= 3 ? scores[^3..] : scores[..];

            Console.WriteLine($"{originalNames[name],-8}{grades.Count,6}{average,9:F2}{maximum,5}  {string.Join(" ", lastThree)}");
        }
    }

    public void SearchStudent(string name)
    {
        if (!students.TryGetValue(name, out List<Grade>? grades) || grades.Count == 0)
        {
            Console.WriteLine($"{name}: not found");
            return;
        }

        IEnumerable<string> courseAverages = sortedCourses
            .Where(course => grades.Any(g => g.Course == course))
            .Select(course =>
                $"{course}={grades.Where(g => g.Course == course).Average(g => (decimal)g.Score):F2}");

        Console.WriteLine($"{originalNames[name]}: {string.Join(", ", courseAverages)}");
    }

    public void PrintTopThree()
    {
        var top = studentOrder
            .Where(name => students[name].Count > 0)
            .Select(name => new
            {
                Name = originalNames[name],
                Average = students[name].Average(g => (decimal)g.Score)
            })
            .OrderByDescending(s => s.Average)
            .ThenBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .Take(3);

        Console.WriteLine("Top 3:");
        foreach (var student in top)
            Console.WriteLine($"  {student.Name,-8}{student.Average:F2}");
    }

    public void PrintCourseReport()
    {
        foreach (string course in sortedCourses)
        {
            var courseStudents = studentOrder
                .Select(name => new
                {
                    Name = originalNames[name],
                    Grades = students[name].Where(g => g.Course == course).ToList()
                })
                .Where(s => s.Grades.Count > 0)
                .Select(s => new
                {
                    s.Name,
                    s.Grades,
                    Average = s.Grades.Average(g => (decimal)g.Score)
                })
                .ToList();

            if (courseStudents.Count == 0)
            {
                Console.WriteLine($"{course,-6} students: 0, average: 0.00, best: none");
                continue;
            }

            decimal average = courseStudents
                .SelectMany(s => s.Grades)
                .Average(g => (decimal)g.Score);

            var best = courseStudents
                .OrderByDescending(s => s.Average)
                .ThenBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
                .First();

            Console.WriteLine($"{course,-6} students: {courseStudents.Count}, average: {average:F2}, best: {best.Name} ({best.Average:F2})");
        }
    }

    public void PrintHistory()
    {
        Console.WriteLine("History:");
        foreach (string action in history)
            Console.WriteLine($"  {action}");
    }

    public void PrintDistribution()
    {
        SortedDictionary<char, int> distribution = new()
        {
            ['A'] = 0,
            ['B'] = 0,
            ['C'] = 0,
            ['D'] = 0,
            ['E'] = 0,
            ['F'] = 0
        };

        foreach (Grade grade in students.Values.SelectMany(grades => grades))
        {
            char letter = grade.Score switch
            {
                >= 91 => 'A',
                >= 81 => 'B',
                >= 71 => 'C',
                >= 61 => 'D',
                >= 51 => 'E',
                _ => 'F'
            };

            distribution[letter]++;
        }

        Console.WriteLine("Distribution: " +
            string.Join(", ", distribution.Select(e => $"{e.Key}={e.Value}")));
    }
}

public static class Demo
{
    public static void Run()
    {
        Gradebook gradebook = new();

        (string Name, string Course, int Score)[] initialScores =
        {
            ("Ana", ".NET", 95),
            ("Ana", "DB", 88),
            ("Ana", "Math", 100),
            ("Gio", ".NET", 72),
            ("Gio", "DB", 81),
            ("luka", ".NET", 65),
            ("Nino", "Math", 91),
            ("Nino", ".NET", 67),
            ("Nino", "DB", 79),
            ("Ana", ".NET", 98)
        };

        foreach (var (name, course, score) in initialScores)
            gradebook.AddScore(name, course, score);

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("1. Add score");
            Console.WriteLine("2. Table");
            Console.WriteLine("3. Search student");
            Console.WriteLine("4. Top 3");
            Console.WriteLine("5. Undo");
            Console.WriteLine("6. Course report");
            Console.WriteLine("7. History");
            Console.WriteLine("0. Exit");
            Console.Write("Choice: ");

            string? choice = Console.ReadLine();
            if (choice is null)
                return;

            switch (choice.Trim())
            {
                case "1":
                    Console.Write("Name: ");
                    string? name = Console.ReadLine();
                    if (name is null)
                        return;

                    Console.Write("Course: ");
                    string? course = Console.ReadLine();
                    if (course is null)
                        return;

                    Console.Write("Score: ");
                    string? scoreText = Console.ReadLine();
                    if (scoreText is null)
                        return;

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        Console.WriteLine("Invalid name");
                    }
                    else if (!int.TryParse(scoreText.Trim(), out int score))
                    {
                        Console.WriteLine($"Invalid score input: {scoreText}");
                    }
                    else
                    {
                        Console.WriteLine(gradebook.AddScore(name.Trim(), course.Trim(), score));
                    }
                    break;

                case "2":
                    gradebook.PrintTable();
                    break;

                case "3":
                    Console.Write("Student name: ");
                    string? query = Console.ReadLine();
                    if (query is null)
                        return;
                    gradebook.SearchStudent(query.Trim());
                    break;

                case "4":
                    gradebook.PrintTopThree();
                    break;

                case "5":
                    Console.WriteLine(gradebook.Undo());
                    break;

                case "6":
                    gradebook.PrintCourseReport();
                    break;

                case "7":
                    gradebook.PrintHistory();
                    break;

                case "0":
                    gradebook.PrintDistribution();
                    return;

                default:
                    Console.WriteLine("Unknown menu option");
                    break;
            }
        }
    }
}
