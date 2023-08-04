using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;


public class Player : MonoBehaviour
{
	public int id = -1;

	List<Card> cards;
	bool isPass = false;

	List<ComboType> hands = new List<ComboType>(13);

	bool isAnalyzing = false;
	bool analyzeAllMatch = true;
	System.Func<Card, bool> analyzeFilter = null;
	ComboType.CombinationType analyzeCombination = ComboType.CombinationType.Invalid;

	public GameObject panel;
	public GameObject passIndicator;
	public TextMeshProUGUI labelTimerText;
	public TextMeshProUGUI labelCountText;

	public List<Card> straightSet = new List<Card>();
	public List<Card> flushtSet = new List<Card>();
	public List<Card> fullHousetSet = new List<Card>();
	public List<Card> fourOfAKindSet = new List<Card>();
	public List<Card> straightFlushSet = new List<Card>();
	public List<Card> royalFlushSet = new List<Card>();
	public List<Card> dragonSet = new List<Card>();
	public List<Card> hintSet = new List<Card>();

	ComboType.CombinationType lastMarkCombination = ComboType.CombinationType.Invalid;
	List<Card> markedCards = new List<Card>();

	public bool Artificial
	{
		get { return id != 0; }
	}

	public List<Card> Cards
	{
		get { return cards; }
		set
		{
			cards = value;
			cards.Sort();
			TotalCard = cards.Count;
			if (!Artificial)
				Display(cards);
		}
	}


	public bool IsPass
	{
		get { return isPass; }
		set
		{
			isPass = value;
			passIndicator.SetActive(isPass);
		}
	}



	public void OnTurnBegin()
	{
		if (panel)
			panel.SetActive(true);
		if (passIndicator)
			passIndicator.SetActive(false);

		var LastTrick = GameManager.instance.LastTrick;
		if (LastTrick != null)
		{
			analyzeCombination = LastTrick.Combination;
			analyzeFilter = trick => trick > LastTrick.Key;
		}
		StartCoroutine("OnTurn");
		StartCoroutine("Analyze");
	}

	IEnumerator OnTurn()
	{
		float time = 15f;
		float lastUpdateTime = Time.time;

		while (time > 0f)
		{
			yield return new WaitForSeconds(0.1f);

			time -= (Time.time - lastUpdateTime);
			lastUpdateTime = Time.time;
			TimeLeft = time;

			if (Artificial && !isAnalyzing)
			{
				if (hands.Count > 0)
				{
					if (!IsInvoking("AutoDeal"))
					{
						Invoke("AutoDeal", Random.Range(Hint.Count * 0.5f, Hint.Count * 2f));
					}
				}
				else
				{
					yield return new WaitForSeconds(0.5f);
					break;
				}
			}
		}
		if (!Artificial && GameManager.instance.IsFirstTurn)
		{
			AutoDeal();
		}
		else
		{
			// Timeout
			Pass();
		}
	}

	public void OnTurnEnd()
	{
		StopCoroutine("Analyze");
		StopCoroutine("OnTurn");

		if (panel)
			panel.SetActive(false);

		if (passIndicator)
			passIndicator.SetActive(false);
	}

	public void AutoDeal()
	{
		Deal(Hint);
	}

	public void Deal(List<Card> dealCards)
	{
		if (dealCards.Count == 0)
			return;

		ComboType hand = ComboType.Make(dealCards);
		if (GameManager.instance.Deal(hand))
		{
			OnDealSuccess();
		}
	}

	public void Deal(ComboType hand)
	{
		if (GameManager.instance.Deal(hand))
		{
			OnDealSuccess();
		}
	}

	public void Pass()
	{
		isPass = true;
		GameManager.instance.Pass();
		OnPass();
	}

	// Helper Events for controller
	public void OnCardMarked(Card card)
	{
		markedCards.Add(card);
		lastMarkCombination = ComboType.CombinationType.Invalid;
	}

	public void OnCardUnmarked(Card card)
	{
		markedCards.Remove(card);
		lastMarkCombination = ComboType.CombinationType.Invalid;
	}


