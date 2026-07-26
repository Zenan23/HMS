import 'package:intl/intl.dart';

final _displayDateFormat = DateFormat('dd.MM.yyyy');

String formatDisplayDate(DateTime? value) {
  if (value == null) return '-';
  return _displayDateFormat.format(value.toLocal());
}
