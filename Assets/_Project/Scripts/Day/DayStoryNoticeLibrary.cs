public static class DayStoryNoticeLibrary
{
    public static string GetStartNotice(DayConfig dayConfig)
    {
        if (dayConfig != null && !string.IsNullOrWhiteSpace(dayConfig.startOfDayNotice))
        {
            return dayConfig.startOfDayNotice;
        }

        var dayNumber = dayConfig != null ? dayConfig.dayNumber : 1;
        switch (dayNumber)
        {
            case 1:
                return "Bem-vindo ao setor de atendimento.\n\nSeu trabalho é simples: conferir documentos, registrar solicitações e encaminhar os alunos.\n\nNotícia do dia:\n\"Novo vírus identificado na Ásia continua sendo monitorado por autoridades internacionais. Especialistas afirmam que não há motivos para preocupação no país neste momento.\"";
            case 2:
                return "Notícia do dia:\n\"Ministério da Saúde confirma os primeiros casos suspeitos do novo vírus em território nacional. Universidades e escolas afirmam que seguem funcionando normalmente.\"";
            case 3:
                return "Notícia do dia:\n\"Número de casos suspeitos aumenta. Instituições de ensino começam a discutir planos de contingência para atividades presenciais.\"";
            case 4:
                return "Notícia do dia:\n\"Autoridades recomendam evitar aglomerações sempre que possível. Algumas universidades do país já anunciaram suspensão temporária de eventos acadêmicos.\"";
            case 5:
                return "Notícia do dia:\n\"Universidade anuncia suspensão das atividades presenciais por tempo indeterminado. Alunos devem acompanhar os próximos comunicados pelos canais oficiais.\"";
            default:
                return string.Empty;
        }
    }

    public static string GetEndNotice(DayConfig dayConfig)
    {
        return dayConfig != null && dayConfig.dayNumber == 5
            ? "Fim do expediente.\nFim das atividades presenciais."
            : string.Empty;
    }
}
