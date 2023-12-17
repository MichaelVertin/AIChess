using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : Piece
{
    // prefab to create physicalGO
    public override GameObject physicalPrefab
    {
        get
        {
            return StaticPrefabs.KNIGHT_PREFAB[owner.id];
        }
    }


    public override List<Turn> GetPieceTurns()
    {
        // 2 in any direction, 1 in the other direction
        List<Coor> moves = new List<Coor>
        {
            new Coor(2,1),
            new Coor(2,-1),
            new Coor(-2,1),
            new Coor(-2,-1),
            new Coor(1,2),
            new Coor(1,-2),
            new Coor(-1,2),
            new Coor(-1,-2),
        };

        // do all moves, remove enemy if necessary
        List<Turn> turns = new List<Turn>();
        foreach( Coor move in moves )
        {
            AddTurnByPosition(turns, this.coor, this.coor + move, 
                                     ON_CONTACT_ENEMY.REMOVE_ENEMY);
        }

        return turns;
    }

}
