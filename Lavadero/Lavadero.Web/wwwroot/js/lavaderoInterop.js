const THEME_KEY = "lavadero-theme";

export function inicializarTema() {
    let tema = localStorage.getItem(THEME_KEY);

    if (tema !== "dark" && tema !== "light") {
        tema = window.matchMedia?.("(prefers-color-scheme: dark)").matches
            ? "dark"
            : "light";
    }

    aplicarTema(tema, false);
    return tema;
}

export function establecerTema(tema) {
    aplicarTema(tema, true);
}

function aplicarTema(tema, guardar) {
    const valor = tema === "dark"
        ? "dark"
        : "light";

    document.documentElement.dataset.theme = valor;

    if (document.body) {
        document.body.dataset.theme = valor;
        document.body.classList.toggle(
            "dark-mode",
            valor === "dark");
    }

    if (guardar) {
        localStorage.setItem(
            THEME_KEY,
            valor);
    }
}

export async function copiarAlPortapapeles(texto) {
    await navigator.clipboard.writeText(texto);
}

export function abrirWhatsApp(numero, mensaje) {
    const telefono =
        (numero ?? "").replace(/\D/g, "");

    if (!telefono) {
        throw new Error(
            "No hay un número de WhatsApp configurado.");
    }

    const url =
        `https://wa.me/${telefono}?text=${encodeURIComponent(mensaje ?? "")}`;

    window.open(
        url,
        "_blank",
        "noopener,noreferrer");
}
