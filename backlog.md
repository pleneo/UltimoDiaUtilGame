# Backlog

Project: **Ultimos Dias Uteis**

This document tracks the initial implementation backlog for the MVP. It should stay short, practical, and focused on delivering a complete playable loop as early as possible.

## MVP Goal

Build a small but fully playable 2D bureaucratic puzzle game inspired by *Papers, Please*, set in a university administration office.

The first playable version should:

- Start from a simple menu with a `Play` button.
- Load directly into Day 1.
- Support three core days.
- Be understandable to an inexperienced development team.
- Prioritize functionality over visual polish.

## Priority 0 - Project Setup

- Create the Unity project using version `6000.4.1f1`.
- Configure Git for the Unity project.
- Add a correct Unity `.gitignore`.
- Create the base folder structure for scripts, scenes, prefabs, art, audio, and ScriptableObjects.
- Create the `AGENTS.md` file as the permanent AI working context.
- Create the `backlog.md` file as the living implementation plan.

## Priority 1 - Core Playable Loop

- Create the main menu scene.
- Add a simple `Play` button.
- Load the gameplay scene from the menu.
- Create the desk gameplay scene.
- Create the basic day loop system.
- Make Day 1 start automatically from the gameplay scene if needed.
- Add day start and day end transitions.
- Add a simple work timer for the day.
- Add a day summary screen.

## Priority 2 - Day 1: Enrollment

Day 1 focuses only on enrollment.

Rules:

- Required documents are `Comprovante de Matrícula`, `Histórico Escolar`, and `Carteira de Identidade`.
- Some cases may have missing documents.
- Some cases may have a name mismatch between documents.
- The player must approve, reject, or forward the case based on the rule set.

Tasks:

- Define the Day 1 rule data.
- Create at least one enrollment case that is valid.
- Create at least one case with a missing document.
- Create at least one case with a name mismatch.
- Implement the validation logic for document presence.
- Implement the validation logic for name comparison.
- Show feedback for correct and incorrect decisions.

## Priority 3 - Day 2: Enrollment Payment

Day 2 adds payment for enrollment.

Rules:

- The student may pay with cash or card.
- Cash can be fake.
- Card can fail or have no available limit.
- The player must verify whether the payment method is valid for the case.

Tasks:

- Add payment document/data to Day 2 cases.
- Create cash payment cases.
- Create card payment cases.
- Implement fake cash detection.
- Implement card failure or no-limit validation.
- Add at least one case where payment is correct.
- Add at least one case where payment is invalid.

## Priority 4 - Day 3: Class Withdrawal

Day 3 introduces class withdrawal bureaucracy.

Rules:

- The player validates trancamento de matricula.
- The case may require extra documents or specific conditions.
- The day should increase complexity without becoming a brand-new system.

Tasks:

- Define the Day 3 rule data.
- Add withdrawal-specific documents or requirements.
- Add at least one withdrawal case with valid documents.
- Add at least one withdrawal case with invalid or missing requirements.
- Add the decision handling for this new request type.

## Priority 5 - Shared Systems

- Create a document model that can be reused across all days.
- Create a case model that stores request type, required documents, and expected outcome.
- Create a rulebook UI panel.
- Create a notice board UI panel.
- Add a simple queue of NPCs/cases.
- Add drag-and-drop for documents.
- Add z-order handling so the selected document appears on top.
- Add basic hover or selection feedback.
- Add money, debt, and warning tracking.
- Add end-of-day money calculation.
- Add game over conditions for too many warnings or negative money.

## Priority 6 - Presentation and Feedback

- Add simple placeholder art for desk, documents, and NPCs.
- Add a basic pixel-art-inspired visual style.
- Add SFX for stamp, paper movement, click, success, and error.
- Add simple gibberish voice or placeholder vocal feedback.
- Add readable UI colors and text.
- Add a visible time, money, and debt display.

## Priority 7 - Technical Concepts for Class Presentation

- Implement translation through document dragging.
- Implement z-order change when selecting a document.
- Implement point-in-polygon in one specific mechanic, if needed for academic demonstration.
- Keep queue handling explicit and easy to explain.

## Post-MVP

These items should only be attempted if the core game is already working and stable:

- Course transfer request type.
- Graduation certificate request type.
- More than one moral dilemma case.
- High contrast accessibility mode.
- Separate music and SFX volume sliders.
- Modular NPC sprite assembly.
- Object pooling for NPCs.
- More advanced art polish.
- Save/load.
- More detailed survival economy.

## Delivery Rule

If a task does not help make the game more playable, understandable, or demonstrable for the class, it should be postponed until after the MVP loop is complete.
