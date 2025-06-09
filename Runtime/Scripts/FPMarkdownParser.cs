namespace FuzzPhyte.Blogger
{
    using System.Collections.Generic;
    using System;

    public class TeleprompterSection
    {
        public string Title;       // Corresponds to ##
        public string Subsection;  // Corresponds to ###
        public string Category;    // Corresponds to ####
        public string Body;
    }

    public static class FPMarkdownParser
    {
        public static List<TeleprompterSection> ParseSections(string markdown)
        {
            var sections = new List<TeleprompterSection>();
            var lines = markdown.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            string currentTitle = "";
            string currentSub = "";
            string currentCat = "";
            TeleprompterSection current = null;
            bool introHandled = false;

            foreach (var line in lines)
            {
                if (line.StartsWith("# ") && !introHandled)
                {
                    if (current != null)
                        sections.Add(current);
                    current = new TeleprompterSection
                    {
                        Title = line.Substring(2).Trim(),
                        Subsection = "",
                        Category = "",
                        Body = ""
                    };
                    introHandled = true;
                }
                else if (line.StartsWith("## "))
                {
                    currentTitle = line.Substring(3).Trim();
                    currentSub = "";
                    currentCat = "";
                    if (current != null) sections.Add(current);
                    current = new TeleprompterSection
                    {
                        Title = currentTitle,
                        Subsection = currentSub,
                        Category = currentCat,
                        Body = ""
                    };
                }
                else if (line.StartsWith("### "))
                {
                    currentSub = line.Substring(4).Trim();
                    if (current != null) sections.Add(current);
                    current = new TeleprompterSection
                    {
                        Title = currentTitle,
                        Subsection = currentSub,
                        Category = currentCat,
                        Body = ""
                    };
                }
                else if (line.StartsWith("#### "))
                {
                    currentCat = line.Substring(5).Trim();
                    if (current != null) sections.Add(current);
                    current = new TeleprompterSection
                    {
                        Title = currentTitle,
                        Subsection = currentSub,
                        Category = currentCat,
                        Body = ""
                    };
                }
                else if (line.Trim() == "---")
                {
                    if (current != null)
                    {
                        sections.Add(current);
                        current = null;
                    }
                }
                else
                {
                    if (current == null)
                    {
                        current = new TeleprompterSection
                        {
                            Title = currentTitle,
                            Subsection = currentSub,
                            Category = currentCat,
                            Body = ""
                        };
                    }
                    current.Body += line + "\n";
                }
            }

            if (current != null)
            {
                sections.Add(current);
            }

            return sections;
        }
    }

}
