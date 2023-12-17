using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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


    public override List<Turn> GetPieceTurns()
    {
        List<Turn> turns = new List<Turn>();
        Coor right = new Coor(1, 0);
        Coor left = new Coor(-1, 0);

        // set direction up if id0, otherwise, direction downwards
        Coor direction = owner.direction;

        // try to move one up, excluding enemy
        if(AddTurnByPosition(turns, this.coor, this.coor + direction,
                                           ON_CONTACT_ENEMY.EXCLUDE))
        {
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

        Coor forwardRight = this.coor + direction + right;
        // if any enemy is in the direction to the right, 
        //     get the turn removing the enemy
        if (board.GetEnemy(forwardRight) != null)
        {
            AddTurnByPosition(turns, this.coor, forwardRight,
                                     ON_CONTACT_ENEMY.REMOVE_ENEMY);
        }

        Coor forwardLeft = this.coor + direction + left;
        // if any enemy is in the direction to the left, 
        //     get the turn removing that enemy
        if (board.GetEnemy(forwardLeft) != null)
        {
            AddTurnByPosition(turns, this.coor, forwardLeft,
                                     ON_CONTACT_ENEMY.REMOVE_ENEMY);
        }
        return turns;
    }
}
