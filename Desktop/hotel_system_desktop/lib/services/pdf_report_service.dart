import 'dart:io';
import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:pdf/pdf.dart';
import 'package:pdf/widgets.dart' as pw;

import '../models/booking.dart';
import '../models/dashboard_statistics.dart';
import '../models/hotel.dart';
import '../models/inventory_transaction.dart';
import '../models/loyalty_points_redemption.dart';
import '../models/price_adjustment.dart';
import '../models/room.dart';
import '../models/room_maintenance_log.dart';
import '../models/service.dart';
import '../models/support_ticket.dart';
import '../models/user.dart';
import '../utils/date_format_utils.dart';
import '../utils/display_labels.dart';
import '../widgets/pdf_report_preview_dialog.dart';

class PdfReportService {
  static final _currency = NumberFormat('#,##0.00', 'bs');
  static final _dateTime = DateFormat('dd.MM.yyyy HH:mm');

  static Future<void> exportVisibleData({
    required BuildContext context,
    required String title,
    required List<String> headers,
    required List<List<String>> rows,
    String? subtitle,
  }) async {
    if (rows.isEmpty) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Nema podataka za PDF izvještaj.'),
            backgroundColor: Colors.orange,
          ),
        );
      }
      return;
    }

    try {
      final result = await _buildTableReport(
        title: title,
        headers: headers,
        rows: rows,
        subtitle: subtitle,
      );
      if (!context.mounted) return;
      await PdfReportPreviewDialog.show(
        context: context,
        title: title,
        fileName: result.fileName,
        pdfBytes: result.bytes,
      );
    } catch (e) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Greška pri generisanju PDF-a: $e'),
            backgroundColor: Colors.red,
          ),
        );
      }
    }
  }

  static Future<void> exportHotels(
    BuildContext context,
    List<Hotel> hotels,
  ) {
    return exportVisibleData(
      context: context,
      title: 'Izvještaj — Hoteli',
      headers: const [
        'ID',
        'Naziv',
        'Grad',
        'Država',
        'Ocjena',
        'Telefon',
        'Email',
      ],
      rows: hotels
          .map((h) => [
                '${h.id}',
                h.name,
                h.city,
                h.country,
                h.averageRating.toStringAsFixed(1),
                h.phoneNumber,
                h.email,
              ])
          .toList(),
    );
  }

  static Future<void> exportBookings(
    BuildContext context,
    List<Booking> bookings,
  ) {
    return exportVisibleData(
      context: context,
      title: 'Izvještaj — Rezervacije',
      headers: const [
        'ID',
        'Prijava',
        'Odjava',
        'Gosti',
        'Soba',
        'Korisnik',
        'Status',
        'Cijena',
        'Zahtjevi',
      ],
      rows: bookings
          .map((b) => [
                '${b.id}',
                formatDisplayDate(b.checkInDate),
                formatDisplayDate(b.checkOutDate),
                '${b.numberOfGuests}',
                b.roomDisplayLabel,
                b.userDisplayLabel,
                bookingStatusLabel(b.status),
                _currency.format(b.totalPrice),
                b.specialRequests,
              ])
          .toList(),
    );
  }

  static Future<void> exportRooms(BuildContext context, List<Room> rooms) {
    return exportVisibleData(
      context: context,
      title: 'Izvještaj — Sobe',
      headers: const [
        'ID',
        'Broj',
        'Tip',
        'Hotel',
        'Cijena/noć',
        'Kapacitet',
        'Dostupna',
      ],
      rows: rooms
          .map((r) => [
                '${r.id}',
                r.roomNumber,
                roomTypeLabel(r.roomType),
                r.hotelName?.isNotEmpty == true
                    ? r.hotelName!
                    : 'Hotel #${r.hotelId}',
                _currency.format(r.pricePerNight),
                '${r.maxOccupancy}',
                r.isAvailable ? 'Da' : 'Ne',
              ])
          .toList(),
    );
  }

  static Future<void> exportServices(
    BuildContext context,
    List<Service> services,
  ) {
    return exportVisibleData(
      context: context,
      title: 'Izvještaj — Servisi',
      headers: const [
        'ID',
        'Naziv',
        'Kategorija',
        'Cijena',
        'Hotel',
        'Dostupan',
        'Aktivan',
      ],
      rows: services
          .map((s) => [
                '${s.id}',
                s.name,
                s.category,
                _currency.format(s.price),
                s.hotelName?.isNotEmpty == true
                    ? s.hotelName!
                    : 'Hotel #${s.hotelId}',
                s.isAvailable ? 'Da' : 'Ne',
                s.isActive ? 'Da' : 'Ne',
              ])
          .toList(),
    );
  }

  static Future<void> exportEmployees(
    BuildContext context,
    List<Employee> employees,
  ) {
    return exportVisibleData(
      context: context,
      title: 'Izvještaj — Uposlenici',
      headers: const [
        'ID',
        'Ime',
        'Korisničko ime',
        'Email',
        'Telefon',
        'Uloga',
        'Aktivan',
        'Zadnja prijava',
      ],
      rows: employees
          .map((e) => [
                '${e.id}',
                e.fullName.isNotEmpty
                    ? e.fullName
                    : '${e.firstName} ${e.lastName}',
                e.username,
                e.email,
                e.phoneNumber,
                userRoleLabel(e.role),
                e.isActive ? 'Da' : 'Ne',
                formatDisplayDate(e.lastLoginDate),
              ])
          .toList(),
    );
  }

  static Future<void> exportUsers(
    BuildContext context,
    List<Employee> users,
  ) {
    return exportVisibleData(
      context: context,
      title: 'Izvještaj — Korisnici',
      headers: const [
        'ID',
        'Ime',
        'Korisničko ime',
        'Email',
        'Telefon',
        'Uloga',
        'Aktivan',
        'Kreiran',
      ],
      rows: users
          .map((u) => [
                '${u.id}',
                u.fullName.isNotEmpty
                    ? u.fullName
                    : '${u.firstName} ${u.lastName}',
                u.username,
                u.email,
                u.phoneNumber,
                userRoleLabel(u.role),
                u.isActive ? 'Da' : 'Ne',
                formatDisplayDate(u.createdAt),
              ])
          .toList(),
    );
  }

  static Future<void> exportSupportTickets(
    BuildContext context,
    List<SupportTicket> tickets,
  ) {
    return exportVisibleData(
      context: context,
      title: 'Izvještaj — Tiketi podrške',
      headers: const [
        'ID',
        'Korisnik',
        'Predmet',
        'Prioritet',
        'Status',
        'Kreiran',
      ],
      rows: tickets
          .map((t) => [
                '${t.id}',
                t.userName.isNotEmpty ? t.userName : 'Korisnik #${t.userId}',
                t.subject,
                supportTicketPriorityLabel(t.priority),
                supportTicketStatusLabel(t.status),
                formatDisplayDate(t.createdAt),
              ])
          .toList(),
    );
  }

  static Future<void> exportPriceAdjustments(
    BuildContext context,
    List<PriceAdjustment> adjustments,
  ) {
    return exportVisibleData(
      context: context,
      title: 'Izvještaj — Prilagodbe cijena',
      headers: const [
        'ID',
        'Naziv',
        'Modifikator %',
        'Od',
        'Do',
        'Kumulativno',
      ],
      rows: adjustments
          .map((a) => [
                '${a.id}',
                a.name,
                a.percentageModifier.toStringAsFixed(2),
                formatDisplayDate(a.startDate),
                formatDisplayDate(a.endDate),
                a.isCumulative ? 'Da' : 'Ne',
              ])
          .toList(),
    );
  }

  static Future<void> exportMaintenanceLogs(
    BuildContext context,
    List<RoomMaintenanceLog> logs,
  ) {
    return exportVisibleData(
      context: context,
      title: 'Izvještaj — Održavanje soba',
      headers: const [
        'ID',
        'Soba',
        'Prijavljeno',
        'Riješeno',
        'Tehničar',
        'Trošak',
        'Opis',
      ],
      rows: logs
          .map((l) => [
                '${l.id}',
                l.roomDisplayLabel,
                formatDisplayDate(l.reportedAt),
                formatDisplayDate(l.resolvedAt),
                l.technicianName,
                _currency.format(l.cost),
                l.description,
              ])
          .toList(),
    );
  }

  static Future<void> exportInventoryTransactions(
    BuildContext context,
    List<InventoryTransaction> transactions,
  ) {
    return exportVisibleData(
      context: context,
      title: 'Izvještaj — Skladišne transakcije',
      headers: const [
        'ID',
        'Artikal',
        'Promjena',
        'Datum',
        'Osoblje',
        'Razlog',
      ],
      rows: transactions
          .map((t) => [
                '${t.id}',
                t.inventoryItemName.isNotEmpty
                    ? t.inventoryItemName
                    : 'Artikal #${t.inventoryItemId}',
                '${t.quantityChange}',
                formatDisplayDate(t.transactionDate),
                t.staffUserName.isNotEmpty
                    ? t.staffUserName
                    : 'Korisnik #${t.staffUserId}',
                t.reason,
              ])
          .toList(),
    );
  }

  static Future<void> exportLoyaltyRedemptions(
    BuildContext context,
    List<LoyaltyPointsRedemption> redemptions,
  ) {
    return exportVisibleData(
      context: context,
      title: 'Izvještaj — Bodovi vjernosti',
      headers: const [
        'ID',
        'Korisnik',
        'Rezervacija',
        'Bodovi',
        'Vrijednost',
        'Datum',
      ],
      rows: redemptions
          .map((r) => [
                '${r.id}',
                r.userName.isNotEmpty ? r.userName : 'Korisnik #${r.userId}',
                r.bookingDisplayLabel,
                '${r.pointsUsed}',
                _currency.format(r.equivalentValueAmount),
                formatDisplayDate(r.redeemedAt),
              ])
          .toList(),
    );
  }

  static Future<void> exportDashboard(
    BuildContext context,
    DashboardStatistics stats, {
    DateTime? fromDate,
    DateTime? toDate,
  }) async {
    try {
      final fonts = await _loadFonts();
      final period = [
        if (fromDate != null) 'Od: ${formatDisplayDate(fromDate)}',
        if (toDate != null) 'Do: ${formatDisplayDate(toDate)}',
      ].join('  ');

      final pdf = pw.Document();
      pdf.addPage(
        pw.MultiPage(
          pageFormat: PdfPageFormat.a4,
          margin: const pw.EdgeInsets.all(32),
          theme: pw.ThemeData.withFont(
            base: fonts.regular,
            bold: fonts.bold,
          ),
          header: (context) => _buildHeader(
            'Izvještaj — Pregled',
            period.isEmpty ? null : period,
            fonts,
          ),
          footer: (context) => _buildFooter(context, fonts),
          build: (context) => [
            pw.SizedBox(height: 12),
            _sectionTitle('Sažetak', fonts),
            _keyValueTable([
              ['Ukupna zarada (neto)', _currency.format(stats.paymentStats.netPayments)],
              ['Ukupna plaćanja', _currency.format(stats.paymentStats.totalPayments)],
              ['Na čekanju', _currency.format(stats.paymentStats.pendingPayments)],
              ['Otkazana plaćanja', _currency.format(stats.paymentStats.cancelledPayments)],
              ['Ukupne rezervacije', '${stats.bookingStats.totalBookings}'],
              ['Potvrđene', '${stats.bookingStats.confirmedBookings}'],
              ['Na čekanju (rez.)', '${stats.bookingStats.pendingBookings}'],
              ['Otkazane', '${stats.bookingStats.cancelledBookings}'],
              ['Završene', '${stats.bookingStats.completedBookings}'],
              ['Prihod od rezervacija', _currency.format(stats.bookingStats.totalRevenue)],
              ['Ukupni korisnici', '${stats.userStats.totalUsers}'],
              ['Aktivni korisnici', '${stats.userStats.activeUsers}'],
              ['Novi ovaj mjesec', '${stats.userStats.newUsersThisMonth}'],
              ['Ukupni hoteli', '${stats.hotelStats.totalHotels}'],
              ['Ukupne sobe', '${stats.hotelStats.totalRooms}'],
              ['Dostupne sobe', '${stats.hotelStats.availableRooms}'],
              ['Prosječna ocjena', stats.reviewStats.averageRating.toStringAsFixed(1)],
              ['Ukupne recenzije', '${stats.reviewStats.totalReviews}'],
            ], fonts),
            pw.SizedBox(height: 20),
            _sectionTitle('Top hoteli', fonts),
            if (stats.hotelStats.topHotels.isEmpty)
              pw.Text('Nema podataka.', style: pw.TextStyle(font: fonts.regular))
            else
              _dataTable(
                headers: const [
                  'Hotel',
                  'Ocjena',
                  'Rezervacije',
                  'Prihod',
                  'Popunjenost %',
                ],
                rows: stats.hotelStats.topHotels
                    .map((h) => [
                          h.name,
                          h.averageRating.toStringAsFixed(1),
                          '${h.totalBookings}',
                          _currency.format(h.totalRevenue),
                          h.occupancyRate.toStringAsFixed(1),
                        ])
                    .toList(),
                fonts: fonts,
              ),
            pw.SizedBox(height: 20),
            _sectionTitle('Recenzije po zvjezdicama', fonts),
            _dataTable(
              headers: const ['Zvjezdice', 'Broj'],
              rows: [
                ['5', '${stats.reviewStats.fiveStarReviews}'],
                ['4', '${stats.reviewStats.fourStarReviews}'],
                ['3', '${stats.reviewStats.threeStarReviews}'],
                ['2', '${stats.reviewStats.twoStarReviews}'],
                ['1', '${stats.reviewStats.oneStarReviews}'],
              ],
              fonts: fonts,
            ),
          ],
        ),
      );

      final bytes = await pdf.save();
      if (!context.mounted) return;
      await PdfReportPreviewDialog.show(
        context: context,
        title: 'Izvještaj — Pregled',
        fileName: 'pregled_izvjestaj.pdf',
        pdfBytes: bytes,
      );
    } catch (e) {
      if (context.mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Greška pri generisanju PDF-a: $e'),
            backgroundColor: Colors.red,
          ),
        );
      }
    }
  }

  static Future<_BuiltPdf> _buildTableReport({
    required String title,
    required List<String> headers,
    required List<List<String>> rows,
    String? subtitle,
  }) async {
    final fonts = await _loadFonts();
    final pdf = pw.Document();

    pdf.addPage(
      pw.MultiPage(
        pageFormat: PdfPageFormat.a4.landscape,
        margin: const pw.EdgeInsets.all(28),
        theme: pw.ThemeData.withFont(
          base: fonts.regular,
          bold: fonts.bold,
        ),
        header: (context) => _buildHeader(title, subtitle, fonts),
        footer: (context) => _buildFooter(context, fonts),
        build: (context) => [
          pw.SizedBox(height: 8),
          pw.Text(
            'Broj zapisa: ${rows.length}',
            style: pw.TextStyle(font: fonts.regular, fontSize: 10),
          ),
          pw.SizedBox(height: 10),
          _dataTable(headers: headers, rows: rows, fonts: fonts),
        ],
      ),
    );

    final safeName = title
        .toLowerCase()
        .replaceAll(RegExp(r'[^a-z0-9čćšžđ]+', caseSensitive: false), '_')
        .replaceAll(RegExp(r'^_|_$'), '');

    return _BuiltPdf(
      bytes: await pdf.save(),
      fileName: '${safeName.isEmpty ? 'izvjestaj' : safeName}.pdf',
    );
  }

  static pw.Widget _buildHeader(
    String title,
    String? subtitle,
    _PdfFonts fonts,
  ) {
    return pw.Column(
      crossAxisAlignment: pw.CrossAxisAlignment.start,
      children: [
        pw.Row(
          mainAxisAlignment: pw.MainAxisAlignment.spaceBetween,
          children: [
            pw.Column(
              crossAxisAlignment: pw.CrossAxisAlignment.start,
              children: [
                pw.Text(
                  'Hotel Sistem',
                  style: pw.TextStyle(
                    font: fonts.bold,
                    fontSize: 16,
                  ),
                ),
                pw.Text(
                  title,
                  style: pw.TextStyle(
                    font: fonts.bold,
                    fontSize: 13,
                  ),
                ),
                if (subtitle != null && subtitle.isNotEmpty)
                  pw.Text(
                    subtitle,
                    style: pw.TextStyle(font: fonts.regular, fontSize: 9),
                  ),
              ],
            ),
            pw.Text(
              'Generisano: ${_dateTime.format(DateTime.now())}',
              style: pw.TextStyle(font: fonts.regular, fontSize: 9),
            ),
          ],
        ),
        pw.SizedBox(height: 6),
        pw.Divider(),
      ],
    );
  }

  static pw.Widget _buildFooter(pw.Context context, _PdfFonts fonts) {
    return pw.Column(
      children: [
        pw.Divider(),
        pw.SizedBox(height: 4),
        pw.Row(
          mainAxisAlignment: pw.MainAxisAlignment.spaceBetween,
          children: [
            pw.Text(
              'Hotel Sistem — PDF izvještaj',
              style: pw.TextStyle(font: fonts.regular, fontSize: 8),
            ),
            pw.Text(
              'Stranica ${context.pageNumber} / ${context.pagesCount}',
              style: pw.TextStyle(font: fonts.regular, fontSize: 8),
            ),
          ],
        ),
      ],
    );
  }

  static pw.Widget _sectionTitle(String text, _PdfFonts fonts) {
    return pw.Padding(
      padding: const pw.EdgeInsets.only(bottom: 8),
      child: pw.Text(
        text,
        style: pw.TextStyle(font: fonts.bold, fontSize: 12),
      ),
    );
  }

  static pw.Widget _keyValueTable(List<List<String>> pairs, _PdfFonts fonts) {
    return pw.Table(
      border: pw.TableBorder.all(color: PdfColors.grey400, width: 0.5),
      columnWidths: {
        0: const pw.FlexColumnWidth(2),
        1: const pw.FlexColumnWidth(1.2),
      },
      children: pairs
          .map(
            (p) => pw.TableRow(
              children: [
                pw.Padding(
                  padding: const pw.EdgeInsets.all(6),
                  child: pw.Text(p[0],
                      style: pw.TextStyle(font: fonts.regular, fontSize: 9)),
                ),
                pw.Padding(
                  padding: const pw.EdgeInsets.all(6),
                  child: pw.Text(p[1],
                      style: pw.TextStyle(font: fonts.bold, fontSize: 9)),
                ),
              ],
            ),
          )
          .toList(),
    );
  }

  static pw.Widget _dataTable({
    required List<String> headers,
    required List<List<String>> rows,
    required _PdfFonts fonts,
  }) {
    return pw.TableHelper.fromTextArray(
      headers: headers,
      data: rows,
      headerStyle: pw.TextStyle(
        font: fonts.bold,
        fontSize: 8,
        color: PdfColors.white,
      ),
      headerDecoration: const pw.BoxDecoration(color: PdfColors.indigo700),
      cellStyle: pw.TextStyle(font: fonts.regular, fontSize: 7.5),
      cellAlignment: pw.Alignment.centerLeft,
      cellPadding: const pw.EdgeInsets.symmetric(horizontal: 4, vertical: 3),
      border: pw.TableBorder.all(color: PdfColors.grey400, width: 0.4),
      oddRowDecoration: const pw.BoxDecoration(color: PdfColors.grey100),
    );
  }

  static Future<_PdfFonts> _loadFonts() async {
    final regularBytes = await _readFontBytes(_regularFontCandidates);
    final boldBytes = await _readFontBytes(_boldFontCandidates);

    if (regularBytes == null) {
      throw Exception(
        'Nije pronađen sistemski font za PDF (Arial/DejaVu).',
      );
    }

    return _PdfFonts(
      regular: pw.Font.ttf(ByteData.sublistView(regularBytes)),
      bold: pw.Font.ttf(
        ByteData.sublistView(boldBytes ?? regularBytes),
      ),
    );
  }

  static Future<Uint8List?> _readFontBytes(List<String> candidates) async {
    for (final path in candidates) {
      final file = File(path);
      if (await file.exists()) {
        return file.readAsBytes();
      }
    }
    return null;
  }

  static const _regularFontCandidates = [
    r'C:\Windows\Fonts\arial.ttf',
    r'C:\Windows\Fonts\segoeui.ttf',
    '/System/Library/Fonts/Supplemental/Arial.ttf',
    '/Library/Fonts/Arial.ttf',
    '/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',
    '/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf',
  ];

  static const _boldFontCandidates = [
    r'C:\Windows\Fonts\arialbd.ttf',
    r'C:\Windows\Fonts\segoeuib.ttf',
    '/System/Library/Fonts/Supplemental/Arial Bold.ttf',
    '/Library/Fonts/Arial Bold.ttf',
    '/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf',
    '/usr/share/fonts/truetype/liberation/LiberationSans-Bold.ttf',
  ];
}

class _BuiltPdf {
  final Uint8List bytes;
  final String fileName;

  const _BuiltPdf({required this.bytes, required this.fileName});
}

class _PdfFonts {
  final pw.Font regular;
  final pw.Font bold;

  const _PdfFonts({required this.regular, required this.bold});
}
