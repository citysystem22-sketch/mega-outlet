# Mega Outlet - WooCommerce WebView Android App

A production-ready Android application using Flutter that wraps your WooCommerce/WordPress e-commerce website in a WebView with push notifications support.

## Features

### Core Features
- **WebView** - Full-screen WebView displaying your website (configurable URL)
- **Back Navigation** - Android back button navigates through WebView history properly
- **Loading Indicator** - Shows progress while pages load
- **Error Handling** - Graceful handling of SSL errors and network failures with retry button

### Push Notifications
- **Firebase Cloud Messaging (FCM)** - Integrated for push notifications
- **Server Notifications** - Supports notifications sent from your server
- **Deep Linking** - Tapping notifications opens relevant URLs in the WebView
- **Android 13+ Support** - Proper permission handling

### UI/UX
- **Splash Screen** - Clean splash with logo during initialization
- **Hamburger Menu** - Side menu with:
  - Home (reload website)
  - Back / Forward navigation
  - Refresh
  - Open in external browser

### Google Play Compliance
- Not a "thin wrapper" - includes push notifications, navigation, splash, error handling
- Ready for Play Store publishing

## Build Instructions

### Prerequisites
- Flutter 3.24.5+
- Android SDK
- Java 17+

### Debug Build
```bash
flutter pub get
flutter build apk --debug
```

### Release Build (AAB)
```bash
flutter build appbundle --release
```

### Release Build (APK)
```bash
flutter build apk --release
```

## Configuration

### App URL
Edit `/lib/config/app_config.dart` to change the website URL:
```dart
static const String websiteUrl = 'https://mega-outlet.pl';
```

### Firebase Setup
1. Create a Firebase project at https://console.firebase.google.com
2. Add your Android app with package name: `com.megaoutlet.mega_outlet_app`
3. Download `google-services.json` and replace the placeholder in `android/app/google-services.json`
4. Enable Cloud Messaging in Firebase console
5. Upload APKs/AABs to Play Store with FCM enabled

## Testing Push Notifications

### Via Firebase Console
1. Go to Firebase Console > Cloud Messaging
2. Create new campaign
3. Compose notification with title and body
4. Target your app
5. In "Additional options", add custom data:
   - Key: `url`
   - Value: `https://mega-outlet.pl/product/product-name`

### From Your Server
Send POST requests to FCM with data payload containing the URL.

## Project Structure
```
lib/
├── config/
│   └── app_config.dart     # App configuration
├── utils/
│   ├── constants.dart    # App constants
│   └── helpers.dart    # Helper utilities
├── services/
│   ├── webview_service.dart      # WebView management
│   └── notification_service.dart  # Push notifications
├── screens/
│   ├── home_screen.dart    # Main WebView screen
│   └── error_screen.dart # Error display
├── widgets/
│   ├── app_webview.dart    # WebView widget
│   ├── app_drawer.dart   # Navigation drawer
│   └── loading_indicator.dart
└── main.dart          # App entry point
```

## License
MIT License
