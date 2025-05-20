using System;
using System.Collections.Generic;

namespace Oscillite.CleanRoom.LSM
{
	
	public class CursorList : List<Cursor>
	{
        private int TOTAL_CURSORS = 2;

        public CursorList()
        {
            CreateDefaultCursors();
        }

        private void CreateDefaultCursors()
        {
            for (int i = 0; i < TOTAL_CURSORS; i++)
            {
                Add(new Cursor
                {
                    Id = i + 1,
                    Position = new Position<double>(0.0)
                });
            }
        }
    }
}
