using System.Collections.Generic;
using UnityEngine;

public class ShopKeeperIIWalkState : ShopKeeperIIState
{
    GameObject targetBox;

    public ShopKeeperIIWalkState(ShopKeeperII shopperKeeper, ShopKeeperIIStateMachine stateMachine, string animBoolName) : base(shopperKeeper, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        GameObject[] allBoxes = GameObject.FindGameObjectsWithTag("CanPickUp");
        List<GameObject> availableBoxes = new List<GameObject>();

        foreach (var box in allBoxes)
        {
            BoxControl boxControl = box.GetComponent<BoxControl>();
            if (boxControl != null && !boxControl.isBeingHeld &&
            !boxControl.mightBeBoxInterst && !boxControl.isStored
            && !boxControl.IsEmpty() &&
            boxControl.boxType != ClassificationType.Fridge
            && boxControl.boxType != ClassificationType.Shelf)
            {
                availableBoxes.Add(box);
            }
        }

        if (availableBoxes.Count > 0)
        {
            targetBox = availableBoxes[Random.Range(0, availableBoxes.Count)];
            shopKeeper.agent.SetDestination(targetBox.transform.position);
        }
        else
        {
            stateMachine.ChangeState(shopKeeper.idleState);
        }
    }

    public override void Update()
    {
        base.Update();
        if (targetBox == null || !targetBox.activeSelf ||
                (targetBox.GetComponent<BoxControl>() != null &&
                 (targetBox.GetComponent<BoxControl>().isBeingHeld ||
                  targetBox.GetComponent<BoxControl>().mightBeBoxInterst
                  || targetBox.GetComponent<BoxControl>().isStored)))
        {
            stateMachine.ChangeState(shopKeeper.idleState);
            return;
        }
        if (shopKeeper.detectedBox != null)
        {
            stateMachine.ChangeState(shopKeeper.walkWithBoxState);
        }

    }

    public override void Exit()
    {
        base.Exit();
        targetBox = null;
    }
}
