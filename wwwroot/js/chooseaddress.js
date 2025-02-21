document.addEventListener("DOMContentLoaded", function () {
    const adresGruplari = window.adresGruplari || {};

    const ilDropdown = document.getElementById("Il");
    const ilceDropdown = document.getElementById("Ilce");
    const mahalleDropdown = document.getElementById("Mahalle");
    const adresKonumuInput = document.getElementById("adresKonumu");
    const girilenAdres = document.getElementById("girilenAdres");
    const adresKonumuGoster = document.getElementById("adresKonumuGoster");

    console.log("✅ Adres Grupları JSON:", adresGruplari);

    // **İl seçildiğinde ilçe dropdown'ını güncelle**
    ilDropdown.addEventListener("change", function () {
        const selectedIl = ilDropdown.value;
        console.log("🟢 İl Seçildi:", selectedIl);

        ilceDropdown.innerHTML = '<option value="" disabled selected>İlçe Seçiniz</option>';
        mahalleDropdown.innerHTML = '<option value="" disabled selected>Mahalle Seçiniz</option>';
        adresKonumuInput.value = "";

        if (adresGruplari[selectedIl]) {
            Object.keys(adresGruplari[selectedIl]).forEach(ilce => {
                console.log("   📍 İlçeler:", ilce);
                const option = document.createElement("option");
                option.value = ilce;
                option.textContent = ilce;
                ilceDropdown.appendChild(option);
            });
        } else {
            console.warn("⚠ İl için ilçeler bulunamadı:", selectedIl);
        }
        updateGirilenAdres();
    });

    // **İlçe seçildiğinde mahalle dropdown'ını güncelle**
    ilceDropdown.addEventListener("change", function () {
        const selectedIl = ilDropdown.value;
        const selectedIlce = ilceDropdown.value;
        console.log("🟡 İlçe Seçildi:", selectedIlce);

        mahalleDropdown.innerHTML = '<option value="" disabled selected>Mahalle Seçiniz</option>';
        adresKonumuInput.value = "";

        if (adresGruplari[selectedIl] && adresGruplari[selectedIlce]) {
            adresGruplari[selectedIl][selectedIlce].forEach(mahalle => {
                console.log("   🏡 Mahalle JSON:", mahalle);

                if (typeof mahalle === "object" && mahalle.hasOwnProperty("MahalleKoyAdi")) {
                    const option = document.createElement("option");
                    option.value = mahalle.MahalleKoyAdi;
                    option.textContent = mahalle.MahalleKoyAdi;
                    mahalleDropdown.appendChild(option);
                } else {
                    console.error("❌ Hatalı mahalle formatı:", mahalle);
                }
            });
        } else {
            console.warn("⚠ İlçe için mahalleler bulunamadı:", selectedIlce);
        }
        updateGirilenAdres();
    });

    // **Mahalle seçildiğinde adresi güncelle**
    mahalleDropdown.addEventListener("change", updateGirilenAdres);

    function updateGirilenAdres() {
        const selectedIl = ilDropdown.value;
        const selectedIlce = ilceDropdown.value;
        const selectedMahalle = mahalleDropdown.value;

        console.log("🔵 Mahalle Seçildi:", selectedMahalle);

        // **Adres Göster**
        const adres = `${selectedIl ? selectedIl : ""} ${selectedIlce ? selectedIlce : ""} ${selectedMahalle ? selectedMahalle : ""}`.trim();
        girilenAdres.textContent = adres ? `Girilen Adres: ${adres}` : "Girilen adres burada görüntülenecek...";

        // **Adres ID'sini bul ve input'a ekle**
        const adresId = findAdresId(selectedIl, selectedIlce, selectedMahalle);
        console.log("🟣 Seçilen Adres ID:", adresId);

        adresKonumuInput.value = adresId ? adresId : "";
        adresKonumuGoster.textContent = `AdresKonumu: ${adresKonumuInput.value}`;
    }

    // **İl, İlçe ve Mahalleye göre Adres ID'yi bulma**
    function findAdresId(il, ilce, mahalle) {
        console.log("🔍 Adres ID Aranıyor → İl:", il, " | İlçe:", ilce, " | Mahalle:", mahalle);

        if (!il || !ilce || !mahalle) {
            console.log("❌ Eksik Bilgi: İl, İlçe veya Mahalle Seçilmedi!");
            return "";
        }

        for (const ilKey in adresGruplari) {
            if (ilKey === il) {
                for (const ilceKey in adresGruplari[ilKey]) {
                    if (ilceKey === ilce) {
                        const mahalleList = adresGruplari[ilKey][ilceKey];
                        for (const mahalleItem of mahalleList) {
                            if (mahalleItem.MahalleKoyAdi === mahalle) {
                                console.log("✅ Bulunan Adres ID:", mahalleItem.Id);
                                return mahalleItem.Id;
                            }
                        }
                    }
                }
            }
        }

        console.log("❌ Adres ID Bulunamadı!");
        return "";
    }
});
