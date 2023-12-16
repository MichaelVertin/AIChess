using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Player
{
    public int id;
    public Color color;
    public Player( int id )
    {
        this.id = id;
        if( this.id == 0 )
        {
            this.color = Color.white;
        }
        else
        {
            this.color = Color.black;
        }
    }

    // called when given control of the Board
    public void OnControlStart( Board board )
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
            if( randomPiece != null )
            {
                List<Turn> turns = randomPiece.GetLegalTurns();
                if( turns.Count <= 0 )
                {
                    continue;
                }
                randomInd = Random.Range(0, turns.Count);
                Turn randomTurn = turns[randomInd];
                if( randomTurn.DoIfVerified() )
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