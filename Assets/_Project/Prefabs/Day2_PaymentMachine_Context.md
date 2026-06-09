# Contexto Dia 2: Maquininha e Trancamento

## Objetivo da feature

O Dia 2 deixou de ser um dia de `EnrollmentQueue` e passou a ser um dia de `ClassWithdrawal` com taxa obrigatória paga via maquininha.

Fluxo desejado:

1. O aluno entrega os documentos de trancamento.
2. O jogador analisa os documentos.
3. O jogador arrasta a maquininha até a mesma área física usada para devolver documentos.
4. A maquininha some por um curto tempo.
5. A maquininha volta para a mesa.
6. A `Via da Maquininha` aparece como novo documento na mesa.
7. O jogador analisa a via.
8. O jogador carimba o `Formulario de Trancamento`.
9. O jogador devolve todos os documentos.
10. O caso é resolvido.

Regra do pagamento no MVP:

- único erro de pagamento: `status = nao autorizado`
- caso com `status = autorizado` deve ser aprovado
- caso com `status = nao autorizado` deve ser rejeitado

## Decisão de layout

Foi decidido unificar o espaço físico:

- a mesma `DocumentSubmissionZone` recebe:
  - documentos
  - maquininha

Não existe mais necessidade prática de `StudentPaymentZone` na cena.

## Scripts adicionados ou alterados

### Regras por caso

- `Assets/_Project/Scripts/Cases/CaseDocumentRules.cs`
  - resolve qual é o documento decisório por caso
  - identifica se o caso exige pagamento

- `Assets/_Project/Scripts/Cases/StudentCaseDefinition.cs`
  - novos campos:
    - `decisionDocumentType`
    - `requiresPaymentProcessing`
    - `expectedPaymentAmount`
    - `paymentReceiptTemplate`
    - `expectedFieldValueRules`

### Validação

- `Assets/_Project/Scripts/Cases/CaseValidator.cs`
  - continua validando documentos obrigatórios e comparações
  - agora também valida campos com valor esperado
  - usado no Dia 2 para exigir:
    - `status = autorizado` em `PaymentReceipt`

- `Assets/_Project/Scripts/Core/GameEnums.cs`
  - adicionado `ValidationIssueType.InvalidFieldValue`

- `Assets/_Project/Scripts/Documents/DocumentModels.cs`
  - adicionado `DocumentExpectedFieldValueRule`

### Fluxo da mesa

- `Assets/_Project/Scripts/Desk/StampReceiver.cs`
  - não está mais preso ao `EnrollmentProof`
  - agora só permite carimbo no documento decisório do caso atual

- `Assets/_Project/Scripts/Desk/DocumentSubmissionZone.cs`
  - continua recebendo documentos
  - agora também recebe a maquininha
  - bloqueia devolução de documentos quando o caso exige pagamento e a via ainda não foi gerada
  - usa `PaymentFlowController`

- `Assets/_Project/Scripts/Desk/DraggablePaymentMachine.cs`
  - script da maquininha arrastável
  - ao soltar, tenta entregar na `DocumentSubmissionZone`

- `Assets/_Project/Scripts/Desk/PaymentFlowController.cs`
  - controla:
    - visibilidade da maquininha
    - início do pagamento
    - delay de processamento
    - retorno da maquininha
    - geração da `PaymentReceipt`
    - bloqueio/liberação do fluxo
  - atualização importante:
    - a visibilidade da maquininha foi reforçada em `Update()` para evitar aparecer fora de casos que exigem pagamento

### Spawn e visualização de documentos

- `Assets/_Project/Scripts/Documents/DocumentManager.cs`
  - ganhou `AddDocument(DocumentRecord record, bool animated = true)`
  - isso permite criar a `PaymentReceipt` no meio do caso

- `Assets/_Project/Scripts/Documents/DocumentView.cs`
  - agora suporta exibir:
    - `cpf`
    - `valor`
    - `status`

## Editor tools criadas

- `Assets/_Project/Scripts/Editor/WithdrawalPaymentDayAssetCreator.cs`

Menu da Unity:

- `Tools > Ultimo Dia Util > Day 2 > Create Withdrawal Payment Documents`
- `Tools > Ultimo Dia Util > Day 2 > Create Withdrawal Payment Cases`
- `Tools > Ultimo Dia Util > Day 2 > Create Withdrawal Payment Day Config`

O comando relevante usado foi:

- `Create Withdrawal Payment Day Config`

Ele cria:

- `Document_WithdrawalForm.asset`
- `Document_PaymentReceipt.asset`
- casos do Dia 2
- regra do Dia 2
- `Day_02_WithdrawalPayments.asset`

## Assets criados

### DayConfig

- `Assets/_Project/ScriptableObjects/Days/Day_02_WithdrawalPayments.asset`

### Casos

- `Assets/_Project/ScriptableObjects/Cases/Day02/Case_Day02_Withdrawal_Authorized.asset`
- `Assets/_Project/ScriptableObjects/Cases/Day02/Case_Day02_Withdrawal_Unauthorized.asset`

Ambos os casos:

- `requestType = ClassWithdrawal`
- `decisionDocumentType = WithdrawalForm`
- `requiresPaymentProcessing = true`

Diferença entre eles:

- caso autorizado:
  - `paymentReceiptTemplate.status = autorizado`
- caso não autorizado:
  - `paymentReceiptTemplate.status = nao autorizado`

### Regras

- `Assets/_Project/ScriptableObjects/Rules/Rule_Day02_WithdrawalPayment.asset`

