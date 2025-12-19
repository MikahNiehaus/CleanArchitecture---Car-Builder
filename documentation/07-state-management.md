# State Management

## Overview

State management is one of the most critical aspects of React applications. This guide covers various state management solutions, when to use each, and best practices for managing both local and global state in React applications.

## The State Management Hierarchy (2025)

```
Local State (useState, useReducer)
         ↓
    Context API
         ↓
    Zustand/Jotai (Lightweight)
         ↓
Redux Toolkit (Enterprise)
         ↓
React Query/TanStack Query (Server State)
```

**Golden Rule:** Start with the simplest approach that works, then graduate to more complex solutions as needed.

## Types of State

### 1. Local Component State
State used by a single component.

```tsx
function CarFilter() {
    const [minPrice, setMinPrice] = useState(0);
    const [maxPrice, setMaxPrice] = useState(100000);

    return (
        <div>
            <input value={minPrice} onChange={e => setMinPrice(Number(e.target.value))} />
            <input value={maxPrice} onChange={e => setMaxPrice(Number(e.target.value))} />
        </div>
    );
}
```

### 2. Lifted State
State shared between components via props.

```tsx
function CarDashboard() {
    const [selectedCar, setSelectedCar] = useState<Car | null>(null);

    return (
        <div>
            <CarList onSelectCar={setSelectedCar} />
            <CarDetails car={selectedCar} />
        </div>
    );
}
```

### 3. Global/Shared State
State needed across many components.

### 4. Server State
Data from backend APIs (use React Query).

### 5. URL State
State stored in URL parameters.

```tsx
import { useSearchParams } from 'react-router-dom';

function CarList() {
    const [searchParams, setSearchParams] = useSearchParams();
    const page = Number(searchParams.get('page') || '1');

    const handlePageChange = (newPage: number) => {
        setSearchParams({ page: newPage.toString() });
    };

    return (/* ... */);
}
```

## Context API

**Best for:** Avoiding prop drilling, theme, auth state, small-medium apps.

**Not for:** Frequent updates (causes re-renders), complex state logic.

### Basic Context

```tsx
// src/contexts/ThemeContext.tsx
import { createContext, useContext, useState, ReactNode } from 'react';

type Theme = 'light' | 'dark';

interface ThemeContextType {
    theme: Theme;
    toggleTheme: () => void;
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export function ThemeProvider({ children }: { children: ReactNode }) {
    const [theme, setTheme] = useState<Theme>('light');

    const toggleTheme = () => {
        setTheme(prev => prev === 'light' ? 'dark' : 'light');
    };

    return (
        <ThemeContext.Provider value={{ theme, toggleTheme }}>
            {children}
        </ThemeContext.Provider>
    );
}

export function useTheme() {
    const context = useContext(ThemeContext);
    if (!context) {
        throw new Error('useTheme must be used within ThemeProvider');
    }
    return context;
}

// Usage
function App() {
    return (
        <ThemeProvider>
            <Header />
            <Main />
        </ThemeProvider>
    );
}

function Header() {
    const { theme, toggleTheme } = useTheme();

    return (
        <header className={theme}>
            <button onClick={toggleTheme}>Toggle Theme</button>
        </header>
    );
}
```

### Complex Context with useReducer

