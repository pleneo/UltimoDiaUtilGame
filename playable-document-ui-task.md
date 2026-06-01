# Task Plan: Playable Document UI Vertical Slice

## Purpose

The project already has several important gameplay systems in place: day configuration, student cases, document data, validation rules, economy, rulebook data, decision buttons, and basic drag-and-drop. However, most of these systems still feel like backend/debug infrastructure. The game does not yet feel like a playable bureaucracy simulation because the central object of interaction, the document, is not yet strong enough as a UI/gameplay element.

This task focuses on turning documents into real playable objects on the desk.

The goal is not to add more request types, more days, or more abstract systems. The goal is to make Day 1 feel like an actual playable slice:

1. A student case starts.
2. Documents appear on the desk.
3. The player can read the documents.
4. The player can move and inspect the documents.
5. The player can stamp documents as approved or rejected.
6. The player can return the documents through a return tray.
7. The case is resolved from the player's physical/document interaction.
8. The existing validation and economy systems receive the final decision.

This is the most important step before adding Day 2 or more content. A simple but complete Day 1 with real document interaction is more valuable than multiple days that only work through debug buttons.

## Current Situation

The current project already includes useful foundations:

- `StudentCaseDefinition` stores case data.
- `DocumentDefinition` stores document type metadata.
- `DocumentRecord` stores runtime document fields.
- `DocumentManager` spawns documents for the active case.
- `DraggableDocument` allows UI documents to be dragged and selected.
- `CaseValidator` can evaluate document correctness.
- `CaseManager` can submit `Approve`, `Reject`, or `Forward`.
- `DayManager` controls the day loop.
- `EconomyManager` applies rewards, penalties, and warnings.
- `StampReceiver` and `DraggableStamp` already exist as an early stamping prototype.

The weakness is that the document interaction is still not expressive enough:

- Documents are not visually distinct by type.
- Documents do not yet feel like real forms/cards/papers.
- Document text is too generic/debug-like.
- Stamping is not integrated into the final case decision flow.
- Returning documents is not part of the gameplay.
- The player can still resolve cases through direct decision buttons, which bypasses the fantasy of desk work.

This task should bridge that gap without replacing the existing architecture.

## Design Target

The target experience for Day 1 is:

The player receives a student who wants enrollment. Three possible document types can appear on the desk:

- Identity Card.
- School Transcript.
- Enrollment Proof.

The player reads the fields on each document and compares:

- Whether all required documents are present.
- Whether the student name matches across all documents.
- Whether the RA matches between School Transcript and Enrollment Proof.

Then the player physically stamps the paperwork:

- Approved stamp means the player believes the case should be approved.
- Rejected stamp means the player believes the case should be rejected.

After stamping, the player moves the relevant documents to a return tray and confirms the return. The game converts this action into a `DecisionType` and submits it to `CaseManager`.

For MVP purposes, it is acceptable if the player stamps only one main document instead of every document. The recommended MVP rule is:

> The final decision is taken from the last valid stamp applied to any returned document.

This keeps the implementation simple while still making the player interact with documents physically.

## Non-Goals

Do not expand the project beyond this slice during this task.

Avoid:

- Adding Day 2 payment cases.
- Adding Day 3 class withdrawal cases.
- Creating a full inventory system.
- Creating a fully general document layout editor.
- Adding complex animation timelines.
- Adding a complete NPC dialogue system.
- Adding save/load.
- Rewriting `CaseValidator`.
- Replacing ScriptableObjects with another data format.
- Building final art.

Placeholder art and simple UI are acceptable as long as the interaction is clear, readable, and demonstrably playable.

## Expected End Result

At the end of this task, the team should be able to enter Play Mode in the `Game` scene and complete Day 1 through document interaction instead of pure debug buttons.

The expected flow is:

1. The first case loads automatically.
2. The student's request text is visible somewhere in the UI.
3. The case documents appear on the desk.
4. Each document has a distinct visual layout and readable fields.
5. The player can drag documents around the desk.
6. Clicking or dragging a document brings it visually to the front.
7. The player can apply an approved or rejected stamp to a document.
8. The applied stamp remains visible on the document.
9. The player drags the stamped document, or all documents, to a return tray.
10. The player confirms the return.
11. The game resolves the case as `Approve` or `Reject`.
12. The existing validation logic decides whether that decision was correct.
13. The economy/HUD/summary update as they already do.
14. The player can advance to the next case.

If this flow works for the three main Day 1 case types, the task is successful:

- Valid enrollment case should be approved.
- Missing document case should be rejected.
- Name mismatch case should be rejected.

## Recommended Implementation Strategy

Implement this as a small vertical slice, not as a large framework.

The project already has `DocumentRecord`, `DocumentDefinition`, `DocumentManager`, and `DraggableDocument`. Reuse these systems. Extend them only where needed.

The recommended implementation pieces are:

- `DocumentView` or improved `DraggableDocument`.
- `DocumentVisualTemplate` or type-specific prefab mapping.
- `DocumentReturnTray`.
- `DocumentDecisionController`.
- A small change to `DocumentManager` so spawned views can be tracked.
- A small change to `CaseManager` or a wrapper so case decisions can be submitted from document interaction.

Names can change if the codebase already suggests better names, but responsibilities should stay clear.

## Detailed Requirements

### 1. Document Visual Models

Create a visual model for each Day 1 document type.

The MVP document types are:

