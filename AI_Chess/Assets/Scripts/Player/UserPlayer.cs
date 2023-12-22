using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Coor_Turn_Pair
{
    public Turn turn;
    public Coor coor;
    public Coor_Turn_Pair(Coor coor, Turn turn)
    {
        this.turn = turn;
        this.coor = coor;
    }
}


// map of start/end coor to list of applicable turns
public class UserOptionsMap
{
    Dictionary<Coor, Dictionary<Coor,List<Turn>>> options =
               new Dictionary<Coor, Dictionary<Coor, List<Turn>>>();

    public List<Turn> this[Coor start, Coor end]
    {
        get
        {
            Ensure(start, end);
            return options[start][end];
        }
    }

    private void Ensure( Coor start, Coor end )
    {
        Ensure(start);
        if( !options[start].ContainsKey(end))
        {
            options[start][end] = new List<Turn>();
        }
    }

    private void Ensure(Coor start)
    {
        if (!options.ContainsKey(start))
        {
            options[start] = new Dictionary<Coor, List<Turn>>();
        }
    }

    public void Add(Turn turn)
    {
        this[turn.mainStartCoor, turn.mainEndCoor].Add(turn);
    }

    public bool StartsWith(Coor start)
    {
        return options.ContainsKey(start);
    }
}

public class UserPlayer : Player
{
    private Board board;
    UserOptionsMap options;

    // the last selected valid startMainCoor
    //  - if none, selectedCoor is null
    private Coor selectedCoor = null;

    public UserPlayer(int id) : base(id)
    {
        
    }

    // called when given control of the Board
    public override void OnControlStart(Board board)
    {
        this.board = board;
        List<Turn> turns = new List<Turn>();
        foreach (Piece piece in board.pieces)
        {
            turns.AddRange(piece.GetLegalTurns());
        }

        // add turn data into options
        options = new UserOptionsMap();
        foreach ( Turn turn in turns)
        {
            options.Add(turn);
        }
    }

    // called when turns ended, 
    // does a random turn before passing control
    public void DoTurn(Turn turn)
    {
        turn.Do();
        board.UpdatePhysical();
        board.Invoke("PassControl", 0f);
    }

    public override void OnSelectCoordinate(Coor coor)
    {
        // check user has already selected a coordinate
        if( selectedCoor != null )
        {
            List<Turn> possibleTurns = options[selectedCoor, coor];

            // check for a number of possible turns exist
            if ( possibleTurns.Count > 0 )
            {
                if( possibleTurns.Count == 1 )
                {
                    Debug.Log("Moved Piece");
                    DoTurn(possibleTurns[0]);
                }
                else
                {
                    
                }
            }

            // unselect cooordinates
            selectedCoor = null;
            Debug.Log("Unselected Piece");
        }
        else
        {
            if( options.StartsWith(coor))
            {
                Debug.Log("Selected Piece");
                selectedCoor = coor;
            }
        }
    }
}

