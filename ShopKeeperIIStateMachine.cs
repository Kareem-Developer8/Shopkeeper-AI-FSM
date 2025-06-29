using UnityEngine;

public class ShopKeeperIIStateMachine
{
    public ShopKeeperIIState currentState;
    public void Instatiate(ShopKeeperIIState _startState)
    {
        currentState = _startState;
        currentState.Enter();
    }
    public void ChangeState(ShopKeeperIIState _newState)
    {
        currentState.Exit();
        currentState = _newState;
        currentState.Enter();
    }
}