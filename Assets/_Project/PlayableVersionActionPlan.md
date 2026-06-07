# Playable Version Action Plan

## Purpose

This document defines the next practical steps required to turn the current project state into a playable Day 1 version of **Ultimos Dias Uteis**.

The project already has several important systems implemented, but the current goal is not to expand scope. The goal is to make the first day feel complete, understandable, and playable without relying on debug buttons or the Unity Console.

The target first playable version is:

```text
The player starts Day 1, receives enrollment cases, reads documents, stamps a decision, delivers the document, receives feedback, advances through the queue, and reaches an end-of-day summary.
```

Only after this loop works reliably should the team move to Day 2, additional request types, more characters, or visual polish.

## Current State

The project currently has:

- A Unity project configured and compiling.
- A `Game` scene.
- Core systems:
  - `GameManager`
  - `DayManager`
  - `CaseManager`
  - `CaseValidator`
  - `DocumentManager`
  - `EconomyManager`
  - `UIManager`
- Day/case/document ScriptableObject data.
- Day 1 enrollment data.
- Document prefabs for:
  - Identity Card
  - School Transcript
  - Enrollment Proof
  - Generic fallback document
- Runtime document spawning by `DocumentType`.
- Document data binding through `DocumentView`.
- Drag-and-drop documents through `DraggableDocument`.
- Z-order behavior when selecting or dragging documents.
- Stamp tools through `DraggableStamp`.
- Stamp receiving through `StampReceiver`.
- Submission detection through `DocumentSubmissionZone`.
- Case decision submission through `CaseManager.SubmitDecision`.

The project is close to a playable Day 1, but several integration and presentation tasks remain.

## Main Risks

The main risk is moving to new content too early.

Avoid doing the following before this plan is complete:

- Starting Day 2 payment rules.
- Creating Day 3 withdrawal rules.
- Adding complex NPC behavior.
- Adding final art polish.
- Building save/load.
- Adding more document types.
- Expanding the economy system.

The current priority is to finish the first playable loop.

## Definition of Playable Day 1

Day 1 is considered playable when all of the following are true:

```text
[ ] The Game scene starts Day 1 correctly.
[ ] The active DayConfig is the complete enrollment Day 1 asset.
[ ] A student/case appears.
[ ] Documents spawn on the desk.
[ ] Documents show readable case data.
[ ] Documents can be dragged.
[ ] Clicking/dragging brings documents to the front.
[ ] The player can apply approved/rejected stamps.
[ ] The player can deliver a stamped document to the gray submission area.
[ ] A document without a stamp returns to the desk.
[ ] The submitted stamp becomes Approve or Reject.
[ ] CaseValidator decides whether the choice is correct.
[ ] Economy changes after the decision.
[ ] The player can advance to the next case.
[ ] The day can end.
[ ] A day summary appears.
[ ] The loop can be played without looking at the Console.
```

## Priority 1: Use the Correct DayConfig

### Problem

The `GameManager` may currently be pointing to an older/simple Day 1 asset:

```text
Assets/_Project/ScriptableObjects/Day/Day_01.asset
```

The more complete Day 1 asset is:

```text
Assets/_Project/ScriptableObjects/Days/Day_01_EnrollmentBasics.asset
```

The complete asset includes enrollment generation data, rulebook entries, and the intended enrollment cases.

### Required Work

In the `Game` scene:

1. Select:

```text
Game > Systems
```

2. Find the `GameManager` component.

3. In `Day Sequence`, set element 0 to:

```text
Day_01_EnrollmentBasics
```

4. Save the scene.

### Expected Result

When the game starts, Day 1 uses the complete enrollment configuration.

### Acceptance Criteria

```text
[ ] GameManager.daySequence[0] points to Day_01_EnrollmentBasics.
[ ] The rulebook has Day 1 enrollment rules.
[ ] Multiple generated enrollment cases can appear.
[ ] Manual/debug-only Day_01.asset is no longer used for the playable flow.
```

## Priority 2: Lock the Core Document Interaction Flow

### Goal

The intended player flow should be:

```text
Read documents -> stamp document -> drag to gray area -> resolve case.
```

The player-facing flow should not require direct debug decision buttons.

### Current Supporting Systems

Relevant scripts:

