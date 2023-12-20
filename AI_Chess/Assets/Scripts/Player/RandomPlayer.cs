using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPlayer : Player
{
    public RandomPlayer(int id) : base(id)
    {

    }

    // called when given control of the Board
    public override void OnControlStart(Board board)
    {
        int num_iter = 0;
        // try to make a move 100 times
        while (num_iter < 100)
        {
            // find a random piece
            int randomInd;
            if (board.pieces.Count <= 0)
            {
                continue;
            }
            randomInd = Random.Range(0, board.pieces.Count);
            Piece randomPiece = board.pieces[randomInd];

            // check for a valid randomPiece
            if (randomPiece != null)
            {
                List<Turn> turns = randomPiece.GetLegalTurns();
                if (turns.Count <= 0)
                {
                    continue;
                }
                randomInd = Random.Range(0, turns.Count);
                Turn randomTurn = turns[randomInd];
                if (randomTurn.DoIfVerified())
                {
                    board.Invoke("PassControl", 1f);
                    return;
                }
            }
            num_iter++;
        }
        board.Invoke("PassControl", 1f);
    }
}
