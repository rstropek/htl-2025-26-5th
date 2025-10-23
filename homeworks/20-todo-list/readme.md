# Todo List

## Introduction

With this homework, you will get familiar with the starter/demonstration code in [20-fullstack-starter](../20-fullstack-starter). You will implement a simple Todo List application using a full-stack approach including .NET Aspire, Entity Framework Core, ASP.NET Core Minimal API, xUnit, NSubstitute, and Angular.

## Requirements

### Todo List

Your application must maintain a todo list. Each todo item must have the following properties:

* Id (internal, not relevant for the user)
* Title (string, required)
* Assignee (string, required)
* IsCompleted (boolean, default: false)

### Features

**The implementation of all features must follow the principles demonstrated in the starter code.**

* Importer that can import files like [TodoList.txt](./TodoList.txt) and populate the database with todo items.
* User has a list of todo items displayed in the Angular frontend.
* User can add new todo items.
* User can mark todo items as completed.
* User can filter the list of todo items by assignee and/or completion status.
* User can delete todo items.
* User can update the title and assignee of existing todo items.

### Quality Requirements

Implement unit and integration tests as demonstrated in the starter code.

### Import Files

The application must be able to import todo items from text files with the following structure:

```
Assignee: [Name]
Todos:
* [Todo item title]
* [Another todo item title]
---
Assignee: [Another Name]
Todos:
* [Todo item title]
* [Another todo item title]
```

**File Format Details:**
- Each assignee section starts with `Assignee: [Name]`
- Followed by `Todos:` on the next line
- Todo items are listed with `* ` prefix (asterisk followed by space)
- Assignee sections are separated by `---` (three dashes)
- All imported todo items default to `IsCompleted = false`