```text
Assets/_Project/Scripts/Documents/DocumentManager.cs
Assets/_Project/Scripts/Documents/DocumentView.cs
Assets/_Project/Scripts/Documents/DraggableDocument.cs
Assets/_Project/Scripts/Desk/DraggableStamp.cs
Assets/_Project/Scripts/Desk/StampReceiver.cs
Assets/_Project/Scripts/Desk/DocumentSubmissionZone.cs
```

### Required Work

Verify and configure:

- Documents spawn from `DocumentManager`.
- Each document prefab has:
  - `DraggableDocument`
  - `DocumentView`
  - `StampReceiver`
  - approved stamp visual
  - rejected stamp visual
- The gray submission area has `DocumentSubmissionZone`.
- `DocumentSubmissionZone` references:
  - `DocumentManager`
  - `CaseManager`
  - its own `RectTransform` as `Submission Area`
- Stamps on the desk have `DraggableStamp`.
- Approved stamp uses `StampType.Aprovado`.
- Rejected stamp uses `StampType.Negado`.

### Expected Result

The player can resolve a case by stamping and delivering a document.

### Acceptance Criteria

```text
[ ] Dragging approved stamp over a document shows approved mark.
[ ] Dragging rejected stamp over a document shows rejected mark.
[ ] Dragging unstamped document to gray area returns it to the desk.
[ ] Dragging approved stamped document to gray area submits Approve.
[ ] Dragging rejected stamped document to gray area submits Reject.
[ ] The case clears after successful submission.
[ ] The next-case flow becomes available after case resolution.
```

## Priority 3: Separate Player Controls From Debug Controls

### Problem

The project still contains direct decision controls such as:

```text
Approve
Reject
Forward
```

These are useful for development, but they bypass the physical document flow.

### Required Work

Choose one:

1. Hide the direct decision buttons in the playable scene.
2. Move them into a clearly labeled debug panel.
3. Disable them for presentation builds.

Recommended MVP approach:

```text
Keep debug buttons available for developers, but make the visible player flow use stamps and document delivery.
```

### Expected Result

A player understands that decisions are made by stamping and delivering paperwork, not by pressing debug buttons.

### Acceptance Criteria

```text
[ ] Player-facing UI does not emphasize direct Approve/Reject buttons.
[ ] Debug controls are visually separated or hidden.
[ ] Stamping and delivery are the normal way to resolve a case.
```

## Priority 4: Connect End-of-Day Summary

### Problem

`UIManager` has a `daySummaryPanel` field, but it may not be connected in the scene.

If this is missing, the day can technically end but the player will not receive a clear end-of-day result.

### Required Work

In the `Game` scene:

1. Select:

```text
Game > Systems
```

2. Find the `UIManager` component.

3. Assign the scene's `DaySummaryPanel` object to:

```text
Day Summary Panel
```

If there is no usable summary panel in the scene, create or restore one with:

```text
DaySummaryPanel
```

Expected fields should show:

- day label
- completed cases
- correct decisions
- incorrect decisions
- money/debt/warnings
- continue/restart action if available

### Expected Result

When the day ends, the player sees a summary instead of relying on logs.

### Acceptance Criteria

```text
[ ] Day summary appears when all cases are complete.
[ ] Day summary appears when time runs out.
[ ] Day summary shows correct/incorrect count.
[ ] Day summary shows economy state.
[ ] Player can continue or restart according to current flow.
```

## Priority 5: Improve Minimal Visual Clarity

### Goal

The game does not need final art yet, but it must be readable and understandable.

### Required Work

Documents should have:

- simple paper-like background
- clear title
- readable fields
- visible stamp mark
- enough contrast between paper and desk

Desk should have:

- clear work area
- clear gray submission area
- stamps visibly separate from documents

Avoid spending time on:

- final logo
- detailed texture
- perfect pixel art
- complex animations

### Expected Result

The scene looks like a rough but intentional bureaucracy desk, not a debug test screen.

### Acceptance Criteria

```text
[ ] Documents look like papers/cards, not raw panels.
[ ] Text is readable at gameplay resolution.
[ ] Approved/rejected marks are visible.
[ ] The gray delivery area is understandable.
[ ] Stamps are easy to identify.
```

## Priority 6: Player Feedback

### Goal

