using System.Collections.Generic;

public static class CaseDialogueMockLibrary
{
    public const int MockDialogueCount = 8;

    public static void ApplyMockDialogue(StudentCaseDefinition studentCase, int mockIndex)
    {
        if (studentCase == null)
        {
            return;
        }

        studentCase.preDocumentDialogue.Clear();
        AddDialogueLines(studentCase.preDocumentDialogue, NormalizeMockIndex(mockIndex));

        if (studentCase.preDocumentDialogue.Count > 0)
        {
            studentCase.npcDialogue = studentCase.preDocumentDialogue[0].text;
        }
    }

    public static List<CaseDialogueLine> CreateMockDialogue(int mockIndex)
    {
        var dialogueLines = new List<CaseDialogueLine>();
        AddDialogueLines(dialogueLines, NormalizeMockIndex(mockIndex));
        return dialogueLines;
    }

    public static int GetMockIndexFromCase(StudentCaseDefinition studentCase)
    {
        if (studentCase == null)
        {
            return 0;
        }

        var key = !string.IsNullOrWhiteSpace(studentCase.caseId)
            ? studentCase.caseId
            : studentCase.name;

        if (string.IsNullOrWhiteSpace(key))
        {
            return 0;
        }

        var hash = 0;
        for (var index = 0; index < key.Length; index++)
        {
            hash = (hash * 31) + key[index];
        }

        return NormalizeMockIndex(hash);
    }

    private static int NormalizeMockIndex(int mockIndex)
    {
        var safeIndex = mockIndex % MockDialogueCount;
        return safeIndex < 0 ? safeIndex + MockDialogueCount : safeIndex;
    }

    private static void AddDialogueLines(List<CaseDialogueLine> targetLines, int mockIndex)
    {
        switch (mockIndex)
        {
            case 0:
                AddLine(targetLines, DialogueSpeaker.Npc, "Oi...Eu vim fazer minha matricula.");
                AddLine(targetLines, DialogueSpeaker.Player, "Documentos, por favor.");
                AddLine(targetLines, DialogueSpeaker.Npc, "Eu trouxe tudo eu acho.");
                break;
            case 1:
                AddLine(targetLines, DialogueSpeaker.Npc, "Quero trancar calculo.");
                AddLine(targetLines, DialogueSpeaker.Player, "Existe uma taxa administrativa a ser paga.");
                AddLine(targetLines, DialogueSpeaker.Npc, "Eu sei, nao vou pagar, nao consta sobre a taxa em contrato.");
                break;
            case 2:
                AddLine(targetLines, DialogueSpeaker.Npc, "Desejo trocar de curso de Administracao para Ciencia da Computacao.");
                AddLine(targetLines, DialogueSpeaker.Player, "Para isso, preciso do formulario especifico de troca de curso.");
                AddLine(targetLines, DialogueSpeaker.Npc, "Entendi. Vou procurar esse papel entao.");
                break;
            case 3:
                AddLine(targetLines, DialogueSpeaker.Npc, "Vim buscar meu diploma.");
                AddLine(targetLines, DialogueSpeaker.Player, "Documento com foto.");
                break;
            case 4:
                AddLine(targetLines, DialogueSpeaker.Npc, "Boa tarde queria fazer o pagamento de mensalidade.");
                break;
            case 5:
                AddLine(targetLines, DialogueSpeaker.Npc, "Preciso da segunda via do meu boleto.");
                AddLine(targetLines, DialogueSpeaker.Player, "Declaracao de matricula.");
                AddLine(targetLines, DialogueSpeaker.Npc, "Aqui esta.");
                break;
            case 6:
                AddLine(targetLines, DialogueSpeaker.Npc, "Preciso corrigir meu nome no sistema.");
                AddLine(targetLines, DialogueSpeaker.Player, "Trouxe um documento de identificacao?");
                AddLine(targetLines, DialogueSpeaker.Npc, "Sim.");
                AddLine(targetLines, DialogueSpeaker.Player, "Vou verificar.");
                break;
            default:
                AddLine(targetLines, DialogueSpeaker.Npc, "Vim renovar minha matricula.");
                AddLine(targetLines, DialogueSpeaker.Player, "Ha pendencias financeiras em seu cadastro.");
                AddLine(targetLines, DialogueSpeaker.Npc, "Como assim?");
                AddLine(targetLines, DialogueSpeaker.Player, "Existem mensalidades em atraso.");
                break;
        }
    }

    private static void AddLine(List<CaseDialogueLine> targetLines, DialogueSpeaker speaker, string text)
    {
        targetLines.Add(new CaseDialogueLine
        {
            speaker = speaker,
            text = text
        });
    }
}
