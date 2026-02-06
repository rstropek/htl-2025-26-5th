# Create An Exercise

## Introduction

In preparation for the upcoming final exam, we have been practicing ASP.NET Core, Entity Framework Core, Angular, and Aspire. We started with exercises that I created. You got nearly empty starter code and you had to fill in the blanks. The last exam had a twist: You received a ready-made project and you had to change it based on updated requirements.

Today’s exercise has another twist: **You will create an exercise for your classmates**. The goal is that you think not only about the implementation, but also about the requirements, the structure of the starter code, and the testing of an exercise.

## Requirements

Invent an exercise based on the [existing starter code for our three-tier architecture](https://github.com/rstropek/htl-2025-26-5th/tree/main/20-fullstack-starter) (SQLite, EF Core, ASP.NET Core Web API, Angular). You are free to choose the domain of the exercise. The only requirement is that your exercise must include the following functional requirements:

* One piece of **non-trivial business logic**.
  * The location of the business logic (e.g. something in the Web API, logic in the UI, a validation check during data import, etc.) is up to you.
* Create an **exercise-specific starter code** with a ready-made data model and data context.
  * It must **not** be the task of the students to create the data model and data context. You must provide them.
  * Your starter code must also contain automated tests that verify that the data context works and can be used to perform CRUD operations on the database.
* Some **non-trivial data import**.
  * Your import must require some logic.
  * A simple import of a CSV or JSON file is **not** sufficient.
  * The logic might be related to parsing a custom data format, validating data, processing all files in a folder, error handling, etc.
  * Your exercise-specific starter code must contain some basic code structure for the import that the students can extend.
* Extend or create a **Web API with ASP.NET Core Minimal API**.
  * The exercise-specific starter code must contain some basic code structure for the Web API that the students can extend.
* Extend or create an **Angular UI that consumes the Web API**.
  * As usual, the UI must consume the auto-generated API client (via the Swagger file).
  * The UI must contain at least one non-trivial piece of logic (e.g. filtering a list, a form with non-trivial validation, etc.).
  * The exercise-specific starter code must contain some basic code structure for the Angular UI that the students can extend.
* Extend or create **automated tests**.
  * The tests can cover business logic, data access, or the Web API.
  * Require C# unit tests, not Angular tests.
  * If students should extend/modify existing tests, you must provide some basic code structure for the tests in the starter code.

You have to **document the requirements** of the exercise in a markdown file. The structure and level of detail of the requirement specification must be similar to the exercises you typically get from your teacher.

### What counts as "non-trivial business logic"?

As a rule of thumb, the non-trivial logic should meet at least one of the following criteria:

* It requires a **calculation** with multiple steps and edge cases (e.g. Austrian per diem calculation with different rules depending on duration, etc.).
* It involves **deriving values** from multiple inputs (e.g. calculating the approximate distance between two GPS coordinates using Pythagoras).
* It includes **validation rules** that go beyond basic field validation (e.g. checking for conflicting time ranges, enforcing business constraints, detecting duplicates).
* It requires **processing multiple records** (e.g. aggregating values, applying rules across a set of imported data, computing statistics).
* It has meaningful **error handling and decision logic** (e.g. partial imports, warnings vs errors, retry logic, conflict resolution).

The goal is that your classmates have to implement logic where they need to think, not just write a simple sequence of statements or a single `if` statement.

## Code Review Prompt

Besides the exercise specification and the starter code, you also have to **create a code review prompt**. The code review prompt must be a markdown file that contains instructions for a frontier AI model like Claude Sonnet or GPT 5.x to perform a code review of a student solution.

The prompt must contain:

* A description so that the AI understands what was given (starter code, requirements) and what the student had to do.
* Quality criteria for the code review. Ask yourself: What should the AI consider good code? What should it consider bad code? What are the most important things to check in the review?
* The structure of the code review document that the AI should produce.

## Deliverables

* A markdown file documenting the requirements of the exercise.
* A ready-made starter code for the exercise.
* A markdown file containing the code review prompt for a frontier AI model.
* You do **not** need to provide a sample solution for the exercise.
* Provide all deliverables in a GitHub repository that your fellow students can access/fork.
  * You can choose whether the repository should be public or private. If you choose private, make sure to give your fellow students and your teacher access to it.

## Use of AI

In practice, we cannot prevent you from generating the entire exercise using AI. However, if you do that, the whole exercise becomes pointless: there will be little to no learning effect for you or your fellow students.

You can and you should use AI to **support** you during the creation of the exercise. Examples:

* **You** invent the core idea of the exercise, create a list of keywords, and let the AI generate parts of the specification text based on your input.
* **You** create the core structure of the starter code and let the AI fill in some of the details.
* **You** create the core structure of the code review prompt and let the AI fill in some of the details.
* **You** stay in the "driver seat" and use AI mainly to get feedback on your ideas and implementation.

## Exercise Mode

You can choose to do this exercise in **groups of two or three students**.

**Until Feb. 13th, 2026:** Each student must design and create an exercise as described above.

**Until Feb. 24th, 2026:** Once done, each student must work out a solution for an exercise created by the other student(s) in the group. If your group consists of three students, you can choose which exercise you want to solve. However, ensure that each student solves a different exercise. You must send a **pull request** with your solution to the GitHub repository of the exercise you solved.

**Until Feb. 27th, 2026:** The author of the exercise must create a code review document for the solution created by the other student(s) in the group. You must create the code review document with the help of a frontier AI model based on the code review prompt you created. Of course, you must check the AI's output and rework it if necessary. Check in the code review document in the GitHub repository of the exercise.

Discuss the code review with the other student(s) in your group. You can always ask your teacher for help or feedback.

**On Feb. 27th, 2026:** Everyone must be prepared to present the exercise (the creator of the exercise presents, not the one who solved it). Your teacher will randomly select presenters. Your presentation should be max. 8 minutes long and include:

* A brief introduction to the exercise and the requirements.
  * Describe **why** you added certain requirements and **what learning effect** you wanted to achieve with them.
* A brief introduction into the code review prompt.
* Your learnings from the exercise creation, the solution, and the code review.
  * What was difficult? What was easy?
  * What would you do differently next time?
  * Where did you use AI? How did it help you? What were the limitations of AI in this exercise?
