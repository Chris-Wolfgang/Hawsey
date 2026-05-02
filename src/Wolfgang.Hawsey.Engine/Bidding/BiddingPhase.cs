namespace Wolfgang.Hawsey.Engine;

/// <summary>
/// Manages a single round of bidding. Players bid clockwise starting from
/// the player to the dealer's left. Each bid must beat the previous bid.
/// If no one bids, the dealer is stuck at the minimum.
/// </summary>
public sealed class BiddingPhase
{
    private readonly PlayerPosition _dealer;
    private readonly int _minimumBid;
    private readonly PlayerPosition[] _biddingOrder;
    private readonly BidAction?[] _bids;
    private int _currentBidderIndex;
    private int _highestBid;
    private PlayerPosition? _highestBidder;
    private bool _isComplete;
    private bool _isHawsey;



    /// <summary>
    /// Initializes a new instance of the <see cref="BiddingPhase"/> class.
    /// </summary>
    /// <param name="dealer">The dealer's position.</param>
    /// <param name="minimumBid">The minimum bid allowed.</param>
    public BiddingPhase(PlayerPosition dealer, int minimumBid)
    {
        _dealer = dealer;
        _minimumBid = minimumBid;
        _biddingOrder = new PlayerPosition[4];
        _bids = new BidAction?[4];
        _currentBidderIndex = 0;
        _highestBid = 0;

        // Clockwise from dealer's left
        var position = dealer.NextClockwise();

        for (var i = 0; i < 4; i++)
        {
            _biddingOrder[i] = position;
            position = position.NextClockwise();
        }
    }



    /// <summary>
    /// Gets the player who should bid next, or <c>null</c> if bidding is complete.
    /// </summary>
    /// <returns>The next bidder's position, or <c>null</c>.</returns>
    public PlayerPosition? GetNextBidder()
    {
        if (_isComplete)
        {
            return null;
        }

        return _biddingOrder[_currentBidderIndex];
    }



    /// <summary>
    /// Places a bid for the specified player.
    /// </summary>
    /// <param name="player">The player placing the bid.</param>
    /// <param name="action">The bid action.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if bidding is complete, it's not the player's turn, or the bid is invalid.
    /// </exception>
    public void PlaceBid(PlayerPosition player, BidAction action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (_isComplete)
        {
            throw new InvalidOperationException("Bidding is already complete.");
        }

        if (_biddingOrder[_currentBidderIndex] != player)
        {
            throw new InvalidOperationException
            (
                $"It is not {player}'s turn to bid. Expected {_biddingOrder[_currentBidderIndex]}."
            );
        }

        switch (action)
        {
            case BidAction.PassBid:
                _bids[_currentBidderIndex] = action;
                _currentBidderIndex++;

                if (_currentBidderIndex >= 4)
                {
                    _isComplete = true;
                }

                break;

            case BidAction.NumberBid numberBid:
                ValidateNumberBid(numberBid);
                _bids[_currentBidderIndex] = action;
                _highestBid = numberBid.Amount;
                _highestBidder = player;
                _currentBidderIndex++;

                if (_currentBidderIndex >= 4)
                {
                    _isComplete = true;
                }

                break;

            case BidAction.HawseyBid:
                _bids[_currentBidderIndex] = action;
                _isHawsey = true;
                _highestBidder = player;
                _isComplete = true;
                break;

            default:
                throw new InvalidOperationException($"Unknown bid action type: {action.GetType().Name}");
        }
    }



    /// <summary>
    /// Gets the bidding result after bidding is complete.
    /// </summary>
    /// <returns>The bidding result.</returns>
    /// <exception cref="InvalidOperationException">Thrown if bidding is not yet complete.</exception>
    public BiddingResult GetResult()
    {
        if (!_isComplete)
        {
            throw new InvalidOperationException("Bidding is not yet complete.");
        }

        if (_isHawsey)
        {
            return new BiddingResult(_highestBidder!.Value, 24, isHawsey: true, isStuck: false);
        }

        if (!_highestBidder.HasValue)
        {
            // No one bid — dealer is stuck
            return new BiddingResult(_dealer, _minimumBid, isHawsey: false, isStuck: true);
        }

        return new BiddingResult(_highestBidder.Value, _highestBid, isHawsey: false, isStuck: false);
    }



    /// <summary>
    /// Gets whether bidding is complete.
    /// </summary>
    public bool IsComplete => _isComplete;



    private void ValidateNumberBid(BidAction.NumberBid bid)
    {
        if (bid.Amount < _minimumBid)
        {
            throw new InvalidOperationException
            (
                $"Bid of {bid.Amount} is below the minimum bid of {_minimumBid}."
            );
        }

        if (bid.Amount <= _highestBid)
        {
            throw new InvalidOperationException
            (
                $"Bid of {bid.Amount} does not beat the current highest bid of {_highestBid}."
            );
        }
    }
}
