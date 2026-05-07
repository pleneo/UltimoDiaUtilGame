# AGENTS.md

## Project Context

This repository is intended to become a new Unity project for **Ultimos Dias Uteis**, a 2D bureaucratic puzzle/simulation game inspired by *Papers, Please*, set in a Brazilian university administration office similar in mood and structure to Unifor.

The current repository may not yet contain the Unity project files. Treat this document as the persistent project context for future AI agents and developers. The main design source is `GDD.md`; this file converts that design into implementation guidance, scope boundaries, business rules, and maintainable Unity architecture.

The team is building this game for a Computer Graphics discipline. The developers are mostly inexperienced with game development, so every technical decision should optimize for:

- A functional, complete, playable MVP within approximately one month.
- Clear and teachable Unity/C# code.
- Simple architecture that avoids overengineering.
- High delivery quality for an academic evaluation.
- Visible use of Computer Graphics concepts requested by the GDD.

The game target platform is **Windows**, built with **Unity**, using **C#** and **2D orthographic rendering**.

Also, access `./backlog.md` and `./GDD.md` to more context.

## Product Vision

The player is a university student close to graduation but blocked by unpaid tuition debt. To pay that debt, the player works at the university administration office and must process student requests by validating documents, payments, and special cases under time pressure.

The experience should feel like a Brazilian university bureaucracy satire:

- Strict institutional procedures.
- Low-light, bureaucratic office atmosphere.
- Humor through awkward students, suspicious situations, untrained staff, and rigid rules.
- Moral dilemmas where the legal/procedural decision may conflict with empathy.

The game loop is:

1. A student/NPC arrives at the service desk.
2. The NPC presents a request through dialogue/speech bubble and gibberish audio.
3. The player checks the rulebook and the notice board.
4. The player inspects and moves documents on the desk.
5. The player decides: **Approve**, **Reject**, or **Forward to Supervisor**.
6. The game checks the decision.
7. The player earns money for correct decisions or receives penalties for mistakes.
8. At the end of the day, money, debt, warnings, and survival expenses are calculated.
9. New days add more rules, request types, documents, and difficulty.

Victory condition: finish the final planned day with enough money to pay the university debt.

Game over conditions:

- Too many warnings/strikes.
- Money goes negative or the player cannot survive required expenses.
- The final debt cannot be paid by the end, depending on final balancing.

## MVP Scope

The project has only about one month. Future agents must protect the MVP from scope creep.

The agreed MVP direction is intentionally smaller than the full GDD. The best delivery strategy is a smaller game that is complete, playable, readable, and easy to explain technically, instead of a broad feature set with unfinished systems.

The MVP should include:

- One playable work desk scene.
- A start/menu scene only if time allows; otherwise the first scene may start directly.
- A complete day loop: start day, process queue, end day summary, proceed to next day.
- At least 3 playable days with escalating rules.
- Exactly 3 core request types at first: enrollment, tuition payment, and class withdrawal.
- Approve, reject, and forward decisions.
- Rulebook UI that changes by day.
- Notice board UI for special cases, preferably integrated as a tab in the side panel.
- Drag-and-drop documents on the desk.
- Selected document moves visually to the top using z-order/sibling order.
- Basic NPC queue.
- Money, penalties, warnings, and debt tracking.
- Basic sound feedback for correct/incorrect decisions, stamp, paper movement, click, and gibberish.
- Pixel-art-inspired UI/art, even if placeholder assets are used initially.
- Clear tutorialization through rulebook pages and day progression, not long external explanations.

Nice-to-have features after the MVP works:

- Course transfer requests.
- Graduation certificate issuance requests.
- More NPC variations using modular head/body/accessory sprites.
- More than one moral dilemma case or delayed moral consequences.
- High contrast accessibility mode.
- Separate music/SFX volume settings.
- Animated NPC expressions.
- More document forgery types.
- More polished end-of-day economy decisions such as food/hygiene choices.
- Object pooling for NPCs.

Avoid these unless explicitly requested and time remains:

- Complex save/load systems.
- Procedural narrative generation.
- Real physics simulation.
- Large branching story trees.
- Network features.
- Advanced AI behavior.
- Fully generalized document editor tools.
- Dozens of document types before the core loop is polished.

## Core Business Rules

### Request Types

The GDD defines these university service requests:

