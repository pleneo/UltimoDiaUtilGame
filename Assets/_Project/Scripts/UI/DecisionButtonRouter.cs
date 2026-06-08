using UnityEngine;

public class DecisionButtonRouter : MonoBehaviour
{
    [SerializeField] private CaseManager caseManager;

    private void Awake()
    {
        if (caseManager == null)
        {
            Debug.LogWarning("[DecisionButtonRouter] CaseManager nao foi atribuido no Inspector. Tentando encontrar na cena...", this);
            caseManager = FindObjectOfType<CaseManager>();
        }

        if (caseManager == null)
        {
            Debug.LogError("[DecisionButtonRouter] Nao foi possivel encontrar o CaseManager na cena. Os botoes de decisao NAO VAO FUNCIONAR.", this);
        }
        else
        {
            Debug.Log("[DecisionButtonRouter] CaseManager localizado com sucesso.", this);
        }
    }

    public void Approve()
    {
        Debug.Log("[DecisionButtonRouter] Botao APROVAR clicado.", this);
        if (caseManager == null)
        {
            Debug.LogError("[DecisionButtonRouter] Nao e possivel aprovar, a referencia ao CaseManager e NULA.", this);
            return;
        }
        caseManager.SubmitDecision(DecisionType.Approve);
    }

    public void Reject()
    {
        Debug.Log("[DecisionButtonRouter] Botao REJEITAR clicado.", this);
        if (caseManager == null)
        {
            Debug.LogError("[DecisionButtonRouter] Nao e possivel rejeitar, a referencia ao CaseManager e NULA.", this);
            return;
        }
        caseManager.SubmitDecision(DecisionType.Reject);
    }

    public void Forward()
    {
        caseManager?.SubmitDecision(DecisionType.Forward);
    }
}
