using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Net;
using System.Text;

namespace JealousSite
{
    public class CatUrlRewriter
    {
        private readonly RequestDelegate _next;

        public CatUrlRewriter(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.ToUriComponent();
            if (path.StartsWith("/cats/"))
            {
                path = path.Substring(6);
                path = WebUtility.UrlDecode(path);
                string[] cats = path.Split('&');
                StringBuilder query = new StringBuilder();
                query.Append("?");
                bool first = true;
                foreach (string cat in cats)
                {
                    if (!first) query.Append("&");
                    first = false;
                    query.Append(WebUtility.UrlEncode(cat));
                }
                query.Replace("%2B", "+");
                context.Request.QueryString = new QueryString(query.ToString());
                context.Request.Path = "/";
            }
            await _next.Invoke(context);

        }
    }
}