- `IdentityCard`
- `SchoolTranscript`
- `EnrollmentProof`

Each document should be visually readable and distinct.

Recommended visual style:

- Use UI prefabs under `Assets/_Project/Prefabs/Documents/`.
- Use `RectTransform`, `Image`, and `TMP_Text`.
- Keep the size large enough to read without zooming.
- Use slightly different colors, headers, or icons per document type.
- Keep a bureaucratic paper look: muted paper background, stamped areas, labels, and field rows.

Each document prefab should include:

- A title/header.
- A body area for fields.
- Optional footer/notes area.
- A stamp mark area.
- A `DraggableDocument` or equivalent script.
- A `StampReceiver` or integrated stamping receiver.

Minimum readable fields by document:

Identity Card:

- Document title: `Carteira de Identidade`.
- Name: value from field key `nome`.
- Optional issue date if available.
- Optional expiry date if available.
- Optional notes if available.

School Transcript:

- Document title: `Historico Escolar`.
- Name: value from field key `nome`.
- RA: value from field key `ra`.
- Course: value from field key `curso`.
- Optional issue date.
- Optional notes.

Enrollment Proof:

- Document title: `Comprovante de Matricula`.
- Name: value from field key `nome`.
- RA: value from field key `ra`.
- Course: value from field key `curso`.
- Optional issue date.
- Optional notes.

If a field is missing, the UI should not crash. It should show something clear, such as:

```text
RA: ---
```

or omit the row if that is more readable. For debugging and learning, showing `---` is better because it makes missing data visible.

### 2. Document Data Binding

The document UI must be populated from `DocumentRecord`.

Do not hard-code a student's name directly in the prefab. The prefab should receive a `DocumentRecord` and display whatever fields are in that record.

The current `DocumentRecord` already has:

- `definition`
- `fields`
- `hasValidStamp`
- `isSuspicious`
- `isFake`
- `issueDateIso`
- `expiryDateIso`
- `notes`

The document view should use this data.

Expected behavior:

- If `record.definition.displayName` exists, use it as the document title.
- Use `record.TryGetFieldValue("nome", out value)` for the name.
- Use `record.TryGetFieldValue("ra", out value)` for RA.
- Use `record.TryGetFieldValue("curso", out value)` for course.
- Use `record.issueDateIso` and `record.expiryDateIso` if present.
- Use `record.notes` if present.
- If `record.isFake` or `record.isSuspicious` is true, optionally show subtle suspicious visual markers, but do not make them too obvious unless the rulebook teaches the player to check them.

This preserves the data-driven design from the existing code.

### 3. Type-Specific Layout Selection

`DocumentManager` currently has a single `documentPrefab`.

For this task, it should support different visual prefabs per `DocumentType`.

Recommended simple solution:

Create a serializable mapping:

```csharp
[Serializable]
public class DocumentPrefabMapping
{
    public DocumentType documentType;
    public DraggableDocument prefab;
}
```

Then `DocumentManager` can expose:

```csharp
[SerializeField] private DraggableDocument fallbackDocumentPrefab;
[SerializeField] private List<DocumentPrefabMapping> documentPrefabs;
```

When spawning:

- Look up the prefab for `record.GetDocumentType()`.
- If found, instantiate it.
- If not found, instantiate the fallback prefab.
- Bind the `DocumentRecord` to the view.

This avoids a complicated factory system and remains easy for the team to configure in the Inspector.

### 4. Desk Spawn Positions

Documents should not all spawn on top of each other.

Add simple spawn positioning to `DocumentManager`.

Recommended implementation:

- Expose a list of `Vector2` anchored positions in the Inspector.
- When spawning document index `i`, use `spawnPositions[i]` if available.
- If there are more documents than positions, offset them slightly from the last known position.

Example positions:

```text
Document 1: (-220, 80)
Document 2: (0, 40)
Document 3: (220, 0)
```

Expected behavior:

- The first document appears on the left side of the desk.
- The second document appears near the center.
- The third document appears on the right side.
- Documents remain within the desk area.
- The player can rearrange them.

This is important for the Computer Graphics concept of translation: the player visibly moves documents in X/Y space.

### 5. Dragging and Z-Order

The current `DraggableDocument` already supports dragging and `transform.SetAsLastSibling()`.

Keep this behavior and polish it.

Required behavior:

- Dragging a document moves it across the desk.
- Clicking a document selects it.
- Selected document moves to the top of the visual stack.
- Selected document gets a slight scale or highlight.
- Releasing the drag should not permanently leave the document in a weird selected scale unless that is intended.

Recommended improvements:

- Add hover highlight using `IPointerEnterHandler` and `IPointerExitHandler`.
- Keep selected scale small, such as `1.03`.
- Add a subtle outline or color change if easy.

Acceptance criteria:

- If two documents overlap, clicking the lower one brings it above the other.
- Dragging does not make the document disappear.
- Dragging does not move the document outside the Canvas due to incorrect coordinate conversion.
- The selected visual is clear but not distracting.

### 6. Stamping Interaction

The project already has:

- `DraggableStamp`
- `StampReceiver`
- `StampType.Aprovado`
- `StampType.Negado`

Use these as the base.

Required behavior:

- There are two stamp tools on the desk:
  - Approved.
  - Rejected.
- The player can drag a stamp over a document.
- Releasing the stamp over the document applies the stamp.
- The stamp tool returns to its original position.
- The document displays the applied stamp.

