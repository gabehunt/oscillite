using System;


namespace Oscillite.CleanRoom.LSM
{
	
	public class Position<T>
	{
		private T position;
		
		public T Value
		{
			get
			{
				return position;
			}
			set
			{
				position = value;
			}
		}

		public Position(T position)
		{
			this.position = position;
		}
	}
}
