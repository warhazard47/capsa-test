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
	public TextMeshProUGUI statusText;
	public Shortcut shortcut;

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

		statusText.gameObject.SetActive(true);
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
			Skip();
		}
	}

	public void OnTurnEnd()
	{
		StopCoroutine("Analyze");
		StopCoroutine("OnTurn");

		if (panel)
			panel.SetActive(false);
		if (shortcut)
			shortcut.Interactable = false;
		if (passIndicator)
			passIndicator.SetActive(false);
		statusText.gameObject.SetActive(false);
	}

	public void AutoDeal()
	{
		Submit(Hint);
	}

	public void Submit(List<Card> dealCards)
	{
		if (dealCards.Count == 0)
			return;

		ComboType hand = ComboType.Make(dealCards);
		if (GameManager.instance.Deal(hand))
		{
			OnSubmitSuccess();
		}
	}

	public void Deal(ComboType hand)
	{
		if (GameManager.instance.Deal(hand))
		{
			OnSubmitSuccess();
		}
	}

	public void Skip()
	{
		isPass = true;
		GameManager.instance.Pass();
		OnSkip();
	}

	IEnumerator Analyze()
	{
		isAnalyzing = true;
		hands.Clear();
		List<ComboType> result;

		// One analysis each frame
		switch (analyzeCombination)
		{
			case ComboType.CombinationType.Invalid:
				goto case ComboType.CombinationType.One;
			case ComboType.CombinationType.One:
				hands.AddRange(One.instance.LazyEvaluator(cards, false, analyzeFilter));

				if (analyzeCombination == ComboType.CombinationType.Invalid)
					goto case ComboType.CombinationType.Pair;
				break;
			case ComboType.CombinationType.Pair:
				hands.AddRange(Pair.instance.LazyEvaluator(cards, analyzeAllMatch, analyzeFilter));

				if (analyzeCombination == ComboType.CombinationType.Invalid)
					goto case ComboType.CombinationType.Triple;
				break;
			case ComboType.CombinationType.Triple:
				hands.AddRange(Triple.instance.LazyEvaluator(cards, analyzeAllMatch, analyzeFilter));

				if (analyzeCombination == ComboType.CombinationType.Invalid)
					goto case ComboType.CombinationType.Straight;
				break;
			case ComboType.CombinationType.Straight:
				result = Straight.instance.LazyEvaluator(cards, analyzeAllMatch, analyzeFilter);
				hands.AddRange(result);
				if (result.Count > 0)
					SetStraight = result[result.Count - 1].Cards;
				if (!analyzeAllMatch && hands.Count > 0)
					break;
				yield return null;
				goto case ComboType.CombinationType.Flush;
			case ComboType.CombinationType.Flush:
				result = Flush.instance.LazyEvaluator(cards, analyzeAllMatch, analyzeFilter);
				hands.AddRange(result);
				if (result.Count > 0)
					SetFlush = result[result.Count - 1].Cards;
				if (!analyzeAllMatch && hands.Count > 0)
					break;
				yield return null;
				goto case ComboType.CombinationType.FullHouse;
			case ComboType.CombinationType.FullHouse:
				result = FullHouse.instance.LazyEvaluator(cards, analyzeAllMatch, analyzeFilter);
				hands.AddRange(result);
				if (result.Count > 0)
					SetFullHouse = result[result.Count - 1].Cards;
				if (!analyzeAllMatch && hands.Count > 0)
					break;
				yield return null;
				goto case ComboType.CombinationType.FourOfAKind;
			case ComboType.CombinationType.FourOfAKind:
				result = FourOfAKind.instance.LazyEvaluator(cards, analyzeAllMatch, analyzeFilter);
				hands.AddRange(result);
				if (result.Count > 0)
					SetFourOfAKind = result[result.Count - 1].Cards;
				if (!analyzeAllMatch && hands.Count > 0)
					break;
				yield return null;
				goto case ComboType.CombinationType.StraightFlush;
			case ComboType.CombinationType.StraightFlush:
				result = StraightFlush.instance.LazyEvaluator(cards, analyzeAllMatch, analyzeFilter);
				hands.AddRange(result);
				if (result.Count > 0)
					SetStraightFlush = result[result.Count - 1].Cards;
				if (!analyzeAllMatch && hands.Count > 0)
					break;
				yield return null;
				goto case ComboType.CombinationType.RoyalFlush;
			case ComboType.CombinationType.RoyalFlush:
				result = RoyalFlush.instance.LazyEvaluator(cards, analyzeAllMatch, analyzeFilter);
				hands.AddRange(result);
				if (result.Count > 0)
					SetRoyalFlush = result[result.Count - 1].Cards;
				if (!analyzeAllMatch && hands.Count > 0)
					break;
				yield return null;
				goto case ComboType.CombinationType.Dragon;
			case ComboType.CombinationType.Dragon:
				result = Dragon.instance.LazyEvaluator(cards, analyzeAllMatch, analyzeFilter);
				hands.AddRange(result);
				if (result.Count > 0)
					SetDragon = result[result.Count - 1].Cards;
				break;
		}

		if (hands.Count > 0)
		{
			yield return null;
			var curr = hands[0];
			if (hands[hands.Count - 1].Combination == ComboType.CombinationType.Dragon)
			{
				curr = hands[hands.Count - 1];
			}
			else
			{
				for (int i = 0; i < hands.Count; ++i)
				{
					if (GameManager.instance.IsFirstTurn
						&& !(hands[i].Cards[0].Nominal == "3" && hands[i].Cards[0].elements == Card.Elements.Diamond))
						continue;
					if (curr.Combination != hands[i].Combination)
						curr = hands[i];
					if (curr.Combination >= ComboType.CombinationType.Straight)
						break;
				}
			}
			Hint = curr.Cards;
		}
		else
		{
			Hint = null;
		}

		isAnalyzing = false;
		analyzeFilter = null;
		analyzeCombination = ComboType.CombinationType.Invalid;
	}




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


	public void OnSubmit()
	{
		Submit(markedCards);
	}

	public void OnSubmitSuccess()
	{
		markedCards.Clear();
		TotalCard = Cards.Count;
	}

	public void OnSkip()
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

		if (reset)
		{
			for (int i = 0; i < Cards.Count; ++i)
			{
				Cards[i].Select(false);
			}
		}

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
		set
		{
			if (labelCountText)
			{
				labelCountText.text = "" + value;
			}
		}
	}

	public float TimeLeft
	{
		set
		{
			if (labelTimerText)
			{
				labelTimerText.text = "" + Mathf.CeilToInt(value);
			}
		}
	}

	public List<Card> SetStraight
	{
		get { return straightSet; }
		set
		{
			straightSet = value;
			if (shortcut) shortcut.straight.interactable = true;
		}
	}

	public List<Card> SetFlush
	{
		get { return flushtSet; }
		set
		{
			flushtSet = value;
			if (shortcut) shortcut.flush.interactable = true;
		}
	}

	public List<Card> SetFullHouse
	{
		get { return fullHousetSet; }
		set
		{
			fullHousetSet = value;
			if (shortcut) shortcut.fullHouse.interactable = true;
		}
	}

	public List<Card> SetFourOfAKind
	{
		get { return fourOfAKindSet; }
		set
		{
			fourOfAKindSet = value;
			if (shortcut) shortcut.fourOfAKind.interactable = true;
		}
	}

	public List<Card> SetStraightFlush
	{
		get { return straightFlushSet; }
		set
		{
			straightFlushSet = value;
			if (shortcut) shortcut.straightFlush.interactable = true;
		}
	}

	public List<Card> SetRoyalFlush
	{
		get { return royalFlushSet; }
		set
		{
			royalFlushSet = value;
			if (shortcut) shortcut.royalFlush.interactable = true;
		}
	}

	public List<Card> SetDragon
	{
		get { return dragonSet; }
		set
		{
			dragonSet = value;
			if (shortcut) shortcut.dragon.interactable = true;
		}
	}

	public List<Card> Hint
	{
		get { return hintSet; }
		set { hintSet = value; }
	}
}