import 'package:flutter/material.dart';
import 'package:fl_chart/fl_chart.dart';
import 'package:intl/intl.dart';
import '../services/api_service.dart';
import '../models/dashboard_statistics.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key});

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  DashboardStatistics? _statistics;
  bool _isLoading = true;
  String? _error;
  DateTime? _fromDate;
  DateTime? _toDate;

  @override
  void initState() {
    super.initState();
    _loadStatistics();
  }

  Future<void> _loadStatistics() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    try {
      final statistics = await ApiService().getDashboardStatistics(
        fromDate: _fromDate,
        toDate: _toDate,
      );
      setState(() {
        _statistics = statistics;
        _isLoading = false;
      });
    } catch (e) {
      setState(() {
        _error = e.toString();
        _isLoading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Dashboard'),
        actions: [
          Row(
            children: [
              ElevatedButton(
                onPressed: () async {
                  final date = await showDatePicker(
                    context: context,
                    initialDate: _fromDate ?? DateTime.now(),
                    firstDate: DateTime(2020),
                    lastDate: DateTime.now(),
                  );
                  if (date != null) {
                    setState(() {
                      _fromDate = date;
                    });
                    _loadStatistics();
                  }
                },
                child: Text(_fromDate != null
                    ? DateFormat('dd.MM.yyyy').format(_fromDate!)
                    : 'Od datuma'),
              ),
              const SizedBox(width: 8),
              ElevatedButton(
                onPressed: () async {
                  final date = await showDatePicker(
                    context: context,
                    initialDate: _toDate ?? DateTime.now(),
                    firstDate: DateTime(2020),
                    lastDate: DateTime.now(),
                  );
                  if (date != null) {
                    setState(() {
                      _toDate = date;
                    });
                    _loadStatistics();
                  }
                },
                child: Text(_toDate != null
                    ? DateFormat('dd.MM.yyyy').format(_toDate!)
                    : 'Do datuma'),
              ),
              const SizedBox(width: 8),
              ElevatedButton(
                onPressed: _loadStatistics,
                child: const Text('Osvježi'),
              ),
              const SizedBox(width: 16),
            ],
          ),
        ],
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? Center(
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      const Icon(
                        Icons.error,
                        size: 64,
                        color: Colors.red,
                      ),
                      const SizedBox(height: 16),
                      const Text(
                        'Greška pri učitavanju podataka',
                        style: TextStyle(
                            fontSize: 18, fontWeight: FontWeight.bold),
                      ),
                      const SizedBox(height: 8),
                      Text(_error!),
                      const SizedBox(height: 16),
                      ElevatedButton(
                        onPressed: _loadStatistics,
                        child: const Text('Pokušaj ponovo'),
                      ),
                    ],
                  ),
                )
              : _statistics == null
                  ? const Center(child: Text('Nema podataka'))
                  : SingleChildScrollView(
                      padding: const EdgeInsets.all(16),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          _buildSummaryCards(),
                          const SizedBox(height: 24),
                          _buildCharts(),
                          const SizedBox(height: 24),
                          _buildDetailedTables(),
                        ],
                      ),
                    ),
    );
  }

  Widget _buildSummaryCards() {
    if (_statistics == null) return const SizedBox.shrink();

    return GridView.count(
      crossAxisCount: 4,
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      crossAxisSpacing: 16,
      mainAxisSpacing: 16,
      children: [
        _buildSummaryCard(
          'Ukupna zarada',
          '${NumberFormat.currency(locale: 'bs_BA', symbol: 'KM').format(_statistics!.paymentStats.netPayments)}',
          Icons.attach_money,
        ),
        _buildSummaryCard(
          'Ukupne rezervacije',
          '${_statistics!.bookingStats.totalBookings}',
          Icons.calendar_today,
        ),
        _buildSummaryCard(
          'Ukupni korisnici',
          '${_statistics!.userStats.totalUsers}',
          Icons.people,
        ),
        _buildSummaryCard(
          'Prosječna ocjena',
          '${_statistics!.reviewStats.averageRating.toStringAsFixed(1)}',
          Icons.star,
        ),
      ],
    );
  }

  Widget _buildSummaryCard(String title, String value, IconData icon) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Row(
          children: [
            Icon(icon, size: 32, color: Theme.of(context).primaryColor),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(
                    title,
                    style: const TextStyle(fontSize: 12, color: Colors.grey),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    value,
                    style: const TextStyle(
                        fontSize: 20, fontWeight: FontWeight.bold),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildCharts() {
    if (_statistics == null) return const SizedBox.shrink();

    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Expanded(
          flex: 2,
          child: Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Mesečna zarada',
                    style: const TextStyle(
                        fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 16),
                  SizedBox(
                    height: 300,
                    child: LineChart(
                      LineChartData(
                        gridData: FlGridData(show: true),
                        titlesData: FlTitlesData(
                          leftTitles: AxisTitles(
                            sideTitles: SideTitles(
                              showTitles: true,
                              reservedSize: 60,
                              getTitlesWidget: (value, meta) {
                                return Text(
                                  NumberFormat.compact().format(value),
                                  style: const TextStyle(fontSize: 10),
                                );
                              },
                            ),
                          ),
                          bottomTitles: AxisTitles(
                            sideTitles: SideTitles(
                              showTitles: true,
                              getTitlesWidget: (value, meta) {
                                if (value.toInt() <
                                    _statistics!
                                        .paymentStats.monthlyData.length) {
                                  return Text(
                                    'M${value.toInt() + 1}',
                                    style: const TextStyle(fontSize: 10),
                                  );
                                }
                                return const Text('');
                              },
                            ),
                          ),
                          topTitles: AxisTitles(
                              sideTitles: SideTitles(showTitles: false)),
                          rightTitles: AxisTitles(
                              sideTitles: SideTitles(showTitles: false)),
                        ),
                        borderData: FlBorderData(show: true),
                        lineBarsData: [
                          LineChartBarData(
                            spots: _statistics!.paymentStats.monthlyData
                                .asMap()
                                .entries
                                .map((entry) => FlSpot(entry.key.toDouble(),
                                    entry.value.totalAmount))
                                .toList(),
                            isCurved: true,
                            color: Theme.of(context).primaryColor,
                            barWidth: 3,
                            dotData: FlDotData(show: true),
                          ),
                        ],
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
        const SizedBox(width: 16),
        Expanded(
          child: Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Status rezervacija',
                    style: const TextStyle(
                        fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 16),
                  SizedBox(
                    height: 300,
                    child: PieChart(
                      PieChartData(
                        sections: [
                          PieChartSectionData(
                            value: _statistics!.bookingStats.confirmedBookings
                                .toDouble(),
                            title: 'Potvrđene',
                            color: Theme.of(context).primaryColor,
                            radius: 60,
                          ),
                          PieChartSectionData(
                            value: _statistics!.bookingStats.pendingBookings
                                .toDouble(),
                            title: 'Na čekanju',
                            color: Theme.of(context).primaryColor,
                            radius: 60,
                          ),
                          PieChartSectionData(
                            value: _statistics!.bookingStats.cancelledBookings
                                .toDouble(),
                            title: 'Otkazane',
                            color: Colors.red,
                            radius: 60,
                          ),
                          PieChartSectionData(
                            value: _statistics!.bookingStats.completedBookings
                                .toDouble(),
                            title: 'Završene',
                            color: Theme.of(context).primaryColor,
                            radius: 60,
                          ),
                        ],
                        centerSpaceRadius: 40,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildDetailedTables() {
    if (_statistics == null) return const SizedBox.shrink();

    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Expanded(
          child: Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Top hoteli po ratingu:',
                    style: const TextStyle(
                        fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 16),
                  SizedBox(
                    height: 300,
                    child: ListView.builder(
                      itemCount: _statistics!.hotelStats.topHotels.length,
                      itemBuilder: (context, index) {
                        final hotel = _statistics!.hotelStats.topHotels[index];
                        return ListTile(
                          leading: CircleAvatar(
                            child: Text('${index + 1}'),
                          ),
                          title: Text(hotel.name),
                          subtitle: Text('Rating: ${hotel.averageRating}'),
                          trailing: Text(
                            '${hotel.averageRating}',
                            style: const TextStyle(fontWeight: FontWeight.bold),
                          ),
                        );
                      },
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
        const SizedBox(width: 16),
        Expanded(
          child: Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Ocjene soba po zvjezdicama',
                    style: const TextStyle(
                        fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 16),
                  SizedBox(
                    height: 300,
                    child: ListView.builder(
                      itemCount: 5,
                      itemBuilder: (context, index) {
                        final stars = 5 - index;
                        final count = _getStarCount(stars);
                        return ListTile(
                          leading: Row(
                            mainAxisSize: MainAxisSize.min,
                            children: List.generate(
                                5,
                                (i) => Icon(
                                      i < stars
                                          ? Icons.star
                                          : Icons.star_border,
                                      size: 16,
                                      color: i < stars
                                          ? Colors.amber
                                          : Colors.grey,
                                    )),
                          ),
                          title: Text('$stars zvjezdice'),
                          trailing: Text(
                            '$count',
                            style: const TextStyle(fontWeight: FontWeight.bold),
                          ),
                        );
                      },
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ],
    );
  }

  int _getStarCount(int stars) {
    switch (stars) {
      case 5:
        return _statistics!.reviewStats.fiveStarReviews;
      case 4:
        return _statistics!.reviewStats.fourStarReviews;
      case 3:
        return _statistics!.reviewStats.threeStarReviews;
      case 2:
        return _statistics!.reviewStats.twoStarReviews;
      case 1:
        return _statistics!.reviewStats.oneStarReviews;
      default:
        return 0;
    }
  }
}
