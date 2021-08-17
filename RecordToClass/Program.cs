using System;
using RecordToClass.Models;

namespace RecordToClass
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var record = Record.Parse(@"
public record User(
    int Id,
    [Required(AllowEmptyStrings = false, ErrorMessage = ""Specify the first name"")] 
    string FirstName,
    [Required(AllowEmptyStrings = false, ErrorMessage = ""Specify the last name"")]
    string LastName,
    [RegularExpression(@""^(?=.*[A-Za-zА-Яа-яЁё])(?=.*\d).{6,}$"")]
    string Password,
    bool IsDeleted = false)
{
    public string GetFullName() => $""{FirstName} {LastName}"";
    
    //Something {{}}}}}}}
    
    //blah blah blah
}".TrimStart());

            Console.WriteLine(record.ToClassString());
        }
    }
}