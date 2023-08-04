using System.Collections.Generic;

public class StraightFlush : Inheritor<StraightFlush> {
	List<Card> bucketSpades = new List<Card>();
	List<Card> specialSpades = new List<Card>();

	List<Card> bucketHearts = new List<Card>();
	List<Card> specialHearts = new List<Card>();

	List<Card> bucketClubs = new List<Card>();
	List<Card> specialClubs = new List<Card>();

	List<Card> bucketDiamonds = new List<Card>();
	List<Card> specialDiamonds = new List<Card>();

	protected override void PreEvaluate () {
		Straight.PreEvaluateStraight (ref bucketSpades, ref specialSpades);
		Straight.PreEvaluateStraight (ref bucketHearts, ref specialHearts);
		Straight.PreEvaluateStraight (ref bucketClubs, ref specialClubs);
		Straight.PreEvaluateStraight (ref bucketDiamonds, ref specialDiamonds);
	}
	
	public override void Evaluate(int index) {
		switch (card[index].elements) {
		case Card.Elements.Spade:
			Straight.EvaluateStraight(ref results, card[index], ref bucketSpades, ref specialSpades, filter);
			break;
		case Card.Elements.Heart:
			Straight.EvaluateStraight(ref results, card[index], ref bucketHearts, ref specialHearts, filter);
			break;
		case Card.Elements.Club:
			Straight.EvaluateStraight(ref results, card[index], ref bucketClubs, ref specialClubs, filter);
			break;
		case Card.Elements.Diamond:
			Straight.EvaluateStraight(ref results, card[index], ref bucketDiamonds, ref specialDiamonds, filter);
			break;
		}
	}

	protected override void PostEvaluate () {
		Straight.PostEvaluateStraight (ref results, ref bucketSpades, ref specialSpades);
		Straight.PostEvaluateStraight (ref results, ref bucketHearts, ref specialHearts);
		Straight.PostEvaluateStraight (ref results, ref bucketClubs, ref specialClubs);
		Straight.PostEvaluateStraight (ref results, ref specialDiamonds, ref specialDiamonds);

		results.Sort ();
	}
	
	public override bool IsValid(List<Card> cards, bool isSorted = false) { 
		return Flush.instance.IsValid(cards, isSorted) && Straight.instance.IsValid(cards, isSorted);
	}
}