Recommended visual behavior:

- Approved stamp should look green or blue.
- Rejected stamp should look red.
- The stamp mark should be visible but should not hide the document fields.
- The stamp should be placed in a consistent area of the document, such as lower right.

State behavior:

- A document should store which stamp was applied.
- For MVP, either allow replacing a stamp or prevent restamping.
- Recommended MVP behavior: allow restamping while testing, but make this configurable.

Current `StampReceiver` already has:

```csharp
public StampType? UltimoCarimbo { get; private set; }
```

For code consistency, consider renaming Portuguese members only if the team wants English-only code. Do not rename just for style if it causes unnecessary risk.

Acceptance criteria:

- Applying approved stamp shows approved visual.
- Applying rejected stamp shows rejected visual.
- Applying one stamp after another behaves predictably.
- The final applied stamp can be read by the decision flow.

### 7. Return Tray

Create a return tray area on the desk.

The return tray is where the player places documents after deciding what to do. It represents giving the documents back to the student or forwarding the paperwork.

Recommended object:

```text
ReturnTray
```

Recommended script:

```text
DocumentReturnTray
```

Responsibilities:

- Detect when a document is dropped inside the tray area.
- Track returned documents.
- Visually indicate that a document is inside the tray.
- Expose the list of returned documents to the decision controller.

Implementation options:

Option A, simple UI rectangle:

- Put an `Image` on the Canvas as the tray.
- Use `RectTransformUtility.RectangleContainsScreenPoint` to check if a dropped document is inside the tray.
- When the document is dropped inside, register it.

Option B, event-based drop target:

- Implement `IDropHandler` on the tray.
- Keep `CanvasGroup.blocksRaycasts` behavior correct during document drag.
- Register documents from `OnDrop`.

For beginners, Option A may be easier to debug because it mirrors the current stamp overlap code.

Expected visual:

- A labeled tray area, such as `Devolver`.
- It should sit near the bottom or side of the desk.
- It should not overlap the rulebook or decision UI.
- It should highlight when a dragged document is over it if possible.

Acceptance criteria:

- Dragging a document into the tray registers it.
- Dragging it back out either unregisters it or keeps it registered until case resolution. Choose one behavior and keep it consistent.
- The player can clearly see where documents must be returned.

Recommended MVP behavior:

> A document counts as returned if its center point is inside the return tray when the player clicks `Finish Service`.

This avoids needing perfect live enter/exit state tracking.

### 8. Finish Service Button

Replace, hide, or de-emphasize the current direct decision buttons for the normal gameplay flow.

For this task, create a button:

```text
Finish Service
```

or, in Portuguese UI:

```text
Finalizar Atendimento
```

This button should:

1. Check the returned documents in the return tray.
2. Determine the player's intended decision from the applied stamp.
3. Submit the decision to `CaseManager`.

Recommended MVP decision rule:

- If at least one returned document has an approved stamp, submit `DecisionType.Approve`.
- If at least one returned document has a rejected stamp, submit `DecisionType.Reject`.
- If returned documents contain both approved and rejected stamps, use the most recently applied stamp.
- If no returned document has a stamp, show feedback and do not resolve the case.

Alternative simpler rule:

- Require exactly one stamped returned document.
- Use that document's stamp.
- If zero or multiple different stamps exist, show an error message.

Recommended for beginners:

> Use exactly one final stamped document. The player must stamp one document and place it in the tray. `Finish Service` resolves the case from that stamp.

This is easy to explain and easy to test.

Acceptance criteria:

- Clicking `Finish Service` with no returned stamped document does not resolve the case.
- The UI tells the player what is missing.
- Clicking it with an approved stamped document submits `Approve`.
- Clicking it with a rejected stamped document submits `Reject`.
- Existing case validation still decides whether that was correct.

### 9. Decision Integration

Do not duplicate validation logic.

The document interaction should only decide what the player chose:

- Approved stamp means player chose `Approve`.
- Rejected stamp means player chose `Reject`.

Then call the existing case system:

```csharp
caseManager.SubmitDecision(decision);
```

`CaseValidator` should remain responsible for determining the correct answer.

The goal is:

- UI interaction captures player intent.
- `CaseManager` resolves the case.
- `CaseValidator` evaluates correctness.
- `EconomyManager` applies consequences.
- `DayManager` advances the loop.

Do not put business validation rules inside document UI scripts.

Bad direction:

```text
Document UI checks if name matches and directly decides correct/incorrect.
```

Good direction:

```text
Document UI reads the stamp and submits Approve/Reject.
CaseValidator checks if that choice was correct.
```

### 10. Document Return Feedback

The player needs clear feedback when they attempt to finish incorrectly.

Examples:

- No document returned:
  - `Place a stamped document in the return tray.`
- Document returned without stamp:
  - `Stamp the document before returning it.`
- Conflicting stamps:
  - `Use only one final stamp for this case.`

This can be a small TMP text near the tray or near the current case panel.

Keep the text short and functional. Avoid long tutorials inside the UI.

### 11. Debug Buttons

The current direct `Approve`, `Reject`, and `Forward` buttons are useful for development.

During this task, choose one of these approaches:

Option A:

- Keep debug buttons visible but label them clearly as debug controls.

Option B:

- Hide them from the normal layout.
- Keep them available in a collapsible debug panel.

Option C:

