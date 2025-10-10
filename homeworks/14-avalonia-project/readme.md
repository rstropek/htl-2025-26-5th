# Mini Project: Learning App with C#, .NET, and Avalonia

## Overview

**Design and implement a small learning application.** Examples include a vocabulary trainer, a basic math practice tool for elementary learners, or a flashcard-style quiz.

Your app should be intentionally compact yet complete: a focused feature set, a pleasant UI, and cleanly structured code that demonstrates your understanding of Avalonia and .NET.

## Goal

Show that you can take an idea from concept to a working desktop application. You will write a short specification, a concise technical design document, and a well-structured implementation that uses data binding correctly, persists data with SQLite, and presents a consistent visual style.

## Technologies and scope

Use C# and .NET with Avalonia for the UI.

Persist real application data in a SQLite database that you create and access from your code. 

Present at least one navigable list of items (for example, an `ItemsControl`, `ListBox`, or `DataGrid`) and at least one form that accepts user input and writes to the database. Apply Avalonia data binding in a way that keeps view and state clearly separated.

Style the app so it feels cohesive and pleasant to use (leveraging AI tools for design ideas or assets is permitted as long as you credit and curate the outcome).

## Functional guidelines

Keep the scope small but meaningful. A minimal end-to-end slice could allow users to add items through a form, see them in a list view, edit or delete entries, and track simple progress or statistics.

Prefer MVVM or a comparable pattern with view models that expose bindable properties and commands.

## Data binding quality

Use bindings rather than manual UI updates. Commands should drive behavior; properties should raise change notifications. Keep code-behind minimal and free of business logic.

## Individual work and ethics

This is an individual assignment. Do not collaborate or share code. You may use AI to brainstorm, to generate boilerplate, or to refine copy and styles, but you must critically review and integrate the results and you remain fully responsible for the outcome. Cite nontrivial external sources and generated assets in your design document.

## Timeline

Submit everything by **October 24** (end of day, local time). Manage your time so that specification and design are completed early enough to guide implementation.

## What to hand in

Provide the following artifacts in a single GitHub monorepo:

- A brief **specification** that states the problem your app solves, the target user, and the core workflow. Keep it to one to two pages.
- A **technical design document** that explains the architecture (e.g., MVVM), key classes, data model and schema, binding strategy, and any notable trade-offs. Include a short section on styling decisions (particularly used layout panels).
- The **source code** of the Avalonia app.
- Filled SQLite database file (if applicable) or code that creates and seeds the database on first run.

## How to submit

Push your repository to GitHub and provide the URL. The repository must build and run with clear instructions (README at the root). Include a short demo GIF (animated) or screenshots in the README to illustrate the main flow.