1. Enrollment/matricula.
2. Course withdrawal or class cancellation/trancamento de cadeira.
3. Tuition payment/pagamento de mensalidade.
4. Course transfer/troca de curso.
5. Graduation certificate issuance/emissao de certificado de conclusao.

For the first playable version, implement only the first three MVP request types:

- **Enrollment**: best for missing documents, identity/name mismatch, and date validation.
- **Tuition payment**: best for fake money, wrong amount, boleto/payment matching, and suspicious characters.
- **Class withdrawal**: best for fee rules, deadlines, required signatures, and one moral dilemma.

Course transfer and graduation certificate issuance should remain backlog content until the full loop is stable.

Each request type must define:

- Required documents.
- Required fields to compare across documents.
- Valid date rules.
- Required stamps/signatures.
- Whether money/payment validation is involved.
- Whether the request may be forwarded to a supervisor.
- Whether a moral dilemma exception may exist.

### Decision Outcomes

Player decisions:

- **Approve**: the request is accepted.
- **Reject**: the request is denied.
- **Forward**: the case is sent to a superior/supervisor.

For the MVP, Forward is not necessary, it should be a feature if time permits.

The expected correct decision for a case must be data-driven whenever possible. Avoid scattering hard-coded case logic across UI scripts.

Rules for correctness:

- Approve is correct when all required documents and conditions are valid and the case is not listed as supervisor-only.
- Reject is correct when at least one required condition fails and there is no mandatory supervisor forwarding rule.
- Forward is correct when the student/case appears on the notice board or the case is flagged as requiring supervisor review.
- Moral dilemma cases may intentionally allow a procedurally wrong decision to have narrative consequences. Represent this explicitly in case data rather than special-casing it in button scripts.

### Document Validation Rules

A document can be invalid because:

- It is missing.
- It has an expired or incorrect date.
- A required field does not match another document.
- A name, course, registration number, payment amount, or identity field is inconsistent.
- A stamp/signature is missing.
- A stamp/signature is visually invalid/fake.
- Physical/visual integrity is suspicious.
- Money is fake or the payment amount is wrong.
- The request conflicts with a rule introduced in the current day.

The GDD specifically expects these validation activities:

- Check document expiration dates.
- Check whether the correct document was provided.
- Check physical/visual integrity.
- Check required stamps.
- Compare information between document A and document B.
- Detect fake money.
- Check the notice board for special cases to forward.

### Economy Rules

The GDD defines these reward/penalty concepts:

- Correct judgments earn money.
- More correct judgments in a day should increase total daily earnings.
- Incorrect judgments increase the fine/penalty burden for the day.
- Excessive mistakes or negative money can cause game over.
- The player has a university debt that must be paid to graduate.
- Personal expenses such as food and hygiene may pressure the player to save money.

For MVP implementation, use simple and transparent economy rules:

- `dailyGrossPay = correctDecisions * payPerCorrectDecision`
- `dailyPenalty = incorrectDecisions * penaltyPerMistake`
- `dailyNet = dailyGrossPay - dailyPenalty - dailyExpenses`
- `currentMoney += dailyNet`
- `remainingDebt` is reduced by explicit end-of-day payment or automatically by available money, depending on final design choice.
- `warnings += incorrectCriticalDecisions`, and reaching the warning limit triggers game over.

Do not implement a full food/hygiene survival system before the main loop is complete. Personal expenses may appear as a fixed daily deduction or as end-of-day flavor text in the MVP.

Use concrete, readable balancing numbers during early development. For example: initial debt, pay per correct case, penalty per mistake, daily expense, and max warnings should all be visible in one config asset.

Keep balancing values in `ScriptableObject` assets or serialized config fields so non-programmers can tune them.

### Day Progression

The game is structured in daily work cycles. Each day should define:

- Work time limit.
- Number of NPCs/cases in the queue.
- Available request types.
- New rulebook pages/rules.
- Notice board entries.
- Reward and penalty values.
- Which document validation rules are active.
- Narrative or moral dilemma cases.

Difficulty must increase gradually by adding cognitive load:

- More documents per case.
- More fields to compare.
- More possible fake/invalid data.
- More rules in the rulebook.
- More pressure from time or queue length.

Do not introduce multiple new mechanics in a single day unless the day is explicitly a later challenge day.

## Required Computer Graphics Concepts

