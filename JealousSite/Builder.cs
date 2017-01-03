using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace JealousSite
{
    public class Builder
    {
        private const string baseUrl = "https://jealousmarkup.xyz";
        private const string siteTitle = "Jealous Markup";

        private class TextInfo
        {
            public string Rel;
            public bool NoIndex = false;
            public string Title = "";
            public string Lede = "";
            public DateTime Date;
            public string Keywords = "";
            public string Description = "";
            public List<string> Cats = new List<string>();
            public List<string> Tags = new List<string>();
        }

        private string strIndex;

        private static readonly string devIncludeHead =
            "<link rel='stylesheet' href='/_dev/jealous.css'>";

        private static readonly string prodIncludeHead =
            "<link rel='stylesheet' href='/static/jealous.min.css'>";

        private static readonly string devIncludeBody =
            "<script src='/_lib/jquery-3.1.1.min.js'></script>\n" +
            "<script src='/_lib/jquery.tooltipster.min.js'></script>\n" +
            "<script src='/_dev/jealous.js'></script>";

        private static readonly string prodIncludeBody =
            "<script src='/static/jealous.min.js'></script>";

        private readonly List<TextInfo> texts = new List<TextInfo>();
        private readonly HashSet<string> cats = new HashSet<string>();

        /// <summary>
        /// Regex to identify/extract metainformation included in HTML files as funny DIVs.
        /// </summary>
        private readonly Regex reMetaDiv = new Regex("^<div +id=\"x\\-([^\"]+)\">(.*)<\\/div>[ \t]*$");

        public Builder()
        {
            readIndex();
        }

        private void readIndex()
        {
            using (FileStream fs = new FileStream("./structure/index.html", FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs))
            {
                strIndex = sr.ReadToEnd();
            }
        }

        public void RebuildAll()
        {
            texts.Clear();
            cats.Clear();
            readIndex();
            DirectoryInfo diRoot = new DirectoryInfo("./texts");
            buildTextsRecursive(diRoot);
            texts.Sort((x, y) => y.Date.CompareTo(x.Date));
            foreach (var ti in texts)
                foreach (var cat in ti.Cats)
                    cats.Add(cat);
            rebuildFile("./structure/home.html", true);
            buildSitemap();
        }

        private void buildTextsRecursive(DirectoryInfo di)
        {
            var files = di.GetFiles("*.html");
            foreach (var file in files)
            {
                var textInfo = rebuildFile(file.FullName, false);
                texts.Add(textInfo);
            }
            var subDirs = di.GetDirectories();
            foreach (var subDir in subDirs) buildTextsRecursive(subDir);
        }

        private TextInfo rebuildFile(string srcPath, bool isHomePage)
        {
            TextInfo ti = new TextInfo();

            StringBuilder sbDev = new StringBuilder(strIndex);
            sbDev.Replace("{{head-include}}", devIncludeHead);
            sbDev.Replace("{{body-include}}", devIncludeBody);

            StringBuilder sbProd = new StringBuilder(strIndex);
            sbProd.Replace("{{head-include}}", prodIncludeHead);
            sbProd.Replace("{{body-include}}", prodIncludeBody);

            StringBuilder sbContent = new StringBuilder();
            using (FileStream fs = new FileStream(srcPath, FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Match m = reMetaDiv.Match(line);
                    if (!m.Success)
                    {
                        sbContent.AppendLine(line);
                        continue;
                    }
                    string key = m.Groups[1].Value;
                    string value = m.Groups[2].Value;
                    if (key == "title") ti.Title = WebUtility.HtmlDecode(value);
                    else if (key == "noindex") ti.NoIndex = true;
                    else if (key == "rel") ti.Rel = value;
                    else if (key == "lede") ti.Lede = value;
                    else if (key == "date") ti.Date = parseDate(value);
                    else if (key == "cats") ti.Cats = parseList(WebUtility.HtmlDecode(value));
                    else if (key == "tags") ti.Tags = parseList(WebUtility.HtmlDecode(value));
                    // TO-DO: all other meta.
                    // Noindex!!!
                }
            }
            if (ti.Title.Trim() != "")
            {
                sbDev.Replace("{{title}}", WebUtility.HtmlEncode(ti.Title + " - " + siteTitle));
                sbProd.Replace("{{title}}", WebUtility.HtmlEncode(ti.Title + " - " + siteTitle));
            }
            else
            {
                sbDev.Replace("{{title}}", WebUtility.HtmlEncode(siteTitle));
                sbProd.Replace("{{title}}", WebUtility.HtmlEncode(siteTitle));
            }
            sbContent.Replace("{{lede}}", ti.Lede);
            if (!ti.NoIndex)
            {
                sbDev.Replace("{{noindex}}", "");
                sbProd.Replace("{{noindex}}", "");
            }
            else
            {
                sbDev.Replace("{{noindex}}", "<meta name='robots' content='noindex,nofollow' />");
                sbProd.Replace("{{noindex}}", "<meta name='robots' content='noindex,nofollow' />");
            }

            // If this is home aka TOC, content has extra placeholders
            if (isHomePage) generateToc(sbContent);

            string strContent = sbContent.ToString();
            sbDev.Replace("{{content}}", strContent);
            sbProd.Replace("{{content}}", strContent);

            string trgFolder = "./wwwroot";
            if (!isHomePage)
            {
                if (!ti.Rel.StartsWith("!")) trgFolder += "/texts" + ti.Rel;
                else trgFolder += "/" + ti.Rel.Substring(1);
            }
            if (!Directory.Exists(trgFolder)) Directory.CreateDirectory(trgFolder);
            string devFileName = Path.Combine(trgFolder, "dev-index.html");
            string prodFileName = Path.Combine(trgFolder, "index.html");

            using (FileStream fs = new FileStream(devFileName, FileMode.Create, FileAccess.ReadWrite))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(sbDev.ToString());
            }
            using (FileStream fs = new FileStream(prodFileName, FileMode.Create, FileAccess.ReadWrite))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(sbProd.ToString());
            }

            return ti;
        }

        private static List<string> parseList(string str)
        {
            List<string> res = new List<string>();
            string[] parts = str.Split(',');
            foreach (var x in parts)
            {
                string trimmed = x.Trim();
                if (x != "") res.Add(trimmed);
            }
            return res;
        }

        private static DateTime parseDate(string str)
        {
            string[] parts = str.Split('-');
            return new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
        }

        private static readonly string[] months =
        {
            "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"
        };

        private void generateToc(StringBuilder sbContent)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var ti in texts)
            {
                if (ti.Rel.StartsWith("!")) continue;

                sb.AppendLine("<div class='toc-item'>");
                sb.AppendLine("<h2><a href='/texts/" + ti.Rel + "'>" + WebUtility.UrlDecode(ti.Title) + "</a></h2>");
                sb.AppendLine("<div class='toc-meta'>");
                sb.AppendLine("<span class='date'>" + months[ti.Date.Month - 1] + " " + ti.Date.Day + ", " + ti.Date.Year + "</span>");
                if (ti.Cats.Count != 0)
                {
                    sb.AppendLine("<span class='filedunder'>");
                    //sb.AppendLine("<i class='fa fa-bookmark-o' aria-hidden='true'></i>");
                    foreach (var cat in ti.Cats)
                        sb.AppendLine("<span>" + WebUtility.HtmlEncode(cat) + "</span>");
                    sb.AppendLine("</span>"); // <span class='filedunder'>
                }
                if (ti.Tags.Count != 0)
                {
                    sb.AppendLine("<span class='filedunder'>");
                    sb.AppendLine("<i class='fa fa-tag' aria-hidden='true'></i>");
                    foreach (var cat in ti.Tags)
                        sb.AppendLine("<span>" + WebUtility.HtmlEncode(cat) + "</span>");
                    sb.AppendLine("</span>"); // <span class='filedunder'>
                }
                sb.AppendLine("</div>"); // <div class='toc-meta'>
                //sb.AppendLine("<p>" + ti.Lede + "</p>");
                sb.AppendLine("</div>"); // <div class='toc-item'>
            }
            sbContent.Replace("{{TOC}}", sb.ToString());

            // Categories
            sb.Clear();
            List<string> catList = new List<string>();
            foreach (var cat in cats) catList.Add(cat);
            catList.Sort((x, y) => x.ToLowerInvariant().CompareTo(y.ToLowerInvariant()));
            foreach (var cat in catList) sb.AppendLine("<span>" + cat + "</span>");
            sbContent.Replace("{{CATS}}", sb.ToString());
        }

        private void buildSitemap()
        {
            using (FileStream fs = new FileStream("./wwwroot/sitemap.txt", FileMode.Create, FileAccess.ReadWrite))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                foreach (var ti in texts)
                {
                    if (ti.NoIndex) continue;
                    string rel = ti.Rel;
                    if (rel.StartsWith("!")) rel = "/" + rel.Substring(1);
                    else rel = "/texts" + rel;
                    string line = baseUrl + rel;
                    sw.WriteLine(line);
                }
            }
        }
    }
}
