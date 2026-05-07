using UnityEngine;

public class DecisionButtonRouter : MonoBehaviour
{
    [SerializeField] private CaseManager caseManager;

    private void Awake()
    {
        if (caseManager == null)
        {
            caseManager = FindObjectOfType<CaseManager>();
        }
    }

    public void Approve()
    {
        caseManager?.SubmitDecision(DecisionType.Approve);
    }

    public void Reject()
    {
        caseManager?.SubmitDecision(DecisionType.Reject);
    }

    public void Forward()
    {
        caseManager?.SubmitDecision(DecisionType.Forward);
    }
}
