using UnityEngine;
using UnityEngine.EventSystems;

public class PromotionPieces : MonoBehaviour, IPointerUpHandler
{
	void Start()
	{
		// Start is called before the first frame update
	}

	void Update()
	{
		// Update is called once per frame
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		string forsythEdwardsNotation = string.Empty;

		if (string.Compare(gameObject.name, "white_queen_promotion") == 0)
		{
			forsythEdwardsNotation = "Q";
		}
		else if (string.Compare(gameObject.name, "white_rook_promotion") == 0)
		{
			forsythEdwardsNotation = "R";
		}
		else if (string.Compare(gameObject.name, "white_knight_promotion") == 0)
		{
			forsythEdwardsNotation = "N";
		}
		else if (string.Compare(gameObject.name, "white_bishop_promotion") == 0)
		{
			forsythEdwardsNotation = "B";
		}

		GameSystem.Instance.GotWhitePawnPromotion(forsythEdwardsNotation);
	}
}