using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
Contains static prefabs for pieces
Assigned in the inspector (must be instantiated before accessed)
Accessed using StaticPrefabs.<PIECE_NAME>_PREFAB
NOTE: only intended for one StaticPrefab to be instantiated
      additional instances created will override previous instances
*/
public class StaticPrefabs : MonoBehaviour
{
    // basic prefabs
    public static GameObject BASIC_PREFAB = null;
    public GameObject BASIC_PREFAB_INSPECTOR = null;

    // GameObjects to access from scripts
    public static GameObject PAWN_PREFAB;
    public static GameObject ROOK_PREFAB;
    public static GameObject KNIGHT_PREFAB;
    public static GameObject BISHOP_PREFAB;
    public static GameObject KING_PREFAB;
    public static GameObject QUEEN_PREFAB;

    // GameObjects to assign from inspector
    public GameObject PAWN_PREFAB_INSPECTOR = null;
    public GameObject ROOK_PREFAB_INSPECTOR = null;
    public GameObject KNIGHT_PREFAB_INSPECTOR = null;
    public GameObject BISHOP_PREFAB_INSPECTOR = null;
    public GameObject KING_PREFAB_INSPECTOR = null;
    public GameObject QUEEN_PREFAB_INSPECTOR = null;


    // Set all static values from non-static values on Awake
    void Awake()
    {
        BASIC_PREFAB = BASIC_PREFAB_INSPECTOR;

        PAWN_PREFAB = PAWN_PREFAB_INSPECTOR;
        ROOK_PREFAB = ROOK_PREFAB_INSPECTOR;
        KNIGHT_PREFAB = KNIGHT_PREFAB_INSPECTOR;
        BISHOP_PREFAB = BISHOP_PREFAB_INSPECTOR;
        KING_PREFAB = KING_PREFAB_INSPECTOR;
        QUEEN_PREFAB = QUEEN_PREFAB_INSPECTOR;
    }
}
