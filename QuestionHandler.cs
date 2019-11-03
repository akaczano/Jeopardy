using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;

namespace TestProject{

    public class QuestionHandler
    {
        private static QuestionHandler _instance;

        public static QuestionHandler Instance {
            get {
                if (_instance == null)
                {
                    _instance = new QuestionHandler();                    
                }                
                return _instance;
                
            }                
        }

        public dynamic[,] GameBoard { get; set; }

        
    }
}
