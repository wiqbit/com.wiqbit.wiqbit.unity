using System.Collections.Generic;
using System.Threading;

namespace Assets.Scripts.Chess
{
	public class Chess
	{
		public Chess()
		{
		}

		public string GetMove(string fen)
		{
			Position position = new Position(fen);

			string result = position.GetMove();

			return result;
		}

		public Dictionary<string, List<string>> GetMoves(string fen)
		{
			Position position = new Position(fen);

			Dictionary<string, List<string>> result = position.GetMoves();

			return result;
		}
	}
}