- Temporarily remove them from the player-facing scene after document flow works.

Recommended:

> Keep the direct buttons during development, but separate them from the intended player controls. The final Day 1 playtest should use `Finalizar Atendimento`, not direct decision buttons.

This helps the team debug without confusing the MVP presentation.

### 12. Rulebook Relevance

The rulebook must support the document interaction.

For Day 1, the rulebook text should clearly tell the player:

```text
For enrollment, require:
- Identity Card.
- School Transcript.
- Enrollment Proof.

The name must match across all documents.
The RA must match between School Transcript and Enrollment Proof.

Approve only when everything is correct.
Reject if a required document is missing or information does not match.
```

This does not need to be fancy. It just needs to be visible and readable.

Acceptance criteria:

- Player can understand Day 1 rules without external explanation.
- Rulebook text matches the actual validation logic.
- The document fields visible on the desk are the same fields the rulebook asks the player to compare.

### 13. Case Presentation

The student/NPC system can remain simple during this task, but the current case must be visible enough.

Minimum required display:

- Applicant name.
- Request type.
- Student dialogue or case summary.

Example:

```text
Plinio Gomes
Enrollment Request
"I brought everything they asked for my enrollment."
```

This is enough for the player to understand why documents appeared.

Do not block this task on final NPC art.

### 14. Visual Quality Bar

This task does not require final art, but documents must look intentional.

Minimum acceptable quality:

- Documents have paper-like backgrounds.
- Text is aligned and readable.
- Fields have clear labels.
- Stamps are visibly different from normal text.
- Documents fit on the desk.
- Dragging and stamping do not create overlapping, broken, or unreadable UI.

Avoid:

- Tiny text.
- Raw debug dumps as the main document view.
- All document types looking identical.
- Stamps covering important fields.
- Documents spawning outside the visible desk.

### 15. Computer Graphics Concepts

This task should make the required Computer Graphics concepts easier to demonstrate.

Translation:

- Dragging documents visibly changes their X/Y position on the desk.

Z-Order:

- Clicking or dragging a document brings it above the others.

Point-in-Polygon / Hit Testing:

- The return tray and stamp receiver can use rectangle hit testing for MVP.
- If there is time, one document can include a custom irregular clickable/stamp area using `PointInPolygonUtility`.

Queue:

- The existing day/case flow already moves from one student case to the next.

For academic presentation, this task gives the team something concrete to show: documents moving, stacking, being stamped, and being returned.

## Suggested File Changes

Exact file names can change, but the expected areas are:

### Existing Scripts to Update

```text
Assets/_Project/Scripts/Documents/DraggableDocument.cs
Assets/_Project/Scripts/Documents/DocumentManager.cs
Assets/_Project/Scripts/Desk/StampReceiver.cs
Assets/_Project/Scripts/Desk/DraggableStamp.cs
Assets/_Project/Scripts/UI/DecisionButtonRouter.cs
Assets/_Project/Scripts/UI/UIManager.cs
```

Only update what is needed.

### New Scripts to Consider

```text
Assets/_Project/Scripts/Documents/DocumentPrefabMapping.cs
Assets/_Project/Scripts/Documents/DocumentView.cs
Assets/_Project/Scripts/Desk/DocumentReturnTray.cs
Assets/_Project/Scripts/Desk/DocumentDecisionController.cs
```

Possible responsibilities:

`DocumentView`:

- Renders a `DocumentRecord`.
- Displays title and fields.
- Exposes applied stamp.

`DocumentReturnTray`:

- Knows its own tray area.
- Can determine which spawned documents are inside it.

`DocumentDecisionController`:

- Reads returned stamped documents.
- Converts stamp state into `DecisionType`.
- Calls `CaseManager.SubmitDecision`.
- Shows feedback when the player tries to finish too early.

### New Prefabs to Consider

```text
Assets/_Project/Prefabs/Documents/Document_IdentityCard.prefab
Assets/_Project/Prefabs/Documents/Document_SchoolTranscript.prefab
Assets/_Project/Prefabs/Documents/Document_EnrollmentProof.prefab
Assets/_Project/Prefabs/Documents/Document_GenericFallback.prefab
```

### Scene Updates

```text
Assets/_Project/Scenes/Game.unity
```

Expected scene updates:

- Assign document prefab mappings to `DocumentManager`.
- Add approved stamp tool.
- Add rejected stamp tool.
- Add return tray.
- Add finish service button.
- Wire finish button to `DocumentDecisionController`.
- Ensure documents spawn under the correct desk/document root.

## Step-by-Step Implementation Plan

### Step 1: Build a Better Generic Document View

Before making type-specific prefabs, improve the generic document so it can render real fields clearly.

Expected work:

- Replace raw `BuildSummary()` display with structured text fields.
- Add title text.
- Add field rows.
- Add optional notes text.
- Add stamp visual area.

Expected result:

- Any `DocumentRecord` can produce a readable document card.
- The existing Day 1 documents become readable even before custom prefabs exist.

### Step 2: Create Type-Specific Prefabs

Create one prefab per document type.

Expected work:

- Identity Card prefab.
- School Transcript prefab.
- Enrollment Proof prefab.
- Generic fallback prefab.

Expected result:

- Different document types are visually recognizable.
- The player can scan the desk quickly.

### Step 3: Update DocumentManager Prefab Selection

Expected work:

