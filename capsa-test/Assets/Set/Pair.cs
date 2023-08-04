using System.Collections.Generic;

// Pair card with same number
public class Pair : Inheritor<Pair> {

	public override void Evaluate(int index) {
		base.Evaluate (index);

		// Max index is (n-1) - 1, don't evaluate
		if (index > card.Count - 2)
			return;

		Card [] pair = { card[index], card[index + 1] };
		if (pair[0].Score == pair[1].Score && filter(pair[1])) {
			results.Add(new ComboType(card, pair[1], ComboType.CombinationType.Pair));
		}
	}

	public override bool IsValid(List<Card> cards, bool isSorted = false) { 
		if (cards.Count != 2)
			return false;

		return cards [0].Score == cards [1].Score;
	}
}