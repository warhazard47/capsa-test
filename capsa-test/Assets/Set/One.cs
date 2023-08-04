using System.Collections.Generic;

public class One : Inheritor<One> {
	public override void Evaluate(int index) {
		if (filter(card[index])) {
			var set = new List<Card>();
			set.Add(card[index]);
			results.Add(new ComboType(set, set[0], ComboType.CombinationType.One));
		}
	}

	public override bool IsValid(List<Card> cards, bool isSorted = false) { 
		return cards.Count == 1;
	}
}