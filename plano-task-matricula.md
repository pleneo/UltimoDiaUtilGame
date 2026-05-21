# Plano da Task: Regra Real de Matricula

Objetivo: completar a task de matricula do Dia 1 usando a estrutura atual do projeto Unity.

Esta task deve usar os sistemas ja existentes:

- `StudentCaseDefinition`
- `DocumentDefinition`
- `DayConfig`
- `DocumentRecord`
- `DocumentRequirement`
- `DocumentComparisonRule`
- `CaseValidator`

O foco e configurar dados de jogo e validar a regra de matricula sem criar uma arquitetura paralela.

## Epico 1: Preparar os Dados Base de Matricula

Objetivo: criar os tipos de documento usados pela matricula.

### Automatizado

Criar as pastas, se ainda nao existirem:

```text
Assets/_Project/ScriptableObjects/Documents/
Assets/_Project/ScriptableObjects/Cases/Day01/
Assets/_Project/ScriptableObjects/Days/
Assets/_Project/ScriptableObjects/Rules/
```

Criar os assets:

```text
Document_IdentityCard
Document_SchoolTranscript
Document_EnrollmentProof
```

Configuracao esperada:

```text
Carteira de Identidade
DocumentType: IdentityCard

Historico Escolar
DocumentType: SchoolTranscript

Comprovante de Matricula
DocumentType: EnrollmentProof
```

### Feito na Unity

- Abrir a Unity.
- Rodar o menu de geracao, caso exista um script de Editor para isso.
- Conferir se os 3 assets apareceram no Inspector.
- Verificar se `documentType` e `displayName` estao corretos.

### Entrega

- Os 3 documentos obrigatorios da matricula existem como `DocumentDefinition`.

## Epico 2: Definir a Regra Real de Matricula

Objetivo: transformar a regra da task em dados reutilizaveis nos casos.

Regra MVP:

```text
Para aprovar matricula, o aluno deve apresentar:
- Carteira de Identidade
- Historico Escolar
- Comprovante de Matricula

Todos devem ter nome compativel.
Historico Escolar e Comprovante de Matricula devem ter o mesmo RA.
```

Campos usados:

```text
nome
ra
curso
```

Regras obrigatorias:

```text
IdentityCard precisa do campo nome.
SchoolTranscript precisa dos campos nome e ra.
EnrollmentProof precisa dos campos nome e ra.
```

Comparacoes:

```text
IdentityCard.nome == SchoolTranscript.nome
IdentityCard.nome == EnrollmentProof.nome
SchoolTranscript.ra == EnrollmentProof.ra
```

### Automatizado

Gerar essas regras dentro dos proprios `StudentCaseDefinition`, preenchendo:

- `requiredDocuments`
- `comparisonRules`
- `documents`

Opcionalmente, melhorar o `CaseValidator` para normalizar melhor nomes, por exemplo ignorar espacos extras:

```text
"Plinio Gomes" == " Plinio Gomes "
```

Nao e recomendado ignorar acentos ou variacoes complexas neste momento, porque isso pode esconder erro real de documento.

### Feito na Unity

- Conferir no Inspector se cada caso recebeu:
  - 3 documentos obrigatorios;
  - comparacao de nome;
  - comparacao de RA.

### Entrega

- A regra de matricula esta representada nos dados do caso.
- O validador consegue aprovar ou rejeitar com base nesses dados.

## Epico 3: Criar os 3 Casos Obrigatorios

Objetivo: entregar pelo menos 3 casos funcionando.

Os casos devem ficar em:

```text
Assets/_Project/ScriptableObjects/Cases/Day01/
```

### Caso 1: Matricula Correta

Asset:

```text
Case_Day01_Enrollment_Valid
```

Configuracao:

```text
caseId: day01_enrollment_valid
caseTitle: Matricula regular
applicantName: Plinio Gomes
requestType: Enrollment
```

Documentos entregues:

