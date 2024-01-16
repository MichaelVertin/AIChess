using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using static UnityEditorInternal.VersionControl.ListControl;

class Pair<T1,T2>
{
    public T1 First;
    public T2 Second;

    public Pair()
    {
        
    }

    public Pair(T1 firstVal, T2 secondVal)
    {
        First = firstVal;
        Second = secondVal;
    }
}



public class AIPlayer : Player
{
    public AIPlayer(int id, Board board) : base(id, board)
    {

    }

    // called when given control of the Board
    public override void OnControlStart()
    {
        // use alphabeta algorithm to find the best turn
        Turn bestTurn = alphabeta(3.01f).First;

        // if alphabeta failed (all solutions checkmate), do 1-level search
        if( bestTurn == null )
        {
            bestTurn = FindBestTurn();
        }


        // desired turn obtained: do the turn if it exists
        if( bestTurn != null )
        {
            bestTurn.Do();
        }
        else
        {
            Debug.Log("Unable to find a turn");
        }

        // pass control to the next player
        board.PassControl();
    }

    public Turn FindBestTurn()
    {
        // test all legal turns
        List<Turn> turns = board.GetLegalTurns();

        // identify the best score and corresponding turn
        float maxScore = Mathf.NegativeInfinity;
        Turn bestTurn = null;
        foreach (Turn turn in turns)
        {
            // select the new turn if better than the previous best
            float turnScore = ScoreTurn(turn);
            if (turnScore > maxScore)
            {
                bestTurn = turn;
                maxScore = turnScore;
            }
        }

        // case all turns score checkmate, but one still valid
        if( bestTurn == null && turns.Count > 0 )
        {
            bestTurn = turns[0];
        }

        return bestTurn;
    }

    public Turn FindWorstTurn()
    {
        // test all legal turns
        List<Turn> turns = board.GetLegalTurns();

        // identify the worst score and corresponding turn
        float minScore = Mathf.Infinity;
        Turn worstTurn = null;
        foreach (Turn turn in turns)
        {
            // select the new turn if worse than the previous worst
            float turnScore = ScoreTurn(turn);
            if (turnScore < minScore)
            {
                worstTurn = turn;
                minScore = turnScore;
            }
        }

        // case all turns score checkmate, but one still valid
        if (worstTurn == null && turns.Count > 0)
        {
            worstTurn = turns[0];
        }

        return worstTurn;
    }


    // SOURCE: https://en.wikipedia.org/wiki/Alpha%E2%80%93beta_pruning
    private Pair<Turn,float> alphabeta(float depth, 
                                       float alpha = Mathf.NegativeInfinity, 
                                       float beta = Mathf.Infinity, 
                                       bool maximizing = true)
    {
        // end of recursion: return null turn, current state of board
        if( depth < 1 )
        {
            return new Pair<Turn,float>(null,ScoreCurrentState());
        }

        List<Turn> legalTurns = board.GetLegalTurns();

        // no turns available: player in checkmate or stalemate
        if (legalTurns.Count == 0)
        {
            // TODO: for now, give maximum penalty for not being able to move
            float worstScore;
            if (maximizing)
            {
                worstScore = Mathf.NegativeInfinity;
            }
            else
            {
                worstScore = Mathf.Infinity;
            }
            return new Pair<Turn, float>(null, worstScore);
        }

        Pair<Turn, float> bestState = new Pair<Turn, float>(null, 0);

        // maximizing player: choose the turn that maximizes the score (WRT this)
        if ( maximizing )
        {
            bestState.Second = Mathf.NegativeInfinity;
            foreach( Turn turn in legalTurns )
            {
                // move into the next node before continuing to iterate
                turn.Do();
                Pair<Turn,float> testState = alphabeta(depth - 1f, alpha, beta, !maximizing);
                turn.Undo();

                // set the new best state
                if(testState.Second > bestState.Second )
                {
                    bestState = testState;
                    testState.First = turn;
                }

                // short exit
                if(bestState.Second >= beta )
                {
                    break;
                }
                alpha = Mathf.Max(alpha, bestState.Second);
            }
        }

        // mimimizing player: choose the turn that minimizes the score (WRT this)
        else
        {
            bestState.Second = Mathf.Infinity;
            foreach( Turn turn in legalTurns)
            {
                // move into the next node before continuing to iterate
                turn.Do();
                Pair<Turn, float> testState = alphabeta(depth - 1f, alpha, beta, !maximizing);
                turn.Undo();

                // set the new best state
                if ( testState.Second < bestState.Second )
                {
                    bestState = testState;
                    testState.First = turn;
                }
                // short exit
                if (bestState.Second <= alpha )
                {
                    break;
                }
                beta = Mathf.Min(beta, bestState.Second);
            }
        }

        return bestState;
    }


    public float ScoreTurn(Turn turn)
    {
        // do the turn
        turn.Do();

        // select the new turn if better than the previous best
        float score = ScoreCurrentState();

        // undo the turn
        turn.Undo();

        // return resulting score
        return score;
    }


    // basic score
    public float ScoreCurrentState()
    {
        Dictionary<Coor, float> protectionMap = new Dictionary<Coor, float>();

        // options: how many options friends have compared to enemies
        float optionsScore = 0f;
        float pieceScore = 0f;
        float protectionScore = 0f;
        foreach (Piece piece in board.pieces)
        {
            // friendly piece if owned by this
            bool isFriendly = ReferenceEquals(piece.owner, this);
            float friendlyMultiplier = 1f;
            if (!isFriendly) { friendlyMultiplier *= -1f; }



            // options: how many options friends have compared to enemies
            List<Turn> turns = piece.GetTurns(false); // would GetLegalTurns be better? 
            optionsScore += turns.Count * friendlyMultiplier;



            // pieces: how many friends are there than enemies
            pieceScore += friendlyMultiplier * piece.Value;



            // protection: how many critical positions are protected by friends compared to enemies
            foreach (Turn turn in turns)
            {
                Coor finalCoor = turn.mainEndCoor;

                // critical point if piece at the finalCoor
                if (board.GetPiece(finalCoor) != null)
                {
                    // set default to 0, then add/subtract 1
                    if (!protectionMap.ContainsKey(finalCoor))
                    {
                        protectionMap[finalCoor] = 0f;
                    }
                    protectionMap[finalCoor] += 1f * friendlyMultiplier;
                }
            }
        }

        foreach (KeyValuePair<Coor, float> protectionPair in protectionMap)
        {
            // add sqrt of protection score, include sign after
            protectionScore += Mathf.Sqrt(Mathf.Abs(protectionPair.Value)) * Mathf.Sign(protectionPair.Value);
        }

        // piece: 100 points
        // protection: 1 point
        // option: 4 point          // NOTE: this is identical to protection,
                                    //       but also applies to empty positions
                                    // protection should be more important, 
                                    //   but protecting a piece not in danger 
                                    //   isn't as beneficial
        float totalScore = pieceScore * 100f + optionsScore * 4f + protectionScore * 1f;
        return totalScore;
    }
}
