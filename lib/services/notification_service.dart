import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:permission_handler/permission_handler.dart';

class NotificationService {
  NotificationService._();
  
  static final NotificationService instance = NotificationService._();
  
  final FirebaseMessaging _firebaseMessaging = FirebaseMessaging.instance;
  
  Function(String?)? _onNotificationTapped;
  
  Future<void> initialize({Function(String?)? onNotificationTapped}) async {
    _onNotificationTapped = onNotificationTapped;
    
    // Request notification permission
    final status = await Permission.notification.status;
    if (status.isDenied) {
      await Permission.notification.request();
    }
    
    // Listen for messages
    FirebaseMessaging.onMessage.listen(_handleRemoteMessage);
    FirebaseMessaging.onMessageOpenedApp.listen(_handleRemoteMessage);
    
    // Get initial message
    final initialMessage = await FirebaseMessaging.instance.getInitialMessage();
    if (initialMessage != null) {
      _handleRemoteMessage(initialMessage);
    }
  }
  
  void _handleRemoteMessage(RemoteMessage message) {
    final url = message.data['url'] as String?;
    _onNotificationTapped?.call(url);
  }
  
  Future<String?> getToken() async {
    return await _firebaseMessaging.getToken();
  }
  
  Future<void> subscribeToTopic(String topic) async {
    await _firebaseMessaging.subscribeToTopic(topic);
  }
}
