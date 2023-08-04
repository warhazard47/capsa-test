using UnityEngine;
using UnityEngine.UI;

public class Shortcut : MonoBehaviour
{
	public Button straight;
	public Button flush;
	public Button fullHouse;
	public Button fourOfAKind;
	public Button straightFlush;
	public Button royalFlush;
	public Button dragon;

	void Awake()
	{
		straight = transform.Find("Straight").GetComponent<Button>();
		flush = transform.Find("Flush").GetComponent<Button>();
		fullHouse = transform.Find("Full house").GetComponent<Button>();
		fourOfAKind = transform.Find("4 of a kind").GetComponent<Button>();
		straightFlush = transform.Find("Straight flush").GetComponent<Button>();
		royalFlush = transform.Find("Royal flush").GetComponent<Button>();
		dragon = transform.Find("Dragon").GetComponent<Button>();
	}

	public bool Interactable
	{
		set
		{
			straight.interactable = value;
			flush.interactable = value;
			fullHouse.interactable = value;
			fourOfAKind.interactable = value;
			straightFlush.interactable = value;
			royalFlush.interactable = value;
			dragon.interactable = value;
		}
	}
}