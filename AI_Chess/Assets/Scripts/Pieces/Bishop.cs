using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Bishop : Piece
{
    // value of the piece (for AIPlayer)
    public override float Value
    {
        get
        {
            return 3;
        }
    }

    // prefab to create physicalGO
    public override GameObject physicalPrefab
    {
        get
        {
            return StaticPrefabs.BISHOP_PREFAB[owner.id];
        }
    }

    public override List<Turn> GetPieceTurns()
    {
        List<Coor> directions = new List<Coor>
        {
            new Coor(-1,-1),
            new Coor(-1,1),
            new Coor(1,-1),
            new Coor(1,1)
        };

        return GetTurnsByLinearMovements(this.coor, directions, 
                                         ON_CONTACT_ENEMY.REMOVE_ENEMY);
    }

    public override string PieceType()
    {
        return "bishop";
    }

}

