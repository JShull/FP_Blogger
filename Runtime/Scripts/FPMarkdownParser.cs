namespace FuzzPhyte.Blogger
{
    using System.Collections.Generic;
    using System;

    public class TeleprompterSection
    {
        public string Title;       // Corresponds to #
        public string Subsection;  // Corresponds to ##
        public string Category;    // Corresponds to ###
        public string SubCategory; // Corresponds to ####
        public string Body;

        public float SectionTimeSeconds; // automate time next/previous section
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
            string currentSubCat = "";
            float currentSectionTime = 0f;
            TeleprompterSection current = null;
            bool introHandled = false;
            
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.StartsWith("# ") && !introHandled)
                {
                    current = null;
                    currentTitle = line.Substring(2).Trim();
                    introHandled = true;
                }
                else if (line.StartsWith("## "))
                {
                    ResetCurrent(ref current, ref sections);
                    currentSub = line.Substring(3).Trim();
                }
                else if (line.StartsWith("### "))
                {
                    ResetCurrent(ref current, ref sections);
                    currentCat = line.Substring(4).Trim();
                }
                else if (line.StartsWith("#### "))
                {
                    ResetCurrent(ref current, ref sections);
                    currentSubCat = line.Substring(5).Trim();
                }
                else if (line.Trim() == "---")
                {
                    ResetCurrent(ref current, ref sections);
                }
                else if (line.StartsWith("~ "))
                {
                    var sectionTime = line.Substring(2).Trim();
                    //cast to float
                    if (float.TryParse(sectionTime, out currentSectionTime))
                    {
                        
                        if (current != null)
                        {
                            current.SectionTimeSeconds += currentSectionTime;
                        }
                    }
                }
                else
                {
                    //body content
                    //if line isn't empty, add to current section
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue; // Skip empty lines
                    }
                    if (current == null)
                    {
                        current = new TeleprompterSection
                        {
                            Title = currentTitle,
                            Subsection = currentSub,
                            Category = currentCat,
                            SubCategory = currentSubCat,
                            SectionTimeSeconds = 0,
                            Body = line + "\n"
                        };
                    }
                    else
                    {
                        current.Body += line + "\n";
                    }
                }
            }
/*
bool gotContent = false;
            foreach (var line in lines)
            {
                if (line.StartsWith("# ") && !introHandled)
                {
                    if (current != null)
                    {
                        sections.Add(current);
                    }
                    else
                    {
                        currentTitle = line.Substring(2).Trim();
                        current = new TeleprompterSection
                        {
                            Title = line.Substring(2).Trim(),
                            Subsection = "",
                            Category = "",
                            Body = ""
                        };
                    }
                    introHandled = true;
                }
                else if (line.StartsWith("## "))
                {
                    //currentTitle = line.Substring(3).Trim();
                    currentSub = line.Substring(3).Trim();
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
                    gotContent = true;
                }
            }

            if (current != null && gotContent)
            {
                sections.Add(current);
                gotContent = false;
            }
*/
            return sections;
        }
        private static void ResetCurrent(ref TeleprompterSection currentPrompt, ref List<TeleprompterSection> sections)
        {
            if (currentPrompt != null)
            {
                sections.Add(currentPrompt);
            }
            currentPrompt = null;
        }
    }

}
