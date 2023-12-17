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
    // GameObjects to access from scripts
    public static List<GameObject> PAWN_PREFAB = new List<GameObject>();
    public static List<GameObject> ROOK_PREFAB = new List<GameObject>();
    public static List<GameObject> KNIGHT_PREFAB = new List<GameObject>();
    public static List<GameObject> BISHOP_PREFAB = new List<GameObject>();
    public static List<GameObject> KING_PREFAB = new List<GameObject>();
    public static List<GameObject> QUEEN_PREFAB = new List<GameObject>();

    // GameObjects to assign from inspector
    public GameObject BLACK_PAWN_PREFAB_INSPECTOR = null;
    public GameObject BLACK_ROOK_PREFAB_INSPECTOR = null;
    public GameObject BLACK_KNIGHT_PREFAB_INSPECTOR = null;
    public GameObject BLACK_BISHOP_PREFAB_INSPECTOR = null;
    public GameObject BLACK_KING_PREFAB_INSPECTOR = null;
    public GameObject BLACK_QUEEN_PREFAB_INSPECTOR = null;
    public GameObject WHITE_PAWN_PREFAB_INSPECTOR = null;
    public GameObject WHITE_ROOK_PREFAB_INSPECTOR = null;
    public GameObject WHITE_KNIGHT_PREFAB_INSPECTOR = null;
    public GameObject WHITE_BISHOP_PREFAB_INSPECTOR = null;
    public GameObject WHITE_KING_PREFAB_INSPECTOR = null;
    public GameObject WHITE_QUEEN_PREFAB_INSPECTOR = null;


    // Set all static values from non-static values on Awake
    void Awake()
    {
        PAWN_PREFAB.Add(WHITE_PAWN_PREFAB_INSPECTOR);
        PAWN_PREFAB.Add(BLACK_PAWN_PREFAB_INSPECTOR);
        KNIGHT_PREFAB.Add(WHITE_KNIGHT_PREFAB_INSPECTOR);
        KNIGHT_PREFAB.Add(BLACK_KNIGHT_PREFAB_INSPECTOR);
        BISHOP_PREFAB.Add(WHITE_BISHOP_PREFAB_INSPECTOR);
        BISHOP_PREFAB.Add(BLACK_BISHOP_PREFAB_INSPECTOR);
        KING_PREFAB.Add(WHITE_KING_PREFAB_INSPECTOR);
        KING_PREFAB.Add(BLACK_KING_PREFAB_INSPECTOR);
        QUEEN_PREFAB.Add(WHITE_QUEEN_PREFAB_INSPECTOR);
        QUEEN_PREFAB.Add(BLACK_QUEEN_PREFAB_INSPECTOR);
        ROOK_PREFAB.Add(WHITE_ROOK_PREFAB_INSPECTOR);
        ROOK_PREFAB.Add(BLACK_ROOK_PREFAB_INSPECTOR);
    }
}