```tsx
// src/contexts/CartContext.tsx
import { createContext, useContext, useReducer, ReactNode } from 'react';

interface CartItem {
    carId: string;
    quantity: number;
    price: number;
}

interface CartState {
    items: CartItem[];
    total: number;
}

type CartAction =
    | { type: 'ADD_ITEM'; payload: CartItem }
    | { type: 'REMOVE_ITEM'; payload: string }
    | { type: 'UPDATE_QUANTITY'; payload: { carId: string; quantity: number } }
    | { type: 'CLEAR_CART' };

interface CartContextType {
    state: CartState;
    addItem: (item: CartItem) => void;
    removeItem: (carId: string) => void;
    updateQuantity: (carId: string, quantity: number) => void;
    clearCart: () => void;
}

const CartContext = createContext<CartContextType | undefined>(undefined);

function cartReducer(state: CartState, action: CartAction): CartState {
    switch (action.type) {
        case 'ADD_ITEM': {
            const existingIndex = state.items.findIndex(
                item => item.carId === action.payload.carId
            );

            if (existingIndex > -1) {
                const newItems = [...state.items];
                newItems[existingIndex].quantity += action.payload.quantity;
                return {
                    items: newItems,
                    total: calculateTotal(newItems)
                };
            }

            const newItems = [...state.items, action.payload];
            return {
                items: newItems,
                total: calculateTotal(newItems)
            };
        }

        case 'REMOVE_ITEM': {
            const newItems = state.items.filter(item => item.carId !== action.payload);
            return {
                items: newItems,
                total: calculateTotal(newItems)
            };
        }

        case 'UPDATE_QUANTITY': {
            const newItems = state.items.map(item =>
                item.carId === action.payload.carId
                    ? { ...item, quantity: action.payload.quantity }
                    : item
            );
            return {
                items: newItems,
                total: calculateTotal(newItems)
            };
        }

        case 'CLEAR_CART':
            return { items: [], total: 0 };

        default:
            return state;
    }
}

function calculateTotal(items: CartItem[]): number {
    return items.reduce((sum, item) => sum + item.price * item.quantity, 0);
}

export function CartProvider({ children }: { children: ReactNode }) {
    const [state, dispatch] = useReducer(cartReducer, { items: [], total: 0 });

    const addItem = (item: CartItem) => dispatch({ type: 'ADD_ITEM', payload: item });
    const removeItem = (carId: string) => dispatch({ type: 'REMOVE_ITEM', payload: carId });
    const updateQuantity = (carId: string, quantity: number) =>
        dispatch({ type: 'UPDATE_QUANTITY', payload: { carId, quantity } });
    const clearCart = () => dispatch({ type: 'CLEAR_CART' });

    return (
        <CartContext.Provider value={{ state, addItem, removeItem, updateQuantity, clearCart }}>
            {children}
        </CartContext.Provider>
    );
}

export function useCart() {
    const context = useContext(CartContext);
    if (!context) {
        throw new Error('useCart must be used within CartProvider');
    }
    return context;
}
```

### Optimizing Context Performance

```tsx
// Split contexts to prevent unnecessary re-renders
const AuthStateContext = createContext<AuthState | undefined>(undefined);
const AuthActionsContext = createContext<AuthActions | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [state, setState] = useState<AuthState>(initialState);

    // Actions don't change, won't cause re-renders
    const actions = useMemo(
        () => ({
            login: (user: User) => setState({ user, isAuthenticated: true }),
            logout: () => setState({ user: null, isAuthenticated: false })
        }),
        []
    );

    return (
        <AuthStateContext.Provider value={state}>
            <AuthActionsContext.Provider value={actions}>
                {children}
            </AuthActionsContext.Provider>
        </AuthStateContext.Provider>
    );
}

// Separate hooks for state and actions
export function useAuthState() {
    const context = useContext(AuthStateContext);
    if (!context) throw new Error('useAuthState must be used within AuthProvider');
    return context;
}

export function useAuthActions() {
    const context = useContext(AuthActionsContext);
    if (!context) throw new Error('useAuthActions must be used within AuthProvider');
    return context;
}
```

## Zustand (Recommended for Most Apps)

**Best for:** Medium-sized apps, simple API, excellent performance, minimal boilerplate.

### Installation

```bash
npm install zustand
```

### Basic Store

```tsx
// src/stores/carStore.ts
import { create } from 'zustand';

interface Car {
    id: string;
    make: string;
    model: string;
    price: number;
}

interface CarStore {
    cars: Car[];
    selectedCar: Car | null;
    loading: boolean;
    error: string | null;

    // Actions
    setCars: (cars: Car[]) => void;
    selectCar: (car: Car | null) => void;
    addCar: (car: Car) => void;
    removeCar: (id: string) => void;
    fetchCars: () => Promise<void>;
}

export const useCarStore = create<CarStore>((set, get) => ({
    cars: [],
    selectedCar: null,
    loading: false,
    error: null,

    setCars: (cars) => set({ cars }),

    selectCar: (car) => set({ selectedCar: car }),

    addCar: (car) => set((state) => ({
        cars: [...state.cars, car]
    })),

    removeCar: (id) => set((state) => ({
        cars: state.cars.filter(car => car.id !== id)
    })),

    fetchCars: async () => {
        set({ loading: true, error: null });
        try {
            const response = await fetch('/api/cars');
            const cars = await response.json();
            set({ cars, loading: false });
        } catch (error) {
            set({ error: error.message, loading: false });
        }
    }
}));

// Usage in component
function CarList() {
    const { cars, loading, fetchCars } = useCarStore();

    useEffect(() => {
        fetchCars();
    }, [fetchCars]);

    if (loading) return <Spinner />;

    return (
        <div>
            {cars.map(car => <CarCard key={car.id} car={car} />)}
        </div>
    );
}

// Select only needed state to prevent unnecessary re-renders
function CarCount() {
    const carCount = useCarStore(state => state.cars.length);
    return <div>Total Cars: {carCount}</div>;
}
```

