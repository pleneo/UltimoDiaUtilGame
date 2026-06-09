using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class StudentPaymentZone : MonoBehaviour
{
    [SerializeField] private PaymentFlowController paymentFlowController;

    private RectTransform zoneRectTransform;

    private void Awake()
    {
        zoneRectTransform = GetComponent<RectTransform>();

        if (paymentFlowController == null)
        {
            paymentFlowController = FindObjectOfType<PaymentFlowController>();
        }
    }

    public bool TryReceiveMachine(DraggablePaymentMachine machine)
    {
        if (machine == null || zoneRectTransform == null || !RectTransformsOverlap(machine.RectTransform, zoneRectTransform))
        {
            return false;
        }

        return paymentFlowController != null && paymentFlowController.TryStartPayment(machine);
    }

    private static bool RectTransformsOverlap(RectTransform first, RectTransform second)
    {
        if (first == null || second == null)
        {
            return false;
        }

        var firstCorners = new Vector3[4];
        var secondCorners = new Vector3[4];
        first.GetWorldCorners(firstCorners);
        second.GetWorldCorners(secondCorners);

        var firstMin = firstCorners[0];
        var firstMax = firstCorners[2];
        var secondMin = secondCorners[0];
        var secondMax = secondCorners[2];

        return firstMin.x <= secondMax.x &&
               firstMax.x >= secondMin.x &&
               firstMin.y <= secondMax.y &&
               firstMax.y >= secondMin.y;
    }
}
