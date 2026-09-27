using Wolfgang.Hawsey.Engine.Players;

namespace Wolfgang.Hawsey.Engine.Bidding;

/// <summary>
/// One bid made in the current round: who made it and what it was. A UI shows these
/// as each seat's bid ("Pass", "8", "Hawsey").
/// </summary>
/// <param name="Player">The player who bid.</param>
/// <param name="Action">The bid: a pass, a number bid, or Hawsey.</param>
public sealed record PlacedBid(PlayerPosition Player, BidAction Action);
