import 'package:flutter/material.dart';
import 'package:webview_flutter/webview_flutter.dart';
import '../config/app_config.dart';
import '../services/webview_service.dart';
import '../utils/helpers.dart';
import '../widgets/app_webview.dart';
import '../widgets/app_drawer.dart';
import '../widgets/loading_indicator.dart';
import 'error_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  final GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>();
  final WebViewService _webviewService = WebViewService.instance;

  WebViewController? _controller;
  bool _isInitializing = true;
  bool _hasError = false;
  String _errorMessage = '';

  @override
  void initState() {
    super.initState();
    _initializeWebView();
  }

  Future<void> _initializeWebView() async {
    // Initialize WebView 
    try {
      _controller = await _webviewService.initialize();
    } catch (e) {
      if (mounted) {
        setState(() {
          _isInitializing = false;
          _hasError = true;
          _errorMessage = 'WebView error: $e';
        });
        return;
      }
    }
    
    if (mounted) {
      setState(() {
        _isInitializing = false;
      });
    }
  }

  void _handleNotificationTap(String? url) {
    if (url != null && url.isNotEmpty) {
      _webviewService.loadUrl(url);
    }
  }

  Future<bool> _onWillPop() async {
    final canGoBack = await _webviewService.canGoBack();
    if (canGoBack) {
      await _webviewService.goBack();
      return false;
    }
    return true;
  }

  void _openDrawer() {
    _scaffoldKey.currentState?.openDrawer();
  }

  void _handleNavigation(NavAction action) async {
    Navigator.pop(context);
    
    switch (action) {
      case NavAction.home:
        await _webviewService.reload();
        Helpers.showSnackBar(context, 'Reloading website...');
        break;
      case NavAction.back:
        final canGoBack = await _webviewService.canGoBack();
        if (canGoBack) {
          await _webviewService.goBack();
        } else {
          Helpers.showSnackBar(context, 'No previous page', isError: true);
        }
        break;
      case NavAction.forward:
        final canGoForward = await _webviewService.canGoForward();
        if (canGoForward) {
          await _webviewService.goForward();
        } else {
          Helpers.showSnackBar(context, 'No next page', isError: true);
        }
        break;
      case NavAction.refresh:
        await _webviewService.reload();
        Helpers.showSnackBar(context, 'Refreshing...');
        break;
      case NavAction.openInBrowser:
        final url = _webviewService.currentUrl;
        final uri = Uri.parse(url);
        await Helpers.launchInBrowser(uri);
        break;
    }
  }

  @override
  Widget build(BuildContext context) {
    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (bool didPop, Object? result) async {
        if (didPop) return;
        final shouldPop = await _onWillPop();
        if (shouldPop && context.mounted) {
          Navigator.of(context).pop();
        }
      },
      child: Scaffold(
        key: _scaffoldKey,
        appBar: AppBar(
          title: const Text(AppConfig.appName),
          leading: IconButton(
            icon: const Icon(Icons.menu),
            onPressed: _openDrawer,
          ),
          actions: [
            IconButton(
              icon: const Icon(Icons.arrow_back),
              onPressed: () async {
                final canGoBack = await _webviewService.canGoBack();
                if (canGoBack) {
                  await _webviewService.goBack();
                }
              },
            ),
            IconButton(
              icon: const Icon(Icons.arrow_forward),
              onPressed: () async {
                final canGoForward = await _webviewService.canGoForward();
                if (canGoForward) {
                  await _webviewService.goForward();
                }
              },
            ),
            IconButton(
              icon: const Icon(Icons.refresh),
              onPressed: () async {
                await _webviewService.reload();
              },
            ),
          ],
        ),
        drawer: AppDrawer(onNavigation: _handleNavigation),
        body: _buildBody(),
      ),
    );
  }

  Widget _buildBody() {
    if (_isInitializing) {
      return const LoadingIndicator(message: 'Loading Mega Outlet...');
    }

    if (_hasError) {
      return ErrorScreen(message: _errorMessage, onRetry: _initializeWebView);
    }

    return Stack(
      children: [
        AppWebViewWidget(controller: _controller),
        ValueListenableBuilder<int>(
          valueListenable: ValueNotifier(_webviewService.loadingProgress),
          builder: (context, progress, child) {
            if (progress > 0 && progress < 100) {
              return const LinearProgressIndicator();
            }
            return const SizedBox.shrink();
          },
        ),
      ],
    );
  }
}

enum NavAction { home, back, forward, refresh, openInBrowser }