```text
Carteira de Identidade
nome = Plinio Gomes

Historico Escolar
nome = Plinio Gomes
ra = 2026001
curso = Ciencia da Computacao

Comprovante de Matricula
nome = Plinio Gomes
ra = 2026001
curso = Ciencia da Computacao
```

Resultado correto esperado:

```text
Approve
```

### Caso 2: Documento Faltando

Asset:

```text
Case_Day01_Enrollment_MissingDocument
```

Configuracao:

```text
caseId: day01_enrollment_missing_document
caseTitle: Matricula sem historico
applicantName: Plinio Gomes
requestType: Enrollment
```

Documentos obrigatorios continuam sendo os 3:

```text
IdentityCard
SchoolTranscript
EnrollmentProof
```

Mas em `documents`, entregar apenas:

```text
Carteira de Identidade
nome = Plinio Gomes

Comprovante de Matricula
nome = Plinio Gomes
ra = 2026001
curso = Ciencia da Computacao
```

Nao adicionar `Historico Escolar` nos documentos entregues.

Resultado correto esperado:

```text
Reject
```

### Caso 3: Nome Divergente

Asset:

```text
Case_Day01_Enrollment_NameMismatch
```

Configuracao:

```text
caseId: day01_enrollment_name_mismatch
caseTitle: Matricula com nome divergente
applicantName: Plinio Gomes
requestType: Enrollment
```

Documentos entregues:

```text
Carteira de Identidade
nome = Plinio Gomes

Historico Escolar
nome = Plinio Gomes
ra = 2026001
curso = Ciencia da Computacao

Comprovante de Matricula
nome = Plinio Gomis
ra = 2026001
curso = Ciencia da Computacao
```

Resultado correto esperado:

```text
Reject
```

### Automatizado

Criar um gerador de assets que monta os 3 casos completos.

### Feito na Unity

- Conferir cada asset.
- Verificar se o caso correto tem 3 documentos.
- Verificar se o caso de documento faltando realmente nao possui o `Historico Escolar` em `documents`.
- Verificar se o caso divergente tem `Plinio Gomis` em um documento.

### Entrega

- 1 caso correto.
- 1 caso com documento faltando.
- 1 caso com nome divergente.

## Epico 4: Conectar os Casos ao Dia 1

Objetivo: fazer os casos entrarem no fluxo jogavel.

Criar ou atualizar:

```text
Assets/_Project/ScriptableObjects/Days/Day_01_EnrollmentBasics
```

Configuracao:

```text
dayNumber: 1
dayLabel: Dia 1
workDurationSeconds: 300
availableRequestTypes:
- Enrollment

cases:
- Case_Day01_Enrollment_Valid
- Case_Day01_Enrollment_MissingDocument
- Case_Day01_Enrollment_NameMismatch
```

### Automatizado

Gerar o `DayConfig` e colocar os 3 casos nele.

Opcionalmente, criar uma regra de livro:

```text
Rule_Day01_EnrollmentDocuments
```

Texto sugerido:

```text
Para matricula, exigir Carteira de Identidade, Historico Escolar e Comprovante de Matricula. O nome deve ser igual em todos os documentos. O RA deve bater entre Historico Escolar e Comprovante de Matricula.
```

### Feito na Unity

- Abrir a cena `Game`.
- Localizar o objeto que possui `GameManager` ou `DayManager`.
- Verificar onde o projeto espera receber os `DayConfig`.
- Arrastar `Day_01_EnrollmentBasics` para o campo correspondente.
- Garantir que a cena inicia pelo Dia 1.
- Salvar a cena.

### Entrega

- O Dia 1 possui os 3 casos de matricula na fila.

## Epico 5: Testar a Regra no Play Mode

Objetivo: provar que a task esta completa funcionando dentro do jogo.

### Feito na Unity

Entrar em Play Mode e testar os 3 casos:

1. Caso correto:
   - clicar em `Approve` / `Deferido`;
   - deve contar como decisao correta.

