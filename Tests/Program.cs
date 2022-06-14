using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpJSON;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            JSONDecoder Dec = new JSONDecoder("{\"name\": \"centipede\",\"type\": \"insect\",\"body\": {\"legs\": 60,\"length\": {\"min\": 20,\"max\": 60}},\"species\": [\"there\",\"are\",\"many\"]}");
            string MaxLen = Dec["body"]["length"]["max"].Text;
            Console.WriteLine(MaxLen.Trim());

            foreach(JSONElement E in Dec["species"])
            {
                Console.WriteLine(E.Text.Trim());
            }
            int x = 0;
        }
    }
}
