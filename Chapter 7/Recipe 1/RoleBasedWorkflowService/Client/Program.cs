using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Client.Proxy;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceClient client = new ServiceClient();
            Console.WriteLine(client.GetData(10));
            Console.ReadLine();
        }
    }
}
