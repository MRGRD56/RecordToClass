using System.ComponentModel.DataAnnotations;
using RecordToClass.Example;

namespace RecordToClass.Records
{
    public record User (
        System.Int32 Id,
        [Required(AllowEmptyStrings = false, ErrorMessage = "Specify the first name")] 
        string FirstName,
        [Required(AllowEmptyStrings = false, ErrorMessage = "Specify the last name")]
        string LastName,
        [RegularExpression(@"^(?=.*[A-Za-zА-Яа-яЁё])(?=.*\d).{6,}$")]
        string Password,
        bool IsDeleted = false) : IDeletable
    {
        public string GetFullName() => $"{FirstName} {LastName}";

        public override string ToString()
        {
            return $"{Id}, {GetFullName()}";
        }
    }
}