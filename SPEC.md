# Mega Outlet Product Check - Specification

## 1. Project Overview

**Project Name:** Mega Outlet Product Check (MegaOutletCheck)
**Project Type:** Windows Desktop Application (WPF with .NET 8.0)
**Core Feature:** Real-time product search integrated with WooCommerce store for in-store customers
**Target Users:** Customers in physical store (mega-outlet.pl) who need to quickly check product availability and pricing

## 2. UI/UX Specification

### 2.1 Layout Structure

**Window Model:**
- Single main window (frameless for kiosk/fullscreen support)
- Modal dialogs for settings and product details
- Minimum size: 800x600, default: 1024x768
- Supports maximize, fullscreen, and always-on-top modes

**OS-native Style Adaptation:**
- Windows 11 design language with rounded corners
- Support for Windows dark/light theme

**Main Layout Areas:**
```
┌─────────────────────────────────────────────────────────┐
│ [SEARCH BAR - Always visible at top]                    │
│ ┌─────────────────────────────────────────────────┐  │
│ │ 🔍  Search products, SKU, or keywords...        │  │
│ └─────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────┤
│ [RESULTS GRID - Scrollable product cards]            │
│ ┌─────────┐ ┌─────────┐ ┌─────────┐                   │
│ │ Product │ │ Product │ │ Product │                   │
│ │  Card   │ │  Card   │ │  Card   │                   │
│ └─────────┘ └─────────┘ └─────────┘                   │
│ ┌─────────┐ ┌─────────┐ ┌─────────┐                   │
│ │ Product │ │ Product │ │ Product │  ...            │
│ │  Card   │ │  Card   │ │  Card   │                   │
│ └─────────┘ └─────────┘ └─────────┘                   │
├─────────────────────────────────────────────────────────┤
│ [DETAILS PANEL - Slides in from right when product     │
│                 is selected]                        │
│  - Full product info                               │
│  - Price & stock status                          │
│  - Description                                   │
│  - Image                                         │
└─────────────────────────────────────────────────┘
```

### 2.2 Visual Design

**Color Palette:**
- Primary: #2563EB (Blue - brand color)
- Secondary: #1E293B (Dark slate)
- Accent: #10B981 (Green - in stock)
- Error: #EF4444 (Red - out of stock)
- Background: #F8FAFC (Light gray)
- Surface: #FFFFFF (White)
- Text Primary: #0F172A (Almost black)
- Text Secondary: #64748B (Gray)

**Typography:**
- Font Family: Segoe UI Variable (Windows 11 default)
- Heading 1: 28px, SemiBold
- Heading 2: 20px, SemiBold
- Body: 16px, Regular
- Caption: 14px, Regular

**Spacing System:**
- Base unit: 4px
- Small: 8px
- Medium: 16px
- Large: 24px
- XLarge: 32px

**Visual Effects:**
- Card shadows: 0 2px 8px rgba(0,0,0,0.08)
- Hover elevation: 0 4px 16px rgba(0,0,0,0.12)
- Border radius: 12px (cards), 8px (buttons), 24px (search bar)
- Transitions: 200ms ease-out

### 2.3 Components

**Search Bar:**
- Large input field with search icon prefix
- Clear button (X) appears when text present
- States: Default, Focused, Active, Disabled
- Auto-focus on window load

**Product Card:**
- Product image (square, 120x120px)
- Product name (truncate if > 2 lines)
- Price (bold, large)
- Stock status badge (In Stock: green, Out of Stock: red)
- Touch target: entire card is clickable
- States: Default, Hover, Selected, Loading

**Stock Status Badge:**
- In Stock: Green (#10B981), "Dostępny" label
- Out of Stock: Red (#EF4444), "Niedostępny" label
- Low Stock (< 5): Orange (#F59E0B), "Ostatnie sztuki"

**Details Panel:**
- Slides in from right (400px width)
- Large product image (max 300px height)
- Price (large, bold)
- Add to cart button (if applicable)
- Close button (X) top-right
- Description scrollable in HTML format

**Settings Dialog:**
- API Input Key field
- API Secret Key field  
- Store URL field
- Save/Cancel buttons
- Test connection button

## 3. Functional Specification

### 3.1 Core Features

1. **Live Search**
   - Real-time search as user types
   - Debounce: 300ms delay before API call
   - Minimum 2 characters to trigger search
   - Search fields: name, SKU, tags

2. **Product Results Display**
   - Grid layout (3-4 columns based on window width)
   - Lazy loading / pagination (20 items per page)
   - "Load More" button or infinite scroll
   - Empty state: "No products found"

3. **Product Details View**
   - Click card to show details panel
   - Full product information
   - HTML description rendering
   - Product images gallery

4. **WooCommerce Integration**
   - REST API v3
   - OAuth 1.0 or Basic Auth
   - Product search endpoint
   - Stock management

5. **Settings Management**
   - Stored in encrypted local config
   - API credentials
   - Store URL configuration

### 3.2 User Interactions and Flows

**Search Flow:**
1. User taps search bar
2. On-screen keyboard appears (touch mode)
3. User types product name/SKU
4. After 300ms pause, API call triggers
5. Loading indicator shows
6. Results populate grid
7. User taps product card
8. Details panel slides in

### 3.3 Data Flow & Key Modules

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│   MainWindow │    │  ViewModels  │    │   Services   │
│   (Views)    │◄───│  - SearchVM  │◄───│ - WooCommSvc │
│              │    │  - ProductVM │    │ - CacheSvc   │
│              │    │  - SettingsVM│    │ - ConfigSvc  │
└──────────────┘    └──────────────┘    └──────────────┘
                        │                       │
                        ▼                       ▼
                 ┌──────────────┐    ┌──────────────┐
                 │    Models    │    │   Data API   │
                 │ - Product   │    │ - REST Client│
                 │ - Settings  │    │ - JSON Parse │
                 └──────────────┘    └──────────────┘
```

**Key Classes:**
- `WooCommerceService`: API communication
- `ProductCacheService`: Local caching
- `SettingsService`: Configuration management
- `MainViewModel`: Main window logic
- `ProductViewModel`: Product item logic

### 3.4 Edge Cases

- No network connection: Show cached results, display offline notice
- API timeout: Retry with exponential backoff, max 3 attempts
- Invalid API keys: Show configuration dialog
- Empty search results: Display friendly message
- Very long product names: Truncate with ellipsis

## 4. Acceptance Criteria

### 4.1 Success Conditions

1. **Search Performance**
   - Search results appear within 1 second of typing stop
   - UI remains responsive during API calls

2. **Display Accuracy**
   - Stock status matches WooCommerce exactly
   - Prices display correctly with currency symbol (PLN)

3. **Touch Optimization**
   - All interactive elements ≥ 44px touch target
   - Smooth scrolling (60fps)
   - On-screen keyboard auto-triggers

4. **Reliability**
   - App recovers gracefully from network errors
   - Cached data available when offline
   - No crashes or hangs

### 4.2 Visual Checkpoints

1. Main window loads with search bar focused
2. Product cards display in responsive grid
3. Stock status clearly visible with colors
4. Details panel animates smoothly
5. Settings dialog saves and applies changes
6. Polish labels display correctly

## 5. Technical Requirements

- .NET 8.0 WPF
- Target: Windows 10/11 (x64)
- Self-contained deployment (no .NET runtime required)
- WinRT Interop for touch keyboard