- Add document type to prefab mapping.
- Instantiate the correct prefab for each `DocumentRecord`.
- Apply spawn positions.
- Keep a list of spawned views accessible to other systems.

Expected result:

- Case documents spawn as the right visual type.
- Documents appear in sensible desk positions.

### Step 4: Make Stamping Work on Spawned Documents

Expected work:

- Ensure each document prefab has a working stamp receiver.
- Ensure stamp receiver can expose its current stamp to other scripts.
- Ensure stamp visual appears correctly.

Expected result:

- Player can stamp generated case documents in Play Mode.

### Step 5: Add Return Tray

Expected work:

- Create tray UI element.
- Add return tray script.
- Detect which documents are inside the tray.
- Add visual highlight if possible.

Expected result:

- Player has a clear physical place to return documents.

### Step 6: Add Finish Service Flow

Expected work:

- Create `DocumentDecisionController`.
- Read returned stamped documents.
- Convert final stamp to `DecisionType`.
- Submit to `CaseManager`.
- Show short feedback when the action is invalid.

Expected result:

- Player can resolve a case through document interaction.

### Step 7: Playtest Day 1 Cases

Expected work:

- Test valid case with approved stamp.
- Test missing document case with rejected stamp.
- Test name mismatch case with rejected stamp.
- Test wrong choices intentionally.
- Verify money/warnings/summary update.

Expected result:

- Day 1 feels like a playable document-checking loop.

## Acceptance Criteria

The task is complete when all of the following are true:

```text
[ ] Day 1 documents spawn visibly on the desk.
[ ] Documents are readable without using the Console.
[ ] Identity Card, School Transcript, and Enrollment Proof are visually distinguishable.
[ ] Document fields come from DocumentRecord data.
[ ] Missing fields do not break the UI.
[ ] Documents can be dragged around the desk.
[ ] Clicking or dragging a document brings it to the front.
[ ] Documents can receive an approved stamp.
[ ] Documents can receive a rejected stamp.
[ ] The applied stamp remains visible on the document.
[ ] A return tray exists in the scene.
[ ] The player can place a document in the return tray.
[ ] The player can click Finish Service / Finalizar Atendimento.
[ ] The final document stamp is converted into Approve or Reject.
[ ] CaseManager receives the decision through the existing SubmitDecision flow.
[ ] CaseValidator remains responsible for correctness.
[ ] Correct choices reward the player.
[ ] Incorrect choices penalize the player.
[ ] The player can proceed to the next case after resolving one.
[ ] The three basic Day 1 scenarios are playable without debug decision buttons.
```

## Manual Test Checklist

Run these tests in Unity Play Mode.

### Test 1: Valid Enrollment Case

Setup:

- Case has Identity Card.
- Case has School Transcript.
- Case has Enrollment Proof.
- Names match.
- RA matches.

Player action:

- Read documents.
- Stamp one returned document as approved.
- Place it in the return tray.
- Click Finish Service.

Expected:

- Submitted decision is `Approve`.
- Case is correct.
- Money increases.
- No warning is added.

### Test 2: Missing Document Case

Setup:

- One required document is absent.

Player action:

- Notice the missing document.
- Stamp a returned document as rejected.
- Place it in the return tray.
- Click Finish Service.

Expected:

- Submitted decision is `Reject`.
- Case is correct.
- Money increases.
- No warning is added.

### Test 3: Name Mismatch Case

Setup:

- All documents are present.
- One document has a different name.

Player action:

- Compare the name fields.
- Stamp a returned document as rejected.
- Place it in the return tray.
- Click Finish Service.

Expected:

- Submitted decision is `Reject`.
- Case is correct.
- Money increases.
- No warning is added.

### Test 4: Wrong Approval

Setup:

- Use missing document or name mismatch case.

Player action:

- Stamp approved.
- Return document.
- Finish service.

Expected:

- Submitted decision is `Approve`.
- Case is incorrect.
- Penalty is applied.
- Warning count increases if the case is critical.

### Test 5: Finish Without Stamp

Player action:

- Put a document in the return tray without stamping it.
- Click Finish Service.

Expected:

- Case is not resolved.
- Feedback tells the player to stamp the document.

### Test 6: Finish Without Returned Document

Player action:

- Stamp a document.
- Do not place it in the return tray.
- Click Finish Service.

Expected:

- Case is not resolved.
- Feedback tells the player to return a stamped document.

### Test 7: Z-Order

Player action:

- Overlap multiple documents.
- Click the lower document.

Expected:

- Clicked document comes to the front.

### Test 8: Drag Stability

Player action:

- Drag all documents around the desk.
- Release them in different positions.

Expected:

- Documents stay visible.
- Documents do not jump unpredictably.
- Documents remain interactable.

## Risks and Notes

### Risk: Too Much Generalization

It may be tempting to build a fully generic document form renderer. Avoid that for now.

The MVP only needs three document types. A simple shared renderer plus three prefabs is enough.

### Risk: UI Buttons Bypass the Core Fantasy

Direct Approve/Reject buttons are useful for debugging, but the game fantasy requires physical document handling.

The player-facing flow should prioritize stamping and returning documents.

### Risk: Stamping Every Document Becomes Tedious

Do not require the player to stamp every document unless it improves the game. For MVP, one final stamp is enough.

### Risk: Visual Polish Consumes Too Much Time

The goal is readability and function, not final art. Use simple paper panels, readable fonts, and clear colors.

### Risk: Validation Gets Duplicated

