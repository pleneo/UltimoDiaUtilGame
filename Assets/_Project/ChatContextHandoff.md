# Ultimo Dia Util Game - Chat Context Handoff

This document exports the practical context from the current development chat so the project can be continued in another conversation without losing decisions, implementation details, Unity setup instructions, or known issues.

Generated after commit:

```text
77ebc19 Add document interaction and spawn setup
```

The repository path used in this chat was:

```text
C:\Users\pleneo\Documents\Jean\UltimoDiaUtilGame
```

## Project Overview

The project is a Unity 2D game called **Ultimos Dias Uteis**, inspired by *Papers, Please*, set in a Brazilian university administration office. The intended MVP is a playable bureaucratic document-checking loop where the player processes student requests, checks documents, approves or rejects cases, earns money for correct decisions, and receives penalties or warnings for mistakes.

The project should remain scoped for a one-month Computer Graphics discipline project. The priority is not a large game. The priority is a small, complete, playable, explainable MVP.

Important project documents:

- `AGENTS.md`: persistent implementation guidance for future AI agents.
- `GDD.md`: main design source.
- `backlog.md`: backlog and planning context.
- `Assets/_Project/PlayableVersionActionPlan.md`: action plan created during this chat for reaching a playable Day 1 version.

The architectural direction is:

- Keep Unity/C# code simple and beginner-friendly.
- Prefer ScriptableObjects for days, cases, documents, rules, and economy tuning.
- Keep validation logic out of UI button scripts.
- Keep document prefabs focused on presentation and interaction.
- Keep the core loop playable before expanding document types, narrative, or polish.

## Main MVP Direction Agreed In Chat

The user felt that the game had only very basic foundations: there was some day/document structure, but not yet enough to be a playable MVP. The identified missing pieces were:

- Real document UI.
- Document types with their own prefabs.
- Documents displayed on the desk.
- Documents movable by the player.
- Documents that can be approved or rejected.
- A way to submit/return documents.
- Documents with visible written information.
- A playable Day 1 loop.

The agreed immediate direction was to focus on the **document interaction layer** before expanding content:

1. Build basic document prefabs.
2. Build a `DocumentView` to bind runtime data into visible text fields.
3. Make documents draggable.
4. Make documents spawn on the desk.
5. Support approval/rejection via stamps.
6. Let stamped documents be submitted by dragging them to the area outside the desk.
7. Return unstamped documents to the desk instead of submitting them.
8. Fix aspect ratio and document spawn positions so layout is stable across PCs.
9. Start adding visible paper/document art.

## Planning Documents Created

### `Assets/_Project/PlayableVersionActionPlan.md`

A full playable-version action plan was created. It covers what should be done to move from the current prototype toward a playable Day 1 MVP.

The main idea of the plan:

- Do not jump to Day 2 before Day 1 is funtionally playable.
- Day 1 should teach the loop with a small number of cases.
- The player should be able to inspect documents, compare fields, stamp approve/reject, submit cases, receive feedback, and proceed through a case queue.
- Polish should come after the loop is functional.

This file is now committed in `77ebc19`.

## Epic 1 Discussion Summary

The first epic discussed was focused on documents and interaction rather than deep content or final art.

The user asked whether it was already time to design detailed document visuals such as:

- Brazilian-like identity card.
- University documents with logo/header.
- Realistic institutional document layouts.

The recommendation was:

- Do not overinvest in final document design yet.
- Use simple but type-specific prefabs.
- Establish layout, interaction, binding, dragging, stamping, and submission first.
- Later, when the logic is stable, improve the visual design for each document type.

The user accepted this path.

The expected result of Epic 1 was:

- Document prefabs exist.
- Each document type has at least a basic visual identity.
- Documents can show runtime text values.
- Documents can be spawned for the active case.
- Documents can be dragged and layered.
- Documents can be approved/rejected.
- Documents can be submitted.
- Missing/invalid documents affect case correctness.

## Document Prefabs Created/Configured

The user manually created basic prefabs for:

