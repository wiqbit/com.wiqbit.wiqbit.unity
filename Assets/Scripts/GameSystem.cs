using Assets.Scripts.Chess;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSystem : MonoBehaviour
{
	// pgn-extract.exe .\kasparov_topalov_1999.pgn -Wlalg

	// Garry Kasparov vs Veselin Topalov
	// Veselin Topalov resigned
	private const string GAME_1_TITLE = "Garry Kasparov vs Veselin Topalov";
	//private const string GAME_1 = "1. e4 d6 2. d4 Nf6 3. Nc3 g6 4. Be3 Bg7 5. Qd2 c6 6. f3 b5 7. Nge2 Nbd7 8. Bh6 Bxh6 9. Qxh6 Bb7 10. a3 e5 11. O-O-O Qe7 12. Kb1 a6 13. Nc1 O-O-O 14. Nb3 exd4 15. Rxd4 c5 16. Rd1 Nb6 17. g3 Kb8 18. Na5 Ba8 19. Bh3 d5 20. Qf4+ Ka7 21. Rhe1 d4 22. Nd5 Nbxd5 23. exd5 Qd6 24. Rxd4 cxd4 25. Re7+ Kb6 26. Qxd4+ Kxa5 27. b4+ Ka4 28. Qc3 Qxd5 29. Ra7 Bb7 30. Rxb7 Qc4 31. Qxf6 Kxa3 32. Qxa6+ Kxb4 33. c3+ Kxc3 34. Qa1+ Kd2 35. Qb2+ Kd1 36. Bf1 Rd2 37. Rd7 Rxd7 38. Bxc4 bxc4 39. Qxh8 Rd3 40. Qa8 c3 41. Qa4+ Ke1 42. f4 f5 43. Kc1 Rd2 44. Qa7";
	private const string GAME_1 = "1. e2e4 d7d6 2. d2d4 g8f6 3. b1c3 g7g6 4. c1e3 f8g7 5. d1d2 c7c6 6. f2f3 b7b5 7. g1e2 b8d7 8. e3h6 g7h6 9. d2h6 c8b7 10. a2a3 e7e5 11. e1c1 d8e7 12. c1b1 a7a6 13. e2c1 e8c8 14. c1b3 e5d4 15. d1d4 c6c5 16. d4d1 d7b6 17. g2g3 c8b8 18. b3a5 b7a8 19. f1h3 d6d5 20. h6f4+ b8a7 21. h1e1 d5d4 22. c3d5 b6d5 23. e4d5 e7d6 24. d1d4 c5d4 25. e1e7+ a7b6 26. f4d4+ b6a5 27. b2b4+ a5a4 28. d4c3 d6d5 29. e7a7 a8b7 30. a7b7 d5c4 31. c3f6 a4a3 32. f6a6+ a3b4 33. c2c3+ b4c3 34. a6a1+ c3d2 35. a1b2+ d2d1 36. h3f1 d8d2 37. b7d7 d2d7 38. f1c4 b5c4 39. b2h8 d7d3 40. h8a8 c4c3 41. a8a4+ d1e1 42. f3f4 f7f5 43. b1c1 d3d2 44. a4a7";

	// Donald Byrne vs Robert James Fischer
	// Robert James Fischer won
	private const string GAME_2_TITLE = "Donald Byrne vs Robert James Fischer";
	private const string GAME_2 = "1. Nf3 Nf6 2. c4 g6 3. Nc3 Bg7 4. d4 O-O 5. Bf4 d5 6. Qb3 dxc4 7. Qxc4 c6 8. e4 Nbd7 9. Rd1 Nb6 10. Qc5 Bg4 11. Bg5 Na4 12. Qa3 Nxc3 13. bxc3 Nxe4 14. Bxe7 Qb6 15. Bc4 Nxc3 16. Bc5 Rfe8+ 17. Kf1 Be6 18. Bxb6 Bxc4+ 19. Kg1 Ne2+ 20. Kf1 Nxd4+ 21. Kg1 Ne2+ 22. Kf1 Nc3+ 23. Kg1 axb6 24. Qb4 Ra4 25. Qxb6 Nxd1 26. h3 Rxa2 27. Kh2 Nxf2 28. Re1 Rxe1 29. Qd8+ Bf8 30. Nxe1 Bd5 31. Nf3 Ne4 32. Qb8 b5 33. h4 h5 34. Ne5 Kg7 35. Kg1 Bc5+ 36. Kf1 Ng3+ 37. Ke1 Bb4+ 38. Kd1 Bb3+ 39. Kc1 Ne2+ 40. Kb1 Nc3+ 41. Kc1 Rc2#";

	// Alexander Beliavsky vs John Nunn
	// John Nunn resigned
	private const string GAME_3_TITLE = "Alexander Beliavsky vs John Nunn";
	private const string GAME_3 = "1. d4 Nf6 2. c4 g6 3. Nc3 Bg7 4. e4 d6 5. f3 O-O 6. Be3 Nbd7 7. Qd2 c5 8. d5 Ne5 9. h3 Nh5 10. Bf2 f5 11. exf5 Rxf5 12. g4 Rxf3 13. gxh5 Qf8 14. Ne4 Bh6 15. Qc2 Qf4 16. Ne2 Rxf2 17. Nxf2 Nf3+ 18. Kd1 Qh4 19. Nd3 Bf5 20. Nec1 Nd2 21. hxg6 hxg6 22. Bg2 Nxc4 23. Qf2 Ne3+ 24. Ke2 Qc4 25. Bf3 Rf8 26. Rg1 Nc2 27. Kd1 Bxd3";

	// Vasyl Ivanchuk vs Artur Jussupow
	// Artur Jussupow won
	private const string GAME_4_TITLE = "Vasyl Ivanchuk vs Artur Jussupow";
	private const string GAME_4 = "1. c4 e5 2. g3 d6 3. Bg2 g6 4. d4 Nd7 5. Nc3 Bg7 6. Nf3 Ngf6 7. O-O O-O 8. Qc2 Re8 9. Rd1 c6 10. b3 Qe7 11. Ba3 e4 12. Ng5 e3 13. f4 Nf8 14. b4 Bf5 15. Qb3 h6 16. Nf3 Ng4 17. b5 g5 18. bxc6 bxc6 19. Ne5 gxf4 20. Nxc6 Qg5 21. Bxd6 Ng6 22. Nd5 Qh5 23. h4 Nxh4 24. gxh4 Qxh4 25. Nde7+ Kh8 26. Nxf5 Qh2+ 27. Kf1 Re6 28. Qb7 Rg6 29. Qxa8+ Kh7 30. Qg8+ Kxg8 31. Nce7+ Kh7 32. Nxg6 fxg6 33. Nxg7 Nf2 34. Bxf4 Qxf4 35. Ne6 Qh2 36. Rdb1 Nh3 37. Rb7+ Kh8 38. Rb8+ Qxb8 39. Bxh3 Qg3";

	// Georg Rotlewi vs Akiba Rubinstein
	// Georg Rotlewi resigned
	private const string GAME_5_TITLE = "Georg Rotlewi vs Akiba Rubinstein";
	private const string GAME_5 = "1. d4 d5 2. Nf3 e6 3. e3 c5 4. c4 Nc6 5. Nc3 Nf6 6. dxc5 Bxc5 7. a3 a6 8. b4 Bd6 9. Bb2 O-O 10. Qd2 Qe7 11. Bd3 dxc4 12. Bxc4 b5 13. Bd3 Rd8 14. Qe2 Bb7 15. O-O Ne5 16. Nxe5 Bxe5 17. f4 Bc7 18. e4 Rac8 19. e5 Bb6+ 20. Kh1 Ng4 21. Be4 Qh4 22. g3 Rxc3 23. gxh4 Rd2 24. Qxd2 Bxe4+ 25. Qg2 Rh3";

	// Lev Polugaevsky vs Rashid Nezhmetdinov
	// Lev Polugaevsky resigned
	private const string GAME_6_TITLE = "Lev Polugaevsky vs Rashid Nezhmetdinov";
	private const string GAME_6 = "1. d4 Nf6 2. c4 d6 3. Nc3 e5 4. e4 exd4 5. Qxd4 Nc6 6. Qd2 g6 7. b3 Bg7 8. Bb2 O-O 9. Bd3 Ng4 10. Nge2 Qh4 11. Ng3 Nge5 12. O-O f5 13. f3 Bh6 14. Qd1 f4 15. Nge2 g5 16. Nd5 g4 17. g3 fxg3 18. hxg3 Qh3 19. f4 Be6 20. Bc2 Rf7 21. Kf2 Qh2+ 22. Ke3 Bxd5 23. cxd5 Nb4 24. Rh1 Rxf4 25. Rxh2 Rf3+ 26. Kd4 Bg7 27. a4 c5+ 28. dxc6 bxc6 29. Bd3 Nexd3+ 30. Kc4 d5+ 31. exd5 cxd5+ 32. Kb5 Rb8+ 33. Ka5 Nc6+";
	
	private object _instanceLock = new object();

	public void GetMoves(string forsythEdwardsNotation)
	{
		StartCoroutine(GetMovesCoroutine(forsythEdwardsNotation));
	}

	public void MakeChessMove(string forsythEdwardsNotation)
	{
		StartCoroutine(MakeChessMoveCoroutine(forsythEdwardsNotation));
	}

	public void PlayClack()
	{
		Instance.AudioSourceClack1.Play();
	}

	public void PlayClackClack()
	{
		Instance.AudioSourceClack1.Play();
		Instance.AudioSourceClack2.PlayScheduled(AudioSettings.dspTime + Clack1.length * 0.5F);
	}

	public void PlaySwishClack()
	{
		Instance.AudioSourceSwish.Play();
		Instance.AudioSourceClack1.PlayScheduled(AudioSettings.dspTime + Instance.Swish.length);
	}

	public void GetBlackPawnPromotion(string forsytheEdwardsNotation)
	{
		Instance.Board.GotBlackPawnPromotion(forsytheEdwardsNotation);
	}

	public void GetWhitePawnPromotion()
	{
		StartCoroutine(GetWhitePawnPromotionCoroutine());
	}

	public void GotWhitePawnPromotion(string forsythEdwardsNotation)
	{
		Instance.WhiteBishopPromotion.SetActive(false);
		Instance.WhiteKnightPromotion.SetActive(false);
		Instance.WhiteQueenPromotion.SetActive(false);
		Instance.WhiteRookPromotion.SetActive(false);

		Instance.Camera.enabled = true;
		Instance.CameraPromotion.enabled = false;

		Instance.Board.GotWhitePawnPromotion(forsythEdwardsNotation);
	}

	private void Awake()
	{
		lock (_instanceLock)
		{
			if (Instance == null)
			{
				Instance = this;
				Instance.Chess = new Chess();
				Instance.Board = new Board(false); // board calls chess

				foreach (Camera camera in Camera.allCameras)
				{
					if (string.Compare(camera.name, "Camera") == 0)
					{
						Instance.Camera = camera;
						Instance.Camera.enabled = true;
						Instance.AudioSourceClack1 = Instance.Camera.GetComponents<AudioSource>()[0];
						Instance.Clack1 = Resources.Load("no9_Game_Sounds/wooden_chess_sounds/chessman_placing_on_board_strong_no_slide/001", typeof(AudioClip)) as AudioClip;
						Instance.AudioSourceClack1.clip = Instance.Clack1;
						Instance.AudioSourceClack2 = Instance.Camera.GetComponents<AudioSource>()[1];
						Instance.Clack2 = Resources.Load("no9_Game_Sounds/wooden_chess_sounds/chessman_placing_on_board_strong_no_slide/001", typeof(AudioClip)) as AudioClip;
						Instance.AudioSourceClack2.clip = Instance.Clack2;
						Instance.AudioSourceSwish = Instance.Camera.GetComponents<AudioSource>()[2];
						Instance.Swish = Resources.Load("no9_Game_Sounds/wooden_chess_sounds/chessman_sliding/001", typeof(AudioClip)) as AudioClip;
						Instance.AudioSourceSwish.clip = Instance.Swish;
					}
					else if (string.Compare(camera.name, "Camera Promotion") == 0)
					{
						Instance.CameraPromotion = camera;
						Instance.CameraPromotion.enabled = false;
					}
				}

				Instance.WhiteBishopPromotion = GameObject.Find("white_bishop_promotion");
				Instance.WhiteBishopPromotion.SetActive(false);

				Instance.WhiteKnightPromotion = GameObject.Find("white_knight_promotion");
				Instance.WhiteKnightPromotion.SetActive(false);

				Instance.WhiteQueenPromotion = GameObject.Find("white_queen_promotion");
				Instance.WhiteQueenPromotion.SetActive(false);

				Instance.WhiteRookPromotion = GameObject.Find("white_rook_promotion");
				Instance.WhiteRookPromotion.SetActive(false);

				Instance.GUISkin = Resources.Load("GUISkin", typeof(GUISkin)) as GUISkin;

				Instance.SkillLevel = 1F;
				Instance.SkillLevelRatings = new List<float>()
				{
					1320.1F,
					1467.6F,
					1608.4F,
					1742.3F,
					1922.9F,
					2203.7F,
					2363.2F,
					2499.5F,
					2596.2F,
					2702.8F,
					2788.3F,
					2855.5F,
					2923.1F,
					2972.9F,
					3024.8F,
					3069.5F,
					3111.2F,
					3141.3F,
					3170.3F,
					3191.1F
				};

				//Instance.Board.TestWhitePawnPromotionQueenside();
				//Instance.Board.TestWhitePawnPromotionKingside();
				//Instance.Board.TestBlackPawnPromotionQueenside();
				//Instance.Board.TestBlackPawnPromotionKingside();
				//Instance.Board.TestWhiteEnPassant();
				//Instance.Board.TestBlackEnPassant();
				//Instance.Board.PlayGame(GAME_2);
				
				//Instance.Testing = true;
				//StartCoroutine(Instance.PlayGameCoroutine());
			}
		}
	}

	private void OnGUI()
	{
		float spaceBetweenBottomOfChessBoardAndBottomOfScreen = 90F; // Chess Board Rotation Y
		spaceBetweenBottomOfChessBoardAndBottomOfScreen += -20F; // Chess Board Position Z
		spaceBetweenBottomOfChessBoardAndBottomOfScreen /= 2F;

		GUIContent guiContent1 = null;
		if (Instance.Testing)
		{
			guiContent1 = new GUIContent(Instance.Title);
		}
		else
		{
			guiContent1 = new GUIContent("1");
		}
		Vector2 vector21 = GUI.skin.box.CalcSize(guiContent1);
		float padding = spaceBetweenBottomOfChessBoardAndBottomOfScreen - vector21.y;
		padding /= 2F;
		Rect rect1 = new Rect(padding, Screen.height - vector21.y - padding, vector21.x, vector21.y);
		GUI.Label(rect1, guiContent1);

		if (!Instance.Testing)
		{
			GUIContent guiContent0 = new GUIContent("New game");
			Vector2 vector20 = GUI.skin.box.CalcSize(guiContent0);
			Rect rect0 = new Rect(padding, padding, vector20.x + 8, vector20.y + 8);
			bool pressed = GUI.Button(rect0, guiContent0);
			if (pressed)
			{
				Board = new Board(true);
			}
		}

		//GUIContent guiContent2 = new GUIContent("3");
		//Vector2 vector22 = GUI.skin.box.CalcSize(guiContent2);
		//Rect rect2 = new Rect(padding + 100F - vector22.x, Screen.height - vector22.y - padding, vector22.x, vector22.y);
		//if (!Instance.Testing)
		//{
		//	GUI.Label(rect2, guiContent2);
		//}

		//GUI.skin = GUISkin;
		//Rect rect3 = new Rect(padding, rect2.y - 12F, 100F - padding, 12F);
		//float skillLevel = ((float)Math.Round(((double)Instance.SkillLevel), 0));
		//if (!Instance.Testing)
		//{
		//	Instance.SkillLevel = GUI.HorizontalSlider(rect3, skillLevel, 1F, 3F);
		//}

		//GUIContent guiContent4 = new GUIContent($"Skill level: {Instance.SkillLevel}");
		//Vector2 vector24 = GUI.skin.box.CalcSize(guiContent4);
		//Rect rect4 = new Rect(padding, rect3.y - vector24.y, vector24.x, vector24.y);
		//if (!Instance.Testing)
		//{
		//	GUI.Label(rect4, guiContent4);
		//}

		GUIContent guiContent5 = new GUIContent(Instance.Status);
		Vector2 vector25 = GUI.skin.box.CalcSize(guiContent5);
		Rect rect5 = new Rect(Screen.width / 2 - vector25.x / 2, Screen.height - vector25.y - padding, vector25.x, vector25.y);
		GUI.Label(rect5, guiContent5);

		GUIContent guiContent6 = new GUIContent(Instance.LastMoveBlack);
		Vector2 vector26 = GUI.skin.box.CalcSize(guiContent6);
		Rect rect6 = new Rect(padding, Screen.height - vector26.y - padding, vector26.x, vector26.y);
		GUI.Label(rect6, guiContent6);

		GUIContent guiContent7 = new GUIContent(Instance.LastMoveWhite);
		Vector2 vector27 = GUI.skin.box.CalcSize(guiContent7);
		Rect rect7 = new Rect(padding, rect6.y - vector27.y + 6, vector27.x, vector27.y);
		GUI.Label(rect7, guiContent7);

		GUIContent guiContent8 = new GUIContent("Last moves:");
		Vector2 vector28 = GUI.skin.box.CalcSize(guiContent8);
		Rect rect8 = new Rect(padding, rect7.y - vector28.y + 6, vector28.x, vector28.y);
		GUI.Label(rect8, guiContent8);
	}

	private IEnumerator GetMovesCoroutine(string forsythEdwardsNotation)
	{
		yield return 0;

		Dictionary<string, List<string>> moves = Instance.Chess.GetMoves(forsythEdwardsNotation);

		Instance.Board.GotMoves(moves);
	}

	private IEnumerator GetWhitePawnPromotionCoroutine()
	{
		yield return new WaitForEndOfFrame();

		Instance.WhiteBishopPromotion.SetActive(true);
		Instance.WhiteKnightPromotion.SetActive(true);
		Instance.WhiteQueenPromotion.SetActive(true);
		Instance.WhiteRookPromotion.SetActive(true);

		Instance.Camera.enabled = false;
		Instance.CameraPromotion.enabled = true;

		Instance.Status = "Which piece should your pawn promote to?";
	}

	private IEnumerator MakeChessMoveCoroutine(string forsythEdwardsNotation)
	{
		yield return 0;

		string move = Instance.Chess.GetMove(forsythEdwardsNotation);

		Instance.Board.MadeChessMove(move);
	}

	private IEnumerator PlayGameCoroutine()
	{
		Instance.Title = GAME_1_TITLE;
		
		yield return new WaitForSeconds(15);

		Instance.Board.PlayGame(GAME_1);
	}

	public static GameSystem Instance { get; set; }
	private AudioSource AudioSourceClack1 { get; set; }
	private AudioSource AudioSourceClack2 { get; set; }
	private AudioSource AudioSourceSwish { get; set; }
	public Board Board { get; set; }
	private AudioClip Clack1 { get; set; }
	private AudioClip Clack2 { get; set; }
	public Camera Camera { get; private set; }
	private Camera CameraPromotion { get; set; }
	public Chess Chess { get; set; }
	private GUISkin GUISkin { get; set; }
	public string LastMoveBlack { private get; set; } = "Black: N/A";
	public string LastMoveWhite { private get; set; } = "White: N/A";
	private float SkillLevel { get; set; }
	private List<float> SkillLevelRatings { get; set; }
	public string Status { private get; set; } = "Your move";
	private AudioClip Swish { get; set; }
	private bool Testing { get; set; }
	private string Title { get; set; }
	private GameObject WhiteBishopPromotion { get; set; }
	private GameObject WhiteKnightPromotion { get; set; }
	private GameObject WhiteQueenPromotion { get; set; }
	private GameObject WhiteRookPromotion { get; set; }
}