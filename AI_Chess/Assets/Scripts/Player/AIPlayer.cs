using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : Player
{
    public AIPlayer(int id, Board board) : base(id, board)
    {

    }

    // called when given control of the Board
    public override void OnControlStart()
    {
        Turn bestTurn = FindBestTurn();

        if( bestTurn != null )
        {
            bestTurn.Do();
        }
        else
        {
            Debug.Log("Unable to find a turn");
        }

        // do the best turn, pass control to the next player
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

        return bestTurn;
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
            pieceScore += friendlyMultiplier;



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
