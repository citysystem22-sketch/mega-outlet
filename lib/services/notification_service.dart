import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/foundation.dart';
import 'package:permission_handler/permission_handler.dart';

/// Service for managing Firebase Cloud Messaging push notifications
class NotificationService {
  NotificationService._();
  
  static final NotificationService instance = NotificationService._();
  
  final FirebaseMessaging _firebaseMessaging = FirebaseMessaging.instance;
  
  Function(String?)? _onNotificationTapped;
  bool _isInitialized = false;
  
  /// Initialize the notification service
  Future<bool> initialize({Function(String?)? onNotificationTapped}) async {
    if (_isInitialized) return true;
    
    try {
      _onNotificationTapped = onNotificationTapped;
      
      // Request notification permission for Android 13+
      await _requestNotificationPermission();
      
      // Set up message handlers
      _setupMessageHandlers();
      
      // Get initial message if app was launched from notification
      await _handleInitialMessage();
      
      _isInitialized = true;
      return true;
    } catch (e) {
      debugPrint('NotificationService initialization error: $e');
      return false;
    }
  }
  
  /// Request notification permission
  Future<void> _requestNotificationPermission() async {
    try {
      // Check current permission status
      final status = await Permission.notification.status;
      
      if (status.isDenied) {
        // Request permission
        final result = await Permission.notification.request();
        if (result.isGranted) {
          debugPrint('Notification permission granted');
        }
      } else if (status.isGranted) {
        debugPrint('Notification permission already granted');
      }
    } catch (e) {
      debugPrint('Error requesting notification permission: $e');
    }
  }
  
  /// Set up message handlers
  void _setupMessageHandlers() {
    // Listen for foreground messages
    FirebaseMessaging.onMessage.listen(_handleForegroundMessage);
    
    // Listen for when user taps notification to open app
    FirebaseMessaging.onMessageOpenedApp.listen(_handleMessageOpened);
  }
  
  /// Handle messages received while app is in foreground
  void _handleForegroundMessage(RemoteMessage message) {
    _handleMessage(message);
  }
  
  /// Handle when user taps notification to open app
  void _handleMessageOpened(RemoteMessage message) {
    _handleMessage(message);
  }
  
  /// Handle a remote message
  void _handleMessage(RemoteMessage message) {
    // Extract URL from data payload
    final url = message.data['url'] as String?;
    final link = message.data['link'] as String?;
    final urlToOpen = url ?? link ?? message.notification?.body;
    
    // Notify callback
    _onNotificationTapped?.call(urlToOpen);
    
    debugPrint('Notification tapped with URL: $urlToOpen');
  }
  
  /// Handle initial message (app launched from notification)
  Future<void> _handleInitialMessage() async {
    try {
      final RemoteMessage? initialMessage = await FirebaseMessaging.instance.getInitialMessage();
      if (initialMessage != null) {
        _handleMessage(initialMessage);
      }
    } catch (e) {
      debugPrint('Error handling initial message: $e');
    }
  }
  
  /// Get FCM token for this device
  Future<String?> getToken() async {
    try {
      return await _firebaseMessaging.getToken();
    } catch (e) {
      debugPrint('Error getting FCM token: $e');
      return null;
    }
  }
  
  /// Subscribe to a topic for targeted notifications
  Future<void> subscribeToTopic(String topic) async {
    try {
      await _firebaseMessaging.subscribeToTopic(topic);
      debugPrint('Subscribed to topic: $topic');
    } catch (e) {
      debugPrint('Error subscribing to topic: $e');
    }
  }
  
  /// Unsubscribe from a topic
  Future<void> unsubscribeFromTopic(String topic) async {
    try {
      await _firebaseMessaging.unsubscribeFromTopic(topic);
    } catch (e) {
      debugPrint('Error unsubscribing from topic: $e');
    }
  }
}
