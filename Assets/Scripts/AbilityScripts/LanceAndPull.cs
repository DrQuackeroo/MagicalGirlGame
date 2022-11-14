using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LanceAndPull : Ability
{
    //string _displayName = "LanceAndPull";
    protected GameObject _player;
    protected PlayerControls _playerControls;
    
    public override void Activate(GameObject player)
    {
        Debug.Log("LanceAndPull activated");
        if (_player == null)
        {
            _player = player;
            _playerControls = player.GetComponent<PlayerControls>();
            _displayName = "LanceAndPull";
        }
    }

    public override void Deactivate(GameObject player)
    {

    }
}