The GDD names specific graphics/math concepts. These should be visible in the implementation and, ideally, easy to explain during evaluation.

### Translation

Documents and desk objects must move on the X/Y plane during drag-and-drop.

Implementation guidance:

- Use mouse/touch input converted from screen coordinates to world coordinates or canvas coordinates.
- Keep dragged document movement deterministic and easy to inspect.
- Avoid Rigidbody2D unless there is a real need. This game mostly needs direct transform movement.

### Z-Order

The last selected document should appear above the other documents.

Implementation guidance:

- For UI documents, use `transform.SetAsLastSibling()`.
- For SpriteRenderer documents, update `sortingOrder`.
- Centralize this in a small `DeskItem` or `DraggableDocument` script.

### Point-in-Polygon / Hit Testing

The GDD asks for point-in-polygon verification to determine whether the mouse coordinates are inside a document area and to provide visual feedback.

Implementation guidance:

- For rectangular placeholder documents, Unity UI raycasts are acceptable during early MVP.
- For final academic visibility, implement or keep a small point-in-polygon utility for one specific, demonstrable mechanic rather than for every click in the game.
- Good uses: detecting whether the cursor is inside an irregular damaged-document region, a signature zone, a stamp area, or a custom-shaped document hotspot.
- The utility should be isolated, tested with simple cases, and commented enough for the team to explain it.
- Hover feedback should include highlight and slight zoom.

### Queue

The NPC line must be managed as a queue.

Implementation guidance:

- Use `Queue<StudentCase>` or a list with explicit index progression.
- Keep queue logic separate from NPC visuals.
- Object pooling for NPC visual instances is optional for MVP. The queue working correctly matters more than optimizing NPC instantiation in this small game.

## Recommended Unity Architecture

Prefer a small, understandable architecture over a heavy framework. The team should be able to explain every script.

### Scene Structure

Recommended scenes:

- `Boot` or `MainMenu` only if needed.
- `Game` for the main desk/day gameplay.
- Optional `EndScreen` if final presentation needs it.

For MVP, one main scene with day summary overlays is acceptable.

### Folder Structure

Recommended Unity project folders:

```text
Assets/
  _Project/
    Art/
      Characters/
      Documents/
      UI/
      Environment/
    Audio/
      Music/
      SFX/
      Voice/
    Prefabs/
      Documents/
      NPCs/
      UI/
      Desk/
    Scenes/
    ScriptableObjects/
      Days/
      Documents/
      Cases/
      Rules/
      Economy/
    Scripts/
      Core/
      Day/
      Cases/
      Documents/
      UI/
      Desk/
      Audio/
      Utilities/
    Settings/
```

Keep third-party assets outside `_Project` if imported.

### Manager Responsibilities

Use managers sparingly. A manager should own a system-level lifecycle, not become a place for random logic.

Recommended managers:

- `GameManager`: high-level state transitions: boot, day start, active case, day end, game over, victory.
- `DayManager`: loads day config, queue, timer, active rules, and end-of-day summary.
- `CaseManager`: presents the current student case, checks decisions, records result.
- `DocumentManager` or `DeskController`: spawns and clears documents for the active case.
- `EconomyManager`: money, debt, penalties, warnings, daily calculations.
- `AudioManager`: music, SFX, gibberish voice playback.
- `UIManager`: coordinates screen panels, buttons, rulebook, notice board, money/time display.

Avoid global singletons everywhere. If using singletons for beginner simplicity, keep them minimal and documented. Prefer serialized references in the Inspector for scene-specific dependencies.

### Data-Driven Design

Use `ScriptableObject` assets for editable content:

- `DayConfig`
- `StudentCaseDefinition`
- `DocumentDefinition`
- `RuleDefinition`
- `EconomyConfig`
- `NPCDefinition`

The reason is practical: designers/artists can create and tune content without editing C# code.

Example conceptual fields:

```csharp
public class DayConfig : ScriptableObject
{
    public int dayNumber;
    public float workDurationSeconds;
    public int maxWarningsBeforeGameOver;
    public List<RuleDefinition> activeRules;
    public List<StudentCaseDefinition> cases;
    public List<NoticeBoardEntry> noticeBoardEntries;
    public int payPerCorrectDecision;
    public int penaltyPerMistake;
}
```

Do not overbuild a universal rule engine too early. For the MVP, simple validators attached by enum/type are acceptable as long as the code remains centralized and readable.

