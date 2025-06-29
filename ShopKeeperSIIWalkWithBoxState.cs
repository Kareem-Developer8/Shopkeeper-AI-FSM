using System.Linq;
using UnityEngine;

public class ShopKeeperSIIWalkWithBoxState : ShopKeeperIIState
{
    bool movingToBin;

    public ShopKeeperSIIWalkWithBoxState(ShopKeeperII shopperKeeper, ShopKeeperIIStateMachine stateMachine, string animBoolName) : base(shopperKeeper, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        if (shopKeeper.handPos.childCount > 0)
        {
            shopKeeper.detectedBox = shopKeeper.handPos.GetChild(0).gameObject;
        }

        // Set up box physics and parenting
        if (shopKeeper.detectedBox != null)
        {
            Rigidbody rb = shopKeeper.detectedBox.GetComponent<Rigidbody>();
            Collider boxCol = shopKeeper.detectedBox.GetComponent<Collider>();
            Collider keeperCol = shopKeeper.GetComponent<Collider>();

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.interpolation = RigidbodyInterpolation.None;
            }
            if (boxCol != null) boxCol.enabled = false;

            BoxControl boxControl = shopKeeper.detectedBox.GetComponent<BoxControl>();
            if (boxControl != null)
            {
                boxControl.SetBeingHeld(true);
            }

            shopKeeper.detectedBox.transform.SetParent(shopKeeper.handPos);
            shopKeeper.detectedBox.transform.localPosition = Vector3.zero;
            shopKeeper.detectedBox.transform.localRotation = Quaternion.identity;

            if (boxCol != null && keeperCol != null)
            {
                Physics.IgnoreCollision(boxCol, keeperCol, true);
            }
        }
        FindNewPlaceholderDestination();
        movingToBin = IsBoxEmpty();
        if (movingToBin)
        {
            if (shopKeeper.detectedBox != null)
            {
                BoxControl boxControl = shopKeeper.detectedBox.GetComponent<BoxControl>();
                if (boxControl != null)
                {
                    boxControl.StopAnimation();
                }
            }
            MoveToBin();
        }
    }
    bool IsBoxEmpty()
    {
        return shopKeeper.detectedBox != null &&
               shopKeeper.detectedBox.GetComponent<BoxControl>()?.IsEmpty() == true;
    }

    void MoveToBin()
    {
        if (shopKeeper.transformBin != null)
        {
            shopKeeper.agent.SetDestination(shopKeeper.transformBin.position);
            shopKeeper.agent.isStopped = false;
        }
        else
        {
            Debug.LogError("Bin transform not assigned!");
            stateMachine.ChangeState(shopKeeper.idleState);
        }
    }

    void FindNewPlaceholderDestination()
    {
        if (shopKeeper.detectedBox == null)
        {
            Debug.LogError("No box taken, cannot find placeholder");
            return;
        }

        BoxControl boxControl = shopKeeper.detectedBox.GetComponent<BoxControl>();
        if (boxControl == null)
        {
            Debug.LogError("Taken box has no BoxControl component");
            return;
        }

        ClassificationType requiredType = boxControl.boxType;
        Grocery requiredGrocery = boxControl.grocery;

        // Priority: same grocery and has empty spots
        var priorityPlaceholders = Object.FindObjectsByType<PlaceHolderOverlap>(FindObjectsSortMode.None)
            .Where(p => p.placeType == requiredType
                     && p.assignedGrocery == requiredGrocery
                     && p.isPartiallyFull)
            .ToArray();

        if (priorityPlaceholders.Length > 0)
        {
            shopKeeper.selectedGroceryPlaceholder = priorityPlaceholders[Random.Range(0, priorityPlaceholders.Length)];
        }
        else
        {
            var emptyPlaceholders = Object.FindObjectsByType<PlaceHolderOverlap>(FindObjectsSortMode.None)
            .Where(p => p.placeType == requiredType && p.isEmpty)
            .ToArray();

            if (emptyPlaceholders.Length > 0)
            {
                shopKeeper.selectedGroceryPlaceholder = emptyPlaceholders[Random.Range(0, emptyPlaceholders.Length)];
            }
            else
            {
                // 3. No valid targets - go to standby
                Debug.Log("No valid placeholders - waiting in standby");
                stateMachine.ChangeState(shopKeeper.standCarryBoxState);
                return;
            }
        }

        shopKeeper.standingPlaceHolder = shopKeeper.selectedGroceryPlaceholder.standingArea;

        if (shopKeeper.standingPlaceHolder != null)
        {
            shopKeeper.agent.SetDestination(shopKeeper.standingPlaceHolder.position);
            shopKeeper.agent.isStopped = false;
        }
        else
        {
            Debug.LogError("Selected placeholder has no standing area");
            stateMachine.ChangeState(shopKeeper.standCarryBoxState);
        }

    }

    public override void Update()
    {
        base.Update();
        if (movingToBin)
        {
            if (!shopKeeper.agent.pathPending &&
                shopKeeper.agent.remainingDistance <= shopKeeper.agent.stoppingDistance)
            {
                Debug.Log("Reached the bin!");

                // Destroy the box and clear references
                if (shopKeeper.detectedBox != null)
                {
                    GameObject.Destroy(shopKeeper.detectedBox);
                    shopKeeper.detectedBox = null;
                }

                stateMachine.ChangeState(shopKeeper.idleState);
            }
        }
        else
        {
            if (!shopKeeper.agent.pathPending &&
                shopKeeper.agent.remainingDistance <= shopKeeper.agent.stoppingDistance)
            {
                Debug.Log("Reached the selected placeholder!");
                stateMachine.ChangeState(shopKeeper.standCarryBoxState);
            }
        }
    }
    public override void Exit()
    {
        base.Exit();
    }
}
