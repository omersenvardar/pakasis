document.addEventListener("DOMContentLoaded", function () {
    function execCmd(command, value = null) {
        document.execCommand(command, false, value);
    }

    // Araç çubuğundaki butonların formu tetiklemesini engelle
    document.querySelectorAll(".custom-toolbar button").forEach(button => {
        button.addEventListener("click", function (event) {
            event.preventDefault();
            execCmd(this.dataset.cmd, this.value || null);
        });
    });

    // Seçim elemanları için event ekle
    document.querySelectorAll(".custom-toolbar select, .custom-toolbar .color-picker").forEach(element => {
        element.addEventListener("change", function (event) {
            event.preventDefault();
            execCmd(this.dataset.cmd, this.value);
        });
    });

    // Form gönderildiğinde editör içeriğini textarea'ya aktar
    const form = document.querySelector("form");
    if (form) {
        form.addEventListener("submit", function () {
            document.getElementById("customEditorContent").value = document.getElementById("customEditor").innerHTML;
        });
    }
});

document.addEventListener("DOMContentLoaded", function () {
    function execCmd(command, value = null) {
        document.execCommand(command, false, value);
    }

    // Renk butonlarını seç
    document.querySelectorAll(".color-picker").forEach(colorBtn => {
        colorBtn.addEventListener("click", function () {
            let color = this.classList.contains("color-black") ? "black" :
                        this.classList.contains("color-red") ? "red" :
                        this.classList.contains("color-blue") ? "blue" :
                        this.classList.contains("color-green") ? "green" :
                        this.classList.contains("color-orange") ? "orange" : "black";

            execCmd("foreColor", color);

            // Aktif rengi belirle
            document.querySelectorAll(".color-picker").forEach(btn => btn.classList.remove("active"));
            this.classList.add("active");
        });
    });

    // Form submit olduğunda içeriği textarea'ya aktar
    
    if (form) {
        form.addEventListener("submit", function () {
            document.getElementById("customEditorContent").value = document.getElementById("customEditor").innerHTML;
        });
    }
});