### Suggested Runtime Flow

1. `GameManager` starts the selected day.
2. `DayManager` loads `DayConfig`.
3. UI updates rulebook and notice board.
4. The first `StudentCaseDefinition` is dequeued.
5. `CaseManager` shows NPC dialogue and request type.
6. `DocumentManager` spawns the case's documents on the desk.
7. The player inspects and drags documents.
8. The player clicks Approve, Reject, or Forward.
9. `CaseValidator` determines whether the decision is correct.
10. `EconomyManager` records money/warnings/penalties.
11. Documents are cleared and next case starts.
12. When time or queue ends, `DayManager` shows daily summary.
13. The player proceeds to the next day, game over, or victory.

## Code Standards

Write code for beginners to maintain.

Rules:

- Use clear class names that match Unity object responsibilities.
- Keep MonoBehaviours small and focused.
- Avoid large `Update()` methods. Use events, button callbacks, and state transitions.
- If `Update()` is needed for dragging or timer, keep it tiny and obvious.
- Keep business rules out of button/UI scripts.
- Keep UI display logic separate from validation/economy logic.
- Prefer serialized private fields with `[SerializeField]` over public mutable fields.
- Validate required Inspector references in `Awake()` or `OnValidate()` when useful.
- Use enums for request/decision/document types where it improves readability.
- Avoid clever generic abstractions until duplication is painful and understood.
- Use comments only when they clarify non-obvious logic, especially validation rules or math.

Recommended naming:

- Classes and methods: `PascalCase`
- Fields and local variables: `camelCase`
- Private serialized fields: `camelCase` or `_camelCase`, but stay consistent with existing project style once established.
- ScriptableObject assets: readable names such as `Day_01_EnrollmentBasics`.

## UI and Interaction

The screen layout from the GDD:

- Center: work desk/documents.
- Left: student queue.
- Right: side panel with tabs for rulebook and notice board.
- Top: day timer, money, debt, and warnings.
- Main buttons: Approve, Reject, Forward.

Prefer the tabbed side panel for MVP instead of showing a full rulebook and a full notice board at the same time. This keeps the screen readable and avoids crowding the desk.

Interaction requirements:

- Documents can be dragged and repositioned.
- Clicking/hovering documents gives visual feedback.
- The selected document is brought to the front.
- Stamps/buttons should feel tactile with sound feedback.
- The rulebook must be readable and usable during gameplay.
- The notice board must be easy to check quickly.

Accessibility goals:

- Use readable pixel-style fonts, not overly decorative text.
- Do not rely on color alone; combine colors with icons/text.
- Green means correct, red means error, yellow means attention.
- Keep high contrast mode as a nice-to-have unless the core MVP is already complete.
- Separate music and SFX volume settings if time permits.

## Art Direction

The game should be 2D, orthographic, and pixel-art-inspired.

Visual mood:

- Low-light university administration office.
- Cold/desaturated base colors: gray, dark blue, institutional green.
- Brighter highlight colors for interaction, warnings, stamps, and errors.
- Satirical Brazilian university identity, but avoid direct trademark or brand dependency unless the team has permission.

Character direction:

- Simple pixel art silhouettes.
- Slight caricature.
- Readable states such as anxious, angry, sad, suspicious, nostalgic.
- For MVP, use a small set of complete NPC sprites first.
- Modular sprites such as head + body + accessories are useful later, but should not block the first playable version.

Known character concepts from the GDD:

- Blond boy: wants to enroll but has messy/incomplete documents.
- Calculus repeater: wants to withdraw from a class and argues about the withdrawal fee.
- Father and daughter: paying tuition, suspicious and anxious.
- Red-haired woman: wants to transfer courses and talks emotionally.
- Graduated student: requests certificate and is nostalgic.

## Audio Direction

Essential SFX:

- Stamp: dry and satisfying.
- Paper movement.
- Selection click.
- Error sound: low/heavy.
- Correct sound: light/positive.
- Gibberish voice, similar in spirit to Animal Crossing but original.

Music:

- Lo-fi or repetitive corporate ambience.
- Intensity may increase as the workday timer gets close to ending.

Implementation guidance:

- Use `AudioMixer` if the team implements separate volume settings.
- Use short, reusable clips before adding many variations.
- Audio feedback should reinforce the result without requiring the player to look away from documents.