- `DocumentIdentityCard`
- `DocumentSchoolTranscript`
- `DocumentEnrollmentProof`
- `DocumentGenericFallback`

These are under:

```text
Assets/_Project/Prefabs/Documents/
```

The user intentionally kept them simple. They did not add every specific field yet because it was considered too early and too costly for the current stage.

Important manual decision:

- Most documents should have RA.
- `IssueDateValue` was considered unnecessary for now.
- The identity card does not need the same fields as university documents.

The documents eventually showed runtime text correctly after fixing duplicated legacy text generation.

## `DocumentView`

Implemented file:

```text
Assets/_Project/Scripts/Documents/DocumentView.cs
```

Purpose:

- Owns the visible TMP text fields of a document prefab.
- Receives a `DocumentRecord`.
- Writes values into configured UI text fields.
- Keeps the document prefab presentation separate from drag logic.

Current serialized fields:

- `titleText`
- `nameValueText`
- `raValueText`
- `courseValueText`
- `notesText`
- `emptyValueText`

Current behavior:

- `Bind(DocumentRecord record)` fills title, name, RA, course, and notes.
- If a value is missing, it uses `---`.
- It reads fields from `DocumentRecord` using keys:
  - `nome`
  - `ra`
  - `curso`

Important setup:

- Each prefab must have a `DocumentView`.
- The TMP fields in the prefab must be assigned to the matching serialized fields.
- Legacy `bodyText` generated by `DraggableDocument` should not be used for these prefabs once `DocumentView` is configured.

## `DraggableDocument`

Implemented/updated file:

```text
Assets/_Project/Scripts/Documents/DraggableDocument.cs
```

Purpose:

- Makes a UI document draggable.
- Brings the selected document to the top with sibling order.
- Binds document data.
- Delegates visible text rendering to `DocumentView` when available.
- Tracks the last valid position so an unstamped document can be returned to the desk.

Important behavior:

- On begin drag:
  - Selects the document.
  - Calls `transform.SetAsLastSibling()` so it appears above others.
  - Saves `lastValidAnchoredPosition`.
  - Disables raycast blocking while dragging through `CanvasGroup`.
- On drag:
  - Converts screen position to local point in parent rect.
  - Updates `anchoredPosition`.
- On end drag:
  - Re-enables raycast blocking.
  - Raises `DragEnded`.
- `ReturnToLastValidPosition()` moves the document back to its last valid desk position.

Important resolved issue:

At one point documents showed old text plus new text. This happened because the old fallback text creation in `DraggableDocument` was still producing text while `DocumentView` fields also existed. The fix was to rely on `DocumentView` for configured prefabs and clear/avoid the old `bodyText` field setup in those prefabs.

## `DocumentManager`

Updated file:

```text
Assets/_Project/Scripts/Documents/DocumentManager.cs
```

Purpose:

- Spawns document prefabs for the current case.
- Clears old documents between cases.
- Keeps a list of current `DocumentRecord` objects.
- Keeps a list of spawned `DraggableDocument` views.
- Maps each `DocumentType` to a prefab.
- Now also supports visual spawn points.

Current important fields:

- `documentParent`
- `fallbackDocumentPrefab`
- `documentPrefabs`
- `documentSpawnPoints`
- `spawnPoints`
- `spawnPositions`

Important new type:

```csharp
[Serializable]
public class DocumentSpawnPointMapping
{
    public DocumentType documentType = DocumentType.Unknown;
    public RectTransform spawnPoint;
}
```

Spawn priority:

1. If a spawn point exists for the document type in `documentSpawnPoints`, use that.
2. Else, if an index-based spawn point exists in `spawnPoints`, use that.
3. Else, use fallback vector positions in `spawnPositions`.

This was added because index-based spawning can break when a case has a missing document. Example: if the second document is missing, the third document could spawn at the second slot. Type-based spawn points are more stable.

Manual Unity setup required:

- Create visual spawn point objects under `DocumentSpawnRoot`.
- Add `DocumentSpawnPointMarker` to each.
- Assign them into `DocumentManager.documentSpawnPoints` by document type.

