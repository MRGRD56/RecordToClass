﻿using System;
using System.ComponentModel.DataAnnotations;

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
        bool IsDeleted = false) : IDisposable
    {
        public string GetFullName() => $"{FirstName} {LastName}";
        
        //Something {{}}}}}}}
        
        //blah blah blah
        public void Dispose()
        {
        }
    }
}