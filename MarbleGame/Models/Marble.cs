using System;

namespace MarbleGame.Models
{
    public class Marble : BoardObject
    {
        public bool IsInHole { get; set; }

        public void Move(Sides liftedSide)
        {
            switch (liftedSide)
            {
                case Sides.East:
                    Position.Column--;
                    break;
                case Sides.West:
                    Position.Column++;
                    break;
                case Sides.North:
                    Position.Row++;
                    break;
                case Sides.South:
                    Position.Row--;
                    break;
                default:
                    throw new Exception("Board side not found!");
            }
        }
    }
}