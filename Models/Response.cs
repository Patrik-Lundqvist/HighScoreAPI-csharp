using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighscoreAPI
{
    public class Response<T>
    {
        public bool isSuccess { get; internal set; }

        public string Data { get; set; }

        public T DataObject { get; set; }

        public static Response<T> Success()
        {
            return new Response<T> { isSuccess = true};
        }

        public static Response<T> Error()
        {
            return new Response<T> { isSuccess = false };
        }  
    }
}
