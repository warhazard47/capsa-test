using System.Collections.Generic;


// Three card with same number
public class Triple : Inheritor<Triple> {


	public override void Evaluate(int index) {
		// Max index is (n-1) - 2, don't evaluate
		if (index > card.Count - 3)
			return;

		// Compare first and last nominal, since it's already sorted
		Card [] triple = { card[index], card[index + 1], card[index + 2] };
		if (triple[0].Score == triple[1].Score && triple[1].Score == triple[2].Score && filter(triple[2])) {
			results.Add(new ComboType(new List<Card>(triple), triple[2], ComboType.CombinationType.Triple));
		}
	}
	
	public override bool IsValid(List<Card> cards, bool isSorted = false) { 
		if (cards.Count != 3)
			return false;
		
		return cards [0].Nominal == cards [1].Nominal
			&& cards [1].Nominal == cards [2].Nominal;
	}
}