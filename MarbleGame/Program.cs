using MarbleGame.Models;
using System;

namespace MarbleGame
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var board = new Board();
                Console.WriteLine("Please enter 'N M W' params");
                board.InitializeBasicParams();

                for (int i = 0; !(board.Size == 0 && board.MarblesCount == 0 && board.WallsCount == 0); i++)
                {
                    //Initialize marbles
                    Console.WriteLine("Please enter marbles' positions");
                    board.InitializeBoardObjects(BoardObjectTypes.Marbel);

                    //Initialize holes
                    Console.WriteLine("Please enter holes' positions");
                    board.InitializeBoardObjects(BoardObjectTypes.Hole);

                    //Initialize walls
                    Console.WriteLine("Please enter walls' positions");
                    board.InitializeBoardObjects();

                    //check for possible solution
                    var solution = board.FindRoute(string.Empty);
                    Console.WriteLine("Case {0}: {1}\n", i + 1, solution.Success ? solution.Route.Length + " moves " + solution.Route : "impossible");

                    //try again
                    Console.WriteLine("Please enter 'N M W' params");
                    board = new Board();
                    board.InitializeBasicParams();
                }

                Console.WriteLine("You finished the test!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}