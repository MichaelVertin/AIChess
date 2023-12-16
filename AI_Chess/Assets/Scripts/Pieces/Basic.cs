using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basic : Piece
{
    // prefab to create physicalGO
    public override GameObject physicalPrefab
    {
        get
        {
            return StaticPrefabs.BASIC_PREFAB;
        }
    }


    public override List<Turn> GetTurns()
    {
        // initialize list of turns
        List<Turn> turns = new List<Turn>();

        // returns all positions around the object
        /*
         * If the position contains an enemy, 
         *     removes the enemy from the board
         * If the position contains a friend (including self), 
         *     does not include the position
         */
        for (int xAdj = -1; xAdj <= 1; xAdj++)
        {
            for (int yAdj = -1; yAdj <= 1; yAdj++)
            {
                // calculate the testCoor
                Coor testCoor = new Coor(coor.x + xAdj, coor.y + yAdj);
                Piece enemyPiece = board.GetEnemy(testCoor);
                bool isEmpty = board.IsEmpty(testCoor);

                // only proceed if isEmpty of an enemyPiece
                if (isEmpty || enemyPiece != null)
                {
                    // create the turn object
                    Turn turn = new Turn(board, this.coor, testCoor, this);

                    // remove the enemy piece
                    turn.RemovePiece(enemyPiece);

                    turns.Add(turn);
                }
            }
        }

        return turns;
    }
}
