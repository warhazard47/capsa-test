using System.Collections;
using System.Collections.Generic;

// 1 Triple + 1 Pair
public class FullHouse : Inheritor<FullHouse> {
	public List<ComboType> triples = null;
	public List<ComboType> pairs = null;
	Triple tripleEvaluator = new Triple();
	Pair pairEvaluator = new Pair();

	protected override void PreEvaluate () {
		if (triples != null || pairs != null)
			return;

		tripleEvaluator.Begin (card, filter);
		pairEvaluator.Begin (card, filter);
	}

	public override void Evaluate(int index) {
		base.Evaluate (index);

		if (triples != null || pairs != null)
			return;

		// Evaluate each card
		tripleEvaluator.Evaluate (index);
		pairEvaluator.Evaluate (index);
	}

	protected override void PostEvaluate () {
		if (triples == null && pairs == null) {
			tripleEvaluator.End ();
			pairEvaluator.End ();
			triples = tripleEvaluator.Results;
			pairs = pairEvaluator.Results;
		}

		List<Card> fullHouse = new List<Card> (5);
		for (int tIndex = 0; tIndex < triples.Count; ++tIndex) {
			fullHouse.Clear();
			for (int pIndex = 0; pIndex < pairs.Count; ++pIndex) {
				if (triples[tIndex].Cards[0].Nominal != pairs[pIndex].Cards[0].Nominal) {
					fullHouse.AddRange(pairs[pIndex].Cards);
					fullHouse.AddRange(triples[tIndex].Cards);
					results.Add(new ComboType(fullHouse, triples[tIndex].Cards[2], ComboType.CombinationType.FullHouse));
					break;
				}
			}
		}

		triples = null;
		pairs = null;
	}
	
	public override bool IsValid(List<Card> cards, bool isSorted = false) { 
		if (cards.Count != 5)
			return false;
	
		if (!isSorted)
			cards.Sort ();
		
		return cards[0].Nominal == cards[1].Nominal && cards[1].Nominal == cards[2].Nominal && cards[3].Nominal == cards[4].Nominal
			|| cards[0].Nominal == cards[1].Nominal && cards[2].Nominal == cards[3].Nominal && cards[3].Nominal == cards[4].Nominal;
	}
}