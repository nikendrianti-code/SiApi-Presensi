1..10 | ForEach-Object -Parallel {
    Invoke-RestMethod -Uri "http://localhost:5000/api/presensi" -Method Post `
        -ContentType "application/json" `
        -Body '{"nimMahasiswa":"220001","jadwalKuliahId":1,"latitude":-6.200000,"longitude":106.816666}'
}1..10 | ForEach-Object -Parallel {
    Invoke-RestMethod -Uri "http://localhost:5000/api/presensi" -Method Post `
        -ContentType "application/json" `
        -Body '{"nimMahasiswa":"220001","jadwalKuliahId":1,"latitude":-6.200000,"longitude":106.816666}'
}