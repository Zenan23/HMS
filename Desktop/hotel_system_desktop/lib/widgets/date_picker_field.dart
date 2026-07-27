import 'package:flutter/material.dart';
import '../utils/date_format_utils.dart';

/// Polje za odabir datuma (prikaz dd.MM.yyyy).
class DatePickerField extends StatelessWidget {
  final String label;
  final DateTime? value;
  final ValueChanged<DateTime?> onChanged;
  final bool allowClear;
  final DateTime? firstDate;
  final DateTime? lastDate;

  const DatePickerField({
    super.key,
    required this.label,
    required this.value,
    required this.onChanged,
    this.allowClear = false,
    this.firstDate,
    this.lastDate,
  });

  Future<void> _pick(BuildContext context) async {
    final picked = await showDatePicker(
      context: context,
      initialDate: value ?? DateTime.now(),
      firstDate: firstDate ?? DateTime(2020),
      lastDate: lastDate ?? DateTime.now().add(const Duration(days: 3650)),
    );
    if (picked != null) onChanged(picked);
  }

  @override
  Widget build(BuildContext context) {
    return InputDecorator(
      decoration: InputDecoration(
        labelText: label,
        suffixIcon: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            if (allowClear && value != null)
              IconButton(
                icon: const Icon(Icons.clear, size: 18),
                tooltip: 'Očisti',
                onPressed: () => onChanged(null),
              ),
            IconButton(
              icon: const Icon(Icons.calendar_today, size: 18),
              tooltip: 'Odaberi datum',
              onPressed: () => _pick(context),
            ),
          ],
        ),
      ),
      child: InkWell(
        onTap: () => _pick(context),
        child: Padding(
          padding: const EdgeInsets.symmetric(vertical: 12),
          child: Text(
            value != null ? formatDisplayDate(value) : 'Odaberi datum',
            style: Theme.of(context).textTheme.bodyLarge,
          ),
        ),
      ),
    );
  }
}