Do not check enrollment correctness inside the document UI. Keep validation centralized in `CaseValidator`.

## Recommended Definition of Done

This task is done when a teammate can sit down, play Day 1, and understand the loop without looking at the Console:

> A student asks for enrollment, documents appear, the player reads them, compares fields, stamps the decision, returns the paperwork, and the game reacts correctly.

Once this is working, the project will have a real MVP foundation. After that, adding Day 2 payment documents and Day 3 withdrawal documents becomes a content expansion problem instead of a core interaction problem.

## Epic Breakdown

This section divides the task into implementation epics. Each epic should produce a concrete improvement that can be reviewed and tested before moving to the next one.

The recommended order is important. Do not start the return tray or final decision flow before documents are readable and spawn correctly. The team should avoid creating several half-working systems in parallel.

### Epic 1: Readable Generic Document View

#### Goal

Turn the current document card into a readable UI object that displays real document data from `DocumentRecord`.

This epic does not need type-specific prefabs yet. The goal is to make one generic document view good enough to inspect a case in Play Mode.

#### Why This Matters

The player cannot make decisions if documents are not readable. Before adding stamping, trays, or more rules, the project needs a dependable way to show document fields clearly on the desk.

#### Main Work

Update or extend:

```text
Assets/_Project/Scripts/Documents/DraggableDocument.cs
```

Possibly create:

```text
Assets/_Project/Scripts/Documents/DocumentView.cs
```

The document view should:

- Receive a `DocumentRecord`.
- Display the document title.
- Display important fields in structured rows.
- Display issue date if present.
- Display expiry date if present.
- Display notes if present.
- Display missing fields gracefully.

For Day 1, the important field keys are:

```text
nome
ra
curso
```

Recommended field labels:

```text
nome  -> Name
ra    -> RA
curso -> Course
```

The UI should not show a raw debug dump as the primary presentation. `DocumentRecord.BuildSummary()` can remain useful for debug, but the player-facing view should look like a form.

#### Expected Result

When a Day 1 case starts, each spawned document shows readable data such as:

```text
Carteira de Identidade
Name: Plinio Gomes
Issue Date: 2026-05-21
```

or:

```text
Historico Escolar
Name: Plinio Gomes
RA: 2026001
Course: Ciencia da Computacao
```

#### Acceptance Criteria

```text
[ ] Generic document view displays document title.
[ ] Generic document view displays nome when available.
[ ] Generic document view displays ra when available.
[ ] Generic document view displays curso when available.
[ ] Missing fields do not crash the view.
[ ] Document text is readable in Play Mode.
[ ] The view still supports drag-and-drop.
```

#### Manual Test

Start Day 1 and inspect all spawned documents. Confirm that the data visible on screen matches the fields configured in the `StudentCaseDefinition` or generated case.

---

### Epic 2: Document Prefab Mapping by Document Type

#### Goal

Allow each `DocumentType` to use its own visual prefab.

The generic document view from Epic 1 is still useful as fallback, but Day 1 documents should become visually distinguishable.

#### Why This Matters

In a document-checking game, the player must quickly identify what kind of paper is on the desk. If every document looks identical, the interaction feels like a debug panel instead of a bureaucracy simulation.

#### Main Work

Update:

```text
Assets/_Project/Scripts/Documents/DocumentManager.cs
```

Add a simple serializable mapping, for example:

```csharp
[Serializable]
public class DocumentPrefabMapping
{
    public DocumentType documentType;
    public DraggableDocument prefab;
}
```

Then expose something like:

```csharp
[SerializeField] private DraggableDocument fallbackDocumentPrefab;
[SerializeField] private List<DocumentPrefabMapping> documentPrefabs;
```

Expected prefab selection logic:

1. Get the `DocumentType` from `DocumentRecord.GetDocumentType()`.
2. Search the mapping list.
3. Instantiate the matching prefab.
4. If no matching prefab exists, instantiate the fallback prefab.
5. Bind the `DocumentRecord`.

#### Expected Result

`IdentityCard`, `SchoolTranscript`, and `EnrollmentProof` can each use a different prefab without changing validation logic.

#### Acceptance Criteria

```text
[ ] DocumentManager supports prefab mapping by DocumentType.
[ ] DocumentManager has a fallback prefab.
[ ] IdentityCard can spawn with its own prefab.
[ ] SchoolTranscript can spawn with its own prefab.
[ ] EnrollmentProof can spawn with its own prefab.
[ ] Missing prefab mapping falls back safely instead of crashing.
```

#### Manual Test

Temporarily assign visibly different colors to each prefab. Start Day 1 and confirm that the correct document types spawn with the correct visuals.

---

### Epic 3: Day 1 Document Visual Prefabs

#### Goal

Create three player-facing document prefabs for the Day 1 enrollment flow.

#### Why This Matters

The player needs to believe they are checking university paperwork, not reading generic panels. This epic turns the abstract document system into visible game objects.

#### Main Work

Create prefabs:

```text
Assets/_Project/Prefabs/Documents/Document_IdentityCard.prefab
Assets/_Project/Prefabs/Documents/Document_SchoolTranscript.prefab
Assets/_Project/Prefabs/Documents/Document_EnrollmentProof.prefab
Assets/_Project/Prefabs/Documents/Document_GenericFallback.prefab
```

Each prefab should include:

