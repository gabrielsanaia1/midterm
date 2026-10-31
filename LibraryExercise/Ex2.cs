using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ex2;

public class Book
{
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public int Year { get; set; }

    [JsonIgnore]
    public decimal Price { get; set; }
}

public static class Demo
{
    public static void Run()
    {
        List<Book> books = new()
        {
            new Book
            {
                Title = "Vepkhistkaosani",
                Author = "Shota Rustaveli",
                Year = 1200,
                Price = 50
            },
            new Book
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                Year = 2008,
                Price = 80
            },
            new Book
            {
                Title = "1984",
                Author = "George Orwell",
                Year = 1949,
                Price = 35
            }
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(books, options);

        File.WriteAllText("books.json", json);
        Console.WriteLine("Books saved to books.json");

        string jsonFromFile = File.ReadAllText("books.json");
        List<Book> loadedBooks =
            JsonSerializer.Deserialize<List<Book>>(jsonFromFile) ?? new List<Book>();

        Console.WriteLine("\nLoaded books:");
        foreach (Book book in loadedBooks)
        {
            Console.WriteLine($"Title: {book.Title}, Author: {book.Author}, " +
                              $"Year: {book.Year}, Price: {book.Price}");

        }
    }
}