Suggested spawn point names:

- `SpawnPoint_IdentityCard`
- `SpawnPoint_SchoolTranscript`
- `SpawnPoint_EnrollmentProof`

Suggested mapping:

- `IdentityCard` -> `SpawnPoint_IdentityCard`
- `SchoolTranscript` -> `SpawnPoint_SchoolTranscript`
- `EnrollmentProof` -> `SpawnPoint_EnrollmentProof`

## `DocumentSpawnPointMarker`

Added file:

```text
Assets/_Project/Scripts/Documents/DocumentSpawnPointMarker.cs
```

Purpose:

- Draws a visual rectangle and cross in Scene View for UI spawn points.
- Helps the user position document spawn points visually without needing visible marker graphics in the Game View.

Important behavior:

- Uses `OnDrawGizmos()`.
- Requires a `RectTransform`.
- Draws the rect corners and center cross.

Manual setup:

- Add this script to each spawn point UI object.
- Make the spawn point object a UI object with a `RectTransform`.
- Enable Gizmos in Scene View to see the marker.

## Document Approval/Rejection Flow

The user wanted to know how approving/rejecting works because at first the game only showed document text and had no paper appearance.

The current implemented interaction direction:

- There are approval/rejection stamps.
- A stamp is dragged over a document.
- The document receives an approved or rejected mark.
- Then the player drags the document to the gray area outside the desk to submit it.

Important design decision:

- There should not be a separate return tray.
- The normal flow should be:
  - The player drags a stamped document into the gray area outside the desk.
  - The system recognizes it as submitted.
- If the document has no stamp:
  - It should not submit.
  - It should return to the desk.

## `StampReceiver`

Existing file:

```text
Assets/_Project/Scripts/Desk/StampReceiver.cs
```

Purpose:

- Lives on document prefabs.
- Records whether the document has been stamped.
- Stores which decision was stamped.
- Shows the configured approved/rejected visual mark.

Manual setup:

- Each document prefab should have `StampReceiver`.
- Assign the approved mark object.
- Assign the rejected mark object.
- Approved/rejected mark objects usually start inactive.

Important user issue resolved:

The user asked which stamp object to use because approve/reject marks were not appearing. The solution was to ensure the correct draggable stamp objects were configured and that document prefabs had `StampReceiver` and stamp mark references assigned.

## `DraggableStamp`

Updated file:

```text
Assets/_Project/Scripts/Desk/DraggableStamp.cs
```

Purpose:

- Lets stamp UI objects be dragged.
- Applies a decision stamp to documents.

Important fix:

Initially, stamping only worked if the stamp was dragged to a specific point or the center of the document. The user wanted the stamp to work when dragged to any part of the document.

Implemented behavior:

- The stamp now checks overlap between its `RectTransform` and any `StampReceiver` `RectTransform`.
- This means stamping works when the stamp overlaps any part of the document, not only the center.

Manual setup:

- Stamp objects must have `DraggableStamp`.
- Each stamp should be configured with the intended `CaseDecision`:
  - Approve stamp -> `Approve`
  - Reject stamp -> `Reject`
- Documents need `StampReceiver`.

## `DocumentSubmissionZone`

Implemented/updated file:

```text
Assets/_Project/Scripts/Desk/DocumentSubmissionZone.cs
```

Purpose:

- Detects when a dragged document is dropped into the submission area.
- If the document is stamped, submits the stamped decision to `CaseManager`.
- If the document is not stamped, sends it back to its last valid desk position.

Important design:

- Submission happens by dragging the document into the gray area outside the desk.
- There is no separate return tray.
- No-stamp submission is rejected by the interaction layer, not by validation.

Manual setup:

- Create or use an existing gray/outside-desk UI area.
- Add `DocumentSubmissionZone` to it.
- Ensure it has a `RectTransform`.
- Ensure it can receive UI interaction appropriately.
- Assign `CaseManager` if needed, or let the script find it.

## Case Progression

