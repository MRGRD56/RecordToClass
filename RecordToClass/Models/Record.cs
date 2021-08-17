using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RecordToClass.Models
{
    public record Record(
        string Modifiers,
        string Name,
        IEnumerable<RecordMember> Members,
        string Body,
        string InheritedTypes)
    {
        public static Record Parse(string recordString)
        {
            //GROUPS
            //1: Modifiers?
            //2: Name
            //4: Members? (primary constructor)
            //6: Inherited types?
            //8: Body?
            var match = Regex.Match(recordString, 
                @"^([a-z\s]*\s+)?record\s+([a-zA-Z0-9_@]+)\s*(\(\s*(.*)\s*\))?\s*(\:\s*([a-zA-Z @\,\.]*))?\s*;?\s*(\{\s*(.*)\s*\})?$", 
                RegexOptions.Singleline);
            var modifiers = match.Groups[1].Value.Trim();
            var name = match.Groups[2].Value;
            var membersString = match.Groups[4].Value;
            var inheritedTypes = match.Groups[6].Value;
            var body = match.Groups[8].Value;

            var members = RecordMember.ParseList(membersString);
            return new Record(modifiers, name, members, body, inheritedTypes);
        }
        
        public string ToRecordString()
        {
            var str = $"{Modifiers} record {Name}";
            if (Members?.Any() == true)
            {
                var membersString = Members.ToRecordMembersString().Replace("\r\n", "\n").Replace("\n", "\n    ");
                if (!membersString.StartsWith("    "))
                {
                    membersString = "    " + membersString;
                }
                str += $"(\n{membersString})";
            }

            str += $"{(!string.IsNullOrWhiteSpace(InheritedTypes) ? $" : {InheritedTypes}" : "")}";

            if (!string.IsNullOrWhiteSpace(Body))
            {
                var bodyString = Body;
                if (!bodyString.StartsWith("    "))
                {
                    bodyString = "    " + bodyString;
                }
                if (!bodyString.EndsWith("\n"))
                {
                    bodyString += "\n";
                }
                str += $"\n{{\n{bodyString}}}";
            }

            return str;
        }
        
        public string ToClassString(
            PropertyType propertiesType = PropertyType.Get, 
            bool useThisKeyword = false,
            bool createDeconstructor = true)
        {
            var propertyEnding = PropertyTypes[propertiesType];
            var thisKeyword = useThisKeyword ? "this." : "";
            
            var str = $"{Modifiers} class {Name}{(!string.IsNullOrWhiteSpace(InheritedTypes) ? $" : {InheritedTypes}" : "")}\n{{\n    ";

            if (Members?.Any() == true)
            {
                foreach (var member in Members)
                {
                    var stringToAdd = $"{(!string.IsNullOrWhiteSpace(member.Attributes) ? $"{member.Attributes}\n" : "")}" +
                                      $"public {member.Type} {member.Name}" +
                                      $"{(!string.IsNullOrWhiteSpace(member.DefaultValue) ? $" = {member.DefaultValue}" : "")}{propertyEnding}\n";

                    str += stringToAdd.Replace("\n\r", "\n").Replace("\n", "\n    ");
                }
            
                str += $"\n    public {Name}({string.Join(", ", Members.Select(m => $"{m.Type} {ToCamelCase(m.Name)}"))})\n    {{\n";
            
                foreach (var member in Members)
                {
                    str += $"        {thisKeyword}{member.Name} = {ToCamelCase(member.Name)};\n";
                }

                str += "    }";
            }

            if (!string.IsNullOrWhiteSpace(Body))
            {
                str += "\n\n    " + Body.TrimEnd();
            }

            if (createDeconstructor && Members?.Any() == true)
            {
                str += $"\n\n    public void Deconstruct({string.Join(", ", Members.Select(m => $"out {m.Type} {ToCamelCase(m.Name)}"))})\n    {{\n";
                foreach (var member in Members)
                {
                    str += $"        {ToCamelCase(member.Name)} = {thisKeyword}{member.Name};\n";
                }

                str += "    }";
            }

            str += "\n}";
            return str;
        }

        public override string ToString() => ToRecordString();

        private static readonly Dictionary<PropertyType, string> PropertyTypes = new()
        {
            { PropertyType.GetSet, " { get; set; }" },
            { PropertyType.Get, " { get; }" },
            { PropertyType.Set, " { set; }" },
            { PropertyType.GetInit, " { get; init; }" },
            { PropertyType.Field, ";" }
        };

        private static string ToCamelCase(string pascalCase)
        {
            var stringBuilder = new StringBuilder(pascalCase);
            stringBuilder[0] = char.ToLower(stringBuilder[0]);
            return stringBuilder.ToString();
        }
    }
}