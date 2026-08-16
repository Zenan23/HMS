import 'package:flutter/material.dart';
import '../models/city.dart';
import '../models/country.dart';
import '../services/city_service.dart';
import '../services/country_service.dart';
import '../utils/error_helper.dart';
import '../widgets/city_form.dart';
import '../widgets/country_form.dart';

/// Upravljanje referentnim/šifarnik tabelama Grad/Država — dropdown izvori
/// za Hotel.CityId (zamjena za slobodan tekstualni unos grada/države).
class CitiesScreen extends StatefulWidget {
  const CitiesScreen({super.key});

  @override
  State<CitiesScreen> createState() => _CitiesScreenState();
}

class _CitiesScreenState extends State<CitiesScreen>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  final _countryService = CountryService();
  final _cityService = CityService();
  bool _isLoadingCountries = false;
  bool _isLoadingCities = false;
  List<Country> _countries = [];
  List<City> _cities = [];

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
    _fetchCountries();
    _fetchCities();
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<void> _fetchCountries() async {
    setState(() => _isLoadingCountries = true);
    try {
      final countries = await _countryService.getAllForDropdown();
      if (mounted) setState(() => _countries = countries);
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
    if (mounted) setState(() => _isLoadingCountries = false);
  }

  Future<void> _fetchCities() async {
    setState(() => _isLoadingCities = true);
    try {
      final cities = await _cityService.getAllForDropdown();
      if (mounted) setState(() => _cities = cities);
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
    if (mounted) setState(() => _isLoadingCities = false);
  }

  Future<void> _openCountryForm({Country? country}) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (_) => CountryFormDialog(country: country),
    );
    if (result == true) {
      _fetchCountries();
      _fetchCities(); // CountryName prikazan uz grad, osvježi i tu listu
    }
  }

  Future<void> _openCityForm({City? city}) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (_) => CityFormDialog(city: city),
    );
    if (result == true) _fetchCities();
  }

  Future<void> _deleteCountry(Country country) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Potvrda brisanja'),
        content: Text(
            'Obrisati državu „${country.name}”? Nije moguće ako postoje gradovi vezani za nju.'),
        actions: [
          TextButton(
              onPressed: () => Navigator.pop(context, false),
              child: const Text('Ne')),
          ElevatedButton(
              onPressed: () => Navigator.pop(context, true),
              child: const Text('Da, obriši')),
        ],
      ),
    );
    if (confirm != true) return;
    try {
      await _countryService.delete(country.id);
      _fetchCountries();
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
  }

  Future<void> _deleteCity(City city) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Potvrda brisanja'),
        content: Text(
            'Obrisati grad „${city.name}”? Nije moguće ako postoje hoteli u njemu.'),
        actions: [
          TextButton(
              onPressed: () => Navigator.pop(context, false),
              child: const Text('Ne')),
          ElevatedButton(
              onPressed: () => Navigator.pop(context, true),
              child: const Text('Da, obriši')),
        ],
      ),
    );
    if (confirm != true) return;
    try {
      await _cityService.delete(city.id);
      _fetchCities();
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Gradovi i države'),
        bottom: TabBar(
          controller: _tabController,
          tabs: const [
            Tab(text: 'Gradovi'),
            Tab(text: 'Države'),
          ],
        ),
      ),
      floatingActionButton: AnimatedBuilder(
        animation: _tabController,
        builder: (context, _) => FloatingActionButton(
          onPressed: () => _tabController.index == 0
              ? _openCityForm()
              : _openCountryForm(),
          tooltip: _tabController.index == 0 ? 'Novi grad' : 'Nova država',
          child: const Icon(Icons.add),
        ),
      ),
      body: TabBarView(
        controller: _tabController,
        children: [
          _isLoadingCities
              ? const Center(child: CircularProgressIndicator())
              : _cities.isEmpty
                  ? const Center(child: Text('Nema gradova.'))
                  : ListView.builder(
                      itemCount: _cities.length,
                      itemBuilder: (context, i) {
                        final city = _cities[i];
                        return Card(
                          margin: const EdgeInsets.symmetric(
                              horizontal: 12, vertical: 4),
                          child: ListTile(
                            title: Text(city.name),
                            subtitle: Text(city.countryName),
                            trailing: Row(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                IconButton(
                                  icon: const Icon(Icons.edit),
                                  tooltip: 'Uredi',
                                  onPressed: () =>
                                      _openCityForm(city: city),
                                ),
                                IconButton(
                                  icon: const Icon(Icons.delete),
                                  tooltip: 'Obriši',
                                  onPressed: () => _deleteCity(city),
                                ),
                              ],
                            ),
                          ),
                        );
                      },
                    ),
          _isLoadingCountries
              ? const Center(child: CircularProgressIndicator())
              : _countries.isEmpty
                  ? const Center(child: Text('Nema država.'))
                  : ListView.builder(
                      itemCount: _countries.length,
                      itemBuilder: (context, i) {
                        final country = _countries[i];
                        return Card(
                          margin: const EdgeInsets.symmetric(
                              horizontal: 12, vertical: 4),
                          child: ListTile(
                            title: Text(country.name),
                            trailing: Row(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                IconButton(
                                  icon: const Icon(Icons.edit),
                                  tooltip: 'Uredi',
                                  onPressed: () =>
                                      _openCountryForm(country: country),
                                ),
                                IconButton(
                                  icon: const Icon(Icons.delete),
                                  tooltip: 'Obriši',
                                  onPressed: () => _deleteCountry(country),
                                ),
                              ],
                            ),
                          ),
                        );
                      },
                    ),
        ],
      ),
    );
  }
}
