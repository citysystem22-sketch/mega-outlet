import 'package:flutter/material.dart';
import 'package:webview_flutter/webview_flutter.dart';

class AppWebViewWidget extends StatelessWidget {
  final WebViewController? controller;

  const AppWebViewWidget({
    super.key,
    required this.controller,
  });

  @override
  Widget build(BuildContext context) {
    if (controller == null) {
      return const Center(
        child: Text('WebView not initialized'),
      );
    }

    return WebViewWidget(controller: controller!);
  }
}
