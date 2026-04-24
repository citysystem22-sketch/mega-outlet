import 'package:flutter/material.dart';
import '../screens/home_screen.dart';

class AppDrawer extends StatelessWidget {
  final Function(NavAction) onNavigation;

  const AppDrawer({
    super.key,
    required this.onNavigation,
  });

  @override
  Widget build(BuildContext context) {
    return Drawer(
      child: Column(
        children: [
          Container(
            height: 180,
            width: double.infinity,
            decoration: const BoxDecoration(
              color: Color(0xFFFF6B00),
            ),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Container(
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(16),
                  ),
                  child: const Icon(
                    Icons.shopping_bag,
                    size: 64,
                    color: Color(0xFFFF6B00),
                  ),
                ),
                const SizedBox(height: 16),
                const Text(
                  'Mega Outlet',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 24,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ],
            ),
          ),
          Expanded(
            child: ListView(
              padding: EdgeInsets.zero,
              children: [
                ListTile(
                  leading: const Icon(Icons.home, color: Color(0xFFFF6B00)),
                  title: const Text('Home'),
                  onTap: () => onNavigation(NavAction.home),
                ),
                const Divider(),
                ListTile(
                  leading: const Icon(Icons.arrow_back, color: Color(0xFFFF6B00)),
                  title: const Text('Back'),
                  onTap: () => onNavigation(NavAction.back),
                ),
                ListTile(
                  leading: const Icon(Icons.arrow_forward, color: Color(0xFFFF6B00)),
                  title: const Text('Forward'),
                  onTap: () => onNavigation(NavAction.forward),
                ),
                const Divider(),
                ListTile(
                  leading: const Icon(Icons.refresh, color: Color(0xFFFF6B00)),
                  title: const Text('Refresh'),
                  onTap: () => onNavigation(NavAction.refresh),
                ),
                const Divider(),
                ListTile(
                  leading: const Icon(Icons.open_in_browser, color: Color(0xFFFF6B00)),
                  title: const Text('Open in Browser'),
                  onTap: () => onNavigation(NavAction.openInBrowser),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