2. Caso com documento faltando:
   - clicar em `Reject` / `Indeferido`;
   - deve contar como decisao correta.

3. Caso com nome divergente:
   - clicar em `Reject` / `Indeferido`;
   - deve contar como decisao correta.

Tambem testar erros de proposito:

```text
Aprovar caso com documento faltando deve contar como erro.
Aprovar caso com nome divergente deve contar como erro.
Rejeitar caso correto deve contar como erro.
```

### Automatizado

Adicionar logs no `CaseManager` para mostrar no Console:

```text
[CaseManager] Caso: Matricula sem historico | Aluno: Plinio Gomes | Escolha: Reject | Esperado: Reject | Decisao CORRETA
[CaseManager] Problema: MissingDocument | Documento obrigatorio ausente: SchoolTranscript | Origem: SchoolTranscript | Alvo: Unknown
```

Logs esperados durante o teste:

```text
Caso correto + Approve:
[CaseManager] Caso: Matricula regular | Aluno: Plinio Gomes | Escolha: Approve | Esperado: Approve | Decisao CORRETA
[CaseManager] Validacao: nenhum problema encontrado.

Caso sem Historico Escolar + Reject:
[CaseManager] Caso: Matricula sem historico | Aluno: Plinio Gomes | Escolha: Reject | Esperado: Reject | Decisao CORRETA
[CaseManager] Problema: MissingDocument | Documento obrigatorio ausente: SchoolTranscript | Origem: SchoolTranscript | Alvo: Unknown

Caso com nome divergente + Reject:
[CaseManager] Caso: Matricula com nome divergente | Aluno: Plinio Gomes | Escolha: Reject | Esperado: Reject | Decisao CORRETA
[CaseManager] Problema: FieldMismatch | O nome da Carteira de Identidade deve bater com o Comprovante de Matricula. | Origem: IdentityCard | Alvo: EnrollmentProof
```

Tambem devem aparecer logs de decisao `INCORRETA` se o jogador escolher o contrario da regra.

### Entrega

- Os 3 casos funcionam no fluxo.
- O jogo marca decisoes corretas e incorretas com base nas regras.
- O Console da Unity mostra caso, aluno, decisao escolhida, decisao esperada e problemas encontrados.

## Epico 6: Criterio Final de Aceite

A task pode ser marcada como concluida quando tudo abaixo estiver verdadeiro:

```text
[ ] Existe estrutura de dados para caso de matricula.
[ ] Existem documentos obrigatorios definidos.
[ ] O jogo valida presenca de documento.
[ ] O jogo valida nome entre documentos.
[ ] O jogo valida RA entre Historico Escolar e Comprovante de Matricula.
[ ] Existe 1 caso correto.
[ ] Existe 1 caso com documento faltando.
[ ] Existe 1 caso com nome divergente.
[ ] Aprovar o caso correto e decisao correta.
[ ] Rejeitar os dois casos invalidos e decisao correta.
[ ] Decisoes contrarias sao marcadas como erro.
```

## Divisao Pratica

### Automatizado

```text
1. Criar pastas de ScriptableObjects.
2. Criar script de Editor para gerar dados do Dia 1.
3. Gerar DocumentDefinitions.
4. Gerar 3 StudentCaseDefinitions.
5. Gerar DayConfig do Dia 1.
6. Opcional: gerar RuleDefinition do livro de regras.
7. Opcional: melhorar logs de validacao.
```

### Feito na Unity

```text
1. Rodar o gerador no menu da Unity.
2. Conferir os assets no Inspector.
3. Conectar Day_01_EnrollmentBasics na cena.
4. Conferir referencias de GameManager, DayManager, CaseManager e DocumentManager.
5. Rodar Play Mode.
6. Testar decisoes dos 3 casos.
7. Salvar cena e assets.
```

## Recomendacao

Comecar pelo gerador automatico de assets. Isso reduz erro manual no Inspector e deixa a task repetivel caso alguem apague ou altere os dados sem querer.
