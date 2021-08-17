using System.ComponentModel.DataAnnotations;

namespace RecordToClass.Classes
{
    public class User
    {
        public System.Int32 Id { get; }
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "Specify the first name")]
        public string FirstName { get; }
        
        [Required(AllowEmptyStrings = false, ErrorMessage = "Specify the last name")]
        public string LastName { get; }
        
        [RegularExpression(@"^(?=.*[A-Za-zА-Яа-яЁё])(?=.*\d).{6,}$")]
        public string Password { get; }
        
        public bool IsDeleted { get; }

        public User(System.Int32 id, string firstName, string lastName, string password, bool isDeleted = false)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Password = password;
            IsDeleted = isDeleted;
        }
        
        public string GetFullName() => $"{FirstName} {LastName}";
        
        //Something {{}}}}}}}
        
        //blah blah blah
    }
}