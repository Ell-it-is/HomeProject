using System.Linq;

namespace Odberatele
{
    public static class Pomocne
    {
        public static string ReturnFromTo(int from, char to, string where)
        {
            string result = where.Substring(from);
            int t = result.IndexOf(to);
            result = result.Remove(t);
            return result;
        } 
        
        public static string RemoveZeros(string word)
        {
            var result = from c in word
                where c != '0'
                select c;

            return string.Concat(result);
        }
    }
}