- `RectTransform`
- background `Image`
- title `TMP_Text`
- field label/value `TMP_Text` elements
- stamp mark area
- `DraggableDocument` or document view script
- `CanvasGroup`
- `StampReceiver` or equivalent stamp receiver component

Visual direction:

- `IdentityCard`: smaller, card-like layout.
- `SchoolTranscript`: form/list layout, academic-looking.
- `EnrollmentProof`: official receipt/protocol-style layout.

The assets can be simple. Use UI panels, colors, borders, and text hierarchy. Do not wait for final pixel art.

#### Expected Result

When Day 1 starts, the three document types look like different types of paperwork.

#### Acceptance Criteria

```text
[ ] Identity Card prefab exists.
[ ] School Transcript prefab exists.
[ ] Enrollment Proof prefab exists.
[ ] Generic fallback prefab exists.
[ ] All prefabs bind to DocumentRecord data.
[ ] All prefabs are readable at gameplay resolution.
[ ] All prefabs can be dragged.
[ ] All prefabs can receive a stamp.
```

#### Manual Test

Run Day 1 and inspect each document type. Confirm that a player can distinguish document type without reading every line.

---

### Epic 4: Desk Spawn Layout and Document Handling Polish

#### Goal

Make documents appear in sensible places on the desk and ensure moving/selecting them feels stable.

#### Why This Matters

If documents spawn stacked on top of each other or jump around while dragged, the game feels broken even if the data is correct.

#### Main Work

Update:

```text
Assets/_Project/Scripts/Documents/DocumentManager.cs
Assets/_Project/Scripts/Documents/DraggableDocument.cs
```

Add spawn positions:

```csharp
[SerializeField] private List<Vector2> spawnPositions;
```

Expected behavior:

- First document appears at first configured position.
- Second document appears at second configured position.
- Third document appears at third configured position.
- Additional documents use a safe fallback offset.

Polish dragging:

- Clicking selects a document.
- Dragging selects a document.
- Selected document goes to front with `SetAsLastSibling()`.
- Selected/hovered document has visible feedback.
- Document scale returns to normal when appropriate.

#### Expected Result

Documents appear spread across the desk and can be rearranged naturally.

#### Acceptance Criteria

```text
[ ] Documents spawn inside the visible desk area.
[ ] Documents do not all spawn at the same anchored position.
[ ] Dragging works for every document prefab.
[ ] Clicking a document brings it to the front.
[ ] Overlapping documents can be reordered by clicking.
[ ] Selected/hover feedback is visible.
[ ] Dragging does not make documents disappear.
```

#### Manual Test

Overlap three documents manually. Click the back document and confirm it comes to the front. Drag all documents around the desk and confirm they remain visible and interactable.

---

### Epic 5: Stamping Documents

#### Goal

Make approved and rejected stamps work reliably on spawned case documents.

#### Why This Matters

Stamping is the main physical action that turns document inspection into gameplay. It also matches the bureaucratic fantasy strongly.

#### Main Work

Update or verify:

```text
Assets/_Project/Scripts/Desk/DraggableStamp.cs
Assets/_Project/Scripts/Desk/StampReceiver.cs
```

Scene work:

- Add approved stamp tool to desk.
- Add rejected stamp tool to desk.
- Ensure each spawned document can receive a stamp.
- Ensure stamp mark visuals exist inside each document prefab.

Expected behavior:

- Player drags the approved stamp onto a document.
- Document shows approved mark.
- Player drags the rejected stamp onto a document.
- Document shows rejected mark.
- Stamp tool returns to original position.
- Applied stamp state can be read by another script.

Recommended MVP rule:

- Allow restamping during development, or expose a setting for it.
- The most recent stamp should be clear.

#### Expected Result

The player can physically mark the paperwork as approved or rejected.

#### Acceptance Criteria

```text
[ ] Approved stamp can be dragged.
[ ] Rejected stamp can be dragged.
[ ] Dropping approved stamp over a document applies approved mark.
[ ] Dropping rejected stamp over a document applies rejected mark.
[ ] Stamp tool returns to its starting position.
[ ] Stamp mark remains visible on the document.
[ ] Another script can read the applied stamp state.
```

#### Manual Test

Apply both stamp types to each Day 1 document prefab. Confirm visuals and internal stamp state match.

---

### Epic 6: Return Tray

#### Goal

Create a return tray area where the player places the final stamped document before finishing service.

#### Why This Matters

The player should not simply click a decision button. Returning paperwork gives the case a physical end step and makes the desk interaction feel complete.

#### Main Work

Create:

```text
Assets/_Project/Scripts/Desk/DocumentReturnTray.cs
```

Scene work:

- Add tray UI object to the desk.
- Label it clearly, such as `Devolver` or `Return`.
- Give it a visible area.
- Optionally add hover/active highlight.

Recommended MVP logic:

When the player clicks `Finish Service`, the tray checks which document centers are currently inside its rectangle.

This is simpler than maintaining live enter/exit state.

The tray should provide a method like:

```csharp
public List<DraggableDocument> GetDocumentsInsideTray(IEnumerable<DraggableDocument> documents)
```

or:

```csharp
public bool ContainsDocument(DraggableDocument document)
```

#### Expected Result

The player has a clear place to put the final stamped document.

#### Acceptance Criteria

```text
[ ] Return tray exists in the Game scene.
[ ] Return tray is visually clear.
[ ] Return tray can detect a document inside it.
[ ] Return tray ignores documents outside it.
[ ] Detection works after dragging documents.
```

