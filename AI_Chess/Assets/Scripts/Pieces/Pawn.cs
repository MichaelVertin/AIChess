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
            // if first time moving, also try to move two up (also exluding enemy)
            if( turnCount == 0 )
            {
                AddTurnByPosition(turns, this.coor, this.coor + direction * 2,
                         ON_CONTACT_ENEMY.EXCLUDE);
            }
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

        foreach( Turn turn in turns )
        {
            if( turn.endCoor.y == owner.promotionY)
            {
                Piece queen = board.CreateQueen(turn.endCoor, owner);
                turn.AddPiece(queen);
                turn.RemovePiece(this);
            }
        }
        return turns;
    }
}