### DocumentDefinitions

- `Assets/_Project/ScriptableObjects/Documents/Document_WithdrawalForm.asset`
- `Assets/_Project/ScriptableObjects/Documents/Document_PaymentReceipt.asset`

## Prefabs criados

- `Assets/_Project/Prefabs/Documents/DocumentWithdrawalForm.prefab`
- `Assets/_Project/Prefabs/Documents/DocumentPaymentReceipt.prefab`

### Origem dos prefabs

- `DocumentWithdrawalForm.prefab`
  - clonado de `DocumentEnrollmentProof.prefab`
  - motivo: já tinha estrutura funcional de documento principal e carimbo

- `DocumentPaymentReceipt.prefab`
  - clonado de `DocumentGenericFallback.prefab`
  - motivo: suficiente para a via aparecer de forma legível

## Configuração correta no DocumentManager

No `DocumentManager.documentPrefabs`, os mapeamentos corretos são:

- `IdentityCard` -> `DocumentIdentityCard`
- `SchoolTranscript` -> `DocumentSchoolTranscript`
- `EnrollmentProof` -> `DocumentEnrollmentProof`
- `WithdrawalForm` -> `DocumentWithdrawalForm`
- `PaymentReceipt` -> `DocumentPaymentReceipt`

Erro que aconteceu durante integração:

- `WithdrawalForm` e `PaymentReceipt` estavam apontando para `DocumentEnrollmentProof`
- isso foi corrigido manualmente no Editor

## Spawn points

Para o Dia 2, recomenda-se usar `documentSpawnPoints` por tipo:

- `IdentityCard`
- `EnrollmentProof`
- `WithdrawalForm`
- `PaymentReceipt`

Os dois novos pontos importantes:

- `SpawnPoint_WithdrawalForm`
- `SpawnPoint_PaymentReceipt`

`PaymentReceipt` deve nascer em posição que deixe claro que ela voltou da maquininha.

## Estrutura de cena esperada

### Objetos relevantes na `Game` scene

- objeto com `GameManager`
- objeto com `DayManager`
- objeto com `CaseManager`
- objeto com `DocumentManager`
- objeto com `DocumentSubmissionZone`
- objeto `PaymentFlowController`
- objeto `PaymentMachine`

### PaymentMachine

Objeto UI com:

- `Image`
- `CanvasGroup`
- `DraggablePaymentMachine`

Ela deve existir na cena inteira, mas só aparecer quando o caso exigir pagamento.

### PaymentFlowController

Campos esperados:

- `caseManager`
- `documentManager`
- `paymentMachine`
- `fallbackPaymentReceiptDefinition`

### DocumentSubmissionZone

Campo importante:

- `paymentFlowController`

Se isso estiver nulo, o bloqueio de pagamento e a entrega da maquininha não funcionam direito.

## Problemas encontrados e diagnóstico

### Problema 1

Sintoma:

- no Dia 2 não apareciam casos de maquininha

Causa:

- o `GameManager.daySequence` ainda apontava para `Day_02_EnrollmentQueue`
- não para `Day_02_WithdrawalPayments`

Correção manual necessária:

- trocar o segundo item da sequência de dias no `GameManager`

### Problema 2

Sintoma:

- a maquininha aparecia o tempo todo

Causas possíveis encontradas:

- `DocumentSubmissionZone.paymentFlowController` estava nulo na cena
- ordem de inicialização podia deixar a maquininha visível

Correções:

- ligar `paymentFlowController` na `DocumentSubmissionZone`
- reforçar a visibilidade em `PaymentFlowController.Update()`

### Problema 3

Sintoma:

- no segundo caso, parecia que não dava para usar a maquininha

Comportamento esperado:

- mesmo no caso `nao autorizado`, a maquininha precisa ser entregue
- a via deve voltar com `status = nao autorizado`
- só então o jogador deve rejeitar

Se isso falhar novamente, verificar:

- se o dia carregado é realmente `Day_02_WithdrawalPayments`
- se `DocumentSubmissionZone.paymentFlowController` está ligado
- se a maquininha está sendo solta dentro da área cinza

## Status atual da feature

### Implementado em código

- documento decisório por caso
- pagamento exigido por caso
- geração da via no meio do atendimento
- maquininha arrastável
- unificação da maquininha na `DocumentSubmissionZone`
- validação de `status = autorizado`
- prefabs base do Dia 2
- assets base do Dia 2

### Ainda depende de Unity / cena

- garantir `daySequence` correto no `GameManager`
- garantir `paymentFlowController` ligado na `DocumentSubmissionZone`
- garantir `documentPrefabs` corretos no `DocumentManager`
- garantir spawn points do Dia 2
- validar visual final dos prefabs

## Regra funcional final do Dia 2

1. O caso de trancamento carrega.
2. Os documentos base aparecem.
3. Não é permitido devolver documentos antes do pagamento.
4. O jogador entrega a maquininha na mesma área cinza.
5. O sistema processa.
6. Surge `PaymentReceipt`.
7. O jogador carimba o `WithdrawalForm`.
8. O jogador devolve todos os documentos.
9. O validador decide:
   - `status = autorizado` -> `Approve`
   - `status = nao autorizado` -> `Reject`

## Observação importante para outro agente

O script `StudentPaymentZone.cs` ainda existe no projeto, mas ficou obsoleto depois da unificação da área física.

O fluxo real agora deve considerar:

- `DocumentSubmissionZone` como única área de drop

Se outro agente continuar o trabalho, não deve reintroduzir uma zona separada sem necessidade de layout real.
