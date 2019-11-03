using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace TestProject.Pages
{
    public class CreateBoardModel : PageModel
    {

        [BindProperty(SupportsGet = true)]
        public int ColumnCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public int RowCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public IEnumerable<string> Categories { get; set; }

        public IActionResult OnGet()
        {
            if (QuestionHandler.Instance.GameBoard != null)
            {
                return Redirect("/GameBoard");
            }
            else return null;
        }


    }
}