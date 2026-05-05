# Mega Outlet Product Check - Build & Run Instructions

## Overview

This is a Windows desktop application (WPF with .NET 8.0) integrated with WooCommerce via REST API. It's designed for in-store customers to quickly check product availability, price, and description.

## System Requirements

- **Operating System:** Windows 10/11 (64-bit)
- **CPU:** x64 architecture
- **RAM:** 4 GB minimum, 8 GB recommended
- **Disk Space:** 200 MB (self-contained, no .NET runtime required)

## Build Instructions

### Option 1: Using pre-built executable

The executable is located at:
```
publish/MegaOutletCheck.exe
```

This is a self-contained single-file executable that includes the .NET runtime.

### Option 2: Building from source

#### Prerequisites
- .NET 8.0 SDK (or higher)
- Windows 10/11 development environment

#### Build commands

```bash
# Navigate to project directory
cd MegaOutletCheck

# Restore dependencies
dotnet restore

# Build debug build
dotnet build

# Build release (self-contained)
dotnet publish -c Release -r win-x64 --self-contained true -o publish
```

## Configuration

### First-time Setup

1. Launch the application
2. The Settings dialog will open automatically on first run
3. Enter your WooCommerce API credentials:

#### Getting WooCommerce API Keys

1. Log in to your WordPress/WooCommerce admin panel
2. Go to **WooCommerce → Settings → Advanced → REST API**
3. Click **Add key**
4. Configure permissions:
   - Description: "Mega Outlet Check App"
   - Permissions: **Read** (or Read/Write)
5. Click **Generate API key**
6. Copy the:
   - **Consumer Key** (API Key)
   - **Consumer Secret** (API Secret)

#### Configure the App

In the Settings dialog:
1. **Store URL:** `https://mega-outlet.pl`
2. **Consumer Key:** Paste your consumer key
3. **Consumer Secret:** Paste your consumer secret
4. Click **Test Connection** to verify
5. Click **Save Settings**

### Settings Storage

Settings are stored in:
```
%LOCALAPPDATA%\MegaOutletCheck\settings.json
```

Cache is stored in:
```
%LOCALAPPDATA%\MegaOutletCheck\cache\
```

## Usage

### Main Features

1. **Live Search**
   - Type in the search bar to search products
   - Results update automatically as you type (300ms debounce)
   - Searches product name, SKU, and keywords

2. **Product Results**
   - Products display in a responsive grid
   - Each card shows: product name, price, stock status
   - Stock status color-coded:
     - 🟢 Green: In stock
     - 🟠 Orange: Low stock (≤5 items)
     - 🔴 Red: Out of stock

3. **Product Details**
   - Click a product card to view details
   - Shows full description, price, stock status
   - Click outside or X to close

4. **Touch/Kiosk Mode**
   - Maximize window for fullscreen
   - All elements are touch-friendly (minimum 44px)
   - On-screen keyboard shows on tap

### Keyboard Shortcuts

- **Escape:** Clear search / Close panel
- **Tab:** Navigate between elements
- **Enter:** Focus search (if nothing selected)

## Troubleshooting

### Connection Errors

- **"Invalid API keys"**: Check consumer key and secret
- **"Connection failed"**: Check store URL and network
- **"404 Not Found"**: Verify WooCommerce REST API is enabled

### Display Issues

- Products not showing images: Check network/firewall
- Interface too small: Increase display scaling in Windows settings

### Offline Mode

If network fails, the app uses cached products. Cache is updated automatically when searching.

To refresh cache:
1. Go to Settings
2. Click Test Connection
3. If successful, cache updates on next search

## Architecture

```
MegaOutletCheck/
├── Models/           # Data models (Product, AppSettings)
├── Services/        # API, cache services
├── ViewModels/     # MVVM view models
├── Views/           # WPF windows
├── Converters/      # UI value converters
└── App.xaml        # Application resources/styles
```

### Key Components

- **WooCommerceService:** REST API integration
- **ProductCacheService:** Local caching for offline fallback
- **MainViewModel:** Main window logic (MVVM)
- **MainWindow:** WPF user interface

## Security Notes

- API keys are stored locally in plain text (for simplicity)
- No data is sent to external servers except WooCommerce
- All communication uses HTTPS
- Application runs locally with no cloud dependencies

## Polish Language

The application is localized in Polish:
- UI Labels: "Szukaj produkty", "Dostępny", "Niedostępny"
- Error Messages: Polish language
- Date/Number Formats: Polish locale

## License

MIT License - Free to use and modify.