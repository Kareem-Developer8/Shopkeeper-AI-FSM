using UnityEngine;

public class ShopKeeperIIState
{
    protected ShopKeeperII shopKeeper;
    protected ShopKeeperIIStateMachine stateMachine;
    string boolName;
    protected Rigidbody rb;
    protected float timer;
    public ShopKeeperIIState(ShopKeeperII shopperKeeper, ShopKeeperIIStateMachine stateMachine, string animBoolName)
    {
        this.shopKeeper = shopperKeeper;
        this.stateMachine = stateMachine;
        this.boolName = animBoolName;
    }
    public virtual void Enter()
    {
        shopKeeper.animator.SetBool(boolName, true);
        rb = shopKeeper.rb;
    }
    public virtual void Update()
    {
        timer -= Time.deltaTime;
    }
    public virtual void Exit()
    {
        shopKeeper.animator.SetBool(boolName, false);
    }
}