	public void OnDeal()
	{
		Deal(markedCards);
	}

	public void OnDealSuccess()
	{
		markedCards.Clear();
		TotalCard = Cards.Count;
	}

	public void OnPass()
	{
		passIndicator.SetActive(IsPass);
	}

	// Helper events for interface
	public void OnSelectStraight()
	{
		MarkAll(straightSet, lastMarkCombination != ComboType.CombinationType.Straight);
		lastMarkCombination = ComboType.CombinationType.Straight;
	}

	public void OnSelectFlush()
	{
		MarkAll(flushtSet, lastMarkCombination != ComboType.CombinationType.Flush);
		lastMarkCombination = ComboType.CombinationType.Flush;
	}

	public void OnSelectFullHouse()
	{
		MarkAll(fullHousetSet, lastMarkCombination != ComboType.CombinationType.FullHouse);
		lastMarkCombination = ComboType.CombinationType.FullHouse;
	}

	public void OnSelectFourOfAKind()
	{
		MarkAll(fourOfAKindSet, lastMarkCombination != ComboType.CombinationType.FourOfAKind);
		lastMarkCombination = ComboType.CombinationType.FourOfAKind;
	}

	public void OnSelectStraightFlush()
	{
		MarkAll(straightFlushSet, lastMarkCombination != ComboType.CombinationType.StraightFlush);
		lastMarkCombination = ComboType.CombinationType.StraightFlush;
	}

	public void OnSelectRoyalFlush()
	{
		MarkAll(royalFlushSet, lastMarkCombination != ComboType.CombinationType.RoyalFlush);
		lastMarkCombination = ComboType.CombinationType.RoyalFlush;
	}

	public void OnSelectDragon()
	{
		MarkAll(dragonSet, lastMarkCombination != ComboType.CombinationType.Dragon);
		lastMarkCombination = ComboType.CombinationType.Dragon;
	}

	public void OnSelectHint()
	{
		MarkAll(hintSet, true);
		lastMarkCombination = ComboType.CombinationType.Invalid;
	}

	void MarkAll(List<Card> set, bool reset)
	{
		if (set == null)
			return;

		// Deselect all
		if (reset)
		{
			for (int i = 0; i < Cards.Count; ++i)
			{
				Cards[i].Select(false);
			}
		}

		// Select new card
		for (int i = 0; i < set.Count; ++i)
		{
			set[i].ToggleSelect();
		}
	}

	public void Display(List<Card> set)
	{
		for (int i = 0; i < set.Count; ++i)
		{
			set[i].transform.SetParent(transform, false);
		}
	}


	public List<Card> MarkedCards
	{
		get { return markedCards; }
	}

	public int TotalCard
	{
		set { if (labelCountText) labelCountText.text = "" + value; }
	}

	public float TimeLeft
	{
		set { if (labelTimerText) labelTimerText.text = "" + Mathf.CeilToInt(value); }
	}

	public List<Card> Straight
	{
		get { return straightSet; }
		set
		{
			straightSet = value;
		}
	}

	public List<Card> Flush
	{
		get { return flushtSet; }
		set
		{
			flushtSet = value;
		}
	}

	public List<Card> FullHouse
	{
		get { return fullHousetSet; }
		set
		{
			fullHousetSet = value;
		}
	}

	public List<Card> FourOfAKind
	{
		get { return fourOfAKindSet; }
		set
		{
			fourOfAKindSet = value;
		}
	}

	public List<Card> StraightFlush
	{
		get { return straightFlushSet; }
		set
		{
			straightFlushSet = value;
		}
	}

	public List<Card> RoyalFlush
	{
		get { return royalFlushSet; }
		set
		{
			royalFlushSet = value;
		}
	}

	public List<Card> Dragon
	{
		get { return dragonSet; }
		set
		{
			dragonSet = value;
		}
	}

	public List<Card> Hint
	{
		get { return hintSet; }
		set { hintSet = value; }
	}
}