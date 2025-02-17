document.addEventListener("DOMContentLoaded", function () {
    const durumFiltre = document.getElementById("durumFiltre");
    const tarihSiralama = document.getElementById("tarihSiralama");
    const fiyatSiralama = document.getElementById("fiyatSiralama");
    const ilanListesi = document.querySelector(".ilan-listesi");

    function filtreleVeSirala() {
        let seciliDurum = durumFiltre.value;
        let siralamaTarih = tarihSiralama.value;
        let siralamaFiyat = fiyatSiralama.value;

        let ilanlar = Array.from(ilanListesi.getElementsByClassName("ilan-kapsayici"));

        ilanlar.forEach(ilan => {
            let ilanDurumu = ilan.getAttribute("data-durum");
            ilan.style.display = (!seciliDurum || ilanDurumu === seciliDurum) ? "flex" : "none";
        });

        let siraliIlanlar = ilanlar.sort((a, b) => parseFloat(b.getAttribute("data-fiyat")) - parseFloat(a.getAttribute("data-fiyat")));

        ilanListesi.innerHTML = "";
        siraliIlanlar.forEach(ilan => ilanListesi.appendChild(ilan));
    }

    durumFiltre.addEventListener("change", filtreleVeSirala);
    tarihSiralama.addEventListener("change", filtreleVeSirala);
    fiyatSiralama.addEventListener("change", filtreleVeSirala);

    filtreleVeSirala();
});