### Zustand with Immer (for Complex Updates)

```tsx
import { create } from 'zustand';
import { immer } from 'zustand/middleware/immer';

interface TodoStore {
    todos: Todo[];
    addTodo: (text: string) => void;
    toggleTodo: (id: string) => void;
}

export const useTodoStore = create<TodoStore>()(
    immer((set) => ({
        todos: [],

        addTodo: (text) => set((state) => {
            state.todos.push({ id: crypto.randomUUID(), text, completed: false });
        }),

        toggleTodo: (id) => set((state) => {
            const todo = state.todos.find(t => t.id === id);
            if (todo) {
                todo.completed = !todo.completed;
            }
        })
    }))
);
```

### Zustand with Persistence

```tsx
import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export const useSettingsStore = create(
    persist<SettingsStore>(
        (set) => ({
            theme: 'light',
            language: 'en',
            setTheme: (theme) => set({ theme }),
            setLanguage: (language) => set({ language })
        }),
        {
            name: 'app-settings', // localStorage key
            partialize: (state) => ({ theme: state.theme }) // Only persist theme
        }
    )
);
```

## Redux Toolkit (Enterprise Applications)

**Best for:** Large teams, complex state, time-travel debugging, extensive middleware needs.

### Installation

```bash
npm install @reduxjs/toolkit react-redux
```

### Store Setup

```tsx
// src/store/index.ts
import { configureStore } from '@reduxjs/toolkit';
import carsReducer from './slices/carsSlice';
import authReducer from './slices/authSlice';

export const store = configureStore({
    reducer: {
        cars: carsReducer,
        auth: authReducer
    },
    middleware: (getDefaultMiddleware) =>
        getDefaultMiddleware({
            serializableCheck: {
                ignoredActions: ['cars/fetchCars/fulfilled']
            }
        })
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

// src/store/hooks.ts
import { TypedUseSelectorHook, useDispatch, useSelector } from 'react-redux';
import type { RootState, AppDispatch } from './index';

export const useAppDispatch = () => useDispatch<AppDispatch>();
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;
```

### Slice with Async Thunks

```tsx
// src/store/slices/carsSlice.ts
import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { carsApi } from '@/features/cars/api/carsApi';

interface CarsState {
    items: Car[];
    selectedCar: Car | null;
    loading: boolean;
    error: string | null;
}

const initialState: CarsState = {
    items: [],
    selectedCar: null,
    loading: false,
    error: null
};

// Async thunk
export const fetchCars = createAsyncThunk(
    'cars/fetchCars',
    async (params: GetCarsParams, { rejectWithValue }) => {
        try {
            const result = await carsApi.getCars(params);
            return result.items;
        } catch (error) {
            return rejectWithValue(error.message);
        }
    }
);

export const createCar = createAsyncThunk(
    'cars/createCar',
    async (car: Omit<Car, 'id'>, { rejectWithValue }) => {
        try {
            const result = await carsApi.createCar(car);
            return result;
        } catch (error) {
            return rejectWithValue(error.message);
        }
    }
);

const carsSlice = createSlice({
    name: 'cars',
    initialState,
    reducers: {
        selectCar: (state, action: PayloadAction<Car | null>) => {
            state.selectedCar = action.payload;
        },
        clearError: (state) => {
            state.error = null;
        }
    },
    extraReducers: (builder) => {
        builder
            // Fetch cars
            .addCase(fetchCars.pending, (state) => {
                state.loading = true;
                state.error = null;
            })
            .addCase(fetchCars.fulfilled, (state, action) => {
                state.loading = false;
                state.items = action.payload;
            })
            .addCase(fetchCars.rejected, (state, action) => {
                state.loading = false;
                state.error = action.payload as string;
            })
            // Create car
            .addCase(createCar.fulfilled, (state, action) => {
                state.items.push(action.payload);
            });
    }
});

export const { selectCar, clearError } = carsSlice.actions;
export default carsSlice.reducer;

// Usage
function CarList() {
    const dispatch = useAppDispatch();
    const { items: cars, loading, error } = useAppSelector(state => state.cars);

    useEffect(() => {
        dispatch(fetchCars({ page: 1, pageSize: 10 }));
    }, [dispatch]);

    if (loading) return <Spinner />;
    if (error) return <Error message={error} />;

    return (
        <div>
            {cars.map(car => <CarCard key={car.id} car={car} />)}
        </div>
    );
}
```

