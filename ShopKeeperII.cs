using UnityEngine;

public class ShopKeeperII : Charcter
{
    public float detectionRadius;

    public Transform handPos;
    public GameObject detectedBox;
    public PlaceHolderOverlap selectedGroceryPlaceholder;
    public Transform standingPlaceHolder;
    public Transform transformBin; 
    #region States
    public ShopKeeperIIStateMachine stateMachine;
    public ShopKeeperIIIdleState idleState { get; private set; }
    public ShopKeeperIICarryStandState standCarryBoxState { get; private set; }
    public ShopKeeperSIIWalkWithBoxState walkWithBoxState { get; private set; }
    public ShopKeeperIIWalkState walkState { get; private set; }
    #endregion
    private void Awake()
    {
        InstializeStates();
    }
    void InstializeStates()
    {
        stateMachine = new ShopKeeperIIStateMachine();
        idleState = new ShopKeeperIIIdleState(this, stateMachine, "Idle");
        standCarryBoxState = new ShopKeeperIICarryStandState(this, stateMachine, "CarryStanding");
        walkWithBoxState = new ShopKeeperSIIWalkWithBoxState(this, stateMachine, "WalkWithBox");
        walkState = new ShopKeeperIIWalkState(this, stateMachine, "Walk");
    }
    protected override void Start()
    {
        base.Start();
        stateMachine.Instatiate(idleState);
    }
    private void Update()
    {
        stateMachine.currentState.Update();
        DetectBoxesII();
    }
    private void DetectBoxesII()
    {
        if (handPos.childCount > 0)
        {
            detectedBox = handPos.GetChild(0).gameObject;
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var collider in hitColliders)
        {
            BoxControl boxControl = collider.GetComponent<BoxControl>();
            if (collider.CompareTag("CanPickUp") && boxControl != null && !boxControl.IsTransforming()
                && !boxControl.isBeingHeld && !boxControl.mightBeBoxInterst
                && !boxControl.isStored &&
                !boxControl.IsEmpty())
            {
                detectedBox = collider.gameObject;
                Debug.DrawLine(transform.position, detectedBox.transform.position, Color.green);
                return;
            }
        }

        detectedBox = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

}
