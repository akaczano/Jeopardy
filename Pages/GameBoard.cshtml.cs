using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TestProject.Pages
{
    public class GameBoardModel : PageModel
    {
        private readonly ILogger<GameBoardModel> _logger;
        public dynamic[,] GameBoard { get; set; }

        [BindProperty(SupportsGet = true)]
        public int RowCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public int ColumnCount { get; set; }

        public GameBoardModel(ILogger<GameBoardModel> logger)
        {
            _logger = logger;
        }

        public string[,] GetDisplay() {
            string[,] displayBoard = new string[GameBoard.GetLength(0),
               GameBoard.GetLength(1)];
            for (int i = 0; i < displayBoard.GetLength(0); i++) {
                for (int j = 0; j < displayBoard.GetLength(1); j++) {
                    if (i == 0)
                    {
                        displayBoard[i, j] =GameBoard[i, j].title.ToString();
                    }
                    else {
                        if (GameBoard[i, j].value.ToString().Length > 0)
                        {
                            displayBoard[i, j] = GameBoard[i, j].value.ToString();
                        }
                        else {
                            displayBoard[i, j] = (i * 200) + "";
                        }
                    }
                }

            }
            return displayBoard;
        }

        public void OnGet()
        {
        }

        public void OnPost() {
            InitGameBoard(RowCount, ColumnCount);
        }


        public void InitGameBoard(int rows, int cols)
        {
            GameBoard = new dynamic[rows + 1, cols];
            using (WebClient wc = new WebClient())
            {
                Random rand = new Random();

                int offset = rand.Next(0, 1000);
                string catsURL = string.Format("http://jservice.io/api/categories?offset={0}&count={1}",
                    offset, cols);
                string rawCategories = wc.DownloadString(catsURL);
                dynamic categories = JsonConvert.DeserializeObject(rawCategories);
                for (int i = 0; i < cols; i++)
                {
                    GameBoard[0, i] = categories[i];
                    string questionsURL = string.Format("http://jservice.io/api/clues?category={0}&count={1}",
                        categories[i].id, rows);
                    string rawQuestions = wc.DownloadString(questionsURL);
                    dynamic questions = JsonConvert.DeserializeObject(rawQuestions);
                    for (int j = 1; j <= rows; j++)
                    {
                        GameBoard[j, i] = questions[j - 1];
                    }
                }

            }
        }

    }
}
