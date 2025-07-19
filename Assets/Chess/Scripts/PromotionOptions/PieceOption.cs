using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceOption : MonoBehaviour
{
    private Turn turn;
    private UserPlayer owner;

    public void Init(Turn turn, UserPlayer owner)
    {
        this.turn = turn;
        this.owner = owner;
    }

    // called when clicked by mouse, 
    //    notifies menu that this was selected
    public void OnMouseDown()
    {
        owner.OnPieceOptionSelect(turn);
    }
}
