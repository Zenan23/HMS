import 'dart:io';
import 'dart:typed_data';

import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:printing/printing.dart';

class PdfReportPreviewDialog extends StatelessWidget {
  final String title;
  final String fileName;
  final Uint8List pdfBytes;

  const PdfReportPreviewDialog({
    super.key,
    required this.title,
    required this.fileName,
    required this.pdfBytes,
  });

  static Future<void> show({
    required BuildContext context,
    required String title,
    required String fileName,
    required Uint8List pdfBytes,
  }) {
    return showDialog<void>(
      context: context,
      barrierDismissible: false,
      builder: (_) => PdfReportPreviewDialog(
        title: title,
        fileName: fileName,
        pdfBytes: pdfBytes,
      ),
    );
  }

  String get _safeFileName {
    final name = fileName.toLowerCase().endsWith('.pdf')
        ? fileName
        : '$fileName.pdf';
    return name.replaceAll(RegExp(r'[<>:"/\\|?*]'), '_');
  }

  Future<void> _export(BuildContext context) async {
    try {
      final path = await FilePicker.platform.saveFile(
        dialogTitle: 'Sačuvaj PDF izvještaj',
        fileName: _safeFileName,
        type: FileType.custom,
        allowedExtensions: const ['pdf'],
      );

      if (path == null) return;

      final outPath = path.toLowerCase().endsWith('.pdf') ? path : '$path.pdf';
      await File(outPath).writeAsBytes(pdfBytes, flush: true);

      if (!context.mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('PDF sačuvan: $outPath'),
          backgroundColor: Colors.green,
        ),
      );
    } catch (e) {
      if (!context.mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Greška pri exportu: $e'),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final size = MediaQuery.of(context).size;

    return Dialog(
      insetPadding: const EdgeInsets.all(24),
      child: SizedBox(
        width: size.width * 0.92,
        height: size.height * 0.9,
        child: Column(
          children: [
            Material(
              color: Theme.of(context).colorScheme.surface,
              child: Padding(
                padding:
                    const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                child: Row(
                  children: [
                    const Icon(Icons.picture_as_pdf),
                    const SizedBox(width: 8),
                    Expanded(
                      child: Text(
                        title,
                        style: const TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.w600,
                        ),
                        overflow: TextOverflow.ellipsis,
                      ),
                    ),
                    ElevatedButton.icon(
                      onPressed: () => _export(context),
                      icon: const Icon(Icons.download),
                      label: const Text('Export'),
                    ),
                    const SizedBox(width: 8),
                    TextButton(
                      onPressed: () => Navigator.of(context).pop(),
                      child: const Text('Zatvori'),
                    ),
                  ],
                ),
              ),
            ),
            const Divider(height: 1),
            Expanded(
              child: PdfPreview(
                build: (_) async => pdfBytes,
                pdfFileName: _safeFileName,
                allowPrinting: true,
                allowSharing: false,
                canChangePageFormat: false,
                canChangeOrientation: false,
                canDebug: false,
                initialPageFormat: null,
                actions: const [],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