At one point the user confirmed:

- The approve/reject buttons and progression to next case were working.
- Later, stamping and submitting documents also became the intended flow.

The current intended player action flow is:

1. Read NPC/request text.
2. Inspect documents.
3. Compare fields.
4. Drag approve/reject stamp over a document.
5. Drag stamped document outside the desk into gray submission area.
6. The case manager evaluates the decision.
7. The next case is loaded.

## Aspect Ratio And Resolution Discussion

The user wanted to lock the game to 1920x1080 because documents spawned or appeared differently on different PCs.

Important distinction:

- The Game View preset controls editor preview.
- The Canvas Scaler controls UI scaling.
- Player Settings control build resolution.
- Spawn points control where documents are instantiated.

Recommended Unity setup:

### Game View

Create/select a Game View resolution:

```text
1920 x 1080
16:9
```

### Canvas Scaler

Select the `Canvas` and configure:

```text
UI Scale Mode: Scale With Screen Size
Reference Resolution: 1920 x 1080
Screen Match Mode: Match Width Or Height
Match: 0.5
```

This makes UI layout more consistent across screens.

### Player Settings

The user showed that `Project Settings > Player > Resolution and Presentation` only had:

- `Fullscreen Mode`
- `Default Is Native Resolution`
- `Resizable Window`
- related standalone options

Instruction given:

- Disable `Default Is Native Resolution`.
- Then Unity should reveal `Default Screen Width` and `Default Screen Height`.
- Set:
  - Width: `1920`
  - Height: `1080`
- Set `Fullscreen Mode` to `Windowed` for now.
- Disable `Resizable Window`.

Important note:

This affects the final build, not the editor Game View.

## Document Spawn Position Visualization

The user wanted to see spawn positions in the Scene View and define where documents are born.

Implemented support:

- `DocumentManager.documentSpawnPoints`
- `DocumentSpawnPointMarker`

Recommended setup:

1. Under `DocumentSpawnRoot`, create UI objects:
   - `SpawnPoint_IdentityCard`
   - `SpawnPoint_SchoolTranscript`
   - `SpawnPoint_EnrollmentProof`
2. Give each a `RectTransform`.
3. Add `DocumentSpawnPointMarker`.
4. Position them in Scene View.
5. Assign them to `DocumentManager.documentSpawnPoints`.

Important:

- These spawn points are editor-visible markers.
- They do not need an `Image`.
- If they have an `Image`, it can be disabled after positioning.

## Paper Sprite / Document Background Discussion

The user created a paper PNG under:

```text
Assets/_Project/Art/Objects/Papel.png
```

The user set it as `Source Image` on the document prefabs, but initially nothing changed and only text appeared.

Diagnosis:

- `DocumentView` does not erase or change the background.
- The sprite was actually applied in the prefab.
- The real issue was that the root `RectTransform` sizes were broken.
- Some root `Image` components also still had alpha around `0.392`, making them highly transparent.

Prefab issues found:

- `DocumentSchoolTranscript` root had negative width and height `0`.
- `DocumentEnrollmentProof` root had negative width and almost zero height.
- `DocumentIdentityCard` root had strange anchors and near-zero dimensions.
- Some root images were still transparent.

Fix applied directly in prefabs:

- `DocumentSchoolTranscript`: root size set to `320 x 460`
- `DocumentEnrollmentProof`: root size set to `320 x 460`
- `DocumentIdentityCard`: root size set to `320 x 200`
- Alpha set to `1` on problematic root images.

After this, the paper became visible.

## Paper Scale And Text Contrast

The user then showed a screenshot where:

- The A4 papers were too large on the desk.
- The identity card looked too small.
- Text had poor contrast because it was white on a light paper texture.

Recommendation given:

- Do not resize the PNG file externally.
- Use `RectTransform` size in Unity for visual scale.
- Use darker text colors for readability.

Suggested document sizes:

```text
DocumentSchoolTranscript: 260 x 370
DocumentEnrollmentProof: 260 x 370
DocumentIdentityCard: 340 x 210
```

