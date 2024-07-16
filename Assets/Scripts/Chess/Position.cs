using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Chess
{
	internal class Position
	{
		private char[,] _squares = new char[8, 8];
		private string _activeColor;
		private bool _whiteCanCastleKingside = false;
		private bool _whiteCanCastleQueenside = false;
		private bool _blackCanCastleKingside = false;
		private bool _blackCanCastleQueenside = false;
		private string _possibleEnPassantTarget = string.Empty;
		private char _from;
		private char _to;

		public Position(string fen) 
		{
			string[] fenParts = fen.Split(" ");

			// piece placement
			string piecePlacement = fenParts[0];

			int piecePlacementIndex = 0;

			for (int rank = 7; rank >= 0; rank--)
			{
				for (int file = 0; file < 8; file++)
				{
					_squares[rank, file] = piecePlacement[piecePlacementIndex];

					piecePlacementIndex++;
				}

				piecePlacementIndex++; // skip /
			}

			// active color
			string activeColor = fenParts[1];

			_activeColor = activeColor;

			// castling rights
			string castlineRights = fenParts[2];

			_whiteCanCastleKingside = castlineRights.IndexOf("K") >= 0;
			_whiteCanCastleQueenside = castlineRights.IndexOf("Q") >= 0;
			_blackCanCastleKingside = castlineRights.IndexOf("k") >= 0;
			_blackCanCastleQueenside = castlineRights.IndexOf("q") >= 0;

			// possible en passant target
			string possibleEnPassantTarget = fenParts[3];

			_possibleEnPassantTarget = possibleEnPassantTarget;
		}

		public int Evaluate()
		{
			int result = 0;

			for (int rank = 0; rank < 8; rank++)
			{
				for (int file = 0; file < 8; file++)
				{
					char square = _squares[rank, file];

					if ("KQRNBP".IndexOf(square) >= 0)
					{
						result -= GetPieceValue(square);
						result -= PieceSquareTables.Scores[square][rank, file];
					}
					else if ("kqrnbp".IndexOf(square) >= 0)
					{
						result += GetPieceValue(square);
						result += PieceSquareTables.Scores[square][rank, file];
					}
				}
			}

			//if (string.Compare(color, _whoCaptured) == 0)
			//{
			//	result += _captured;
			//}

			return result;
		}

		public string GetMove()
		{
			int bestValue = int.MinValue;
			string bestMove = string.Empty;
			char bestPiece = ' ';

			Dictionary<string, List<string>> moves = GetMoves();

			string fen = GetFEN();

			foreach (string from in moves.Keys)
			{
				foreach (string to in moves[from])
				{
					Position position = new Position(fen);

					position.MakeMove(from, to);

					int value = Minimax(position, 2, int.MinValue, int.MaxValue, false);

					if (value >= bestValue)
					{
						bestValue = value;
						bestMove = $"{from}{to}";

						(int rank, int file) = GetRankAndFile(to);

						bestPiece = _squares[rank, file];
					}
				}
			}

			if (bestPiece == 'p'
				&& bestMove.StartsWith("8"))
			{
				bestMove += "p";
			}

			return bestMove;
		}

		public Dictionary<string, List<string>> GetMoves()
		{
			Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

			for (int rank = 0; rank < 8; rank++)
			{
				for (int file = 0; file < 8; file++)
				{
					if (string.Compare(_activeColor, "w") == 0)
					{
						switch (_squares[rank, file])
						{
							case 'K':
								GetMovesForKing(rank, file, result);
								break;

							case 'Q':
								GetMovesForQueen(rank, file, result);
								break;

							case 'R':
								GetMovesForRook(rank, file, result);
								break;

							case 'N':
								GetMovesForKnight(rank, file, result);
								break;

							case 'B':
								GetMovesForBishop(rank, file, result);
								break;

							case 'P':
								GetMovesForWhitePawn(rank, file, result);
								break;
						}
					}
					else if (string.Compare(_activeColor, "b") == 0)
					{
						switch (_squares[rank, file])
						{
							case 'k':
								GetMovesForKing(rank, file, result);
								break;

							case 'q':
								GetMovesForQueen(rank, file, result);
								break;

							case 'r':
								GetMovesForRook(rank, file, result);
								break;

							case 'n':
								GetMovesForKnight(rank, file, result);
								break;

							case 'b':
								GetMovesForBishop(rank, file, result);
								break;

							case 'p':
								GetMovesForBlackPawn(rank, file, result);
								break;
						}
					}
				}
			}

			return result;
		}

		// helpers

		private void AddMove(int fromRank, int fromFile, int toRank, int toFile, Dictionary<string, List<string>> result)
		{
			string from = GetAlgebraicNotation(fromRank, fromFile);
			string to = GetAlgebraicNotation(toRank, toFile);

			if (!result.ContainsKey(from))
			{
				result.Add(from, new List<string>());
			}

			result[from].Add(to);
		}

		private bool CanMoveTo(char from, char to, out bool stop, bool vertical = false, bool diagonal = false)
		{
			stop = false;

			if (to == '1')
			{
				if ((from == 'P' || from == 'p')
					&& diagonal)
				{
					return false;
				}

				return true;
			}

			if (to == 'K'
				|| to == 'k')
			{
				return false;
			}

			char fromColor = from == 'K' || from == 'Q' || from == 'R' || from == 'N' || from == 'B' || from == 'P' ? 'w' : 'b';
			char toColor = to == 'K' || to == 'Q' || to == 'R' || to == 'N' || to == 'B' || to == 'P' ? 'w' : 'b';
			
			if (fromColor == toColor)
			{
				return false;
			}

			if ((from == 'P' || from == 'p')
				&& vertical)
			{
				return false;
			}

			stop = true;

			return true;
		}

		private string GetAlgebraicNotation(int rank, int file)
		{
			string result = $"{"abcdefgh".Substring(file, 1)}{(rank + 1)}";

			return result;
		}

		private string GetFEN()
		{
			StringBuilder result = new StringBuilder();

			// piece placement
			for (int rank = 7; rank >= 0; rank--)
			{
				for (int file = 0; file < 8; file++)
				{
					result.Append(_squares[rank, file]);
				}

				if (rank != 0)
				{
					result.Append("/");
				}
			}

			// active color
			result.Append($" {_activeColor}");

			// castling rights
			string castlingRights = "";

			if (_whiteCanCastleKingside)
			{
				castlingRights += "K";
			}

			if (_whiteCanCastleQueenside)
			{
				castlingRights += "Q";
			}

			if (_blackCanCastleKingside)
			{
				castlingRights += "k";
			}

			if (_blackCanCastleQueenside)
			{
				castlingRights += "q";
			}

			if (string.IsNullOrEmpty(castlingRights))
			{
				castlingRights = "-";
			}

			result.Append($" {castlingRights}");

			// possible en passant target
			result.Append($" {_possibleEnPassantTarget}");

			return result.ToString();
		}

		private (int rank, int file) GetKing()
		{
			int rank = 0;
			int file = 0;
			char piece = string.Compare(_activeColor, "w") == 0 ? 'K' : 'k';
			bool found = false;

			for (rank = 0; rank < 8; rank++)
			{
				for (file = 0; file < 8; file++)
				{
					if (_squares[rank, file] == piece)
					{
						found = true;
						break;
					}
				}

				if (found)
				{
					break;
				}
			}

			return (rank, file);
		}

		private int GetPieceValue(char piece)
		{
			switch (piece.ToString().ToUpper())
			{
				case "Q":
					return 900;

				case "R":
					return 500;

				case "N":
				case "B":
					return 300;

				case "P":
					return 100;

				default:
					return 0;
			}
		}

		private (int rank, int file) GetRankAndFile(string algebraicNotation)
		{
			int rank = int.Parse(algebraicNotation[1].ToString()) - 1;
			int file = "abcdefgh".IndexOf(algebraicNotation[0]);

			return (rank, file);
		}

		private bool InCheck(int rankFrom, int fileFrom, int rankTo, int fileTo)
		{
			bool result = false;
			int r = 0;
			int f = 0;

			char pieceFrom = _squares[rankFrom, fileFrom];
			char pieceTo = _squares[rankTo, fileTo];

			_squares[rankFrom, fileFrom] = '1';
			_squares[rankTo, fileTo] = pieceFrom;

			(int rank, int file) = GetKing();
			char piece = _squares[rank, file];

			List<char> verticalChecks = new List<char>();
			List<char> diagonalChecks = new List<char>();
			List<char> horizontalChecks = new List<char>();
			List<char> knightChecks = new List<char>();

			if (string.Compare(_activeColor, "w") == 0)
			{
				verticalChecks.AddRange(new[] { 'q', 'r' });
				diagonalChecks.AddRange(new[] { 'q', 'b', 'p' });
				horizontalChecks.AddRange(new[] { 'q', 'r' });
				knightChecks.AddRange(new[] { 'n' });
			}
			else if (string.Compare(_activeColor, "b") == 0)
			{
				verticalChecks.AddRange(new[] { 'Q', 'R' });
				diagonalChecks.AddRange(new[] { 'Q', 'B', 'P' });
				horizontalChecks.AddRange(new[] { 'Q', 'R' });
				knightChecks.AddRange(new[] { 'N' });
			}

			// north
			for (r = rank + 1; r < 8; r++)
			{
				if (_squares[r, file] != '1')
				{
					if (verticalChecks.Contains(_squares[r, file]))
					{
						result = true;
					}

					break;
				}
			}

			// north east
			r = rank + 1;
			f = file + 1;

			while (r < 8
				&& f < 8)
			{
				if (_squares[r, f] != '1')
				{
					if (diagonalChecks.Contains(_squares[r, f]))
					{
						if (piece == 'k'
							&& _squares[r, f] == 'P')
						{
							// black king can only be checked by white pawns south east and south west
						}
						else if (piece == 'K'
							&& _squares[r, f] == 'p')
						{
							// white king can only be checked by black pawns north east and north west
							result = r == rank + 1
								&& f == file + 1;
						}
						else
						{
							result = true;
						}
					}

					break;
				}

				r++;
				f++;
			}
			
			// east
			for (f = file + 1; f < 8; f++)
			{
				if (_squares[rank, f] != '1')
				{
					if (horizontalChecks.Contains(_squares[rank, f]))
					{
						result = true;
					}

					break;
				}
			}

			// south east
			r = rank - 1;
			f = file + 1;

            while (r >= 0
				&& f < 8)
            {
				if (_squares[r, f] != '1')
				{
					if (diagonalChecks.Contains(_squares[r, f]))
					{
						if (piece == 'K'
							&& _squares[r, f] == 'p')
						{
							// white king can only be checked by black pawns north east and north west
						}
						else if (piece == 'k'
							&& _squares[r, f] == 'P')
						{
							// black king can only be checked by white pawns south east and south west
							result = r == rank - 1
								&& f == file + 1;
						}
						else
						{
							result = true;
						}
					}

					break;
				}

				r--;
				f++;
            }

			// south
			for (r = rank - 1; r >= 0; r--)
			{
				if (_squares[r, file] != '1')
				{
					if (verticalChecks.Contains(_squares[r, file]))
					{
						result = true;
					}

					break;
				}
			}

			// south west
			r = rank - 1;
			f = file - 1;

			while (r >= 0
				&& f >= 0)
			{
				if (_squares[r, f] != '1')
				{
					if (diagonalChecks.Contains(_squares[r, f]))
					{
						if (piece == 'K'
							&& _squares[r, f] == 'p')
						{
							// white king can only be checked by black pawns north east and north west
						}
						else if (piece == 'k'
							&& _squares[r, f] == 'P')
						{
							// black king can only be checked by white pawns south east and south west
							result = r == rank - 1
								&& f == file - 1;
						}
						else
						{
							result = true;
						}
					}

					break;
				}

				r--;
				f--;
			}

			// west
			for (f = file - 1; f >= 0; f--)
			{
				if (_squares[rank, f] != '1')
				{
					if (horizontalChecks.Contains(_squares[rank, f]))
					{
						result = true;
					}

					break;
				}
			}

			// north west
			r = rank + 1;
			f = file - 1;

			while (r < 8
				&& f >= 0)
			{
				if (_squares[r, f] != '1')
				{
					if (diagonalChecks.Contains(_squares[r, f]))
					{
						if (piece == 'k'
							&& _squares[r, f] == 'P')
						{
							// black king can only be checked by white pawns south east and south west
						}
						else if (piece == 'K'
							&& _squares[r, f] == 'p')
						{
							// white king can only be checked by black pawns north east and north west
							result = r == rank + 1
								&& f == file - 1;
						}
						else
						{
							result = true;
						}
					}

					break;
				}

				r++;
				f--;
			}

			// knights
			List<(int rank, int file)> coordinates = new List<(int rank, int file)>();

			coordinates.Add((rank + 2, file + 1));
			coordinates.Add((rank + 1, file + 2));
			coordinates.Add((rank - 1, file + 2));
			coordinates.Add((rank - 2, file + 1));

			coordinates.Add((rank - 2, file - 1));
			coordinates.Add((rank - 1, file - 2));
			coordinates.Add((rank + 1, file - 2));
			coordinates.Add((rank + 2, file - 1));

			foreach ((int rank, int file) coordinate in coordinates)
			{
				if (coordinate.rank >= 0
					&& coordinate.rank < 8
					&& coordinate.file >= 0
					&& coordinate.file < 8
					&& knightChecks.Contains(_squares[coordinate.rank, coordinate.file]))
				{
					result = true;
				}
			}

			_squares[rankFrom, fileFrom] = pieceFrom;
			_squares[rankTo, fileTo] = pieceTo;

			return result;
		}

		private void MakeMove(string from, string to)
		{
			(int rankFrom, int fileFrom) = GetRankAndFile(from);
			(int rankTo, int fileTo) = GetRankAndFile(to);

			_from = _squares[rankFrom, fileFrom];
			_to = _squares[rankTo, fileTo];

			_squares[rankTo, fileTo] = _from;
			_squares[rankFrom, fileFrom] = '1';

			_activeColor = string.Compare(_activeColor, "w") == 0 ? "b" : "w";
		}

		private int Minimax(Position position, int depth, int alpha, int beta, bool isMaximizingPlayer)
		{
			if (depth == 0)
			{
				int result = position.Evaluate();

				return result;
			}

			if (isMaximizingPlayer)
			{
				int bestValue = int.MinValue;

				Dictionary<string, List<string>> moves = position.GetMoves();

				string fen = position.GetFEN();

				foreach (string from in moves.Keys)
				{
					foreach (string to in moves[from])
					{
						Position newPosition = new Position(fen);

						newPosition.MakeMove(from, to);

						int value = Minimax(newPosition, depth - 1, alpha, beta, false);

						bestValue = Math.Max(value, bestValue);

						alpha = Math.Max(alpha, value);

						if (beta <= alpha)
						{
							break;
						}
					}
				}

				return bestValue;
			}
			else
			{
				int bestValue = int.MaxValue;

				Dictionary<string, List<string>> moves = position.GetMoves();

				string fen = position.GetFEN();

				foreach (string from in moves.Keys)
				{
					foreach (string to in moves[from])
					{
						Position newPosition = new Position(fen);

						newPosition.MakeMove(from, to);

						// todo: position or newPosition?
						int value = Minimax(newPosition, depth - 1, alpha, beta, true);

						bestValue = Math.Min(value, bestValue);

						beta = Math.Min(beta, value);

						if (beta <= alpha)
						{
							break;
						}
					}
				}

				return bestValue;
			}
		}

		private void UndoMove(string from, string to)
		{
			(int rankFrom, int fileFrom) = GetRankAndFile(from);
			(int rankTo, int fileTo) = GetRankAndFile(to);

			_squares[rankTo, fileTo] = _to;
			_squares[rankFrom, fileFrom] = _from;

			_activeColor = string.Compare(_activeColor, "w") == 0 ? "b" : "w";
		}

		// pieces

		private void GetMovesForKing(int rank, int file, Dictionary<string, List<string>> moves)
		{
			GetMovesForNorth(rank, file, moves, 1);
			GetMovesForNorthEast(rank, file, moves, 1);

			int eastLimit = 1;

			if (string.Compare(_activeColor, "w") == 0
				&& _whiteCanCastleKingside)
			{
				eastLimit++;
			}
			else if (string.Compare(_activeColor, "b") == 0
				&& _blackCanCastleKingside)
			{
				eastLimit++;
			}

			GetMovesForEast(rank, file, moves, eastLimit);
			GetMovesForSouthEast(rank, file, moves, 1);
			GetMovesForSouth(rank, file, moves, 1);
			GetMovesForSouthWest(rank, file, moves, 1);

			int westLimit = 1;

			if (string.Compare(_activeColor, "w") == 0
				&& _whiteCanCastleQueenside)
			{
				westLimit++;
			}
			else if (string.Compare(_activeColor, "b") == 0
				&& _blackCanCastleQueenside)
			{
				westLimit++;
			}

			GetMovesForWest(rank, file, moves, westLimit);
			GetMovesForNorthWest(rank, file, moves, 1);
		}

		private void GetMovesForQueen(int rank, int file, Dictionary<string, List<string>> moves)
		{
			GetMovesForNorth(rank, file, moves);
			GetMovesForNorthEast(rank, file, moves);
			GetMovesForEast(rank, file, moves);
			GetMovesForSouthEast(rank, file, moves);
			GetMovesForSouth(rank, file, moves);
			GetMovesForSouthWest(rank, file, moves);
			GetMovesForWest(rank, file, moves);
			GetMovesForNorthWest(rank, file, moves);
		}

		private void GetMovesForRook(int rank, int file, Dictionary<string, List<string>> moves)
		{
			GetMovesForNorth(rank, file, moves);
			GetMovesForEast(rank, file, moves);
			GetMovesForSouth(rank, file, moves);
			GetMovesForWest(rank, file, moves);
		}

		private void GetMovesForKnight(int rank, int file, Dictionary<string, List<string>> moves)
		{
			List<(int rank, int file)> coordinates = new List<(int, int)>();

			coordinates.Add((rank + 2, file + 1));
			coordinates.Add((rank + 1, file + 2));
			coordinates.Add((rank - 1, file + 2));
			coordinates.Add((rank - 2, file + 1));

			coordinates.Add((rank - 2, file - 1));
			coordinates.Add((rank - 1, file - 2));
			coordinates.Add((rank + 1, file - 2));
			coordinates.Add((rank + 2, file - 1));

			foreach ((int rank, int file) coordinate in coordinates)
			{
				if (coordinate.rank >= 0
					&& coordinate.rank < 8
					&& coordinate.file >= 0
					&& coordinate.file < 8)
				{
					bool stop;

					if (CanMoveTo(_squares[rank, file], _squares[coordinate.rank, coordinate.file], out stop))
					{
						if (!InCheck(rank, file, coordinate.rank, coordinate.file))
						{
							AddMove(rank, file, coordinate.rank, coordinate.file, moves);
						}
					}
				}
			}
		}

		private void GetMovesForBishop(int rank, int file, Dictionary<string, List<string>> moves)
		{
			GetMovesForNorthEast(rank, file, moves);
			GetMovesForSouthEast(rank, file, moves);
			GetMovesForSouthWest(rank, file, moves);
			GetMovesForNorthWest(rank, file, moves);
		}

		private void GetMovesForWhitePawn(int rank, int file, Dictionary<string, List<string>> moves)
		{
			GetMovesForNorth(rank, file, moves, rank == 1 ? 2 : 1);
			GetMovesForNorthEast(rank, file, moves, 1);
			GetMovesForNorthWest(rank, file, moves, 1);

			// en passant
			if (string.Compare(_possibleEnPassantTarget, "-") != 0
				&& _possibleEnPassantTarget.EndsWith("6"))
			{
				(int r, int f) = GetRankAndFile(_possibleEnPassantTarget);
				
				if (r - 1 == rank
					&& f + 1 == file)
				{
					AddMove(rank, file, r, f, moves);
				}
				else if (r - 1 == rank
					&& f - 1 == file)
				{
					AddMove(rank, file, r, f, moves);
				}
			}
		}

		private void GetMovesForBlackPawn(int rank, int file, Dictionary<string, List<string>> moves)
		{
			GetMovesForSouth(rank, file, moves, rank == 6 ? 2 : 1);
			GetMovesForSouthEast(rank, file, moves, 1);
			GetMovesForSouthWest(rank, file, moves, 1);

			// en passant
			if (string.Compare(_possibleEnPassantTarget, "-") != 0
				&& _possibleEnPassantTarget.EndsWith("3"))
			{
				(int r, int f) = GetRankAndFile(_possibleEnPassantTarget);

				if (r + 1 == rank
					&& f + 1 == file)
				{
					AddMove(rank, file, r, f, moves);
				}
				else if (r + 1 == rank
					&& f - 1 == file)
				{
					AddMove(rank, file, r, f, moves);
				}
			}
		}

		// directions

		private void GetMovesForNorth(int rank, int file, Dictionary<string, List<string>> moves, int limit = int.MaxValue)
		{
			int count = 0;
			bool stop;

			for (int r = rank + 1; r < 8; r++)
			{
				if (CanMoveTo(_squares[rank, file], _squares[r, file], out stop, true))
				{
					if (!InCheck(rank, file, r, file))
					{
						AddMove(rank, file, r, file, moves);
					}
				}
				else
				{
					break;
				}

				count++;

				if (count == limit 
					|| stop)
				{
					break;
				}
			}
		}

		private void GetMovesForNorthEast(int rank, int file, Dictionary<string, List<string>> moves, int limit = int.MaxValue)
		{
			int count = 0;
			bool stop;
			int r = rank + 1;
			int f = file + 1;

			while (r < 8
				&& f < 8)
			{
				if (CanMoveTo(_squares[rank, file], _squares[r, f], out stop, false, true))
				{
					if (!InCheck(rank, file, r, f))
					{
						AddMove(rank, file, r, f, moves);
					}
				}
				else
				{
					break;
				}

				count++;
				r++;
				f++;

				if (count == limit 
					|| stop)
				{
					break;
				}
			}
		}

		private void GetMovesForEast(int rank, int file, Dictionary<string, List<string>> moves, int limit = int.MaxValue)
		{
			int count = 0;
			bool stop;

			for (int f = file + 1; f < 8; f++)
			{
				if (CanMoveTo(_squares[rank, file], _squares[rank, f], out stop))
				{
					if (!InCheck(rank, file, rank, f))
					{
						AddMove(rank, file, rank, f, moves);
					}
				}
				else
				{
					break;
				}

				count++;

				if (count == limit
					|| stop)
				{
					break;
				}
			}
		}

		private void GetMovesForSouthEast(int rank, int file, Dictionary<string, List<string>> moves, int limit = int.MaxValue)
		{
			int count = 0;
			bool stop;
			int r = rank - 1;
			int f = file + 1;

			while (r >= 0
				&& f < 8)
			{
				if (CanMoveTo(_squares[rank, file], _squares[r, f], out stop, false, true))
				{
					if (!InCheck(rank, file, r, f))
					{
						AddMove(rank, file, r, f, moves);
					}
				}
				else
				{
					break;
				}

				count++;
				r--;
				f++;

				if (count == limit
					|| stop)
				{
					break;
				}
			}
		}

		private void GetMovesForSouth(int rank, int file, Dictionary<string, List<string>> moves, int limit = int.MaxValue)
		{
			int count = 0;
			bool stop;

			for (int r = rank - 1; r >= 0; r--)
			{
				if (CanMoveTo(_squares[rank, file], _squares[r, file], out stop, true))
				{
					if (!InCheck(rank, file, r, file))
					{
						AddMove(rank, file, r, file, moves);
					}
				}
				else
				{
					break;
				}

				count++;

				if (count == limit 
					|| stop)
				{
					break;
				}
			}
		}

		private void GetMovesForSouthWest(int rank, int file, Dictionary<string, List<string>> moves, int limit = int.MaxValue)
		{
			int count = 0;
			bool stop;
			int r = rank - 1;
			int f = file - 1;

			while (r >= 0
				&& f >= 0)
			{
				if (CanMoveTo(_squares[rank, file], _squares[r, f], out stop, false, true))
				{
					if (!InCheck(rank, file, r, f))
					{
						AddMove(rank, file, r, f, moves);
					}
				}
				else
				{
					break;
				}

				count++;
				r--;
				f--;

				if (count == limit 
					|| stop)
				{
					break;
				}
			}
		}

		private void GetMovesForWest(int rank, int file, Dictionary<string, List<string>> moves, int limit = int.MaxValue)
		{
			int count = 0;
			bool stop;

			for (int f = file - 1; f >= 0; f--)
			{
				if (CanMoveTo(_squares[rank, file], _squares[rank, f], out stop))
				{
					if (!InCheck(rank, file, rank, f))
					{
						AddMove(rank, file, rank, f, moves);
					}
				}
				else
				{
					break;
				}

				count++;
				
				if (count == limit
					|| stop)
				{
					break;
				}
			}
		}

		private void GetMovesForNorthWest(int rank, int file, Dictionary<string, List<string>> moves, int limit = int.MaxValue)
		{
			int count = 0;
			bool stop;
			int r = rank + 1;
			int f = file - 1;

			while (r < 8
				&& f >= 0)
			{
				if (CanMoveTo(_squares[rank, file], _squares[r, f], out stop, false, true))
				{
					if (!InCheck(rank, file, r, f))
					{
						AddMove(rank, file, r, f, moves);
					}
				}
                else
                {
					break;
                }

				count++;
				r++;
				f--;

				if (count == limit
					|| stop)
				{
					break;
				}
			}
		}

	}
}