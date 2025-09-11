# Avalonia Homework: Pocket Calculator

## Overview

Create a reactive pocket calculator using Avalonia UI with the MVVM pattern. This homework will help you practice the Avalonia concepts you've learned, including Grid layouts, data binding, commands, reactive UI updates, and error handling with message boxes.

## Requirements

### Core Functionality
Your calculator should:
1. Display digits 0-9 using individual buttons
2. Support basic arithmetic operations (+, -, *, /)
3. Include an "=" button to perform calculations
4. Show the current input and result in a display area
5. Process operations sequentially without operator precedence (left-to-right evaluation)
6. Update the display reactively as users interact with buttons

### User Interface Design

Create a calculator interface using a **Grid layout** with the following structure:

```
+------------------+
| [ Display Area ] |
+------------------+
| [7] [8] [9] [/]  |
+------------------+
| [4] [5] [6] [*]  |
+------------------+
| [1] [2] [3] [-]  |
+------------------+
| [0] [C] [+] [=]  |
+------------------+
```

### Technical Requirements

#### MVVM Architecture

Your application must follow the MVVM pattern:
- **Model**: Create a simple calculator logic class
- **ViewModel**: Implement `CalculatorViewModel` inheriting from `ViewModelBase`
- **View**: XAML-based UI with data binding

#### ViewModel Implementation

Your `CalculatorViewModel` must include:

- `[ObservableProperty]` for the display text
- `[RelayCommand]` methods for digit input (0-9)
- `[RelayCommand]` methods for operators (+, -, *, /)
- `[RelayCommand]` for equals operation
- `[RelayCommand]` for clear operation
- Properties that automatically notify the UI of Challenges

#### XAML Structure

Your calculator view must use:

- **Grid** layout with appropriate rows and columns
- Data binding for the display
- Command binding for all buttons
- Consistent styling for buttons and display

#### Data Binding Requirements

- Display area must be bound to a ViewModel property
- All buttons must use command binding (no event handlers in code-behind)
- UI must update automatically when ViewModel properties change

## Error Handling Requirements

### Division by Zero

- Detect division by zero operations
- Display "Error: Cannot divide by zero!" using `MessageBox.Avalonia`
- Reset calculator state after error acknowledgment

## Bonus Challenges (Optional)

If you finish early, try implementing memory functions:

1. **Memory Store (MS)**: Add a button to save the currently displayed value
2. **Memory Recall (MR)**: Add a button to retrieve the stored value
3. **Memory Clear (MC)**: Add a button to clear the memory slot