## React Query (Server State Management)

**Best for:** API data fetching, caching, synchronization.

```tsx
// src/features/cars/hooks/useCars.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { carsApi } from '../api/carsApi';

export function useCars(params?: GetCarsParams) {
    return useQuery({
        queryKey: ['cars', params],
        queryFn: () => carsApi.getCars(params),
        staleTime: 5 * 60 * 1000, // 5 minutes
        gcTime: 10 * 60 * 1000, // 10 minutes (was cacheTime)
        retry: 3
    });
}

export function useCreateCar() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: carsApi.createCar,
        onMutate: async (newCar) => {
            // Optimistic update
            await queryClient.cancelQueries({ queryKey: ['cars'] });

            const previousCars = queryClient.getQueryData(['cars']);

            queryClient.setQueryData(['cars'], (old: any) => ({
                ...old,
                items: [...(old?.items || []), { ...newCar, id: 'temp-' + Date.now() }]
            }));

            return { previousCars };
        },
        onError: (err, newCar, context) => {
            // Rollback on error
            queryClient.setQueryData(['cars'], context?.previousCars);
        },
        onSettled: () => {
            // Refetch after mutation
            queryClient.invalidateQueries({ queryKey: ['cars'] });
        }
    });
}

// Usage
function CarList() {
    const { data, isLoading, error } = useCars({ page: 1, pageSize: 10 });
    const createCar = useCreateCar();

    if (isLoading) return <Spinner />;
    if (error) return <Error message={error.message} />;

    return (
        <div>
            {data?.items.map(car => <CarCard key={car.id} car={car} />)}
            <button onClick={() => createCar.mutate({ make: 'Toyota', model: 'Camry' })}>
                Add Car
            </button>
        </div>
    );
}
```

## Decision Guide (2025)

### Use `useState` when:
- State is local to one component
- Simple state (primitives, simple objects)

### Use Context API when:
- Avoiding prop drilling (2-3 levels)
- Theme, locale, auth state
- Infrequent updates
- Small to medium apps

### Use Zustand when:
- Medium-sized apps
- Need global state with minimal boilerplate
- Want excellent performance
- Don't need time-travel debugging
- **Recommended for most applications**

### Use Redux Toolkit when:
- Large enterprise applications
- Complex state management needs
- Large teams needing structure
- Need time-travel debugging
- Extensive middleware requirements

### Use React Query when:
- Managing server state
- Need caching and synchronization
- Want automatic background refetching
- **Always use for API data**

### Combine Tools:
```tsx
// Typical modern app stack
function App() {
    return (
        <QueryClientProvider client={queryClient}>  {/* Server state */}
            <AuthProvider>                           {/* Auth context */}
                <ThemeProvider>                      {/* Theme context */}
                    <Router>
                        <Routes />
                    </Router>
                </ThemeProvider>
            </AuthProvider>
        </QueryClientProvider>
    );
}

// Zustand for client-side global state
const useUIStore = create((set) => ({
    sidebarOpen: false,
    toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen }))
}));

// React Query for server state
const { data: cars } = useCars();
```

## Best Practices

- [ ] Start with `useState`, move to more complex solutions as needed
- [ ] Use React Query for all server state
- [ ] Keep state as local as possible
- [ ] Avoid prop drilling with Context
- [ ] Use Zustand for simple global state
- [ ] Use Redux Toolkit for complex enterprise needs
- [ ] Normalize complex data structures
- [ ] Derive state when possible (don't duplicate)
- [ ] Use selectors to prevent unnecessary re-renders
- [ ] Persist only necessary state to localStorage
- [ ] Don't store everything in global state
- [ ] Combine multiple state management solutions

## Summary

In 2025, the React state management landscape has matured significantly. Most applications benefit from using React Query for server state combined with Zustand or Context API for client state. Reserve Redux Toolkit for truly complex enterprise applications where its structure and tooling provide clear benefits.
