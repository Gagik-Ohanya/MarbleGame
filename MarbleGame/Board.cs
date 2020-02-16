using MarbleGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarbleGame
{
    public class Board
    {
        public int Size { get; set; }
        public int MarblesCount { get; set; }
        public int WallsCount { get; set; }
        public Marble[] Marbles { get; set; }
        public Hole[] Holes { get; set; }
        public WallPosition[] Walls { get; set; }

        //initializes N M W
        public void InitializeBasicParams()
        {
            string[] values = ReadValues();
            if (values.Length < 3)
                throw new Exception("Error! You should have entered 3 positive numbers.");

            Size = Convert.ToInt32(values[0]);
            MarblesCount = Convert.ToInt32(values[1]);
            WallsCount = Convert.ToInt32(values[2]);
            Marbles = new Marble[MarblesCount];
            Holes = new Hole[MarblesCount];
            Walls = new WallPosition[WallsCount];
        }

        //initializes marbles/holes
        public void InitializeBoardObjects(BoardObjectTypes objectType)
        {
            for (int i = 0; i < MarblesCount; i++)
            {
                Console.Write("{0}. ", i + 1);
                string[] values = ReadValues();
                if (values.Length < 2)
                    throw new Exception("Error! You should have entered 2 positive numbers for row and column values of a marble.");

                var boardObject = new BoardObject
                {
                    Number = i + 1,
                    Position = new Position
                    {
                        Row = Convert.ToInt32(values[0]),
                        Column = Convert.ToInt32(values[1])
                    }
                };
                if (objectType == BoardObjectTypes.Marbel)
                    Marbles[i] = new Marble { Number = boardObject.Number, Position = boardObject.Position };
                else if (objectType == BoardObjectTypes.Hole)
                    Holes[i] = new Hole { Number = boardObject.Number, Position = boardObject.Position, IsFilled = false };
            }
        }

        //initializes walls
        public void InitializeBoardObjects()
        {
            for (int i = 0; i < WallsCount; i++)
            {
                Console.Write("{0}. ", i + 1);
                string[] values = ReadValues();
                if (values.Length < 4)
                    throw new Exception("Error! You should have entered 4 positive numbers for coordinates of two sides of a wall.");

                //swap if wall coordinates entered in wrong order
                int[] intValues = values.Select(v => Convert.ToInt32(v)).ToArray();
                if (intValues[0] == intValues[2])    //vertical wall
                {
                    if (Math.Abs(intValues[1] - intValues[3]) == 1)
                    {
                        if (intValues[1] > intValues[3])
                            Swap(ref intValues[1], ref intValues[3]);
                    }
                    else
                    {
                        throw new Exception("Error! Wall sides should be next to each other.");
                    }
                }
                else if (intValues[1] == intValues[3])    //horizontal wall
                {
                    if (Math.Abs(intValues[0] - intValues[2]) == 1)
                    {
                        if (intValues[0] > intValues[2])
                            Swap(ref intValues[0], ref intValues[2]);
                    }
                    else
                    {
                        throw new Exception("Error! Wall sides should be next to each other.");
                    }
                }
                else
                {
                    throw new Exception("Error! Wall sides should be next to each other");
                }

                var position = new WallPosition
                {
                    FirstSide = new Position
                    {
                        Row = intValues[0],
                        Column = intValues[1]
                    },
                    SecondSide = new Position
                    {
                        Row = intValues[2],
                        Column = intValues[3]
                    }
                };
                Walls[i] = position;
            }
        }

        private string[] ReadValues()
        {
            string positionString = Console.ReadLine();
            return positionString.Split(' ');
        }

        private void Swap(ref int a, ref int b)
        {
            a = a + b;
            b = a - b;
            a = a - b;
        }

        public Solution FindRoute(string moves, bool marblesStateChanged = false)
        {
            //stop the direction if we have a repetition
            if (moves.Length > 7 && moves.Substring(moves.Length - 4) == moves.Substring(moves.Length - 8, 4))
                return new Solution();

            Solution[] routes = new Solution[4] { new Solution(), new Solution(), new Solution(), new Solution() };
            string lastMove = moves.Substring(Math.Max(0, moves.Length - 1));

            //move left / lift East
            if (lastMove != "E" && (lastMove != "W" || marblesStateChanged))
                routes[0] = LiftBoard(Sides.East, moves);

            //move right / lift West
            if (lastMove != "W" && (lastMove != "E" || marblesStateChanged))
                routes[1] = LiftBoard(Sides.West, moves);

            //move down / lift North
            if (lastMove != "N" && (lastMove != "S" || marblesStateChanged))
                routes[2] = LiftBoard(Sides.North, moves);

            //move up / lift South
            if (lastMove != "S" && (lastMove != "N" || marblesStateChanged))
                routes[3] = LiftBoard(Sides.South, moves);


            return routes.Where(r => r.Success).OrderBy(r => r.Route.Length).FirstOrDefault() ?? new Solution();
        }

        private Solution LiftBoard(Sides liftedSide, string moves)
        {
            var newBoardState = new Board
            {
                Size = this.Size,
                Marbles = Marbles.Select(m => new Marble { IsInHole = m.IsInHole, Number = m.Number, Position = new Position { Row = m.Position.Row, Column = m.Position.Column } }).ToArray(),
                Holes = Holes.Select(h => new Hole { IsFilled = h.IsFilled, Number = h.Number, Position = new Position { Row = h.Position.Row, Column = h.Position.Column } }).ToArray(),
                Walls = this.Walls
            };

            DirectionResponse response = newBoardState.MoveMarbles(liftedSide);
            if (response.Success && !newBoardState.BoardStateDidNotChange(response.BoardState))
            {
                moves += liftedSide.ToString()[0];
                if (response.Finished)
                    return new Solution { Success = true, Route = moves };
                else
                {
                    bool marblesStateChanged = newBoardState.Marbles.Count(m => !m.IsInHole) != response.BoardState.Marbles.Count(m => !m.IsInHole);
                    newBoardState.UpdateBoardState(response.BoardState);
                    return newBoardState.FindRoute(moves, marblesStateChanged);
                }
            }
            return new Solution { Route = moves += '*' };
        }

        private DirectionResponse MoveMarbles(Sides liftedSide)
        {
            var newBoardState = new Board
            {
                Size = this.Size,
                Marbles = Marbles.Select(m => new Marble { IsInHole = m.IsInHole, Number = m.Number, Position = new Position { Row = m.Position.Row, Column = m.Position.Column } }).ToArray(),
                Holes = Holes.Select(h => new Hole { IsFilled = h.IsFilled, Number = h.Number, Position = new Position { Row = h.Position.Row, Column = h.Position.Column } }).ToArray(),
                Walls = this.Walls
            };

            var lineMarbles = new List<Marble>();
            for (int i = 0; i < Size; i++)
            {
                lineMarbles = newBoardState.GetMarblesOnTheLine(i, liftedSide);     //in a row or column, depending on the direction
                for (int j = 0; j < lineMarbles.Count; j++)
                {
                    int targetPosition = newBoardState.MarbleTargetPosition(lineMarbles, j, liftedSide);
                    while (MarbleDidNotGetTheDestination(lineMarbles[j].Position, targetPosition, liftedSide) && !lineMarbles[j].IsInHole)  //move till it either doesn't reach the destination or fell in its hole
                    {
                        lineMarbles[j].Move(liftedSide);
                        if (newBoardState.FellInAnotherHole(lineMarbles[j]))    //wrong route, break the move
                            return new DirectionResponse();
                        newBoardState.FillTheHoleIfMarbleIsIn(lineMarbles[j]);
                        if (newBoardState.Marbles.All(m => m.IsInHole))         //solution found
                            return new DirectionResponse { Success = true, Finished = true, BoardState = newBoardState };
                    }
                }
            }
            return new DirectionResponse { Success = true, Finished = false, BoardState = newBoardState };
        }

        //gets the list of marbles located on the row/column depending on the movement direction
        private List<Marble> GetMarblesOnTheLine(int lineIndex, Sides liftedSide)
        {
            switch (liftedSide)
            {
                case Sides.East:
                    return Marbles.Where(m => !m.IsInHole && m.Position.Row == lineIndex).OrderBy(m => m.Position.Column).ToList();
                case Sides.West:
                    return Marbles.Where(m => !m.IsInHole && m.Position.Row == lineIndex).OrderByDescending(m => m.Position.Column).ToList();
                case Sides.North:
                    return Marbles.Where(m => !m.IsInHole && m.Position.Column == lineIndex).OrderByDescending(m => m.Position.Row).ToList();
                case Sides.South:
                    return Marbles.Where(m => !m.IsInHole && m.Position.Column == lineIndex).OrderBy(m => m.Position.Row).ToList();
                default:
                    throw new Exception("Wrong side is lifted!");
            }
        }

        //checks if any marble changed its position after a move
        private bool BoardStateDidNotChange(Board newBoardState)
        {
            Marble newMarble = null;
            foreach (var marble in Marbles.Where(m => !m.IsInHole))
            {
                newMarble = newBoardState.Marbles.First(m => m.Number == marble.Number);
                if (marble.Position.Row != newMarble.Position.Row || marble.Position.Column != newMarble.Position.Column)
                    return false;
            }
            return true;
        }

        //checks if marble gets its intended destination or not
        private bool MarbleDidNotGetTheDestination(Position currentPosition, int target, Sides liftedSide)
        {
            switch (liftedSide)
            {
                case Sides.East:
                    return currentPosition.Column > target;
                case Sides.West:
                    return currentPosition.Column < target;
                case Sides.North:
                    return currentPosition.Row < target;
                case Sides.South:
                    return currentPosition.Row > target;
                default:
                    throw new Exception("Wrong side is lifted!");
            }
        }

        //update board state after move if board state has been changed
        private void UpdateBoardState(Board newBoardState)
        {
            Marble newMarble = null;
            foreach (var marble in Marbles.Where(m => !m.IsInHole))
            {
                newMarble = newBoardState.Marbles.First(m => m.Number == marble.Number);
                marble.IsInHole = newMarble.IsInHole;
                marble.Position.Row = newMarble.Position.Row;
                marble.Position.Column = newMarble.Position.Column;
            }
            Hole newHole = null;
            foreach (var hole in Holes.Where(h => !h.IsFilled))
            {
                newHole = newBoardState.Holes.First(h => h.Number == hole.Number);
                hole.IsFilled = newHole.IsFilled;
            }
        }

        //returns row coordinate (vertical move) or column coordinate (horizontal move)
        private int NearestWallPositionOnTheRoute(Position marblePosition, Sides liftedSide)
        {
            int wallPosition = 0;
            switch (liftedSide)
            {
                case Sides.East:
                    return Walls.Where(w => w.FirstSide.Row == marblePosition.Row && w.SecondSide.Row == marblePosition.Row && marblePosition.Column > w.FirstSide.Column)
                        .OrderBy(w => Math.Abs(w.SecondSide.Column - marblePosition.Column)).Select(w => w.SecondSide.Column).FirstOrDefault();
                case Sides.West:
                    wallPosition = Walls.Where(w => w.FirstSide.Row == marblePosition.Row && w.SecondSide.Row == marblePosition.Row && marblePosition.Column <= w.FirstSide.Column)
                        .OrderBy(w => Math.Abs(w.FirstSide.Column - marblePosition.Column)).Select(w => w.FirstSide.Column).FirstOrDefault();
                    return wallPosition > 0 ? wallPosition : Size - 1;
                case Sides.North:
                    wallPosition = Walls.Where(w => w.FirstSide.Column == marblePosition.Column && w.SecondSide.Column == marblePosition.Column && marblePosition.Row <= w.FirstSide.Row)
                        .OrderBy(w => Math.Abs(w.SecondSide.Row - marblePosition.Row)).Select(w => w.FirstSide.Row).FirstOrDefault();
                    return wallPosition > 0 ? wallPosition : Size - 1;
                case Sides.South:
                    return Walls.Where(w => w.FirstSide.Column == marblePosition.Column && w.SecondSide.Column == marblePosition.Column && marblePosition.Row > w.FirstSide.Row)
                        .OrderBy(w => Math.Abs(w.SecondSide.Row - marblePosition.Row)).Select(w => w.SecondSide.Row).FirstOrDefault();
                default:
                    throw new Exception("Wrong side is lifted!");
            }
        }

        //possible destination of a marble after a move not considering holes on a route
        private int MarbleTargetPosition(List<Marble> lineMarbles, int index, Sides liftedSide)
        {
            int nearestWallPosition = NearestWallPositionOnTheRoute(lineMarbles[index].Position, liftedSide);
            switch (liftedSide)
            {
                case Sides.East:
                    return nearestWallPosition + lineMarbles.Take(index).Count(rm => !rm.IsInHole && rm.Position.Column >= nearestWallPosition);
                case Sides.West:
                    return nearestWallPosition - lineMarbles.Take(index).Count(rm => !rm.IsInHole && rm.Position.Column <= nearestWallPosition);
                case Sides.North:
                    return nearestWallPosition - lineMarbles.Take(index).Count(cm => !cm.IsInHole && cm.Position.Row <= nearestWallPosition);
                case Sides.South:
                    return nearestWallPosition + lineMarbles.Take(index).Count(cm => !cm.IsInHole && cm.Position.Row >= nearestWallPosition);
                default:
                    throw new Exception("Wrong side is lifted!");
            }
        }

        //marble moved to another hole
        private bool FellInAnotherHole(Marble marble)
        {
            return Holes.Any(h => !h.IsFilled && h.Number != marble.Number
                                && h.Position.Row == marble.Position.Row && h.Position.Column == marble.Position.Column);
        }

        //Marble fell in its hole
        private void FillTheHoleIfMarbleIsIn(Marble marble)
        {
            Hole marbleHole = Holes.First(h => h.Number == marble.Number);
            if (marbleHole.Position.Row == marble.Position.Row && marbleHole.Position.Column == marble.Position.Column)
            {
                marbleHole.IsFilled = true;
                marble.IsInHole = true;
            }
        }
    }
}