import 'dart:ui' show Color;
import 'package:flutter/foundation.dart';
import 'package:webview_flutter/webview_flutter.dart';
import '../config/app_config.dart';

class WebViewService {
  WebViewService._();
  
  static final WebViewService instance = WebViewService._();
  
  WebViewController? _controller;
  String _currentUrl = AppConfig.websiteUrl;
  bool _isLoading = true;
  int _loadingProgress = 0;
  bool _hasError = false;
  String _errorMessage = '';
  
  String get currentUrl => _currentUrl;
  bool get isLoading => _isLoading;
  int get loadingProgress => _loadingProgress;
  bool get hasError => _hasError;
  String get errorMessage => _errorMessage;
  WebViewController? get controller => _controller;
  
  Future<WebViewController> initialize() async {
    final controller = WebViewController()
      ..setJavaScriptMode(JavaScriptMode.unrestricted)
      ..setBackgroundColor(const Color(0xFFFFFFFF))
      ..setNavigationDelegate(
        NavigationDelegate(
          onPageStarted: (String url) {
            _isLoading = true;
            _loadingProgress = 0;
            _hasError = false;
            _errorMessage = '';
          },
          onProgress: (int progress) {
            _loadingProgress = progress;
          },
          onPageFinished: (String url) {
            _isLoading = false;
            _loadingProgress = 100;
            _currentUrl = url;
          },
          onWebResourceError: (WebResourceError error) {
            _hasError = true;
            _errorMessage = error.description;
          },
          onNavigationRequest: (NavigationRequest request) {
            return NavigationDecision.navigate;
          },
        ),
      )
      ..addJavaScriptChannel(
        'FlutterChannel',
        onMessageReceived: (JavaScriptMessage message) {
          debugPrint('JavaScript message received: ${message.message}');
        },
      );
    
    _controller = controller;
    await controller.loadRequest(Uri.parse(AppConfig.websiteUrl));
    
    return controller;
  }
  
  Future<bool> goBack() async {
    if (_controller == null) return false;
    final canGoBack = await _controller!.canGoBack();
    if (canGoBack) {
      await _controller!.goBack();
      return true;
    }
    return false;
  }
  
  Future<bool> goForward() async {
    if (_controller == null) return false;
    final canGoForward = await _controller!.canGoForward();
    if (canGoForward) {
      await _controller!.goForward();
      return true;
    }
    return false;
  }
  
  Future<void> reload() async {
    if (_controller == null) return;
    await _controller!.reload();
  }
  
  Future<void> loadUrl(String url) async {
    if (_controller == null) return;
    await _controller!.loadRequest(Uri.parse(url));
    _currentUrl = url;
  }
  
  Future<bool> canGoBack() async {
    if (_controller == null) return false;
    return await _controller!.canGoBack();
  }
  
  Future<bool> canGoForward() async {
    if (_controller == null) return false;
    return await _controller!.canGoForward();
  }
}
