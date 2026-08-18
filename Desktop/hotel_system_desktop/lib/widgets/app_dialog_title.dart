import 'package:flutter/material.dart';

/// Standardni naslov za sve forme/dijaloge u aplikaciji — dodaje vidljivo
/// "X" dugme za zatvaranje gore desno (RSII uputa: svaka forma/dijalog mora
/// imati dugme za zatvaranje, ne samo Otkaži/Escape).
///
/// Koristiti kao zamjenu za `title: Text('...')` unutar `AlertDialog`:
/// `title: AppDialogTitle('Naslov forme')`.
class AppDialogTitle extends StatelessWidget {
  final String title;
  final bool enabled;

  const AppDialogTitle(this.title, {super.key, this.enabled = true});

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Expanded(
          child: Text(title, overflow: TextOverflow.ellipsis),
        ),
        IconButton(
          icon: const Icon(Icons.close),
          tooltip: 'Zatvori',
          splashRadius: 20,
          padding: EdgeInsets.zero,
          constraints: const BoxConstraints(),
          visualDensity: VisualDensity.compact,
          onPressed: enabled ? () => Navigator.of(context).pop() : null,
        ),
      ],
    );
  }
}
