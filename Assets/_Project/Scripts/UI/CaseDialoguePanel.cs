using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CaseDialoguePanel : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text continueButtonText;

    private bool waitingForContinue;

    public void Configure(
        GameObject panelRoot,
        TMP_Text speaker,
        TMP_Text dialogue,
        Button button,
        TMP_Text buttonText)
    {
        root = panelRoot;
        speakerText = speaker;
        dialogueText = dialogue;
        continueButton = button;
        continueButtonText = buttonText;
    }

    private void OnEnable()
    {
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(Continue);
        }
    }

    private void OnDisable()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(Continue);
        }
    }

    public IEnumerator Play(StudentCaseDefinition caseDefinition)
    {
        var dialogueLines = BuildDialogueLines(caseDefinition);
        yield return PlayLines(dialogueLines, lastButtonLabel: "Entregar documentos");
    }

    public IEnumerator PlayLines(List<CaseDialogueLine> lines, string lastButtonLabel = "Continuar")
    {
        if (lines == null || lines.Count == 0)
        {
            Hide();
            yield break;
        }

        Show();

        for (var index = 0; index < lines.Count; index++)
        {
            ShowLine(lines[index], index, lines.Count, lastButtonLabel);

            if (continueButton == null)
            {
                yield return new WaitForSeconds(1.5f);
                continue;
            }

            waitingForContinue = true;

            while (waitingForContinue)
            {
                yield return null;
            }
        }

        Hide();
    }

    public void Hide()
    {
        waitingForContinue = false;

        if (root != null)
        {
            root.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void Show()
    {
        if (root != null)
        {
            root.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    private void ShowLine(CaseDialogueLine line, int lineIndex, int totalLines, string lastButtonLabel = "Continuar")
    {
        if (line == null)
        {
            return;
        }

        if (speakerText != null)
        {
            speakerText.text = GetSpeakerLabel(line.speaker);
        }

        if (dialogueText != null)
        {
            dialogueText.text = line.text;
        }

        if (continueButtonText != null)
        {
            continueButtonText.text = lineIndex + 1 >= totalLines ? lastButtonLabel : "Continuar";
        }
    }

    private void Continue()
    {
        waitingForContinue = false;
    }

    private static List<CaseDialogueLine> BuildDialogueLines(StudentCaseDefinition caseDefinition)
    {
        var dialogueLines = new List<CaseDialogueLine>();
        if (caseDefinition == null)
        {
            return dialogueLines;
        }

        if (caseDefinition.preDocumentDialogue != null)
        {
            for (var index = 0; index < caseDefinition.preDocumentDialogue.Count; index++)
            {
                var line = caseDefinition.preDocumentDialogue[index];
                if (line == null || string.IsNullOrWhiteSpace(line.text))
                {
                    continue;
                }

                dialogueLines.Add(line);
            }
        }

        if (dialogueLines.Count == 0)
        {
            dialogueLines.AddRange(CaseDialogueMockLibrary.CreateMockDialogue(
                CaseDialogueMockLibrary.GetMockIndexFromCase(caseDefinition)));
        }

        if (dialogueLines.Count == 0 && !string.IsNullOrWhiteSpace(caseDefinition.npcDialogue))
        {
            dialogueLines.Add(new CaseDialogueLine
            {
                speaker = DialogueSpeaker.Npc,
                text = caseDefinition.npcDialogue
            });
        }

        return dialogueLines;
    }

    private static string GetSpeakerLabel(DialogueSpeaker speaker)
    {
        switch (speaker)
        {
            case DialogueSpeaker.Player:
                return "Voce";
            case DialogueSpeaker.Narrator:
                return "Atendimento";
            default:
                return "Aluno";
        }
    }
}
