using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    // prefab to create physicalGO
    public override GameObject physicalPrefab
    {
        get
        {
            return StaticPrefabs.PAWN_PREFAB[owner.id];
        }
    }


    public override List<Turn> GetTurns()
    {
        List<Turn> turns = new List<Turn>();
        Coor right = new Coor(1, 0);
        Coor left = new Coor(-1, 0);

        // set direction up if id0, otherwise, direction downwards
        Coor direction = new Coor(0, 1);
        if ( owner.id == 1 )
        {
            direction *= -1;
        }

        // try to move one up, excluding enemy
        Turn turn;
        turn = GetTurnByPosition(this.coor, this.coor + direction,
                                 ON_CONTACT_ENEMY.EXCLUDE);
        if( turn != null )
        {
            turns.Add(turn);
            /* // waiting for moveCount to be implemented
            // if albe to move one up, try again (also exluding enemy)
            turn = GetTurnByPosition(this.coor, this.coor + direction * 2, 
                                     ON_CONTACT_ENEMY.EXCLUDE);
            if( turn != null)
            {
                turns.Add(turn);
            }
            */
        }

        Coor coor;
        // if any enemy is in the direction to the right, 
        //     get the turn removing the enemy
        coor = this.coor + direction + right;
        if(board.GetEnemy(coor) != null)
        {
            turns.Add(GetTurnByPosition(this.coor, coor, 
                                        ON_CONTACT_ENEMY.REMOVE_ENEMY));
        }

        // if any enemy is in the direction to the left, 
        //     get the turn removing that enemy
        coor = this.coor + direction + left;
        if (board.GetEnemy(coor) != null)
        {
            turns.Add(GetTurnByPosition(this.coor, coor,
                                        ON_CONTACT_ENEMY.REMOVE_ENEMY));
        }

        return turns;
    }
}
