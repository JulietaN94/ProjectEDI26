const THEME_KEY = "lavadero-theme";

export function inicializarTema() {
    let tema =
        localStorage.getItem(THEME_KEY);

    if (tema !== "dark" &&
        tema !== "light") {
        tema =
            window.matchMedia?.(
                "(prefers-color-scheme: dark)")
                .matches
                ? "dark"
                : "light";
    }

    aplicarTema(
        tema,
        false);

    return tema;
}

export function establecerTema(tema) {
    aplicarTema(
        tema,
        true);
}

export function abrirDialogoProducto(dialogo, referenciaDotNet) {
    const cancelar = evento => {
        evento.preventDefault();
        referenciaDotNet.invokeMethodAsync("CerrarDesdeDialogo");
    };

    dialogo.addEventListener("cancel", cancelar);
    dialogo.showModal();
    dialogo.querySelector(".detalle-cerrar")?.focus();

    const cerrar = () => {
        if (dialogo.open)
            dialogo.close();
    };

    return {
        close: cerrar,
        dispose: () => {
            dialogo.removeEventListener("cancel", cancelar);
            cerrar();
        }
    };
}

function aplicarTema(
    tema,
    guardar) {
    const valor =
        tema === "dark"
            ? "dark"
            : "light";

    document.documentElement.dataset.theme =
        valor;

    if (document.body) {
        document.body.dataset.theme =
            valor;

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

export async function copiarAlPortapapeles(
    texto) {
    await navigator.clipboard
        .writeText(texto);
}
