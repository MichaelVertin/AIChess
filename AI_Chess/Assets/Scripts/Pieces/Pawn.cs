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
            return StaticPrefabs.PAWN_PREFAB;
        }
    }


    public override List<Turn> GetTurns()
    {
        List<Turn> turns = new List<Turn>();

        return turns;
    }

}
