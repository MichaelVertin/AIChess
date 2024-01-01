using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// menu for user to select a piece
public class PieceMenu : MonoBehaviour
{
    List<Turn> turns;
    UserPlayer controller;

    public void Init(List<Turn> turns, UserPlayer controller)
    {
        this.turns = turns;
        this.controller = controller;
        Draw();
    }

    // index to identify hardcoded piece option prefab
    private int prefabInd = 0;

    // hardcoded option prefab to display to user
    // automatically iterates each time it is read
    private GameObject prefab
    {
        get
        {
            GameObject prefab = null;

            switch (prefabInd)
            {
                case 0:
                    prefab = StaticPrefabs.QUEEN_OPTION_PREFAB;
                    break;
                case 1:
                    prefab = StaticPrefabs.ROOK_OPTION_PREFAB;
                    break;
                case 2:
                    prefab = StaticPrefabs.BISHOP_OPTION_PREFAB;
                    break;
                case 3:
                    prefab = StaticPrefabs.KNIGHT_OPTION_PREFAB;
                    break;
            }
            prefabInd++;
            return prefab;
        }
    }

    // creates PieceOptions and scales this and options accordingly
    private void Draw()
    {
        Vector3 segmentScale = new Vector3( 1f / (float)turns.Count, 1f, 1f );
        float menuHeight = .50f;
        float menuWidth = menuHeight * (float)turns.Count;

        this.transform.localScale = new Vector3(menuWidth, menuHeight, 1f);
        this.transform.position = Vector3.zero;
        Vector3 position = Vector3.zero;
        position.x = -6f;
        for( int i = 0; i < turns.Count; i++ )
        {
            PieceOption pieceOption = Instantiate<GameObject>(prefab, this.transform).GetComponent<PieceOption>();
            pieceOption.transform.localScale = segmentScale;
            pieceOption.transform.position = position;
            pieceOption.Init(turns[i], controller);
            position.x += 4f;
        }
    }

    // called when a PieceOption is selected
    // notifies controller the turn that was selected
    private void OnSelectTurn(Turn turn)
    {
        // temporary: needs new function to call
        controller.OnSelectCoordinate(new Coor());
    }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }
}
