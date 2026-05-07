using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private HUDController hudController;
    [SerializeField] private RulebookPanel rulebookPanel;
    [SerializeField] private NoticeBoardPanel noticeBoardPanel;
    [SerializeField] private DaySummaryPanel daySummaryPanel;

    public void BindDay(DayConfig dayConfig)
    {
        if (rulebookPanel != null)
        {
            rulebookPanel.SetRules(dayConfig != null ? dayConfig.rulebookEntries : null);
        }

        if (noticeBoardPanel != null)
        {
            noticeBoardPanel.SetEntries(dayConfig != null ? dayConfig.noticeBoardEntries : null);
        }
    }

    public void ShowCurrentCase(StudentCaseDefinition caseDefinition)
    {
        if (hudController != null)
        {
            hudController.SetCaseInfo(caseDefinition);
        }
    }

    public void RefreshHUD(EconomySnapshot snapshot, float remainingTimeSeconds, float totalTimeSeconds)
    {
        if (hudController != null)
        {
            hudController.Refresh(snapshot, remainingTimeSeconds, totalTimeSeconds);
        }
    }

    public void ShowDaySummary(DaySummaryData summary)
    {
        if (daySummaryPanel != null)
        {
            daySummaryPanel.Show(summary);
        }
    }

    public void HideDaySummary()
    {
        if (daySummaryPanel != null)
        {
            daySummaryPanel.Hide();
        }
    }
}
