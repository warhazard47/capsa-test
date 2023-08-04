using System.Collections;
using System.Collections.Generic;

public class FourOfAKind : Inheritor<FourOfAKind> {
	
	public override void Evaluate(int index) {
		base.Evaluate (index);

		if (card.Count < 5 || index > card.Count - 4)
			return;

		Card [] quad = { card[index], card[index + 1], card[index + 2], card[index + 3] };
		if (quad[0].Nominal == quad[3].Nominal && filter(quad[3])) {
			var set = new List<Card>();
			set.Add(card[index == 0 ? 4 : 0]);
			set.AddRange(quad);
			results.Add(new ComboType(set, set[4], ComboType.CombinationType.FourOfAKind));
		}
	}
	
	public override bool IsValid(List<Card> cards, bool isSorted = false) { 
		if (cards.Count != 5)
			return false;

		if (!isSorted)
			cards.Sort ();

		return cards[0].Nominal == cards[3].Nominal
			|| cards[1].Nominal == cards[4].Nominal;
	}
}