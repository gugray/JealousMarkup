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
        private class TextInfo
        {
            public string Rel;
            public string Title = "";
            public DateTime Date;
            public string Keywords = "";
            public string Description = "";
            public List<string> Categories = new List<string>();
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

        /// <summary>
        /// Regex to identify/extract metainformation included in HTML files as funny DIVs.
        /// </summary>
        private readonly Regex reMetaDiv = new Regex("<div +id=\"x\\-([^\"]+)\">([^<]*)<\\/div>[ \t]*");

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
            readIndex();
            DirectoryInfo diRoot = new DirectoryInfo("./texts");
            buildTextsRecursive(diRoot);
            texts.Sort((x, y) => y.Date.CompareTo(x.Date));
            rebuildFile("./structure/home.html", true);
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

        public void RebuildText(string srcPath)
        {
            rebuildFile(srcPath, false);
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
                    string value = WebUtility.HtmlDecode(m.Groups[2].Value);
                    if (key == "title") ti.Title = value;
                    else if (key == "rel") ti.Rel = value;
                    // TO-DO: all other meta.
                }
            }
            sbDev.Replace("{{title}}", WebUtility.HtmlEncode(ti.Title));
            sbProd.Replace("{{title}}", WebUtility.HtmlEncode(ti.Title));
            if (isHomePage) generateToc(sbContent);

            string strContent = sbContent.ToString();
            sbDev.Replace("{{content}}", strContent);
            sbProd.Replace("{{content}}", strContent);

            string trgFolder = "./wwwroot";
            if (!isHomePage) trgFolder += "/texts" + ti.Rel;
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

        private void generateToc(StringBuilder sbContent)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var ti in texts)
            {
                sb.Append("<a href='/texts" + ti.Rel + "'>" + WebUtility.HtmlEncode(ti.Title) + "</a><br/>");
                sb.AppendLine();
            }

            // TO-DO: final form
            sbContent.Replace("{{TOC}}", sb.ToString());
        }
    }
}
