using System.Web;

namespace Payfast.Common.Extensions
{
    public static class StringExtensions
    {
        public static string UrlEncode(this string value)
        {
            return HttpUtility.UrlEncode(value);
        }
    }
}
