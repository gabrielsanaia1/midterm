using System;
using System.Collections.Generic;

namespace Ex1;

public interface IDigital
{
    string Url { get; }
}

public interface IBorrowable
{
    bool IsBorrowed { get; }
    bool Borrow();
    void Return();
}

public abstract class LibraryItem
{
    public string Title { get; }

    protected LibraryItem(string title)
    {
        Title = title;
    }

    public virtual string Info() => Title;
}

public abstract class PhysicalItem : LibraryItem, IBorrowable
{
    protected PhysicalItem(string title) : base(title) { }

    public abstract int LoanDays { get; }
    public bool IsBorrowed { get; private set; }

    public bool Borrow()
    {
        if (IsBorrowed)
            return false;

        IsBorrowed = true;
        return true;
    }

    public void Return()
    {
        IsBorrowed = false;
    }

    public override string Info()
    {
        return $"{Title}: {LoanDays} days" + (IsBorrowed ? " [borrowed]" : "");
    }
}

public class Book : PhysicalItem
{
    public Book(string title) : base(title) { }
    public override int LoanDays => 21;
}

public class Dvd : PhysicalItem
{
    public Dvd(string title) : base(title) { }
    public override int LoanDays => 7;
    public override string Info() => base.Info() + " (DVD)";
}

public class EBook : LibraryItem, IDigital
{
    public EBook(string title, string url) : base(title)
    {
        Url = url;
    }

    public string Url { get; }
    public override string Info() => $"{Title}: online only";
}

public static class Demo
{
    public static void Run()
    {
        List<LibraryItem> items = new()
        {
            new Book("Clean Code"),
            new Dvd("Inception"),
            new EBook("C# Basics", "https://books.example.com/csharp")
        };

        foreach (LibraryItem item in items)
            Console.WriteLine(item.Info());

        Console.WriteLine();
        foreach (LibraryItem item in new[] { items[0], items[0], items[2], items[1] })
        {
            if (item is IBorrowable b)
                Console.WriteLine(b.Borrow()
                    ? $"{item.Title}: borrowed"
                    : $"{item.Title}: already borrowed");
            else
                Console.WriteLine($"{item.Title}: cannot be borrowed");
        }

        Console.WriteLine();
        foreach (LibraryItem item in items)
            Console.WriteLine(item.Info());

        ((IBorrowable)items[0]).Return();
        Console.WriteLine($"After return: {items[0].Info()}");

        foreach (LibraryItem item in items)
            if (item is IDigital d)
                Console.WriteLine($"Download: {d.Url}");
    }
}
