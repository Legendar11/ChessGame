using CoreGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessFull
{
    class Program
    {
        static void Main(string[] args)
        {
            GamePoint point1 = new GamePoint() { X = 5, Y = 7 };
            
            Console.WriteLine(point1.X);

            Console.ReadKey();
        }
    }
}
