import 'dart:typed_data';
import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import '../models/city.dart';
import '../models/hotel.dart';
import '../services/api_service.dart';
import '../services/city_service.dart';
import '../utils/api_response.dart';
import '../utils/validation_utils.dart';
import '../utils/image_utils.dart';
import 'app_dialog_title.dart';
import 'city_form.dart';

class HotelFormDialog extends StatefulWidget {
  final Hotel? hotel;
  const HotelFormDialog({super.key, this.hotel});

  @override
  State<HotelFormDialog> createState() => _HotelFormDialogState();
}

class _HotelFormDialogState extends State<HotelFormDialog> {
  final _formKey = GlobalKey<FormState>();
  final _cityService = CityService();
  late int id;
  late String name;
  late String address;
  int? cityId;
  late String phoneNumber;
  late String email;
  late String description;
  late int starRating;
  String? _currentImageUrl;
  PlatformFile? _selectedImage;
  // file_picker v12+: sadržaj fajla se čita async preko readAsBytes()
  // (sinhroni PlatformFile.bytes getter je uklonjen), pa ga čuvamo posebno.
  Uint8List? _selectedImageBytes;
  bool _removeExistingImage = false;
  bool isLoading = false;
  bool _loadingCities = true;
  String? error;
  List<City> _cities = [];

  @override
  void initState() {
    super.initState();
    final h = widget.hotel;
    id = h?.id ?? 0;
    name = h?.name ?? '';
    address = h?.address ?? '';
    cityId = h != null && h.cityId > 0 ? h.cityId : null;
    phoneNumber = h?.phoneNumber ?? '';
    email = h?.email ?? '';
    description = h?.description ?? '';
    starRating = h?.starRating ?? 1;
    _currentImageUrl = h?.imageUrl;
    _fetchCities();
  }

  Future<void> _fetchCities() async {
    try {
      final cities = await _cityService.getAllForDropdown();
      if (mounted) {
        setState(() {
          _cities = cities;
          _loadingCities = false;
        });
      }
    } catch (_) {
      if (mounted) setState(() => _loadingCities = false);
    }
  }

  // Inline dodavanje grada bez napuštanja forme za hotel (RSII uputa: FK
  // objekat treba biti moguće dodati kroz modal, ne napuštanjem toka).
  Future<void> _addCityInline() async {
    final result = await showDialog<bool>(
      context: context,
      builder: (_) => const CityFormDialog(),
    );
    if (result == true) {
      final cities = await _cityService.getAllForDropdown();
      if (!mounted) return;
      setState(() {
        _cities = cities;
        // Novododani grad je obično posljednji u listi po Id-u — odaberi ga.
        if (cities.isNotEmpty) {
          cityId = cities
              .reduce((a, b) => a.id > b.id ? a : b)
              .id;
        }
      });
    }
  }

  Future<void> _pickImage() async {
    // pickFile() (jednina) vraća Future<PlatformFile?> direktno — jednostavnije
    // od pickFiles() (koje od v12 vraća Future<List<PlatformFile>>, ne
    // FilePickerResult sa .files getterom kao ranije) jer nama treba samo 1 fajl.
    final picked = await FilePicker.pickFile(type: FileType.image);
    if (picked == null) return;
    final bytes = await picked.readAsBytes();
    setState(() {
      _selectedImage = picked;
      _selectedImageBytes = bytes;
      _removeExistingImage = false;
    });
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (cityId == null) {
      setState(() => error = 'Odaberite grad.');
      return;
    }
    setState(() {
      isLoading = true;
      error = null;
    });

    final body = {
      'id': id,
      'name': name,
      'address': address,
      'cityId': cityId,
      'phoneNumber': phoneNumber,
      'email': email,
      'description': description,
      'starRating': starRating,
      'imageUrl': _removeExistingImage ? '' : (_currentImageUrl ?? ''),
    };

    try {
      int hotelId;
      if (widget.hotel == null) {
        final response = await ApiService().post('/api/hotels', body);
        final created = ApiResponseParser.parseObject(response, Hotel.fromJson);
        hotelId = created.id;
      } else {
        hotelId = widget.hotel!.id;
        await ApiService().put('/api/hotels/$hotelId', body);
      }

      if (_removeExistingImage) {
        await ApiService().delete('/api/hotels/$hotelId/image');
      } else if (_selectedImage != null && _selectedImageBytes != null) {
        await ApiService().uploadHotelImage(
            hotelId, _selectedImage!.name, _selectedImageBytes!);
      }

      if (mounted) Navigator.pop(context, true);
    } catch (e) {
      setState(() {
        error = e.toString();
      });
    }
    setState(() {
      isLoading = false;
    });
  }

