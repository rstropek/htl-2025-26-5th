# Angular Recipes - Learning Project

This project demonstrates modern Angular patterns and best practices through a customer management application. It showcases the latest Angular features including signals, standalone components, and the new forms API introduced in Angular 21.

## 🎯 Learning Objectives

This project serves as a practical reference for important Angular concepts. Each section below covers a key Angular topic with pointers to where it's implemented in the codebase.

## 📚 Angular Topics Covered

### 1. **Standalone Components**
**Location**: `Frontend/src/app/customer-list/customer-list.ts`

Modern Angular applications use standalone components, eliminating the need for `NgModule`. Components declare their dependencies directly in the `imports` array.

```typescript
@Component({
  selector: 'app-customer-list',
  imports: [DatePipe, RouterLink, CurrencyPipe],
  templateUrl: './customer-list.html',
  styleUrl: './customer-list.css',
})
```

### 2. **Dependency Injection with `inject()`**
**Location**: `Frontend/src/app/customer-list/customer-list.ts`

The `inject()` function provides a modern, functional approach to dependency injection without constructor parameters.

```typescript
private readonly http = inject(HttpClient);
private readonly apiConfig = inject(ApiConfiguration);
```

### 3. **Signals**
**Location**: `Frontend/src/app/customer-list/customer-list.ts`

Signals are Angular's new reactive primitive for state management, providing fine-grained reactivity.

#### Writable Signals
```typescript
protected readonly customers = signal<Customer[]>([]);
protected readonly loading = signal(true);
protected readonly selectedCustomerIds = signal<number[]>([]);
```

#### Computed Signals
Computed signals automatically recalculate when their dependencies change:
```typescript
protected readonly selectedCount = computed(() => this.selectedCustomerIds().length);
protected readonly hasSelection = computed(() => this.selectedCount() > 0);
```

#### Signal Updates (Immutable Pattern)
```typescript
// Using update() for immutable state changes
this.customers.update(currentCustomers => 
  currentCustomers.filter(c => c.id !== id)
);
```

### 4. **Signal-Based Forms (Angular 21+)**
**Location**: `Frontend/src/app/customer-edit/customer-edit.ts`

Brand new in Angular 21, signal-based forms provide a modern, reactive approach to form handling.

```typescript
protected readonly customerModel = signal<CustomerFormModel>({
  name: '',
  dateOfBirth: '',
  revenue: 0,
  customerValue: 0,
  isActive: true,
});

protected readonly customerForm = form(this.customerModel, (schemaPath) => {
  required(schemaPath.name, { message: 'Name is required' });
  maxLength(schemaPath.name, 50, { message: 'Name must be at most 50 characters' });
  min(schemaPath.revenue, 0, { message: 'Revenue must be at least 0' });
  max(schemaPath.customerValue, 10, { message: 'Customer value must be at most 10' });
});
```

### 5. **Component Inputs**
**Location**: `Frontend/src/app/customer-edit/customer-edit.ts`

Modern Angular inputs with type safety and required validation:

```typescript
id = input.required<number>();
```

Route configuration to pass data:
```typescript
{ path: 'customers/:id/edit', component: CustomerEdit }
```

### 6. **Template Syntax**

#### Conditional Rendering (`@if`)
**Location**: `Frontend/src/app/customer-list/customer-list.html`

```html
@if (loading()) {
  <div class="loading">
    <p>Loading customers...</p>
  </div>
}

@if (customers().length === 0) {
  <div class="empty">
    <p>No customers available.</p>
  </div>
} @else {
  <!-- Customer list -->
}
```

#### Loops (`@for` with `track`)
**Location**: `Frontend/src/app/customer-list/customer-list.html`

The `track` function is critical for performance, helping Angular identify which items have changed:

```html
@for (customer of customers(); track customer.id) {
  <div class="grid-cell">{{ customer.name }}</div>
}
```

#### Event Binding
**Location**: `Frontend/src/app/customer-list/customer-list.html`

```html
<button (click)="deleteCustomer(customer.id)" class="delete-btn">Delete</button>
<input (change)="toggleCustomerSelection(customer.id!)" />
```

#### Property Binding & Conditional Classes
**Location**: `Frontend/src/app/customer-list/customer-list.html`

```html
<span [class.active]="customer.isActive" [class.inactive]="!customer.isActive">
  {{ customer.isActive ? 'Active' : 'Inactive' }}
</span>
```

### 7. **Pipes**
**Location**: `Frontend/src/app/customer-list/customer-list.html`

Angular pipes transform data in templates:

```html
<!-- Date formatting -->
{{ customer.dateOfBirth | date:'dd.MM.yyyy' }}

<!-- Currency formatting -->
{{ customer.revenue | currency:'EUR' }}
```

### 8. **Router**

#### Router Links
**Location**: `Frontend/src/app/customer-list/customer-list.html`

```html
<a [routerLink]="['/customers', customer.id, 'edit']" class="edit-link">Edit</a>
```

#### Programmatic Navigation
**Location**: `Frontend/src/app/customer-edit/customer-edit.ts`

```typescript
private readonly router = inject(Router);

// Navigate after successful save
this.router.navigate(['/customers']);
```

#### Route Configuration
**Location**: `Frontend/src/app/app.routes.ts`

```typescript
export const routes: Routes = [
  { path: '', redirectTo: '/customers', pathMatch: 'full' },
  { path: 'customers', component: CustomerList },
  { path: 'customers/:id/edit', component: CustomerEdit }
];
```

### 9. **RxJS Integration**
**Location**: `Frontend/src/app/customer-list/customer-list.ts`

Converting Observables to Promises with `firstValueFrom`:

```typescript
import { firstValueFrom } from 'rxjs';

const response = await firstValueFrom(
  customersGet(this.http, this.apiConfig.rootUrl)
);
```

### 10. **HTTP Client & API Integration**
**Location**: `Frontend/src/app/customer-list/customer-list.ts`, `Frontend/src/app/customer-edit/customer-edit.ts`

The project uses auto-generated API clients from OpenAPI specifications:

```typescript
private readonly http = inject(HttpClient);
private readonly apiConfig = inject(ApiConfiguration);

// GET request
const response = await firstValueFrom(
  customersIdGet(this.http, this.apiConfig.rootUrl, { id: this.id() })
);

// PATCH request
await firstValueFrom(
  customersIdPatch(this.http, this.apiConfig.rootUrl, {
    id: this.id(),
    body: patchDto,
  })
);
```

## 🏗️ Project Structure

```
Frontend/
├── src/
│   ├── app/
│   │   ├── customer-list/        # List view with signals, pipes, loops
│   │   ├── customer-edit/        # Edit form with signal-based forms
│   │   ├── api/                  # Auto-generated API client
│   │   ├── app.routes.ts         # Route configuration
│   │   └── app.config.ts         # Application configuration
│   ├── environments/             # Environment configurations
│   └── styles.css                # Global styles with CSS variables
```

## 🔧 Additional Topics

### CSS Features
The project also demonstrates modern CSS techniques:

- **CSS Variables**: `Frontend/src/styles.css`
- **CSS Grid**: `Frontend/src/app/customer-list/customer-list.css`
- **CSS Flexbox**: `Frontend/src/app/customer-list/customer-list.css`

### API Code Generation
The project uses OpenAPI/Swagger to auto-generate TypeScript API clients:

```bash
npm run generate-web-api
```

This generates type-safe API functions in `src/app/api/` based on the `WebApi.json` specification.