## Content Plan for a One-Month MVP

Suggested sprint order:

### Week 1: Vertical Interaction Prototype

- Unity project setup.
- Main desk scene.
- Drag-and-drop document prototype.
- Z-order on selected document.
- Decision buttons.
- One hard-coded case.
- Basic UI for timer and money.

### Week 2: Data and Day Loop

- ScriptableObject case/day data.
- Queue of cases.
- Side panel tabs for rulebook and notice board.
- Validation for at least enrollment and payment.
- End-of-day summary.

### Week 3: More Rules and Game Feel

- Add class withdrawal.
- Add fake document/missing document/field mismatch rules.
- Add money/debt/warning game over.
- Add sounds.
- Add placeholder/pixel art pass.

### Week 4: Polish and Evaluation Readiness

- Balance days.
- Add narrative flavor and at least one moral dilemma.
- Improve visual feedback.
- Bug fixing.
- Build Windows executable.
- Prepare explanation of Computer Graphics concepts: translation, z-order, point-in-polygon, queue.

## Testing and Quality

Testing should match project risk and team capacity.

Minimum manual test checklist before delivery:

- Start game and complete each day.
- Process all request types.
- Approve, reject, and forward all produce correct feedback.
- Correct and incorrect decisions affect money correctly.
- Warnings can trigger game over.
- Debt/victory condition works.
- Timer ends the day correctly.
- Documents can be dragged without disappearing or getting stuck.
- Last selected document appears on top.
- Rulebook and notice board content match the active day.
- Windows build launches and is playable.

Recommended automated or semi-automated tests if time permits:

- Pure C# tests for validation logic.
- Pure C# tests for point-in-polygon utility.
- Pure C# tests for economy calculations.

Keep validation logic testable by avoiding direct Unity scene dependencies inside the core decision checker.

## Implementation Priorities

When future agents must choose between alternatives, use this order:

1. Keep the game playable end-to-end.
2. Keep the code understandable to inexperienced Unity developers.
3. Keep content editable without code changes.
4. Preserve the GDD's core fantasy and required mechanics.
5. Add polish only after the core loop works.

## Common Pitfalls to Avoid

- Building many document types before the first full day is playable.
- Hiding all rules in code, making it hard for designers to tune content.
- Letting UI scripts decide business correctness.
- Creating too many managers with overlapping responsibilities.
- Using physics for simple dragging.
- Spending too much time on menus while the main game loop is unfinished.
- Adding large narrative systems before validation/economy works.
- Using placeholder art that makes documents unreadable.
- Making the rulebook decorative instead of functional.

## Guidance for Future AI Agents

Before editing:

- Read this file and `GDD.md`.
- Inspect the existing Unity project structure if it already exists.
- Follow the current code style once established.
- Do not rewrite architecture without a strong reason.
- Preserve user changes in the working tree.

When implementing:

- Favor small, complete slices that make the game more playable.
- Explain non-obvious Unity concepts briefly in comments or response summaries because the team is learning.
- Prefer Inspector-configurable fields and ScriptableObjects for gameplay data.
- Keep examples concrete and aligned with the university bureaucracy theme.
- Always verify with Unity editor/build steps when available.

When asked for design decisions:

- Recommend the simplest approach that supports the MVP.
- Separate "must-have for MVP" from "nice-to-have after MVP".
- Consider academic evaluation: the game must clearly demonstrate Computer Graphics concepts and a complete interactive loop.

When finishing a task:

- State what changed.
- State how it was verified.
- Mention any remaining risks or manual Unity setup needed.

## Current Assumptions

These assumptions should be revisited once the Unity project is created:

- Unity version is not yet specified. Use a recent LTS version unless the course requires another version.
- Render pipeline is not specified. Use Unity's built-in 2D renderer or URP 2D only if the team is comfortable with it.
- Input system is not specified. The old Input Manager is acceptable for a small MVP; Unity's new Input System is acceptable if already configured.
- Save/load is not required for MVP.
- The final game length can be 3 to 5 days if the loop is polished.
- The first MVP should use 3 request types: enrollment, tuition payment, and class withdrawal.
- Course transfer, certificate issuance, deep survival expenses, NPC object pooling, and modular character assembly are post-MVP unless the core loop is already stable.
- Direct Unifor branding should be treated carefully; a fictional university inspired by the setting is safer unless the team confirms use is acceptable.
