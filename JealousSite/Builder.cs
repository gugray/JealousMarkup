using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

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
        private string strAtom;
        private string strAtomItem;

        private static readonly string devIncludeHead =
            "<link rel='stylesheet' href='/_dev/jealous.css'>";

        private static readonly string prodIncludeHead =
            "<link rel='stylesheet' href='/static/jealous.min.css?v={{css-hash}}'>";

        private static readonly string devIncludeBody =
            "<script src='/_lib/jquery-3.1.1.min.js'></script>\n" +
            "<script src='/_lib/jquery.tooltipster.min.js'></script>\n" +
            //"<script src='/static/chart-2.1.6.min.js'></script>\n" +
            "<script src='/_dev/jealous.js'></script>\n" +
            "<script src='/_dev/locaQuiz.js'></script>\n" +
            "<script src='/_dev/catJargon.js'></script>\n" +
            "<script src='/_dev/lspKeywords.js'></script>\n";

        private static readonly string prodIncludeBody =
            "<script src='/static/jealous.min.js?v={{js-hash}}'></script>";

        private string prodIncludeHeadBusted;
        private string prodIncludeBodyBusted;
        private readonly List<TextInfo> texts = new List<TextInfo>();
        private readonly HashSet<string> cats = new HashSet<string>();

        /// <summary>
        /// Regex to identify/extract metainformation included in HTML files as funny DIVs.
        /// </summary>
        private readonly Regex reMetaDiv = new Regex("^<div +id=\"x\\-([^\"]+)\">(.*)<\\/div>[ \t]*$");

        private readonly Regex reMetaMeta = new Regex("^<meta +(name|property)=\"([^\"]+)\" +content=\"([^\"]+)\".*\\/>[ \t]*$");

        public Builder()
        {
            readIndexAtom();
        }

        private void readIndexAtom()
        {
            using (FileStream fs = new FileStream("./structure/index.html", FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs))
            {
                strIndex = sr.ReadToEnd();
            }
            using (FileStream fs = new FileStream("./structure/atom.xml", FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs))
            {
                strAtom = sr.ReadToEnd();
            }
            using (FileStream fs = new FileStream("./structure/atomItem.xml", FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs))
            {
                strAtomItem = sr.ReadToEnd();
            }
        }

        public void RebuildAll()
        {
            using (var md5 = MD5.Create())
            {
                string verHash;
                using (var s = File.OpenRead("./wwwroot/static/jealous.min.css"))
                {
                    verHash = toBase32(md5.ComputeHash(s));
                    prodIncludeHeadBusted = prodIncludeHead.Replace("{{css-hash}}", verHash);
                }
                using (var s = File.OpenRead("./wwwroot/static/jealous.min.js"))
                {
                    verHash = toBase32(md5.ComputeHash(s));
                    prodIncludeBodyBusted = prodIncludeBody.Replace("{{js-hash}}", verHash);
                }
            }


            texts.Clear();
            cats.Clear();
            readIndexAtom();
            DirectoryInfo diRoot = new DirectoryInfo("./texts");
            buildTextsRecursive(diRoot);
            texts.Sort((x, y) => y.Date.CompareTo(x.Date));
            foreach (var ti in texts)
                foreach (var cat in ti.Cats)
                    cats.Add(cat);
            rebuildFile("./structure/home.html", true);
            buildSitemap();
            buildAtom();
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
            // DBG
            if (srcPath.Contains("catzine-03"))
            {
                int fdks = 0;
            }

            TextInfo ti = new TextInfo();

            StringBuilder sbDev = new StringBuilder(strIndex);
            sbDev.Replace("{{head-include}}", devIncludeHead);
            sbDev.Replace("{{body-include}}", devIncludeBody);
            sbDev.Replace("{{base-url}}", baseUrl);

            StringBuilder sbProd = new StringBuilder(strIndex);
            sbProd.Replace("{{head-include}}", prodIncludeHeadBusted);
            sbProd.Replace("{{body-include}}", prodIncludeBodyBusted);
            sbProd.Replace("{{base-url}}", baseUrl);

            StringBuilder sbContent = new StringBuilder();
            StringBuilder sbMetaTags = new StringBuilder();
            string metaDescription = "";
            string shortTitle = "";
            using (FileStream fs = new FileStream(srcPath, FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Match m = reMetaDiv.Match(line);
                    if (!m.Success)
                    {
                        m = reMetaMeta.Match(line);
                        if (m.Success)
                        {
                            sbMetaTags.AppendLine(line);
                            if (m.Groups[2].Value == "description")
                                metaDescription = WebUtility.HtmlDecode(m.Groups[3].Value);
                        }
                        else sbContent.AppendLine(line);
                        continue;
                    }
                    string key = m.Groups[1].Value;
                    string value = m.Groups[2].Value;
                    if (key == "title")
                    {
                        ti.Title = WebUtility.HtmlDecode(value);
                        shortTitle = ti.Title;
                    }
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
            // Meta tags
            string strMetaTags = sbMetaTags.ToString();
            sbDev.Replace("{{meta-tags}}", strMetaTags);
            sbProd.Replace("{{meta-tags}}", strMetaTags);

            // Our (in-site) "metas", aka tags/cats/watchamacallit
            StringBuilder sbMeta = new StringBuilder();
            sbMeta.AppendLine("<span class='date'>" + months[ti.Date.Month - 1] + " " + ti.Date.Day + ", " + ti.Date.Year + "</span>");
            if (ti.Cats.Count != 0)
            {
                sbMeta.AppendLine("<span class='filedunder'>");
                foreach (var cat in ti.Cats)
                    sbMeta.AppendLine("<span>" + WebUtility.HtmlEncode(cat) + "</span>");
                sbMeta.AppendLine("</span>"); // <span class='filedunder'>
            }
            string strMeta = sbMeta.ToString();
            sbContent.Replace("{{meta}}", strMeta);


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

            string resolvedRel = "";
            if (!isHomePage)
            {
                if (!ti.Rel.StartsWith("!")) resolvedRel = "/texts" + ti.Rel;
                else resolvedRel = "/" + ti.Rel.Substring(1);
            }

            // URL in page, and other quirky placeholders
            string pageUrl = baseUrl + resolvedRel;
            sbDev.Replace("{{url}}", pageUrl);
            sbProd.Replace("{{url}}", pageUrl);
            sbDev.Replace("{{description}}", WebUtility.HtmlEncode(metaDescription));
            sbProd.Replace("{{description}}", WebUtility.HtmlEncode(metaDescription));
            sbDev.Replace("{{short-title}}", WebUtility.HtmlEncode(shortTitle));
            sbProd.Replace("{{short-title}}", WebUtility.HtmlEncode(shortTitle));
            sbDev.Replace("{{share-text}}", WebUtility.UrlEncode(metaDescription));
            sbProd.Replace("{{share-text}}", WebUtility.UrlEncode(metaDescription));

            // Where the page goes
            string trgFolder = "./wwwroot" + resolvedRel;
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
                if (ti.NoIndex) continue;

                sb.AppendLine("<div class='toc-item'>");
                sb.AppendLine("<h2><a href='/texts" + ti.Rel + "'>" + WebUtility.UrlDecode(ti.Title) + "</a></h2>");
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

        private void buildAtom()
        {
            StringBuilder sb = new StringBuilder(strAtom);
            sb.Replace("{{siteTitle}}", WebUtility.HtmlEncode(siteTitle));
            sb.Replace("{{baseUrl}}", baseUrl);
            string timeStr = DateTime.UtcNow.ToString("r", CultureInfo.InvariantCulture);
            timeStr = timeStr.Replace("GMT", "+0000");
            sb.Replace("{{buildTime}}", timeStr);
            foreach (var ti in texts)
            {
                if (ti.NoIndex) continue;
                string rel = ti.Rel;
                if (rel.StartsWith("!")) continue;
                else rel = "/texts" + rel;
                string url = baseUrl + rel;
                StringBuilder sb2 = new StringBuilder(strAtomItem);
                sb2.Replace("{{title}}", WebUtility.HtmlEncode(ti.Title));
                sb2.Replace("{{url}}", url);
                timeStr = (ti.Date.AddHours(4)).ToString("r", CultureInfo.InvariantCulture);
                timeStr = timeStr.Replace("GMT", "+0000");
                sb2.Replace("{{pubDate}}", timeStr);
                sb.Replace("{{items}}", sb2.ToString() + "\n{{items}}");
            }
            sb.Replace("{{items}}", "");
            if (!Directory.Exists("./wwwroot/rss")) Directory.CreateDirectory("./wwwroot/rss");
            using (FileStream fs = new FileStream("./wwwroot/rss/atom.xml", FileMode.Create, FileAccess.ReadWrite))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(sb.ToString());
            }

        }

        private const string base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        private static string toBase32(byte[] bytes)
        {
            // Prepare container for the final value
            StringBuilder builder = new StringBuilder(bytes.Length * 8 / 5);

            // Position in the input buffer
            int bytesPosition = 0;

            // Offset inside a single byte that <bytesPosition> points to (from left to right)
            // 0 - highest bit, 7 - lowest bit
            int bytesSubPosition = 0;

            // Byte to look up in the dictionary
            byte outputBase32Byte = 0;

            // The number of bits filled in the current output byte
            int outputBase32BytePosition = 0;

            // Iterate through input buffer until we reach past the end of it
            while (bytesPosition < bytes.Length)
            {
                // Calculate the number of bits we can extract out of current input byte to fill missing bits in the output byte
                int bitsAvailableInByte = Math.Min(8 - bytesSubPosition, 5 - outputBase32BytePosition);

                // Make space in the output byte
                outputBase32Byte <<= bitsAvailableInByte;

                // Extract the part of the input byte and move it to the output byte
                outputBase32Byte |= (byte)(bytes[bytesPosition] >> (8 - (bytesSubPosition + bitsAvailableInByte)));

                // Update current sub-byte position
                bytesSubPosition += bitsAvailableInByte;

                // Check overflow
                if (bytesSubPosition >= 8)
                {
                    // Move to the next byte
                    bytesPosition++;
                    bytesSubPosition = 0;
                }

                // Update current base32 byte completion
                outputBase32BytePosition += bitsAvailableInByte;

                // Check overflow or end of input array
                if (outputBase32BytePosition >= 5)
                {
                    // Drop the overflow bits
                    outputBase32Byte &= 0x1F;  // 0x1F = 00011111 in binary

                    // Add current Base32 byte and convert it to character
                    builder.Append(base32Alphabet[outputBase32Byte]);

                    // Move to the next byte
                    outputBase32BytePosition = 0;
                }
            }

            // Check if we have a remainder
            if (outputBase32BytePosition > 0)
            {
                // Move to the right bits
                outputBase32Byte <<= (5 - outputBase32BytePosition);

                // Drop the overflow bits
                outputBase32Byte &= 0x1F;  // 0x1F = 00011111 in binary

                // Add current Base32 byte and convert it to character
                builder.Append(base32Alphabet[outputBase32Byte]);
            }

            return builder.ToString();
        }

    }
}
