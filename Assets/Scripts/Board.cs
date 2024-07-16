using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Board
{
	private class Intersect
	{
		public class Sort : IComparer<Intersect>
		{
			public int Compare(Intersect a, Intersect b)
			{
				int result = 0;

				// z = screen left and right
				// x = screen up and down
				float areaA = a.Bounds.size.z * a.Bounds.size.x;
				float areaB = b.Bounds.size.z * b.Bounds.size.x;

				if (areaA > areaB)
				{
					result = -1;
				}
				else if (areaA < areaB)
				{
					result = 1;
				}

				return result;
			}
		}

		public Square Square { get; set; }
		public Bounds Bounds { get; set; }
	}

	public class Square
	{
		public string AlgebraicNotation { get; set; }
		public string ForsythEdwardsNotation { get; set; }
		public GameObject GameObject { get; set; }
		public Material OriginalMaterial { get; set; }
		public GameObject Piece { get; set; }
		public bool PieceIsRightKnight { get; set; }
		public GameObject PieceAlternate { get; set; }
	}

	private const string STARTING_FORSYTH_EDWARDS_NOTATION = "rnbqkbnr/pppppppp/11111111/11111111/11111111/11111111/PPPPPPPP/RNBQKBNR w KQkq -";
	//private const string STARTING_FORSYTH_EDWARDS_NOTATION = "1k111111/1Q111111/P1111111/11111111/11111111/11111111/11111111/K1111111 b KQkq -";

	private List<GameObject> _additionalWhiteQueens;
	private List<GameObject> _additionalWhiteRooks;
	private List<GameObject> _additionalWhiteKnightsRight;
	private List<GameObject> _additionalWhiteKnightsLeft;
	private List<GameObject> _additionalWhiteBishops;
	private List<GameObject> _additionalBlackQueens;
	private List<GameObject> _additionalBlackRooks;
	private List<GameObject> _additionalBlackKnightsRight;
	private List<GameObject> _additionalBlackKnightsLeft;
	private List<GameObject> _additionalBlackBishops;
	private bool _blackCanCastleKingside = true;
	private bool _blackCanCastleQueenside = true;
	private bool _canMove = false;
	private Square _destinationSquare = null;
	private bool _gotMoves = false;
	private bool _madeChessMove = false;
	private Material _materialSquareBad = null;
	private Material _materialSquareGood = null;
	private Dictionary<string, List<string>> _moves = null;
	private string _possibleEnPassantTarget = string.Empty;
	private Square _selectedSquare = null;
	private Square[,] _squares = new Square[8, 8];
	private bool _testing = false;
	private bool _testingMovesArePGN = false;
	private List<string> _testingChessMoves = new List<string>();
	private List<string> _testingWhiteMoves = new List<string>();
	private string _turn = "w";
	private bool _whiteCanCastleKingside = true;
	private bool _whiteCanCastleQueenside = true;

	public Board(bool reloadScene)
	{
		_materialSquareBad = Resources.Load("Materials/square_bad", typeof(Material)) as Material;
		_materialSquareGood = Resources.Load("Materials/square_good", typeof(Material)) as Material;
		_moves = GameSystem.Instance.Chess.GetMoves(STARTING_FORSYTH_EDWARDS_NOTATION);

		string[] piecesRank0 = new string[] { "white_rook_1", "white_knight_1", "white_bishop_1", "white_queen", "white_king", "white_bishop_2", "white_knight_2", "white_rook_2" };
		string[] piecesRank1 = new string[] { "white_pawn_1", "white_pawn_2", "white_pawn_3", "white_pawn_4", "white_pawn_5", "white_pawn_6", "white_pawn_7", "white_pawn_8" };
		string[] piecesRank6 = new string[] { "black_pawn_1", "black_pawn_2", "black_pawn_3", "black_pawn_4", "black_pawn_5", "black_pawn_6", "black_pawn_7", "black_pawn_8" };
		string[] piecesRank7 = new string[] { "black_rook_1", "black_knight_1", "black_bishop_1", "black_queen", "black_king", "black_bishop_2", "black_knight_2", "black_rook_2" };

		if (reloadScene)
		{
			string sceneName = SceneManager.GetActiveScene().name;
			SceneManager.LoadScene(sceneName);
		}

		for (int rank = 0; rank < 8; rank++) // y
		{
			string rankNumber = (rank + 1).ToString();

			for (int file = 0; file < 8; file++) // x
			{
				string fileLetter = "abcdefgh".Substring(file, 1);

				GameObject gameObject = FindGameObject($"square_{fileLetter}{rankNumber}");

				string forsythEdwardsNotation = "1";

				GameObject piece = null;
				GameObject pieceAlternate = null;

				if (rank == 0)
				{
					forsythEdwardsNotation = "RNBQKBNR".Substring(file, 1);
					piece = FindGameObject(piecesRank0[file]);

					if (file == 1)
					{
						pieceAlternate = FindGameObject($"{piecesRank0[file]}_left");
						pieceAlternate.SetActive(false);
					}
					else if (file == 6)
					{
						pieceAlternate = FindGameObject($"{piecesRank0[file]}_right");
						pieceAlternate.SetActive(false);
					}
				}
				else if (rank == 1)
				{
					forsythEdwardsNotation = "P";
					piece = FindGameObject(piecesRank1[file]);
				}
				else if (rank == 6)
				{
					forsythEdwardsNotation = "p";
					piece = FindGameObject(piecesRank6[file]);
				}
				else if (rank == 7)
				{
					forsythEdwardsNotation = "rnbqkbnr".Substring(file, 1);
					piece = FindGameObject(piecesRank7[file]);

					if (file == 1)
					{
						pieceAlternate = FindGameObject($"{piecesRank7[file]}_left");
						pieceAlternate.SetActive(false);
					}
					else if (file == 6)
					{
						pieceAlternate = FindGameObject($"{piecesRank7[file]}_right");
						pieceAlternate.SetActive(false);
					}
				}

				Square square = new Square()
				{
					AlgebraicNotation = $"{fileLetter}{rankNumber}",
					ForsythEdwardsNotation = forsythEdwardsNotation,
					GameObject = gameObject,
					OriginalMaterial = gameObject.transform.GetComponent<MeshRenderer>().material,
					Piece = piece,
					PieceAlternate = pieceAlternate
				};

				_squares[rank, file] = square;
			}
		}

		_additionalWhiteQueens = GetAdditionalPieces("white", "queen");
		_additionalWhiteRooks = GetAdditionalPieces("white", "rook");
		_additionalWhiteKnightsRight = GetAdditionalPieces("white_right", "knight");
		_additionalWhiteKnightsLeft = GetAdditionalPieces("white_left", "knight");
		_additionalWhiteBishops = GetAdditionalPieces("white", "bishop");
		_additionalBlackQueens = GetAdditionalPieces("black", "queen");
		_additionalBlackRooks = GetAdditionalPieces("black", "rook");
		_additionalBlackKnightsRight = GetAdditionalPieces("black_right", "knight");
		_additionalBlackKnightsLeft = GetAdditionalPieces("black_left", "knight");
		_additionalBlackBishops = GetAdditionalPieces("black", "bishop");
	}

	public void Drag(GameObject piece, Vector2 delta, bool isPointerDown)
	{
		if (!_canMove)
		{
			return;
		}

		Vector3 vector3 = new Vector3(-delta.x, delta.y, 0F);

		piece.transform.Translate(vector3);

		List<Intersect> intersects = new List<Intersect>();

		for (int rank = 0; rank < 8; rank++) // y
		{
			for (int file = 0; file < 8; file++) // x
			{
				Square square = _squares[rank, file];

				Intersect intersect = GetIntersect(piece, square);

				if (intersect != null)
				{
					intersects.Add(intersect);
				}
			}
		}

		Intersect.Sort sort = new Intersect.Sort();

		intersects.Sort(sort.Compare);

		// safety check
		_destinationSquare = null;

		if (intersects.Count > 0)
		{
			for (int i = 0; i < intersects.Count; i++)
			{
				MeshRenderer meshRenderer = intersects[i].Square.GameObject.transform.GetComponent<MeshRenderer>();

				if (i == 0)
				{
					if (isPointerDown)
					{
						_selectedSquare = intersects[i].Square;
					}

					_destinationSquare = intersects[i].Square;

					meshRenderer.material = IsNoMove(_destinationSquare)
						|| IsLegalMove(_destinationSquare) ? _materialSquareGood : _materialSquareBad;
				}
				else
				{
					meshRenderer.material = intersects[i].Square.OriginalMaterial;
				}
			}
		}
	}

	public void GotMoves(Dictionary<string, List<string>> moves)
	{
		_gotMoves = true;

		_moves = moves;

		if (string.Compare(_turn, "w") == 0)
		{
			if (_moves.Count == 0)
			{
				GameSystem.Instance.Status = "Black won";
			}

			if (_madeChessMove
				&& _gotMoves
				&& _testing)
			{
				ContinueTesting();
			}
		}
		else if (string.Compare(_turn, "b") == 0)
		{
			if (_moves.Count == 0)
			{
				GameSystem.Instance.Status = "You won";
			}

			_madeChessMove = false;
			_gotMoves = false;

			string forsythEdwardsNotation = GetForsythEdwardsNotation();

			GameSystem.Instance.MakeChessMove(forsythEdwardsNotation);
		}
	}

	public void GotBlackPawnPromotion(string forsythEdwardsNotation)
	{
		List<GameObject> additionalPieces = null;
		List<GameObject> additionalPieceAlternates = null;
		bool pieceIsRightKnight = false;

		switch (forsythEdwardsNotation)
		{
			case "q":
				additionalPieces = _additionalBlackQueens;
				break;

			case "r":
				additionalPieces = _additionalBlackRooks;
				break;

			case "n":
				if ("abcd".IndexOf(_destinationSquare.AlgebraicNotation.Substring(0, 1)) >= 0)
				{
					additionalPieces = _additionalBlackKnightsRight;
					pieceIsRightKnight = true;
					additionalPieceAlternates = _additionalBlackKnightsLeft;
				}
				else
				{
					additionalPieces = _additionalBlackKnightsLeft;
					additionalPieceAlternates = _additionalBlackKnightsRight;
				}
				break;

			case "b":
				additionalPieces = _additionalBlackBishops;
				break;
		}

		GameObject piece = additionalPieces[0];
		additionalPieces.RemoveAt(0);

		GameObject pieceAlternate = null;

		if (additionalPieceAlternates != null)
		{
			pieceAlternate = additionalPieceAlternates[0];
			additionalPieceAlternates.RemoveAt(0);
		}

		ReplacePiece(piece, pieceAlternate);

		_destinationSquare.ForsythEdwardsNotation = forsythEdwardsNotation;

		_destinationSquare.Piece.SetActive(false);

		_destinationSquare.Piece = piece;
		_destinationSquare.PieceIsRightKnight = pieceIsRightKnight;
		_destinationSquare.PieceAlternate = pieceAlternate;
		_destinationSquare.PieceAlternate?.SetActive(false);

		_turn = string.Compare(_turn, "w") == 0 ? "b" : "w";

		if (string.Compare(_turn, "w") == 0)
		{
			GameSystem.Instance.Status = _testing ? "White's move" : "Your move";
		}
		else if (string.Compare(_turn, "b") == 0)
		{
			GameSystem.Instance.Status = "Black's move";
		}

		forsythEdwardsNotation = GetForsythEdwardsNotation();

		GameSystem.Instance.GetMoves(forsythEdwardsNotation);
	}

	public void GotWhitePawnPromotion(string forsythEdwardsNotation)
	{
		List<GameObject> additionalPieces = null;
		List<GameObject> additionalPieceAlternates = null;
		bool pieceIsRightKnight = false;

		switch (forsythEdwardsNotation)
		{
			case "Q":
				additionalPieces = _additionalWhiteQueens;
				break;

			case "R":
				additionalPieces = _additionalWhiteRooks;
				break;

			case "N":
				if ("abcd".IndexOf(_destinationSquare.AlgebraicNotation.Substring(0, 1)) >= 0)
				{
					additionalPieces = _additionalWhiteKnightsRight;
					pieceIsRightKnight = true;
					additionalPieceAlternates = _additionalWhiteKnightsLeft;
				}
				else
				{
					additionalPieces = _additionalWhiteKnightsLeft;
					additionalPieceAlternates = _additionalWhiteKnightsRight;
				}
				break;

			case "B":
				additionalPieces = _additionalWhiteBishops;
				break;
		}

		GameObject piece = additionalPieces[0];
		additionalPieces.RemoveAt(0);

		GameObject pieceAlternate = null;

		if (additionalPieceAlternates != null)
		{
			pieceAlternate = additionalPieceAlternates[0];
			additionalPieceAlternates.RemoveAt(0);
		}

		ReplacePiece(piece, pieceAlternate);

		_destinationSquare.ForsythEdwardsNotation = forsythEdwardsNotation;

		_destinationSquare.Piece.SetActive(false);

		_destinationSquare.Piece = piece;
		_destinationSquare.PieceIsRightKnight = pieceIsRightKnight;
		_destinationSquare.PieceAlternate = pieceAlternate;
		_destinationSquare.PieceAlternate?.SetActive(false);

		_turn = string.Compare(_turn, "w") == 0 ? "b" : "w";

		if (string.Compare(_turn, "w") == 0)
		{
			GameSystem.Instance.Status = _testing ? "White's move" : "Your move";
		}
		else if (string.Compare(_turn, "b") == 0)
		{
			GameSystem.Instance.Status = "Black's move";
		}

		forsythEdwardsNotation = GetForsythEdwardsNotation();

		GameSystem.Instance.GetMoves(forsythEdwardsNotation);
	}

	public void MadeChessMove(string move)
	{
		_madeChessMove = true;

		if (_testing)
		{
			if (_testingChessMoves.Count == 0)
			{
				_testing = false;
				GameSystem.Instance.Status = "Black resigned";
				return;
			}

			move = _testingChessMoves[0];

			if (_testingMovesArePGN)
			{
				move = GetUCIMoveFromPGNMove(move);
			}
		}

		string fromAlgebraicNotation = move.Substring(0, 2);
		Square fromSquare = GetSquareByAlgebraicNotation(fromAlgebraicNotation);

		string toAlgebraicNotation = move.Substring(2, 2);
		Square toSquare = GetSquareByAlgebraicNotation(toAlgebraicNotation);

		string pawnPromotion = string.Empty;
		if (move.Length == 5)
		{
			pawnPromotion = move.Substring(4, 1);
		}

		_canMove = true;
		_selectedSquare = fromSquare;
		_destinationSquare = toSquare;

		PointerUp(fromSquare.Piece, pawnPromotion);
	}

	public void PlayGame(string pgn)
	{
		_testing = true;

		ConvertPGNToTestingMoves(pgn);

		SimulateMove(_testingWhiteMoves[0]);
	}

	public void PointerDown(GameObject piece, Vector2 delta)
	{
		if (_canMove)
		{
			return;
		}

		_canMove = string.Compare(_turn, "w") == 0;

		Drag(piece, delta, true);
	}

	public void PointerUp(GameObject piece, string pawnPromotion = "")
	{
		if (!_canMove)
		{
			return;
		}

		bool isNoMove = false;
		bool isLegalMove = false;
		string lastMove = string.Empty;
		bool whiteCastledQueenside = false;
		bool whiteCastledKingside = false;
		bool blackCastledQueenside = false;
		bool blackCastledKingside = false;
		bool whiteEnPassant = false;
		bool blackEnPassant = false;

		// safety check
		if (_destinationSquare == null)
		{
			for (int rank = 0; rank < 8; rank++) // y
			{
				for (int file = 0; file < 8; file++) // x
				{
					Square square = _squares[rank, file];

					MeshRenderer meshRenderer = square.GameObject.GetComponent<MeshRenderer>();

					meshRenderer.material = square.OriginalMaterial;
				}
			}

			_destinationSquare = _selectedSquare;

			// In 3D space, you can manipulate Transforms on the x-axis, y-axis, and z-axis.
			// In Unity, these axes are represented by the colors red, green, and blue respectively.
			// x = forward (negative) and backward (positive)
			// y = up and down
			// z = left (positive) and right (negative)
			MeshRenderer meshRendererSquare = _destinationSquare.GameObject.GetComponent<MeshRenderer>();
			MeshRenderer meshRendererPiece = piece.GetComponent<MeshRenderer>();

			Bounds boundsSquare = meshRendererSquare.bounds;
			Bounds boundsPiece = meshRendererPiece.bounds;
			float paddingWidth = (boundsSquare.size.x - boundsPiece.size.x) / 2F;
			float paddingDepth = (boundsSquare.size.z - boundsPiece.size.z) / 2F;

			Vector3 targetPosition = meshRendererSquare.transform.position;
			//targetPosition += new Vector3(paddingWidth, 0F, 0F);
			//targetPosition -= new Vector3(0F, paddingDepth, 0F);

			Vector3 vector3 = meshRendererPiece.transform.position - targetPosition;
			meshRendererPiece.transform.Translate(new Vector3(vector3.z, vector3.x, 0F));
		}
		else
		{
			RevertSquareToOriginalMaterial(_destinationSquare);

			isNoMove = IsNoMove(_destinationSquare);
			isLegalMove = IsLegalMove(_destinationSquare);

			if (!isNoMove
				&& !isLegalMove)
			{
				_destinationSquare = _selectedSquare;
			}

			// In 3D space, you can manipulate Transforms on the x-axis, y-axis, and z-axis.
			// In Unity, these axes are represented by the colors red, green, and blue respectively.
			// x = forward (negative) and backward (positive)
			// y = up and down
			// z = left (positive) and right (negative)
			MeshRenderer meshRendererSquare = _destinationSquare.GameObject.GetComponent<MeshRenderer>();
			MeshRenderer meshRendererPiece = piece.GetComponent<MeshRenderer>();

			Bounds boundsSquare = meshRendererSquare.bounds;
			Bounds boundsPiece = meshRendererPiece.bounds;

			Vector3 targetPosition = meshRendererSquare.transform.position;

			// todo: why?
			if (string.Compare(piece.name, "black_queen") == 0)
			{
				float paddingWidth = (boundsSquare.size.x - boundsPiece.size.x);
				float paddingDepth = (boundsSquare.size.z - boundsPiece.size.z);

				targetPosition += new Vector3(paddingWidth, 0F, 0F);
				targetPosition -= new Vector3(0F, paddingDepth, 0F);
			}

			//targetPosition += new Vector3(paddingWidth, -paddingDepth, 0F);
			//targetPosition -= new Vector3(0F, paddingDepth, 0F);

			Vector3 vector3 = meshRendererPiece.transform.position - targetPosition;
			meshRendererPiece.transform.Translate(new Vector3(vector3.z, vector3.x, 0F));

			if (piece == _selectedSquare.Piece
				&& _selectedSquare.PieceAlternate != null)
			{
				MeshRenderer meshRendererPieceAlternate = _selectedSquare.PieceAlternate.GetComponent<MeshRenderer>();

				boundsPiece = meshRendererPieceAlternate.bounds;

				vector3 = meshRendererPieceAlternate.transform.position - targetPosition;
				meshRendererPieceAlternate.transform.Translate(new Vector3(vector3.z, vector3.x, 0F));
			}
			else if (piece == _selectedSquare.PieceAlternate)
			{
				MeshRenderer meshRendererPieceAlternate = _selectedSquare.Piece.GetComponent<MeshRenderer>();

				boundsPiece = meshRendererPieceAlternate.bounds;

				vector3 = meshRendererPieceAlternate.transform.position - targetPosition;
				meshRendererPieceAlternate.transform.Translate(new Vector3(vector3.z, vector3.x, 0F));
			}

			if (isLegalMove)
			{
				lastMove = $"{_selectedSquare.AlgebraicNotation}{_destinationSquare.AlgebraicNotation}";

				_destinationSquare.ForsythEdwardsNotation = _selectedSquare.ForsythEdwardsNotation;

				// castle rights
				if (string.Compare(_selectedSquare.ForsythEdwardsNotation, "K") == 0)
				{
					_whiteCanCastleQueenside = false;
					_whiteCanCastleKingside = false;

					if (string.Compare(_selectedSquare.AlgebraicNotation, "e1") == 0
						&& string.Compare(_destinationSquare.AlgebraicNotation, "c1") == 0)
					{
						whiteCastledQueenside = true;
					}
					else if (string.Compare(_selectedSquare.AlgebraicNotation, "e1") == 0
						&& string.Compare(_destinationSquare.AlgebraicNotation, "g1") == 0)
					{
						whiteCastledKingside = true;
					}
				}
				else if (string.Compare(_selectedSquare.ForsythEdwardsNotation, "R") == 0
					&& string.Compare(_selectedSquare.AlgebraicNotation, "a1") == 0)
				{
					_whiteCanCastleQueenside = false;
				}
				else if (string.Compare(_selectedSquare.ForsythEdwardsNotation, "R") == 0
					&& string.Compare(_selectedSquare.AlgebraicNotation, "h1") == 0)
				{
					_whiteCanCastleKingside = false;
				}
				else if (string.Compare(_selectedSquare.ForsythEdwardsNotation, "k") == 0)
				{
					_blackCanCastleQueenside = false;
					_blackCanCastleKingside = false;

					if (string.Compare(_selectedSquare.AlgebraicNotation, "e8") == 0
						&& string.Compare(_destinationSquare.AlgebraicNotation, "c8") == 0)
					{
						blackCastledQueenside = true;
					}
					else if (string.Compare(_selectedSquare.AlgebraicNotation, "e8") == 0
						&& string.Compare(_destinationSquare.AlgebraicNotation, "g8") == 0)
					{
						blackCastledKingside = true;
					}
				}
				else if (string.Compare(_selectedSquare.ForsythEdwardsNotation, "r") == 0
					&& string.Compare(_selectedSquare.AlgebraicNotation, "a8") == 0)
				{
					_blackCanCastleQueenside = false;
				}
				else if (string.Compare(_selectedSquare.ForsythEdwardsNotation, "r") == 0
					&& string.Compare(_selectedSquare.AlgebraicNotation, "h8") == 0)
				{
					_blackCanCastleKingside = false;
				}

				// en passant?
				if (string.Compare(_selectedSquare.ForsythEdwardsNotation, "P") == 0
					&& _destinationSquare.AlgebraicNotation.EndsWith("6"))
				{
					string enPassantTarget = $"{_destinationSquare.AlgebraicNotation.Substring(0, 1)}6";

					whiteEnPassant = string.Compare(enPassantTarget, _possibleEnPassantTarget) == 0;
				}
				else if (string.Compare(_selectedSquare.ForsythEdwardsNotation, "p") == 0
					&& _destinationSquare.AlgebraicNotation.EndsWith("3"))
				{
					string enPassantTarget = $"{_destinationSquare.AlgebraicNotation.Substring(0, 1)}3";

					blackEnPassant = string.Compare(enPassantTarget, _possibleEnPassantTarget) == 0;
				}

				// possible en passant target
				if (string.Compare(_selectedSquare.ForsythEdwardsNotation, "P") == 0
					&& _selectedSquare.AlgebraicNotation.EndsWith("2")
					&& _destinationSquare.AlgebraicNotation.EndsWith("4"))
				{
					_possibleEnPassantTarget = $"{_selectedSquare.AlgebraicNotation.Substring(0, 1)}3";
				}
				else if (string.Compare(_selectedSquare.ForsythEdwardsNotation, "p") == 0
					&& _selectedSquare.AlgebraicNotation.EndsWith("7")
					&& _destinationSquare.AlgebraicNotation.EndsWith("5"))
				{
					_possibleEnPassantTarget = $"{_selectedSquare.AlgebraicNotation.Substring(0, 1)}6";
				}
				else
				{
					_possibleEnPassantTarget = string.Empty;
				}

				if (_destinationSquare.Piece == null)
				{
					if (whiteCastledKingside
						|| whiteCastledQueenside
						|| blackCastledKingside
						|| blackCastledQueenside)
					{
						GameSystem.Instance.PlayClackClack();
					}
					else if (whiteEnPassant
						|| blackEnPassant)
					{
						GameSystem.Instance.PlaySwishClack();
					}
					else
					{
						GameSystem.Instance.PlayClack();
					}
				}
				else
				{
					GameSystem.Instance.PlaySwishClack();

					_destinationSquare.Piece.SetActive(false);
					_destinationSquare.PieceAlternate?.SetActive(false);
				}

				_destinationSquare.Piece = _selectedSquare.Piece;
				_destinationSquare.PieceIsRightKnight = _selectedSquare.PieceIsRightKnight;
				_destinationSquare.PieceAlternate = _selectedSquare.PieceAlternate;

				if (string.Compare(_destinationSquare.ForsythEdwardsNotation, "N") == 0
					|| string.Compare(_destinationSquare.ForsythEdwardsNotation, "n") == 0)
				{
					if ("abcd".IndexOf(_destinationSquare.AlgebraicNotation.Substring(0, 1)) >= 0)
					{
						// we want a right-facing knight
						if (piece.name.Contains("promoted"))
						{
							if (_destinationSquare.PieceIsRightKnight)
							{
								_destinationSquare.Piece.SetActive(true);
								_destinationSquare.PieceAlternate.SetActive(false);
							}
							else
							{
								_destinationSquare.Piece.SetActive(false);
								_destinationSquare.PieceAlternate.SetActive(true);
							}
						}
						else if (piece.name.EndsWith("knight_1"))
						{
							_destinationSquare.Piece.SetActive(true);
							_destinationSquare.PieceAlternate.SetActive(false);
						}
						else if (piece.name.EndsWith("knight_2"))
						{
							_destinationSquare.Piece.SetActive(false);
							_destinationSquare.PieceAlternate.SetActive(true);
						}
					}
					else
					{
						// we want a left-facing knight
						if (piece.name.Contains("promoted"))
						{
							if (_destinationSquare.PieceIsRightKnight)
							{
								_destinationSquare.Piece.SetActive(false);
								_destinationSquare.PieceAlternate.SetActive(true);
							}
							else
							{
								_destinationSquare.Piece.SetActive(true);
								_destinationSquare.PieceAlternate.SetActive(false);
							}
						}
						else if (piece.name.EndsWith("knight_1"))
						{
							_destinationSquare.Piece.SetActive(false);
							_destinationSquare.PieceAlternate.SetActive(true);
						}
						else if (piece.name.EndsWith("knight_2"))
						{
							_destinationSquare.Piece.SetActive(true);
							_destinationSquare.PieceAlternate.SetActive(false);
						}
					}
				}

				_selectedSquare.ForsythEdwardsNotation = "1";
				_selectedSquare.Piece = null;
				_selectedSquare.PieceIsRightKnight = false;
				_selectedSquare.PieceAlternate = null;
			}
		}

		_canMove = false;

		if (isLegalMove)
		{
			if (string.Compare(_destinationSquare.ForsythEdwardsNotation, "P") == 0
				&& _destinationSquare.AlgebraicNotation.EndsWith("8"))
			{
				// white pawn promotion
				GameSystem.Instance.GetWhitePawnPromotion();
			}
			else if (string.Compare(_destinationSquare.ForsythEdwardsNotation, "p") == 0
				&& _destinationSquare.AlgebraicNotation.EndsWith("1"))
			{
				// black pawn promotion
				GameSystem.Instance.GetBlackPawnPromotion(pawnPromotion);
			}
			else if (whiteCastledQueenside)
			{
				MoveRook("a1d1");
			}
			else if (whiteCastledKingside)
			{
				MoveRook("h1f1");
			}
			else if (blackCastledQueenside)
			{
				MoveRook("a8d8");
			}
			else if (blackCastledKingside)
			{
				MoveRook("h8f8");
			}
			else
			{
				if (whiteEnPassant
					|| blackEnPassant)
				{
					string algebraicNotation = null;

					if (whiteEnPassant)
					{
						algebraicNotation = $"{_destinationSquare.AlgebraicNotation.Substring(0, 1)}5";
					}
					else if (blackEnPassant)
					{
						algebraicNotation = $"{_destinationSquare.AlgebraicNotation.Substring(0, 1)}4";
					}

					Square square = GetSquareByAlgebraicNotation(algebraicNotation);

					square.Piece.SetActive(false);
					square.Piece = null;
					square.PieceAlternate?.SetActive(false);
					square.PieceAlternate = null;
					square.ForsythEdwardsNotation = "1";
				}

				_turn = string.Compare(_turn, "w") == 0 ? "b" : "w";

				if (string.Compare(_turn, "w") == 0)
				{
					GameSystem.Instance.LastMoveBlack = $"Black: {lastMove}";
					GameSystem.Instance.Status = _testing ? "White's move" : "Your move";
				}
				else if (string.Compare(_turn, "b") == 0)
				{
					GameSystem.Instance.LastMoveWhite = $"White: {lastMove}";
					GameSystem.Instance.Status = "Black's move";
				}

				string forsythEdwardsNotation = GetForsythEdwardsNotation();

				GameSystem.Instance.GetMoves(forsythEdwardsNotation);
			}
		}
	}

	public void TestBlackEnPassant()
	{
		_testing = true;

		_testingWhiteMoves = new List<string>()
		{
			"b1c3",
			"g2g3",
			"d2d4"
		};

		_testingChessMoves = new List<string>()
		{
			"e7e5",
			"e5e4",
			"e4d3"
		};

		SimulateMove(_testingWhiteMoves[0]);
	}

	public void TestBlackPawnPromotionKingside()
	{
		_testing = true;

		_testingWhiteMoves = new List<string>()
		{
			"g1f3",
			"h1g1",
			"e2e4",
			"e4e5",
			"e5e6",
			"d2d3",
			"d3d4",
			"d4d5"
		};

		_testingChessMoves = new List<string>()
		{
			"g7g5",
			"g5g4",
			"g4g3",
			"g3h2",
			"h2h1n",
			"h1g3",
			"g3e4",
			"e4c5"
		};

		SimulateMove(_testingWhiteMoves[0]);
	}

	public void TestBlackPawnPromotionQueenside()
	{
		_testing = true;

		_testingWhiteMoves = new List<string>()
		{
			"b1c3",
			"a1b1",
			"d2d4",
			"d4d5",
			"d5d6",
			"e2e3",
			"e3e4",
			"e4e5"
		};

		_testingChessMoves = new List<string>()
		{
			"b7b5",
			"b5b4",
			"b4b3",
			"b3a2",
			"a2a1n",
			"a1b3",
			"b3d4",
			"d4f5p"
		};

		SimulateMove(_testingWhiteMoves[0]);
	}

	public void TestWhiteEnPassant()
	{
		_testing = true;

		_testingWhiteMoves = new List<string>()
		{
			"e2e4",
			"e4e5",
			"e5d6"
		};

		_testingChessMoves = new List<string>()
		{
			"b8c6",
			"d7d5"
		};

		SimulateMove(_testingWhiteMoves[0]);
	}

	public void TestWhitePawnPromotionKingside()
	{
		_testing = true;

		_testingWhiteMoves = new List<string>()
		{
			"g2g4",
			"g4g5",
			"g5g6",
			"g6h7",
			"h7h8"
		};

		_testingChessMoves = new List<string>()
		{
			"g8f6",
			"h8g8",
			"e7e5",
			"e5e4",
			"e4e3"
		};

		SimulateMove(_testingWhiteMoves[0]);
	}

	public void TestWhitePawnPromotionQueenside()
	{
		_testing = true;

		_testingWhiteMoves = new List<string>()
		{
			"b2b4",
			"b4b5",
			"b5b6",
			"b6a7",
			"a7a8"
		};

		_testingChessMoves = new List<string>()
		{
			"b8c6",
			"a8b8",
			"d7d5",
			"d5d4",
			"d4d3"
		};

		SimulateMove(_testingWhiteMoves[0]);
	}

	private void ContinueTesting()
	{
		_testingWhiteMoves.RemoveAt(0);
		_testingChessMoves.RemoveAt(0);

		if (_testingWhiteMoves.Count > 0)
		{
			SimulateMove(_testingWhiteMoves[0]);
		}
		else
		{
			_testing = false;

			if (_moves.Count > 0)
			{
				GameSystem.Instance.Status = "White resigned";
				return;
			}
		}
	}

	private void ConvertPGNToTestingMoves(string pgn)
	{
		string[] moves = Regex.Split(pgn, @"\d+\.");

		for (int i = 0; i < moves.Length; i++)
		{
			string move = moves[i].Trim();
			move = move.Replace("+", "");

			if (string.IsNullOrEmpty(move))
			{
				continue;
			}

			string[] halfMoves = move.Split(" ");

			_testingWhiteMoves.Add(halfMoves[0]);

			if (halfMoves.Length > 1)
			{
				_testingChessMoves.Add(halfMoves[1]);
			}
		}
	}

	private GameObject FindGameObject(string name)
	{
		foreach (GameObject gameObject in GameObject.FindObjectsOfType<GameObject>(true))
		{
			if (string.Compare(gameObject.name, name) == 0)
			{
				return gameObject;
			}
		}

		return null;
	}

	private List<GameObject> GetAdditionalPieces(string color, string piece)
	{
		List<GameObject> result = new List<GameObject>();

		for (int i = 1; i <= 8; i++)
		{
			string name = $"{color}_{piece}_{i}_promoted";

			GameObject gameObject = GameObject.Find(name);

			result.Add(gameObject);
		}

		return result;
	}

	private string GetForsythEdwardsNotation()
	{
		StringBuilder result = new StringBuilder();

		// piece placement
		for (int rank = 7; rank >= 0; rank--) // y
		{
			if (rank < 7)
			{
				result.Append("/");
			}

			for (int file = 0; file < 8; file++) // x
			{
				result.Append(_squares[rank, file].ForsythEdwardsNotation);
			}
		}

		// active color
		result.Append($" {_turn}");

		// castling rights
		string castlingRights = $"{(_whiteCanCastleQueenside ? "Q" : "")}{(_whiteCanCastleKingside ? "K" : "")}{(_blackCanCastleQueenside ? "q" : "")}{(_blackCanCastleKingside ? "k" : "")}";

		if (string.IsNullOrEmpty(castlingRights))
		{
			castlingRights = "-";
		}

		result.Append($" {castlingRights}");

		// possible en passant target
		string possibleEnPassantTarget = _possibleEnPassantTarget;

		if (string.IsNullOrEmpty(possibleEnPassantTarget))
		{
			possibleEnPassantTarget = "-";
		}

		result.Append($" {possibleEnPassantTarget}");

		return result.ToString();
	}

	private Intersect GetIntersect(GameObject piece, Square square)
	{
		Intersect intersect = null;

		Bounds boundsPiece = piece.transform.GetComponent<MeshRenderer>().bounds;
		Bounds boundsSquare = square.GameObject.transform.GetComponent<MeshRenderer>().bounds;

		if (boundsPiece.Intersects(boundsSquare))
		{
			Bounds intersectBounds = new Bounds();

			intersectBounds.SetMinMax(Vector3.Max(boundsPiece.min, boundsSquare.min), Vector3.Min(boundsPiece.max, boundsSquare.max));

			intersect = new Intersect()
			{
				Square = square,
				Bounds = intersectBounds
			};
		}

		return intersect;
	}

	private string GetTwoSquareMoveFromAlgebraicMove(string algebraicMove)
	{
		// Remove check, checkmate, and capture symbols
		algebraicMove = algebraicMove.Replace("+", "").Replace("#", "").Replace("x", "");

		// Handle castling
		if (algebraicMove == "O-O" || algebraicMove == "O-O-O")
		{
			return algebraicMove; // Return castling notation as-is
		}

		// Regular expression to match promotion (e.g., e8=Q)
		Regex promotionRegex = new Regex(@"([a-h][18])=([QRNB])");
		Match promotionMatch = promotionRegex.Match(algebraicMove);
		if (promotionMatch.Success)
		{
			return promotionMatch.Groups[1].Value; // Return just the destination square for promotion
		}

		// Regular expression to match piece moves and disambiguation
		Regex pieceMoveRegex = new Regex(@"([RNBQK])?([a-h])?([1-8])?([a-h][1-8])");
		Match pieceMoveMatch = pieceMoveRegex.Match(algebraicMove);
		if (pieceMoveMatch.Success)
		{
			string piece = pieceMoveMatch.Groups[1].Value; // Piece type (optional)
			string fromFile = pieceMoveMatch.Groups[2].Value; // From file (optional)
			string fromRank = pieceMoveMatch.Groups[3].Value; // From rank (optional)
			string toSquare = pieceMoveMatch.Groups[4].Value; // To square

			// If no fromFile and fromRank are provided, assume a pawn move
			if (string.IsNullOrEmpty(piece) && string.IsNullOrEmpty(fromFile) && string.IsNullOrEmpty(fromRank))
			{
				return $"{toSquare[0]}{(char)(toSquare[1] - 1)}{toSquare}";
			}

			// Handle moves with disambiguation
			if (!string.IsNullOrEmpty(fromFile) || !string.IsNullOrEmpty(fromRank))
			{
				return $"{fromFile}{fromRank}{toSquare}";
			}

			// Handle standard piece moves
			return toSquare;
		}

		// Fallback
		return algebraicMove;
	}

	private string GetUCIMoveFromPGNMove(string pgnMove)
	{
		string result = null;

		// we don't care about check (+) or mate (#)
		pgnMove = pgnMove.Replace("+", string.Empty);
		pgnMove = pgnMove.Replace("#", string.Empty);

		string firstCharacter = pgnMove.Substring(0, 1);

		// <SAN move descriptor piece moves>   ::= <Piece symbol>[<from file>|<from rank>|<from square>]['x']<to square>
		if (string.Compare(firstCharacter, "O") == 0
			|| string.Compare(firstCharacter, "K") == 0
			|| string.Compare(firstCharacter, "Q") == 0
			|| string.Compare(firstCharacter, "R") == 0
			|| string.Compare(firstCharacter, "N") == 0
			|| string.Compare(firstCharacter, "B") == 0)
		{
			// we don't care about capture (x)
			pgnMove = pgnMove.Replace("x", string.Empty);

			if (pgnMove.Length == 5)
			{
				if (string.Compare(pgnMove, "O-O-O") == 0)
				{
					// castling queenside
					string forsythEdwardsNotation = string.Compare(_turn, "w") == 0 ? "K" : "k";

					for (int file = 0; file < 8; file++)
					{
						for (int rank = 0; rank < 8; rank++)
						{
							Square square = _squares[rank, file];

							if (string.Compare(square.ForsythEdwardsNotation, forsythEdwardsNotation) == 0)
							{
								if (string.Compare(forsythEdwardsNotation, "K") == 0)
								{
									// white
									result = $"{square.AlgebraicNotation}c1";
								}
								else
								{
									// black
									result = $"{square.AlgebraicNotation}c8";
								}

								break;
							}
						}

						if (!string.IsNullOrEmpty(result))
						{
							break;
						}
					}
				}
				else
				{
					// <Piece symbol><from square><to square>
					result = pgnMove.Substring(1, 4);
				}
			}
			else if (pgnMove.Length == 4)
			{
				// <Piece symbol><from file><to square>
				// <Piece symbol><from rank><to square>
				string secondCharacter = pgnMove.Substring(1, 1);

				if ("abcdefgh".IndexOf(secondCharacter) >= 0)
				{
					// <Piece symbol><from file><to square>
					string forsythEdwardsNotation = string.Compare(_turn, "w") == 0 ? firstCharacter : firstCharacter.ToLower();

					for (int file = 0; file < 8; file++)
					{
						for (int rank = 0; rank < 8; rank++)
						{
							Square square = _squares[rank, file];

							if (string.Compare(square.ForsythEdwardsNotation, forsythEdwardsNotation) == 0
								&& string.Compare(square.AlgebraicNotation.Substring(0, 1), secondCharacter) == 0)
							{
								result = $"{square.AlgebraicNotation}{pgnMove.Substring(2, 2)}";
								break;
							}
						}

						if (!string.IsNullOrEmpty(result))
						{
							break;
						}
					}
				}
				else
				{
					// <Piece symbol><from rank><to square>
					string forsythEdwardsNotation = string.Compare(_turn, "w") == 0 ? firstCharacter : firstCharacter.ToLower();

					for (int file = 0; file < 8; file++)
					{
						for (int rank = 0; rank < 8; rank++)
						{
							Square square = _squares[rank, file];

							if (string.Compare(square.ForsythEdwardsNotation, forsythEdwardsNotation) == 0
								&& string.Compare(square.AlgebraicNotation.Substring(1, 1), secondCharacter) == 0)
							{
								result = $"{square.AlgebraicNotation}{pgnMove.Substring(2, 2)}";
								break;
							}
						}

						if (!string.IsNullOrEmpty(result))
						{
							break;
						}
					}
				}
			}
			else if (pgnMove.Length == 3)
			{
				if (string.Compare(pgnMove, "O-O") == 0)
				{
					// castling kingside
					string forsythEdwardsNotation = string.Compare(_turn, "w") == 0 ? "K" : "k";

					for (int file = 0; file < 8; file++)
					{
						for (int rank = 0; rank < 8; rank++)
						{
							Square square = _squares[rank, file];

							if (string.Compare(square.ForsythEdwardsNotation, forsythEdwardsNotation) == 0)
							{
								if (string.Compare(forsythEdwardsNotation, "K") == 0)
								{
									// white
									result = $"{square.AlgebraicNotation}g1";
								}
								else
								{
									// black
									result = $"{square.AlgebraicNotation}g8";
								}

								break;
							}
						}

						if (!string.IsNullOrEmpty(result))
						{
							break;
						}
					}
				}
				else
				{
					// <Piece symbol><to square>
					string forsythEdwardsNotation = string.Compare(_turn, "w") == 0 ? firstCharacter : firstCharacter.ToLower();
					string destination = pgnMove.Substring(1, 2);

					for (int file = 0; file < 8; file++)
					{
						for (int rank = 0; rank < 8; rank++)
						{
							Square square = _squares[rank, file];

							if (string.Compare(square.ForsythEdwardsNotation, forsythEdwardsNotation) == 0)
							{
								if (_moves.ContainsKey(square.AlgebraicNotation))
								{
									foreach (string value in _moves[square.AlgebraicNotation])
									{
										if (string.Compare(value, destination) == 0)
										{
											result = $"{square.AlgebraicNotation}{destination}";
											break;
										}
									}
								}
							}

							if (!string.IsNullOrEmpty(result))
							{
								break;
							}
						}

						if (!string.IsNullOrEmpty(result))
						{
							break;
						}
					}
				}
			}
		}

		// <SAN move descriptor pawn captures> ::= <from file>[<from rank>] 'x' <to square>[<promoted to>]
		else if (pgnMove.Length == 6 // <from file>[<from rank>] 'x' <to square>[<promoted to>]
			|| pgnMove.Length == 5 // <from file>[<from rank>] 'x' <to square>, <from file> 'x' <to square>[<promoted to>]
			|| pgnMove.Length == 4) // <from file> 'x' <to square>
		{
			if (pgnMove.Length == 6)
			{
				// <from file>[<from rank>] 'x' <to square>[<promoted to>]

			}
			else if (pgnMove.Length == 5)
			{
				// <from file>[<from rank>] 'x' <to square>
				// <from file> 'x' <to square>[<promoted to>]

			}
			else if (pgnMove.Length == 4)
			{
				// <from file> 'x' <to square>
				// we don't care about capture (x)
				pgnMove = pgnMove.Replace("x", string.Empty);

				string forsythEdwardsNotation = string.Compare(_turn, "w") == 0 ? "P" : "p";
				string destination = pgnMove.Substring(1, 2);

				for (int file = 0; file < 8; file++)
				{
					for (int rank = 0; rank < 8; rank++)
					{
						Square square = _squares[rank, file];

						if (string.Compare(square.ForsythEdwardsNotation, forsythEdwardsNotation) == 0
							&& _moves.ContainsKey(square.AlgebraicNotation)
							&& _moves[square.AlgebraicNotation].Contains(destination))
						{
							result = $"{square.AlgebraicNotation}{destination}";

							break;
						}
					}

					if (!string.IsNullOrEmpty(result))
					{
						break;
					}
				}
			}
		}

		// <SAN move descriptor pawn push>     ::= <to square>[<promoted to>]
		else
		{
			// unambiguous
			string destination = pgnMove.Substring(0, 2);
			string promotedTo = string.Empty;

			if (pgnMove.Length == 3)
			{
				promotedTo = pgnMove.Substring(2, 1);
			}

			foreach (string key in _moves.Keys)
			{
				foreach (string value in _moves[key])
				{
					if (string.Compare(value, destination) == 0)
					{
						result = $"{key}{value}";
						break;
					}
				}

				if (!string.IsNullOrEmpty(result))
				{
					break;
				}
			}
		}

		return result;
	}

	private Square GetSquareByAlgebraicNotation(string algebraicNotation)
	{
		Square result = null;

		int rank = int.Parse(algebraicNotation.Substring(1, 1)) - 1;
		int file = "abcdefgh".IndexOf(algebraicNotation.Substring(0, 1));

		result = _squares[rank, file];

		return result;
	}

	private bool IsLegalMove(Square square)
	{
		bool result = _moves.ContainsKey(_selectedSquare.AlgebraicNotation)
			&& _moves[_selectedSquare.AlgebraicNotation].Contains(square.AlgebraicNotation);

		return result;
	}

	private bool IsNoMove(Square square)
	{
		bool result = square == _selectedSquare;

		return result;
	}

	private void MoveRook(string move)
	{
		string fromAlgebraicNotation = move.Substring(0, 2);
		Square fromSquare = GetSquareByAlgebraicNotation(fromAlgebraicNotation);

		string toAlgebraicNotation = move.Substring(2, 2);
		Square toSquare = GetSquareByAlgebraicNotation(toAlgebraicNotation);

		_canMove = true;
		_selectedSquare = fromSquare;
		_destinationSquare = toSquare;

		PointerUp(fromSquare.Piece);
	}

	private void ReplacePiece(GameObject piece, GameObject pieceAlternate)
	{
		piece.transform.position = _destinationSquare.Piece.transform.position;

		if (pieceAlternate != null)
		{
			pieceAlternate.transform.position = _destinationSquare.Piece.transform.position;
		}

		// In 3D space, you can manipulate Transforms on the x-axis, y-axis, and z-axis.
		// In Unity, these axes are represented by the colors red, green, and blue respectively.
		// x = forward (negative) and backward (positive)
		// y = up and down
		// z = left (positive) and right (negative)
		MeshRenderer meshRendererSquare = _destinationSquare.GameObject.GetComponent<MeshRenderer>();
		MeshRenderer meshRendererPiece = piece.GetComponent<MeshRenderer>();

		Bounds boundsSquare = meshRendererSquare.bounds;
		Bounds boundsPiece = meshRendererPiece.bounds;
		float paddingWidth = (boundsSquare.size.x - boundsPiece.size.x) / 2F;
		float paddingDepth = (boundsSquare.size.z - boundsPiece.size.z) / 2F;

		Vector3 targetPosition = meshRendererSquare.transform.position;
		//targetPosition -= new Vector3(0F, 0F, paddingWidth);
		//targetPosition += new Vector3(paddingDepth, 0F, 0F);

		Vector3 vector3 = meshRendererPiece.transform.position - targetPosition;
		meshRendererPiece.transform.Translate(new Vector3(vector3.z, vector3.x, 0F));

		if (pieceAlternate != null)
		{
			MeshRenderer meshRendererPieceAlternate = pieceAlternate.GetComponent<MeshRenderer>();

			boundsPiece = meshRendererPiece.bounds;

			vector3 = meshRendererPieceAlternate.transform.position - targetPosition;
			meshRendererPieceAlternate.transform.Translate(new Vector3(vector3.z, vector3.x, 0F));
		}
	}

	private void RevertSquareToOriginalMaterial(Square square)
	{
		MeshRenderer meshRenderer = square.GameObject.GetComponent<MeshRenderer>();

		meshRenderer.material = _destinationSquare.OriginalMaterial;
	}

	private void SimulateMove(string move)
	{
		string originalMove = move;

		if (_testingMovesArePGN)
		{
			move = GetUCIMoveFromPGNMove(move);
		}

		if (string.IsNullOrEmpty(move))
		{
			GameSystem.Instance.Status = "White resigned";
			return;
		}

		string fromAlgebraicNotation = move.Substring(0, 2);
		Square fromSquare = GetSquareByAlgebraicNotation(fromAlgebraicNotation);

		string toAlgebraicNotation = move.Substring(2, 2);
		Square toSquare = GetSquareByAlgebraicNotation(toAlgebraicNotation);

		_canMove = true;
		_selectedSquare = fromSquare;
		_destinationSquare = toSquare;

		PointerUp(fromSquare.Piece);
	}
}