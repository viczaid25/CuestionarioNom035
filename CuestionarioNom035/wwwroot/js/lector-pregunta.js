class LectorPregunta {
    #leyendo = false;
    #btn;
    #textoPreguntaId;
    #opcionesSelector;

    constructor({ btnId, textoPreguntaId, opcionesSelector }) {
        this.#btn = document.getElementById(btnId);
        this.#textoPreguntaId = textoPreguntaId;
        this.#opcionesSelector = opcionesSelector;

        this.#btn.addEventListener('click', () => this.toggle());
        window.addEventListener('pagehide', () => this.detener());
    }

    toggle() {
        this.#leyendo ? this.detener() : this.#iniciar();
    }

    detener() {
        window.speechSynthesis?.cancel();
        this.#leyendo = false;
        this.#btn.classList.remove('hablando');
        document.querySelectorAll(`${this.#opcionesSelector}.leyendo`)
            .forEach(el => el.classList.remove('leyendo'));

        // Volver al ícono de sonido
        document.getElementById('iconoSonido').style.display = '';
        document.getElementById('iconoPausa').style.display = 'none';
    }

    #iniciar() {
        if (!window.speechSynthesis) {
            alert('Tu navegador no soporta síntesis de voz. Usa Chrome o Edge.');
            return;
        }

        this.#leyendo = true;
        this.#btn.classList.add('hablando');

        // Cambiar al ícono de pausa
        document.getElementById('iconoSonido').style.display = 'none';
        document.getElementById('iconoPausa').style.display = '';

        const { texto, iniciosOpciones } = this.#construirTexto();
        const labels = document.querySelectorAll(this.#opcionesSelector);
        const utt = this.#crearUtterance(texto, iniciosOpciones, labels);

        window.speechSynthesis.speak(utt);
    }

    #construirTexto() {
        const pregunta = document.getElementById(this.#textoPreguntaId).textContent.trim();
        const opciones = Array.from(
            document.querySelectorAll(`${this.#opcionesSelector} span`)
        ).map((el, i) => `Opción ${i + 1}: ${el.textContent.trim()}`);

        let texto = pregunta + '. ';
        const iniciosOpciones = [];

        opciones.forEach((op, i) => {
            iniciosOpciones.push(texto.length);
            texto += op + (i < opciones.length - 1 ? '. ' : '.');
        });

        return { texto, iniciosOpciones };
    }

    #crearUtterance(texto, iniciosOpciones, labels) {
        const utt = new SpeechSynthesisUtterance(texto);
        utt.lang = 'es-MX';
        utt.rate = 0.9;
        utt.pitch = 1;

        // Forzar voz en español
        const voz = this.#seleccionarVoz();
        if (voz) utt.voice = voz;

        let opcionActual = -1;

        utt.onboundary = ({ name, charIndex }) => {
            if (name !== 'word') return;

            const nueva = [...iniciosOpciones]
                .reverse()
                .findIndex(inicio => charIndex >= inicio);
            const indice = nueva === -1 ? -1 : iniciosOpciones.length - 1 - nueva;

            if (indice !== opcionActual) {
                labels.forEach(l => l.classList.remove('leyendo'));
                if (indice >= 0) labels[indice].classList.add('leyendo');
                opcionActual = indice;
            }
        };

        utt.onend = () => this.detener();
        utt.onerror = () => this.detener();

        return utt;
    }

    #seleccionarVoz() {
        const voces = window.speechSynthesis.getVoices();

        // Prioridad: es-MX → es-419 → cualquier es-*
        return (
            voces.find(v => v.lang === 'es-MX') ??
            voces.find(v => v.lang === 'es-419') ??
            voces.find(v => v.lang.startsWith('es'))
        );
    }
}