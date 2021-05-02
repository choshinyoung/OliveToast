using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OliveToast.Managements
{
    public class PingPongResult
    {
        public Response response { get; set; }
        public string version { get; set; }

        public class Response
        {
            public List<Reply> replies { get; set; }

            public class Reply
            {
                public From from { get; set; }
                public string type { get; set; }
                public string text { get; set; }
                public Image image { get; set; }

                public class From
                {
                    public double score { get; set; }
                    public string name { get; set; }
                    public string link { get; set; }
                    public string from { get; set; }
                }

                public class Image
                {
                    public string url { get; set; }
                }
            }
        }
    }
}
