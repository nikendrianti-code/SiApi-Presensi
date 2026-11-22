$jobs = 1..10 | ForEach-Object {
    Start-Job -ScriptBlock {
        try {
            $res = Invoke-RestMethod -Uri "http://localhost:5048/api/Presensi/submit" -Method Post `
                -ContentType "application/json" `
                -Body '{"nimMahasiswa":"220004","jadwalKuliahId":1,"latitude":-7.4245,"longitude":109.2291,"keterangan":"Hadir"}'
            "SUKSES: $($res.message)"
        } catch {
            "DITOLAK: $($_.ErrorDetails.Message)"
        }
    }
}
$jobs | Wait-Job | Receive-Job
$jobs | Remove-Job