  Widget _buildImagePreview() {
    if (_selectedImageBytes != null) {
      return Image.memory(
        _selectedImageBytes!,
        width: 160,
        height: 100,
        fit: BoxFit.cover,
      );
    }

    if (!_removeExistingImage && (_currentImageUrl?.isNotEmpty ?? false)) {
      return Image.network(
        resolveImageUrl(_currentImageUrl!),
        width: 160,
        height: 100,
        fit: BoxFit.cover,
        errorBuilder: (_, __, ___) => const Icon(Icons.broken_image, size: 48),
      );
    }

    return Container(
      width: 160,
      height: 100,
      color: Colors.grey.shade200,
      child: const Icon(Icons.photo, size: 48),
    );
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: AppDialogTitle(widget.hotel == null ? 'Dodaj hotel' : 'Uredi hotel'),
      content: SingleChildScrollView(
        child: Form(
          key: _formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                initialValue: name,
                decoration: const InputDecoration(labelText: 'Naziv'),
                onChanged: (v) => name = v,
                validator: ValidationUtils.validateHotelName,
              ),
              TextFormField(
                initialValue: address,
                decoration: const InputDecoration(labelText: 'Adresa'),
                onChanged: (v) => address = v,
                validator: ValidationUtils.validateAddress,
              ),
              _loadingCities
                  ? const Padding(
                      padding: EdgeInsets.symmetric(vertical: 8),
                      child: LinearProgressIndicator(),
                    )
                  : Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Expanded(
                          child: DropdownButtonFormField<int>(
                            value: _cities.any((c) => c.id == cityId)
                                ? cityId
                                : null,
                            decoration:
                                const InputDecoration(labelText: 'Grad'),
                            items: _cities
                                .map((c) => DropdownMenuItem<int>(
                                      value: c.id,
                                      child: Text(c.label),
                                    ))
                                .toList(),
                            onChanged: (v) => setState(() => cityId = v),
                            validator: (v) =>
                                v == null ? 'Odaberite grad.' : null,
                          ),
                        ),
                        IconButton(
                          icon: const Icon(Icons.add_circle_outline),
                          tooltip: 'Dodaj novi grad',
                          onPressed: isLoading ? null : _addCityInline,
                        ),
                      ],
                    ),
              TextFormField(
                initialValue: phoneNumber,
                decoration: const InputDecoration(labelText: 'Telefon'),
                onChanged: (v) => phoneNumber = v,
                validator: ValidationUtils.validatePhoneNumber,
              ),
              TextFormField(
                initialValue: email,
                decoration: const InputDecoration(labelText: 'Email'),
                onChanged: (v) => email = v,
                validator: ValidationUtils.validateEmail,
              ),
              TextFormField(
                initialValue: description,
                decoration: const InputDecoration(labelText: 'Opis'),
                maxLines: 3,
                onChanged: (v) => description = v,
                validator: ValidationUtils.validateDescription,
              ),
              const SizedBox(height: 12),
              const Align(
                alignment: Alignment.centerLeft,
                child: Text('Slika hotela',
                    style: TextStyle(fontWeight: FontWeight.w600)),
              ),
              const SizedBox(height: 8),
              _buildImagePreview(),
              const SizedBox(height: 8),
              Row(
                children: [
                  ElevatedButton.icon(
                    onPressed: isLoading ? null : _pickImage,
                    icon: const Icon(Icons.upload_file),
                    label: const Text('Odaberi sliku'),
                  ),
                  const SizedBox(width: 8),
                  if ((_currentImageUrl?.isNotEmpty ?? false) ||
                      _selectedImage != null)
                    TextButton(
                      onPressed: isLoading
                          ? null
                          : () => setState(() {
                                _selectedImage = null;
                                _selectedImageBytes = null;
                                _removeExistingImage = true;
                              }),
                      child: const Text('Ukloni sliku'),
                    ),
                ],
              ),
              if (error != null)
                Padding(
                  padding: const EdgeInsets.only(top: 8.0),
                  child:
                      Text(error!, style: const TextStyle(color: Colors.red)),
                ),
            ],
          ),
        ),
      ),
      actions: [
        TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Otkaži')),
        ElevatedButton(
          onPressed: isLoading ? null : _submit,
          child: isLoading
              ? const SizedBox(
                  width: 20,
                  height: 20,
                  child: CircularProgressIndicator(strokeWidth: 2))
              : Text(widget.hotel == null ? 'Dodaj' : 'Spasi'),
        ),
      ],
    );
  }
}
