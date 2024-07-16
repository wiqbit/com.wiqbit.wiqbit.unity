using UnityEngine;
using UnityEngine.EventSystems;

public class Pieces : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
	void Start()
	{
		// Start is called before the first frame update
	}

	void Update()
	{
		// Update is called once per frame
	}

	public void OnDrag(PointerEventData pointerEventData)
	{
		GameSystem.Instance.Board.Drag(gameObject, pointerEventData.delta, false);
	}

	public void OnPointerDown(PointerEventData pointerEventData)
	{
		GameSystem.Instance.Board.PointerDown(gameObject, pointerEventData.delta);
	}

	public void OnPointerUp(PointerEventData pointerEventData)
	{
		GameSystem.Instance.Board.PointerUp(gameObject);
	}
}