using System.Collections.Generic;

public interface ICardContainer
{
    public bool IsSingleCardSelector { get; }
    public bool CheckIfCardCanBeAdd(Card card);
    public void FlipLastBackCard();
    public void AddExistingCard(List<Card> cards,bool flipToFront = false);
    public void AddExistingCard(Card cards, bool flipToFront = false);
    public void RemoveFrontCard(List<Card> cards);
    public void ResetCard(List<Card> cards);
}