#### Manual Test

Drag a document into the tray and check that the tray script detects it. Drag it out and check that it is no longer detected if using center-point detection.

---

### Epic 7: Finish Service Decision Flow

#### Goal

Resolve a case from the player's stamped returned document.

#### Why This Matters

This is the point where the physical document interaction connects back into the existing game systems.

#### Main Work

Create:

```text
Assets/_Project/Scripts/Desk/DocumentDecisionController.cs
```

Responsibilities:

- Access current spawned documents from `DocumentManager`.
- Ask `DocumentReturnTray` which documents are inside the tray.
- Find the final stamp.
- Convert stamp to `DecisionType`.
- Call `CaseManager.SubmitDecision(decision)`.
- Show feedback when the player cannot finish yet.

Recommended MVP rule:

```text
The player must place at least one stamped document in the return tray.
If exactly one stamped returned document exists, its stamp is the final decision.
Approved stamp -> DecisionType.Approve.
Rejected stamp -> DecisionType.Reject.
If no stamped document exists, do not resolve the case.
If conflicting stamped documents exist, show feedback and do not resolve the case.
```

Feedback examples:

```text
Place a stamped document in the return tray.
Stamp the document before returning it.
Use only one final stamp for this case.
```

Scene work:

- Add `Finalizar Atendimento` / `Finish Service` button.
- Wire button to `DocumentDecisionController`.
- Keep direct decision buttons only as debug controls.

#### Expected Result

The player resolves Day 1 cases by stamping and returning documents, not by directly pressing `Approve` or `Reject`.

#### Acceptance Criteria

```text
[ ] Finish Service button exists.
[ ] Finish Service with no returned document shows feedback.
[ ] Finish Service with unstamped returned document shows feedback.
[ ] Approved stamped returned document submits Approve.
[ ] Rejected stamped returned document submits Reject.
[ ] Submitted decision goes through CaseManager.SubmitDecision.
[ ] CaseValidator still determines correctness.
[ ] Economy updates after resolved case.
[ ] DayManager can proceed to the next case.
```

#### Manual Test

Run a valid case, stamp approved, return document, and finish service. Confirm the case resolves correctly. Then run an invalid case, stamp rejected, return document, and confirm the case resolves correctly.

---

### Epic 8: Player-Facing Day 1 Cleanup

#### Goal

Make the Day 1 document flow understandable without relying on the Console or debug panel.

#### Why This Matters

Even if the interaction works technically, the MVP needs to be understandable to a player and presentable for class evaluation.

#### Main Work

Update UI presentation:

- Current student/case panel should show applicant name.
- Current case panel should show request/dialogue.
- Rulebook should clearly explain Day 1 rules.
- Return tray feedback should be visible.
- Debug controls should be separated from player controls.

Recommended Day 1 rulebook text:

```text
For enrollment, require:
- Identity Card.
- School Transcript.
- Enrollment Proof.

The name must match across all documents.
The RA must match between School Transcript and Enrollment Proof.

Approve only when everything is correct.
Reject if a required document is missing or information does not match.
```

#### Expected Result

A teammate can play Day 1 without asking what each UI element does.

#### Acceptance Criteria

```text
[ ] Current applicant name is visible.
[ ] Current request/dialogue is visible.
[ ] Rulebook text matches actual validation rules.
[ ] Return tray feedback text is visible.
[ ] Direct decision buttons are hidden, moved, or clearly marked as debug.
[ ] The intended player action is clear: read, stamp, return, finish.
```

#### Manual Test

Ask someone who did not implement the feature to play one case. They should understand that they need to inspect documents, stamp one, place it in the tray, and finish service.

---

### Epic 9: Full Day 1 Playtest and Acceptance Pass

#### Goal

Verify that the document UI vertical slice works across the main Day 1 scenarios.

#### Why This Matters

This epic confirms that the task is not just a collection of working components, but a real playable loop.

#### Main Work

Run Play Mode tests for:

- Valid enrollment.
- Missing document.
- Name mismatch.
- Wrong approval.
- Finish without stamp.
- Finish without returned document.
- Z-order.
- Drag stability.

Fix only issues directly related to this document flow.

Do not start Day 2 during this epic.

#### Expected Result

Day 1 can be played as a small but complete document-checking loop.

#### Acceptance Criteria

```text
[ ] Valid enrollment can be approved through stamp + return flow.
[ ] Missing document case can be rejected through stamp + return flow.
[ ] Name mismatch case can be rejected through stamp + return flow.
[ ] Wrong player decisions are penalized.
[ ] Correct player decisions are rewarded.
[ ] Day summary still appears.
[ ] Next case flow still works.
[ ] No Console errors occur during normal Day 1 play.
```

#### Manual Test

Complete one full Day 1 session in Play Mode. Record any issues that block the player from finishing the day.

## Suggested Epic Order

Use this order:

```text
1. Readable Generic Document View
2. Document Prefab Mapping by Document Type
3. Day 1 Document Visual Prefabs
4. Desk Spawn Layout and Document Handling Polish
5. Stamping Documents
6. Return Tray
7. Finish Service Decision Flow
8. Player-Facing Day 1 Cleanup
9. Full Day 1 Playtest and Acceptance Pass
```

This order keeps the work grounded. Each epic builds on the previous one and should leave the project more playable than before.
