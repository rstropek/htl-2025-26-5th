# Avalonia Introduction

## Overview

This is a comprehensive introductory Avalonia sample application that demonstrates essential cross-platform desktop development concepts. The application showcases modern UI patterns, MVVM architecture, data binding, and various controls through multiple interactive examples.

## What This Sample Demonstrates

### Core Avalonia Concepts
- **Cross-Platform Desktop Development**: Runs on Windows, macOS, and Linux
- **XAML-based UI**: Declarative user interface markup
- **MVVM Architecture**: Model-View-ViewModel pattern implementation
- **Data Binding**: Two-way binding between UI and ViewModels
- **Dependency Injection**: Using Microsoft.Extensions.DependencyInjection
- **ViewLocator Pattern**: Automatic View-ViewModel resolution

### UI Components and Controls
- **Layouts**: DockPanel, Grid, StackPanel arrangements
- **Navigation**: TabControl with multiple views
- **Input Controls**: TextBox, NumericUpDown, ComboBox, CheckBox
- **Data Display**: DataGrid with sorting, filtering, and custom templates
- **Interactive Elements**: Buttons with Commands, ItemsControl templates
- **Styling**: CSS-like selectors, themes, and custom styles

### Advanced Features
- **Message Boxes**: Using third-party MessageBox.Avalonia library
- **Debugging**: Trace and Debug output integration  
- **Commands**: RelayCommand pattern with CanExecute logic
- **Observable Collections**: Dynamic UI updates with ObservableCollection
- **Property Notifications**: Using CommunityToolkit.Mvvm source generators
- **Calculated Properties**: Computed values with change notifications

## Application Structure

### Views (User Interface)
1. **MainWindow** - Main application container with TabControl navigation
2. **MessageBoxAndTracesControl** - Demonstrates dialogs and debug output
3. **CalculatorView** - Simple calculator with data binding and commands
4. **TodoListView** - Task management with dynamic lists and templates
5. **FriendListView** - Data grid with CRUD operations and sorting

### ViewModels (Business Logic)
- **ViewModelBase** - Base class using CommunityToolkit.Mvvm
- **CalculatorViewModel** - Calculator logic with operator selection
- **TodoListViewModel** - Todo item management with filtering
- **FriendListViewModel** - Friend data management with random generation

### Models (Data)
- **TodoItem** - Observable model for task items
- **Friend** - Contact information with calculated properties

### Key Files
- **App.axaml/.cs** - Application startup and dependency injection setup
- **ViewLocator.cs** - Automatic View-ViewModel mapping
- **Program.cs** - Application entry point

## Technologies Used

### Core Frameworks
- **.NET 9.0** - Latest .NET runtime
- **Avalonia 11.3.5** - Cross-platform XAML framework
- **CommunityToolkit.Mvvm 8.2.1** - Modern MVVM helpers and source generators

### UI and Styling
- **Fluent Theme** - Modern Windows 11-inspired design
- **Inter Font** - Clean, readable typography
- **Custom Styles** - Professional button and control styling

### Additional Libraries
- **MessageBox.Avalonia** - Cross-platform dialog boxes
- **Microsoft.Extensions.DependencyInjection** - Built-in dependency injection

## Running the Application

### Prerequisites
- .NET 9.0 SDK installed
- Visual Studio Code or Visual Studio 2022
- C# Dev Kit extension (for VS Code)

### Build and Run
```bash
# Clone and navigate to the project
cd 10-avalonia

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

## Learning Path for Students

### 1. Start with Fundamentals
- Complete the official [Avalonia tutorial](https://docs.avaloniaui.net/docs/get-started/test-drive/introduction)
- Understand MVVM pattern basics
- Learn about data binding concepts

### 2. Explore Views in Recommended Order
1. **MainWindow** - Study the overall structure, DockPanel, Menu, and TabControl
2. **MessageBoxAndTracesControl** - Learn about dialogs and debugging output
3. **CalculatorView** - Understand data binding, commands, and styling
4. **TodoListView** - Explore dynamic lists, templates, and complex binding
5. **FriendListView** - Master DataGrid, CRUD operations, and data management

### 3. Study Key Concepts

#### MVVM Implementation
- Examine how ViewModels inherit from `ViewModelBase`
- Study `[ObservableProperty]` and `[RelayCommand]` source generators
- Understand ViewLocator pattern in `ViewLocator.cs`

#### Data Binding Examples
- Simple two-way binding in CalculatorView
- Collection binding in TodoListView and FriendListView
- Command binding with parameters
- Parent-child ViewModel communication patterns

#### Styling and Theming
- CSS-like selectors and pseudo-classes
- Theme customization with Fluent design
- Consistent styling across controls

### 4. Advanced Topics
- Dependency injection setup in `App.axaml.cs`
- Custom UserControls creation
- DataGrid customization and templates
- Source generators for boilerplate reduction

## Key Learning Resources

### Official Documentation
- [Avalonia Documentation](https://docs.avaloniaui.net/)
- [MVVM Pattern Guide](https://docs.avaloniaui.net/docs/concepts/the-mvvm-pattern/)
- [Data Binding Guide](https://docs.avaloniaui.net/docs/basics/data/data-binding/)

### Controls Reference
- [Grid Layout](https://docs.avaloniaui.net/docs/reference/controls/grid/)
- [DataGrid Control](https://docs.avaloniaui.net/docs/reference/controls/datagrid/)
- [Styling Guide](https://docs.avaloniaui.net/docs/basics/user-interface/styling/styles/)

### Third-Party Libraries
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [MessageBox.Avalonia](https://github.com/AvaloniaCommunity/MessageBox.Avalonia)

## Code Comments and TODOs

The source code contains extensive comments and TODO items that guide you through important concepts:
- Look for `TODO:` comments with learning resources
- Study the XML documentation in XAML files
- Follow links to official documentation for deeper understanding
