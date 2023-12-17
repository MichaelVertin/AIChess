using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    // prefab to create physicalGO
    public override GameObject physicalPrefab
    {
        get
        {
            return StaticPrefabs.KING_PREFAB[owner.id];
        }
    }


    public override List<Turn> GetTurns()
    {
        // 1 in any direction
        List<Coor> moves = new List<Coor>
        {
            new Coor(1,1),
            new Coor(1,0),
            new Coor(1,-1),
            new Coor(0,-1),
            new Coor(-1,-1),
            new Coor(-1,0),
            new Coor(-1,1),
            new Coor(0,1),
        };

        // do all moves, remove enemy if necessary
        List<Turn> turns = new List<Turn>();
        foreach (Coor move in moves)
        {
            Turn turn = GetTurnByPosition(this.coor, this.coor + move,
                                          ON_CONTACT_ENEMY.REMOVE_ENEMY);
            if (turn != null)
            {
                turns.Add(turn);
            }
        }

        return turns;
    }

}
