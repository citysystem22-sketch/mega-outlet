import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import 'package:url_launcher/url_launcher_string.dart';

/// Helper utilities
class Helpers {
  /// Show a snackbar message
  static void showSnackBar(BuildContext context, String message, {bool isError = false}) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        backgroundColor: isError ? Colors.red : Colors.green,
        duration: Duration(seconds: 2),
      ),
    );
  }
  
  /// Launch URL in external browser
  static Future<bool> launchInBrowser(Uri uri) async {
    try {
      if (await canLaunchUrlString(uri.toString())) {
        return await launchUrlString(
          uri.toString(),
          mode: LaunchMode.externalApplication,
        );
      }
      return false;
    } catch (e) {
      return false;
    }
  }
}
