using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RecordToClass.Models
{
    public record RecordMember(
        string Type,
        string Name,
        string Attributes,
        string DefaultValue)
    {
        public static List<RecordMember> ParseList(string membersString)
        {
            if (!membersString.EndsWith(","))
            {
                membersString += ",";
            }

            //GROUPS
            //1: Attributes?
            //2: Type
            //3: Name
            //5: Default Value?
            var matches = Regex.Matches(membersString, 
                @"(\[.*?\])*\s*([a-zA-Z0-9_@\.<>]+\??)\s+([a-zA-Z0-9_@]+)(\s*=\s*([^\n]+))?\,",
                RegexOptions.Singleline);

            return matches
                .Select(match =>
                {
                    var groups = match.Groups;
                    var attributes = groups[1].Value;
                    var type = groups[2].Value;
                    var name = groups[3].Value;
                    var defaultValue = groups[5].Value;

                    return new RecordMember(type, name, attributes, defaultValue);
                })
                .ToList();
        }

        public override string ToString()
        {
            return $"{(!string.IsNullOrWhiteSpace(Attributes) ? $"{Attributes}\n" : "")}{Type} {Name}{(!string.IsNullOrWhiteSpace(DefaultValue) ? $" = {DefaultValue}" : "")}";
        }
    }
}