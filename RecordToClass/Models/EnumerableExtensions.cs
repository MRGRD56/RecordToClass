using System.Collections.Generic;
using System.Linq;

namespace RecordToClass.Models
{
    public static class EnumerableExtensions
    {
        public static string ToRecordMembersString(this IEnumerable<RecordMember> recordMembers)
        {
            return string.Join(",\n", recordMembers.Select(m => m.ToString()));
        }
    }
}