using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Client.ClaimsEnabledWcfServiceProxy;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Service1Client client = new Service1Client();
            Console.WriteLine(client.GetData(10));
            Console.ReadLine();
        }
    }
}
