using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Bishop : Piece
{
    // prefab to create physicalGO
    public override GameObject physicalPrefab
    {
        get
        {
            return StaticPrefabs.BISHOP_PREFAB;
        }
    }

    public override List<Turn> GetTurns()
    {
        List<Turn> turns = new List<Turn>();
        List<Coor> directions = new List<Coor>();
        directions.Add(new Coor(1, 1));
        directions.Add(new Coor(1, -1));
        directions.Add(new Coor(-1, 1));
        directions.Add(new Coor(-1, -1));

        foreach( Coor direction in directions )
        {
            turns.AddRange(GetTurnsByLinearMovement(
                               this.coor, direction, 
                               ON_CONTACT_ENEMY.REMOVE_ENEMY));
        }
        return turns;
    }
}

