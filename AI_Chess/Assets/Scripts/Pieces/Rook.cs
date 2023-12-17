using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : Piece
{
    // prefab to create physicalGO
    public override GameObject physicalPrefab
    {
        get
        {
            return StaticPrefabs.ROOK_PREFAB[owner.id];
        }
    }
    public override List<Turn> GetPieceTurns()
    {
        List<Coor> directions = new List<Coor>
        {
            new Coor(-1,0),
            new Coor(0,-1),
            new Coor(0,1),
            new Coor(1,0)
        };

        return GetTurnsByLinearMovements(this.coor, directions,
                                         ON_CONTACT_ENEMY.REMOVE_ENEMY);
    }
}