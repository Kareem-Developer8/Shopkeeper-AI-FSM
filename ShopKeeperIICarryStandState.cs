using System.Linq;
using UnityEngine;

public class ShopKeeperIICarryStandState : ShopKeeperIIState
{
    public ShopKeeperIICarryStandState(ShopKeeperII shopperKeeper, ShopKeeperIIStateMachine stateMachine, string animBoolName) : base(shopperKeeper, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        if (shopKeeper.detectedBox != null)
        {
            BoxControl boxControl = shopKeeper.detectedBox.GetComponent<BoxControl>();
            if (boxControl != null && !boxControl.IsBoxOpened())
            {
                boxControl.StartAnimation();
            }
        }
        AttemptItemTransfer();
    }

    void AttemptItemTransfer()
    {
        if (shopKeeper.detectedBox == null)
        {
            Debug.LogError("No box taken in StandCarryBoxState");
            stateMachine.ChangeState(shopKeeper.idleState);
            return;
        }

        BoxControl boxControl = shopKeeper.detectedBox.GetComponent<BoxControl>();
        if (boxControl == null)
        {
            Debug.LogError("Box has no BoxControl component");
            stateMachine.ChangeState(shopKeeper.idleState);
            return;
        }

        PlaceHolderOverlap placeholder = shopKeeper.selectedGroceryPlaceholder;
        if (placeholder == null)
        {
            Debug.LogError("No selected placeholder in StandCarryBoxState");
            stateMachine.ChangeState(shopKeeper.idleState);
            return;
        }

        if (placeholder.assignedGrocery != null && placeholder.assignedGrocery != boxControl.grocery)
        {
            Debug.Log("Placeholder grocery type mismatch. Finding new placeholder.");
            FindNewPlaceholderAndMove();
            return;
        }
        bool hasEmptySpots = placeholder.shelfSpots.Any(spot => spot.childCount == 0);
        if (hasEmptySpots && !boxControl.IsEmpty())
        {
            boxControl.TransferNextItemToShelf(placeholder);
        }
        else
        {
            CheckCompletionOrFindNewPlaceholder(boxControl);
        }
    }

    public override void Update()
    {
        base.Update();
        AttemptItemTransfer();
        CheckCompletionOrFindNewPlaceholder(shopKeeper.detectedBox?.GetComponent<BoxControl>());
    }

    void CheckCompletionOrFindNewPlaceholder(BoxControl boxControl)
    {
        if (boxControl == null) return;

        PlaceHolderOverlap placeholder = shopKeeper.selectedGroceryPlaceholder;
        if (placeholder == null) return;

        // Check if placeholder's assigned grocery type has changed
        if (placeholder.assignedGrocery != null && placeholder.assignedGrocery != boxControl.grocery)
        {
            Debug.Log("Placeholder grocery type changed. Finding new placeholder.");
            FindNewPlaceholderAndMove();
            return;
        }

        bool hasEmptySpots = placeholder.shelfSpots.Any(spot => spot.childCount == 0);

        if (boxControl.IsEmpty())
        {
            Debug.Log("All items placed. Process complete.");
            shopKeeper.agent.SetDestination(shopKeeper.transformBin.position);
            stateMachine.ChangeState(shopKeeper.walkWithBoxState);
        }
        else if (!hasEmptySpots)
        {
            FindNewPlaceholderAndMove();
        }
    }

    void FindNewPlaceholderAndMove()
    {
        BoxControl boxControl = shopKeeper.detectedBox.GetComponent<BoxControl>();
        if (boxControl == null)
        {
            Debug.LogError("BoxControl component missing");
            stateMachine.ChangeState(shopKeeper.idleState);
            return;
        }

        ClassificationType requiredType = boxControl.boxType;
        Grocery requiredGrocery = boxControl.grocery;

        // Priority 1: Partially filled placeholders with matching grocery
        var priorityPlaceholders = Object.FindObjectsByType<PlaceHolderOverlap>(FindObjectsSortMode.None)
            .Where(p => p.placeType == requiredType
                     && p.assignedGrocery == requiredGrocery
                     && p.isPartiallyFull)
            .ToArray();

        // Priority 2: Empty placeholders with matching type
        var emptyPlaceholders = Object.FindObjectsByType<PlaceHolderOverlap>(FindObjectsSortMode.None)
            .Where(p => p.placeType == requiredType && p.isEmpty)
            .ToArray();

        PlaceHolderOverlap target = null;

        if (priorityPlaceholders.Length > 0)
        {
            target = priorityPlaceholders[Random.Range(0, priorityPlaceholders.Length)];
        }
        else if (emptyPlaceholders.Length > 0)
        {
            target = emptyPlaceholders[Random.Range(0, emptyPlaceholders.Length)];
        }

        if (target != null)
        {
            shopKeeper.selectedGroceryPlaceholder = target;
            shopKeeper.standingPlaceHolder = target.standingArea;

            if (shopKeeper.standingPlaceHolder != null)
            {
                shopKeeper.agent.SetDestination(shopKeeper.standingPlaceHolder.position);
                shopKeeper.agent.isStopped = false;
                stateMachine.ChangeState(shopKeeper.walkWithBoxState);
            }
        }
        else
        {
            Debug.Log("No valid placeholders - maintaining standby");
        }
    }


    public override void Exit()
    {
        base.Exit();
    }
}
