namespace SiApi.DTOs;

public record PresensiRequestDto(
    string NimMahasiswa,
    int JadwalKuliahId,
    double Latitude,
    double Longitude,
    string? Keterangan = null
);

public record PresensiResponseDto(
    bool Sukses,
    string Pesan,
    string? Status,
    double JarakMeter,
    DateTime WaktuServer
);