Suggested text color:

```text
R: 35
G: 28
B: 20
A: 255
```

Normalized equivalent:

```text
r: 0.14
g: 0.11
b: 0.08
a: 1
```

Suggested font sizes:

For A4-like documents:

- Title: `16` to `20`
- Labels: `10` to `12`
- Main values: `12` to `14`

For identity card:

- Title: `14` to `16`
- Fields: `9` to `11`

Important:

- The PNG can remain large/high-resolution.
- The UI `Image` scales it through `RectTransform`.
- Text should not be baked into the PNG because fields are dynamic.

## Current Git State At Handoff Creation

Before this handoff file was generated, the current work was committed with:

```text
77ebc19 Add document interaction and spawn setup
```

That commit included:

- Paper art import under `Assets/_Project/Art/Objects/`
- `PlayableVersionActionPlan.md`
- `DocumentSpawnPointMarker.cs`
- `DocumentManager.cs` updates
- Document prefab changes
- Scene changes
- ProjectSettings changes
- TextMesh Pro font material change

This handoff file itself was generated after that commit, so depending on whether a later commit is made, it may appear as an uncommitted file.

## Known Dirty/Changed Files Before Commit

Before commit `77ebc19`, `git status --short` showed:

```text
 M "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset"
 M Assets/_Project/Prefabs/Documents/DocumentEnrollmentProof.prefab
 M Assets/_Project/Prefabs/Documents/DocumentGenericFallback.prefab
 M Assets/_Project/Prefabs/Documents/DocumentIdentityCard.prefab
 M Assets/_Project/Prefabs/Documents/DocumentSchoolTranscript.prefab
 M Assets/_Project/Scenes/Game.unity
 M Assets/_Project/Scripts/Documents/DocumentManager.cs
 M ProjectSettings/ProjectSettings.asset
?? Assets/_Project/Art/Objects.meta
?? Assets/_Project/Art/Objects/
?? Assets/_Project/PlayableVersionActionPlan.md
?? Assets/_Project/PlayableVersionActionPlan.md.meta
?? Assets/_Project/Scripts/Documents/DocumentSpawnPointMarker.cs
?? Assets/_Project/Scripts/Documents/DocumentSpawnPointMarker.cs.meta
```

These were all included in the commit.

## Build Verification

Multiple builds were run with:

```text
dotnet build UltimoDiaUtilGame.sln --no-restore
```

The final build after the prefab paper fixes succeeded.

At one point the build showed only obsolete Unity API warnings such as `FindObjectOfType` and `FindObjectsSortMode`. Later, one build after adding `DocumentSpawnPointMarker` reported `0 warnings`, but another full build after prefab changes again showed warnings. These warnings are not currently blocking.

Known warning category:

- Unity API obsolete warnings.
- Examples:
  - `Object.FindObjectOfType<T>()` obsolete.
  - `FindObjectsSortMode` obsolete.
  - `FindFirstObjectByType` warning in editor helper.

These can be cleaned later but are not MVP blockers.

## Important Manual Unity Tasks Still Needed

### 1. Verify Paper Visibility

Open each document prefab:

- `DocumentIdentityCard`
- `DocumentSchoolTranscript`
- `DocumentEnrollmentProof`

Check:

- Root object has `Image`.
- `Source Image` is assigned to paper sprite or document-specific sprite.
- `Color` is white or desired tint.
- Alpha is `255` or `1`.
- Root `RectTransform` size is reasonable.
- Text children are above the background in hierarchy.

### 2. Adjust Document Sizes

The current committed sizes are:

```text
SchoolTranscript: 320 x 460
EnrollmentProof: 320 x 460
IdentityCard: 320 x 200
```

The recommended next adjustment after the latest screenshot is:

```text
SchoolTranscript: 260 x 370
EnrollmentProof: 260 x 370
IdentityCard: 340 x 210
```

This adjustment may not yet be committed unless done after this handoff.

### 3. Improve Text Contrast

Change all document text from white to dark brown/near-black.

