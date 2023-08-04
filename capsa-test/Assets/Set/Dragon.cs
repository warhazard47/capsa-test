using System.Collections.Generic;

// 13 card straight in order
public class Dragon : Inheritor<Dragon> {
	List<Card> dragon = new List<Card>();

	protected override void PreEvaluate () {
		dragon.Clear ();
		dragon.Add (card[0]);
	}

	public override void Evaluate(int index) {
		base.Evaluate (index);

		if (dragon[dragon.Count - 1].Score + 1 == card[index].Score) {
			dragon.Add (card [index]);
		}
	}

	protected override void PostEvaluate () {
		if (dragon.Count == 13)
			results.Add (new ComboType(dragon, dragon[dragon.Count - 1]	, ComboType.CombinationType.Dragon));
	}

	public override bool IsValid(List<Card> cards, bool isSorted = false) { 
		if (cards.Count != 13)
			return false;

		if (!isSorted)
			cards.Sort ();

		for (int i = 0; i < cards.Count - 1; ++i) {
			if (cards[i].Score + 1 != cards[i + 1].Score)
				return false;
		}

		return true;
	}
}