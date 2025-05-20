using System;


namespace Oscillite.CleanRoom.LSM
{
	
public class Cursor
    {
        public int Id { get; set; }
        public bool Enabled { get; set; } = false;
        public Position<double> Position { get; set; }
    }
}
