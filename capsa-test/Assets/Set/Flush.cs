using UnityEngine;
using System.Collections.Generic;


public class Flush : Inheritor<Flush> {
	List<Card> spades = new List<Card>();
	List<Card> hearts = new List<Card>();
	List<Card> clubs = new List<Card>();
	List<Card> diamonds = new List<Card>();

	protected override void PreEvaluate () {
		spades.Clear ();
		hearts.Clear ();
		clubs.Clear ();
		diamonds.Clear ();
	}

	public override void Evaluate(int index) {
		base.Evaluate (index);

		switch (card [index].elements) {
		case Card.Elements.Spade:
			spades.Add(card[index]);
			break;
		case Card.Elements.Heart:
			hearts.Add(card[index]);
			break;
		case Card.Elements.Club:
			clubs.Add(card[index]);
			break;
		case Card.Elements.Diamond:
			diamonds.Add(card[index]);
			break;
		}
	}

	protected override void PostEvaluate () {
		FilterCards (spades);
		FilterCards (hearts);
		FilterCards (clubs);
		FilterCards (diamonds);

		results.Sort ();
	}

	void FilterCards(List<Card> card) {
		for (int i = 0; i < Mathf.Max(0, card.Count - 4); ++i) {
			List<Card> set = card.GetRange (i, 5); 
			if (filter(set[4]))
				results.Add (new ComboType(set, set[4], ComboType.CombinationType.Flush));
		}
	}
	
	public override bool IsValid(List<Card> cards, bool isSorted = false) { 
		if (cards.Count != 5)
			return false;
		
		return cards[0].elements == cards[1].elements
			&& cards[1].elements == cards[2].elements
			&& cards[2].elements == cards[3].elements
			&& cards[3].elements == cards[4].elements;
	}
}