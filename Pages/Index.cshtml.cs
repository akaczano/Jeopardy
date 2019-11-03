using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace TestProject.Pages
{

    struct Assignment
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public List<int> cats { get; set; }
    }

    public class IndexModel : PageModel
    {

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime MinDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime MaxDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Value { get; set; }


        public IEnumerable<SelectListItem> VALUES {
            get {
                List<SelectListItem> vals = new List<SelectListItem>();
                vals.Add(new SelectListItem("Any", "Any"));
                vals.Add(new SelectListItem("100", "100"));
                vals.Add(new SelectListItem("200", "200"));
                vals.Add(new SelectListItem("300", "300"));
                vals.Add(new SelectListItem("400", "400"));
                vals.Add(new SelectListItem("500", "500"));
                vals.Add(new SelectListItem("600", "600"));
                vals.Add(new SelectListItem("700", "700"));
                vals.Add(new SelectListItem("800", "800"));
                vals.Add(new SelectListItem("900", "900"));
                vals.Add(new SelectListItem("1000", "1000"));
                return vals;
            }
        }


        public List<dynamic> Questions { get; set; } = new List<dynamic>();
        public Object QuestionsLockingObject = new Object();

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void LoadCategories(Object o)
        {
            IEnumerable<string> cats = (IEnumerable<string>)o;
            int count = 0;
            foreach (string val in cats)
            {
                if (val == null)
                    continue;
                count++;
                Debug.WriteLine(val);
                using (WebClient wc = new WebClient())
                {
                    string min_date = MinDate.ToString("yyyy-MM-ddTHH:mm:ss") + ".000Z";
                    string max_date = MaxDate.ToString("yyyy-MM-ddTHH:mm:ss") + ".000Z";
                    string url = "http://jservice.io/api/clues?count=5&category=" + val + "&";

                    if (!min_date.Contains("0001"))
                    {
                        url += "min_date=" + min_date + "&";
                    }
                    if (!max_date.Contains("0001"))
                    {
                        url += "max_date=" + max_date + "&";
                    }
                    if (Value != "Any")
                    {
                        url += "value=" + Value;
                    }
                    if (url.EndsWith("&") || url.EndsWith("?"))
                    {
                        url = url.Substring(0, url.Length - 1);
                    }

                    string rawQuestions = wc.DownloadString(url);
                    dynamic questions = JsonConvert.DeserializeObject(rawQuestions);
                    lock (QuestionsLockingObject)
                    {
                        Questions.AddRange(questions);
                    }
                }
            }
            Debug.WriteLine(count);
        }

        public void OnPost()
        {
            if (SearchString == null) {
                return;
            }
            Stopwatch sw = Stopwatch.StartNew();
            JObject cats = JObject.Parse(System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "\\Categories.json"));


            List<string> matchedCategories = cats.AsJEnumerable<JToken>()
                .Where(x => x.ToString().Contains(SearchString))
                .Select(x => x.ToString().Split(":")[x.ToString().Split(":").Length - 1])
                .ToList();
            


            List<Thread> workers = new List<Thread>();
            int increment = matchedCategories.Count / 8;
            if (increment < 1)
                increment = 1;
            for (int i = 0; i < matchedCategories.Count; i+= increment)
            {
                Thread t = new Thread((ParameterizedThreadStart)LoadCategories);
                string[] assignment = new string[increment];
                
                matchedCategories.Skip(i).Take(increment).ToList().CopyTo(assignment, 0);                
                t.Start(assignment);
                workers.Add(t);
            }

            foreach (Thread t in workers)
            {
                t.Join();
            }
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
        }



        public void OnFost()
        {
            Stopwatch sw = Stopwatch.StartNew();
            int offset = 0;

            do
            {



                string url = "http://jservice.io/api/categories?count=100&";
                if (offset != 0)
                {
                    url += "offset=" + offset + "&";
                }


                using (WebClient wc = new WebClient())
                {
                    string rawResults = wc.DownloadString(url);
                    dynamic json = JsonConvert.DeserializeObject(rawResults);
                    foreach (dynamic cat in json)
                    {
                        if (cat.title.ToString().Contains(SearchString))
                        {
                            string u = "http://jservice.io/api/clues?category=" + cat.id + "&count=3";
                            string rawQuestions = wc.DownloadString(u);
                            dynamic questions = JsonConvert.DeserializeObject(rawQuestions);
                            Questions.AddRange(questions);
                        }
                    }
                    if (json.Count < 100)
                    {
                        break;
                    }
                    else
                    {
                        offset += 100;
                    }
                }

            } while (true);
            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds);
        }

        public void OnGet()
        {


        }

    }
}