The player needs clear feedback when the game accepts or rejects an action.

### Required Feedback Cases

1. Document submitted without stamp:

```text
Carimbe o documento antes de entregar.
```

2. Correct decision:

```text
Decision accepted as correct.
```

3. Incorrect decision:

```text
Decision accepted as incorrect.
```

4. Case completed:

```text
The next student/case can proceed.
```

### Required Work

Use an existing feedback text if available, or add a small `TMP_Text` near the desk/HUD.

Connect it to:

```text
DocumentSubmissionZone.feedbackText
```

Later, feedback can be polished with sound and animations.

### Acceptance Criteria

```text
[ ] Submitting an unstamped document gives visible feedback.
[ ] Correct/incorrect result is visible somewhere.
[ ] Player understands when to proceed to the next case.
```

## Priority 7: Full Day 1 Manual Test

### Goal

Prove that Day 1 is actually playable.

### Test Cases

Run the following tests in Play Mode.

#### Test 1: Valid Enrollment

Expected player action:

```text
Read documents.
Confirm required documents are present.
Confirm names match.
Confirm RA matches.
Stamp approved.
Deliver to gray area.
```

Expected result:

```text
DecisionType.Approve is submitted.
Decision is correct.
Money/reward updates.
No warning is added.
```

#### Test 2: Missing Document

Expected player action:

```text
Notice missing required document.
Stamp rejected.
Deliver to gray area.
```

Expected result:

```text
DecisionType.Reject is submitted.
Decision is correct.
Money/reward updates.
No warning is added.
```

#### Test 3: Name Mismatch

Expected player action:

```text
Compare names between documents.
Notice mismatch.
Stamp rejected.
Deliver to gray area.
```

Expected result:

```text
DecisionType.Reject is submitted.
Decision is correct.
Money/reward updates.
No warning is added.
```

#### Test 4: Wrong Decision

Expected player action:

```text
Approve an invalid case, or reject a valid case.
```

Expected result:

```text
Decision is incorrect.
Penalty is applied.
Warning increases if case is critical.
```

#### Test 5: Unstamped Submission

Expected player action:

```text
Drag unstamped document to gray area.
```

Expected result:

```text
Document returns to previous desk position.
Case is not resolved.
Feedback text appears.
```

#### Test 6: Day Completion

Expected player action:

```text
Complete all cases in the day.
```

Expected result:

```text
Day ends.
Summary appears.
Counts and economy are visible.
```

### Acceptance Criteria

```text
[ ] All tests pass in Play Mode.
[ ] No blocking Console errors occur.
[ ] Player can finish Day 1 without debug controls.
```

## Priority 8: Clean Duplicate or Old Assets

### Problem

There appear to be older/test assets in the project, such as:

```text
Assets/_Project/ScriptableObjects/Day/Day_01.asset
Assets/_Project/ScriptableObjects/Case_01_Valido.asset
Assets/_Project/ScriptableObjects/Doc_Identity.asset
Assets/_Project/ScriptableObjects/Test/Economy_Test.asset
```

These may still be useful as prototypes, but they can confuse future work.

### Required Work

After the playable Day 1 flow is stable:

1. Decide which assets are production MVP content.
2. Move old/test assets into a clearly named folder:

```text
Assets/_Project/ScriptableObjects/_Deprecated
```

or delete them if the team agrees.

Do not delete assets until the scene and day config are confirmed stable.

### Acceptance Criteria

```text
[ ] Game scene uses only the intended Day 1 asset.
[ ] Old test assets are not referenced by GameManager.
[ ] Deprecated content is clearly separated.
```

## Priority 9: Only Then Start Day 2

Day 2 should not begin until Day 1 is playable.

Start Day 2 only after:

```text
[ ] Day 1 can be completed.
[ ] Summary appears.
[ ] Document stamping and submission work.
[ ] Rulebook is readable.
[ ] Debug buttons are not required.
```

Then Day 2 can add:

- tuition/payment document type
- fake money or invalid payment rule
- payment-specific cases
- updated rulebook

## Immediate Next Action

The immediate next action is:

```text
Set GameManager.daySequence[0] to Day_01_EnrollmentBasics and run a complete Day 1 playtest.
```

If that fails, fix only the blocker found during that playtest before adding new features.

