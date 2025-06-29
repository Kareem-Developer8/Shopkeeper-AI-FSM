using UnityEngine;

public class ShopKeeperIIIdleState : ShopKeeperIIState
{
    public ShopKeeperIIIdleState(ShopKeeperII shopperKeeper, ShopKeeperIIStateMachine stateMachine, string animBoolName) : base(shopperKeeper, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        shopKeeper.agent.isStopped = true;
        shopKeeper.agent.ResetPath();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        GameObject[] boxes = GameObject.FindGameObjectsWithTag("CanPickUp");
        if (boxes.Length > 0)
        {
            stateMachine.ChangeState(shopKeeper.walkState);
        }
    }
}
