import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/foundation.dart';
import 'package:permission_handler/permission_handler.dart';

/// Service for managing Firebase Cloud Messaging push notifications
class NotificationService {
  NotificationService._();
  
  static final NotificationService instance = NotificationService._();
  
  Function(String?)? _onNotificationTapped;
  bool _isInitialized = false;
  bool _isFirebaseReady = false;
  
  /// Initialize the notification service
  Future<bool> initialize({Function(String?)? onNotificationTapped}) async {
    if (_isInitialized) return _isFirebaseReady;
    
    try {
      _onNotificationTapped = onNotificationTapped;
      
      // Request notification permission for Android 13+
      await _requestNotificationPermission();
      
      // Set up message handlers
      _setupMessageHandlers();
      
      // Get initial message
      await _handleInitialMessage();
      
      _isInitialized = true;
      _isFirebaseReady = true;
      return true;
    } catch (e) {
      debugPrint('NotificationService: $e');
      _isInitialized = true;
      _isFirebaseReady = false;
      return false;
    }
  }
  
  /// Request notification permission
  Future<void> _requestNotificationPermission() async {
    try {
      final status = await Permission.notification.status;
      if (status.isDenied) {
        await Permission.notification.request();
      }
    } catch (e) {
      debugPrint('Permission error: $e');
    }
  }
  
  /// Set up message handlers
  void _setupMessageHandlers() {
    try {
      FirebaseMessaging.onMessage.listen(_handleMessage);
      FirebaseMessaging.onMessageOpenedApp.listen(_handleMessage);
    } catch (e) {
      debugPrint('FCM handler error: $e');
    }
  }
  
  /// Handle incoming message
  void _handleMessage(RemoteMessage message) {
    final url = message.data['url'] ?? message.data['link'];
    _onNotificationTapped?.call(url?.toString());
  }
  
  /// Handle initial message
  Future<void> _handleInitialMessage() async {
    try {
      final msg = await FirebaseMessaging.instance.getInitialMessage();
      if (msg != null) {
        _handleMessage(msg);
      }
    } catch (e) {
      debugPrint('Initial message error: $e');
    }
  }
  
  /// Get FCM token
  Future<String?> getToken() async {
    try {
      return await FirebaseMessaging.instance.getToken();
    } catch (e) {
      return null;
    }
  }
  
  /// Subscribe to topic
  Future<void> subscribeToTopic(String topic) async {
    try {
      await FirebaseMessaging.instance.subscribeToTopic(topic);
    } catch (e) {
      debugPrint('Subscribe error: $e');
    }
  }
}