Suggested:

```text
R: 35
G: 28
B: 20
A: 255
```

This should be done on all TMP text elements in the document prefabs.

### 4. Configure Spawn Points

Create spawn point UI objects under `DocumentSpawnRoot`, add `DocumentSpawnPointMarker`, then assign them to `DocumentManager.documentSpawnPoints`.

### 5. Configure Canvas Scaler

Select `Canvas`:

```text
UI Scale Mode: Scale With Screen Size
Reference Resolution: 1920 x 1080
Screen Match Mode: Match Width Or Height
Match: 0.5
```

### 6. Configure Game View

Use editor Game View resolution:

```text
1920 x 1080
```

### 7. Configure Build Resolution

In Player Settings:

- Disable `Default Is Native Resolution`.
- Set default width/height to `1920 x 1080`.
- Use `Windowed` for now.
- Disable `Resizable Window`.

## Known Scene/Setup Notes

The `Game` scene has:

- `Systems` object.
- A `DocumentManager` component on the systems object.
- A `DocumentSpawnRoot`.
- Prefab mappings for document types.

Earlier, the user thought there was no `DocumentManager`, then realized the scene had the component/object manager setup.

Important:

- If documents do not spawn, check that `DocumentManager` is present and assigned.
- If clones do not appear in the hierarchy during Play Mode, check that the active day/case has documents and that `GameManager`/`CaseManager` starts the case queue.

## Possible Issue With Active Day Asset

Earlier inspection suggested:

- `GameManager.daySequence` may point to an older/simple `Day_01.asset`.
- There may also be a better generated day asset:

```text
Assets/_Project/ScriptableObjects/Days/Day_01_EnrollmentBasics.asset
```

This should be checked later in Unity.

If the wrong day is active, document/case behavior may not reflect the intended enrollment basics setup.

## Product/Design Decisions Preserved

Important decisions from the conversation:

- Do not spend too much time on final document design yet.
- Do create distinct document prefabs.
- Dynamic values should be text components, not baked into paper art.
- Paper/background can be a PNG sprite in UI `Image`.
- Use UI `RectTransform`, not `SpriteRenderer`, for current documents.
- Use `DocumentView` for visible fields.
- Use stamps for approve/reject interaction.
- Dragging a stamped document outside the desk submits the case.
- Dragging an unstamped document outside the desk returns it to the desk.
- No separate return tray for now.
- Stable layout should be handled through Canvas Scaler, 1920x1080 Game View, Player Settings, and visual spawn points.
- Use type-based spawn points, not only index-based spawn positions.

## Suggested Next Steps

The next useful development steps are:

1. Finish document readability:
   - Resize document prefabs.
   - Make text dark.
   - Increase font sizes where needed.
   - Reposition fields within each document.

2. Finalize spawn positions:
   - Create the visual spawn points.
   - Assign them by type.
   - Test Play Mode at 1920x1080.

3. Verify the full document flow:
   - Case loads.
   - Documents spawn in correct places.
   - Documents show paper background.
   - Text is readable.
   - Dragging works.
   - Z-order works.
   - Stamp works on any part of document.
   - Unstamped document dropped outside returns.
   - Stamped document dropped outside submits.
   - Next case loads.

4. Then move toward playable Day 1:
   - Ensure there are multiple cases.
   - Ensure at least one correct approval and one correct rejection.
   - Make feedback clear.
   - Make day/case summary visible.
   - Make money/warnings update.

## Recommended Prompt For Next Chat

Use this prompt in a new chat:

```text
Continue development of the Unity project at C:\Users\pleneo\Documents\Jean\UltimoDiaUtilGame.

Read AGENTS.md, GDD.md, backlog.md, and Assets/_Project/ChatContextHandoff.md first.

The latest committed state is 77ebc19 Add document interaction and spawn setup. The current focus is making the document UI readable and stable: adjust document prefab sizes, darken text, configure 1920x1080 Canvas/Game View behavior, and finish spawn point setup. Preserve user changes and do not rewrite architecture.
```

