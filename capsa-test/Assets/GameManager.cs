using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
	public GameObject resultPanel;

	public Image announcer;
	public Text notifier;
	public TextMeshProUGUI popupText;

	public Card prefabs;
	public List<Player> players;

	List<ComboType> tricks = new List<ComboType>();

	int nextTurnPlayer;
	int lastTurnPlayer;
	int passPlayer;
	bool firstTurn = true;

	public static GameManager instance;

	public void OnGameOver()
	{
		if (resultPanel == null)
			return;
		resultPanel.SetActive(true);

		resultPanel = resultPanel.transform.GetChild(0).gameObject;
		var p = resultPanel.transform.Find("Player 1");
		p.transform.Find("Card").GetComponent<Text>().text = players[0].Cards.Count.ToString();
		p.transform.Find("Points").GetComponent<Text>().text = GetPoint(0).ToString();

		p = resultPanel.transform.Find("Player 2");
		p.transform.Find("Card").GetComponent<Text>().text = players[1].Cards.Count.ToString();
		p.transform.Find("Points").GetComponent<Text>().text = GetPoint(1).ToString();

		p = resultPanel.transform.Find("Player 3");
		p.transform.Find("Card").GetComponent<Text>().text = players[2].Cards.Count.ToString();
		p.transform.Find("Points").GetComponent<Text>().text = GetPoint(2).ToString();

		p = resultPanel.transform.Find("Player 4");
		p.transform.Find("Card").GetComponent<Text>().text = players[3].Cards.Count.ToString();
		p.transform.Find("Points").GetComponent<Text>().text = GetPoint(3).ToString();

		resultPanel = resultPanel.transform.parent.gameObject;
	}

	int GetPoint(int playerId)
	{
		var cardLeft = players[playerId].Cards.Count;
		if (cardLeft >= 13)
		{
			return cardLeft * -3;
		}
		else if (cardLeft >= 10)
		{
			return cardLeft * -2;
		}
		else if (cardLeft > 0)
		{
			return cardLeft * -1;
		}
		else
		{
			var point = 0;
			for (int i = 0; i < players.Count; ++i)
			{
				if (i != playerId)
					point += GetPoint(i);
			}
			return Mathf.Abs(point);
		}
	}

	public void CombinePopup(ComboType.CombinationType comb)
	{
		if (comb == ComboType.CombinationType.One)
			return;

		StopCoroutine("Announcement");
		popupText.text = "";
		switch (comb)
		{
			case ComboType.CombinationType.Pair:
				popupText.text = "Pair!";
				break;
			case ComboType.CombinationType.Triple:
				popupText.text = "Triple!";
				break;
			case ComboType.CombinationType.Straight:
				popupText.text = "Straight!";
				break;
			case ComboType.CombinationType.Flush:
				popupText.text = "Flush!";
				break;
			case ComboType.CombinationType.FullHouse:
				popupText.text = "Full House!";
				break;
			case ComboType.CombinationType.StraightFlush:
				popupText.text = "Straight Flush!";
				break;
			case ComboType.CombinationType.RoyalFlush:
				popupText.text = "Royal Flush!";
				break;
			case ComboType.CombinationType.Dragon:
				popupText.text = "Dragon!";
				break;
			default:
				popupText.text = "";
				break;
		}
		StartCoroutine("Fade");
	}

	IEnumerator Fade()
	{
		float time = 3f;

		while (time > 0)
		{
			time -= Time.deltaTime;
			if (time < 2f)
			{
				var color = announcer.color;
				color.a -= Time.deltaTime / 2f;
				announcer.color = color;
			}
			yield return null;
		}
		announcer.color = Color.clear;
	}

	public void NotifyMessage(string message)
	{
		notifier.text = message;
		if (IsInvoking("ClearNotification"))
			CancelInvoke("ClearNotification");
		Invoke("ClearNotification", 2f);
	}

	void ClearNotification()
	{
		notifier.text = "";
	}

	public ComboType LastTrick
	{
		get
		{
			return tricks.Count > 0 ? tricks[tricks.Count - 1] : null;
		}
	}

	public int CurrentPlayer
	{
		get
		{
			return nextTurnPlayer == 0 ? players.Count - 1 : nextTurnPlayer - 1;
		}
	}

	public bool IsFirstTurn
	{
		get { return firstTurn; }
	}

    void Awake()
    {
		instance = this;    
    }

    void Start()
	{
		// Initialize cards
		Card[] allCard = new Card[52];
		for (int i = 0; i < Card.numberOrder.Length; ++i)
		{
			var spade = Instantiate(prefabs) as Card;
			spade.elements = Card.Elements.Spade;
			spade.Nominal = Card.numberOrder[i];
			allCard[i * 4 + 0] = spade;

			var heart = Instantiate(prefabs) as Card;
			heart.elements = Card.Elements.Heart;
			heart.Nominal = Card.numberOrder[i];
			allCard[i * 4 + 1] = heart;

			var club = Instantiate(prefabs) as Card;
			club.elements = Card.Elements.Club;
			club.Nominal = Card.numberOrder[i];
			allCard[i * 4 + 2] = club;

			var diamond = Instantiate(prefabs) as Card;
			diamond.elements = Card.Elements.Diamond;
			diamond.Nominal = Card.numberOrder[i];
			allCard[i * 4 + 3] = diamond;
		}
		new System.Random().Shuffle(allCard);

		// Initialize players
		var set = new List<Card>();
		set.AddRange(allCard);
		for (int i = 0; i < players.Count; ++i)
		{
			players[i].id = i;
			players[i].Cards = set.GetRange(i * 13, 13);

			// If have '3 diamond' card, play first turn
			var firstCard = players[i].Cards[0];
			if (firstCard.Nominal == "3" && firstCard.elements == Card.Elements.Diamond)
			{
				nextTurnPlayer = lastTurnPlayer = i;
			}
		}
		OnBeginTrick();
	}

	void OnBeginTrick()
	{
		// Reset all player state
		for (int i = 0; i < players.Count; ++i)
		{
			players[i].OnTurnEnd();
			players[i].IsPass = false;
		}

		passPlayer = 0;
		nextTurnPlayer = lastTurnPlayer;

		// Begin Turn
		NextTurn();
	}

	void OnEndTrick()
	{
		// Empty Cards
		for (int i = 0; i < tricks.Count; ++i)
		{
			for (int c = 0; c < tricks[i].Cards.Count; ++c)
			{
				Destroy(tricks[i].Cards[c].gameObject);
			}
		}
		tricks.Clear();

		OnBeginTrick();
	}

	void NextTurn()
	{
		var current = CurrentPlayer;
		players[current].OnTurnEnd();

		// Stop game if one player already spent all his cards
		if (players[current].Cards.Count == 0)
		{
			OnGameOver();
			return;
		}

		while (players[nextTurnPlayer].IsPass)
		{
			nextTurnPlayer = nextTurnPlayer + 1 >= players.Count ? 0 : nextTurnPlayer + 1;
		}

		players[nextTurnPlayer].OnTurnBegin();

		nextTurnPlayer = nextTurnPlayer + 1 >= players.Count ? 0 : nextTurnPlayer + 1;
	}

	public void Pass()
	{
		if (tricks.Count > 0) ++passPlayer;

		if (players.Count - passPlayer > 1)
			NextTurn();
		else
			OnEndTrick();
		firstTurn = false;
	}

	public bool Deal(ComboType hand)
	{
		System.Action TakeCards = () => {
			for (int i = 0; i < hand.Cards.Count; ++i)
			{
				hand.Cards[i].transform.SetParent(transform.GetChild(CurrentPlayer));
				hand.Cards[i].button.interactable = false;
				players[CurrentPlayer].Cards.Remove(hand.Cards[i]);
			}
			CombinePopup(hand.Combination);
			firstTurn = false;
		};

		if (hand.Cards.Count == 0 || hand.Combination == ComboType.CombinationType.Invalid)
		{
			NotifyMessage("Invalid deal!");
			return false;
		}

		if (firstTurn && !(hand.Cards[0].Nominal == "3" && hand.Cards[0].elements == Card.Elements.Diamond))
		{
			NotifyMessage("Must include 3 Diamond");
			return false;
		}

		// Initialize Tricks if still empty
		if (tricks.Count == 0)
		{
			tricks.Add(hand);
			lastTurnPlayer = nextTurnPlayer == 0 ? players.Count - 1 : nextTurnPlayer - 1;
			TakeCards();
			NextTurn();
			return true;
		}
		else
		{
			var last = tricks[tricks.Count - 1];

			if (hand.Combination <= ComboType.CombinationType.Triple && last.Combination <= ComboType.CombinationType.Triple)
			{
				// Single, Pair or Triple rule
				if (hand.Combination == last.Combination && hand.Key > last.Key)
				{
					tricks.Add(hand);
					lastTurnPlayer = nextTurnPlayer == 0 ? players.Count - 1 : nextTurnPlayer - 1;
					TakeCards();
					NextTurn();
					return true;
				}
			}
			else if (hand.Combination > ComboType.CombinationType.Triple && last.Combination > ComboType.CombinationType.Triple)
			{
				// 5-card hand
				if (hand > last)
				{
					tricks.Add(hand);
					lastTurnPlayer = nextTurnPlayer == 0 ? players.Count - 1 : nextTurnPlayer - 1;
					TakeCards();
					NextTurn();
					return true;
				}
			}
			NotifyMessage("Please Check Your Cards!");
			return false;
		}
	}
}

static class RandomExtensions
{
	public static void Shuffle<T>(this System.Random rng, T[] array)
	{
		int n = array.Length;
		while (n > 1)
		{
			int k = rng.Next(n--);
			T temp = array[n];
			array[n] = array[k];
			array[k] = temp;
		}
	}
}
