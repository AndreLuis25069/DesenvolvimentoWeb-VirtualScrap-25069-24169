const scriptTag = document.querySelector('script[data-profile-id]');
const profileUserId = scriptTag ? parseInt(scriptTag.getAttribute('data-profile-id'), 10) : 0;

const profileHubConnection = new signalR.HubConnectionBuilder()
    .withUrl("/signalRHub")
    .build()


    //Receber uma nova avaliação no perfil 
profileHubConnection.on("ReceberAvaliacaoUtilizador", function (autorNome, fotoAutor, estrelas, texto, data, autorId, avaliacaoId) {
    const lista = document.querySelector(".reviews-list");
    if (!lista) return;

    if (lista.innerHTML.includes("feedback") || lista.innerHTML.includes("feedback.")) {
        lista.innerHTML = "";
    }

    const fotoSrc = fotoAutor && fotoAutor !== "noImage.jpg" && fotoAutor !== "No-profile-photo.jpg"
        ? `/images1/${fotoAutor}`
        : '/defaultProfileImage/No-profile-photo.jpg';

    let estrelasString = "⭐".repeat(estrelas);


    const htmlAvaliacao = `
        <div id="avaliacao-block-${avaliacaoId}" class="d-flex gap-2 align-items-start animate__animated animate__fadeIn mb-3">
            <div style="width: 36px; height: 36px; flex-shrink: 0;">
                <img src="${fotoSrc}" alt="Foto de Perfil" class="rounded-circle object-fit-cover w-100 h-100" />
            </div>
            <div class="bg-light p-3 rounded-3 w-100" style="border-top-left-radius: 0px !important;">
                <div class="d-flex justify-content-between align-items-center mb-1">
                    <span class="fw-bold text-dark small">${autorNome}</span>
                    <div class="d-flex align-items-center gap-2">
                        <span class="text-muted" style="font-size: 0.65rem;">📅 ${data}</span>
                    </div>
                </div>
                <div class="collapse show">
                    <div id="estrelas-${avaliacaoId}" class="text-warning mb-1" style="font-size: 0.75rem;">${estrelasString}</div>
                    <p id="texto-avaliacao-${avaliacaoId}" class="text-secondary m-0" style="font-size: 0.85rem; white-space: pre-line;">${texto}</p>
                </div>
            </div>
        </div>
    `;
    lista.insertAdjacentHTML("afterbegin", htmlAvaliacao);
});


//Receber uma edição em alguma avaliação em tempo real
profileHubConnection.on("ReceiveProfileEdit", function (avaliacaoId, novasEstrelas, novoTexto) {
    const paragrafoTexto = document.getElementById(`texto-avaliacao-${avaliacaoId}`);
    const divEstrelas = document.getElementById(`estrelas-${avaliacaoId}`);

    if (paragrafoTexto) {
        paragrafoTexto.textContent = novoTexto;
        paragrafoTexto.classList.add("animate__animated", "animate__flash");
        setTimeout(() => paragrafoTexto.classList.remove("animate__animated", "animate__flash"), 1000);
    }
    if (divEstrelas) {
        divEstrelas.textContent = "⭐".repeat(novasEstrelas);
    }
});

//Mostrar a avaliação a desaparecer, a quando removida
profileHubConnection.on("ReceiveProfileDelete", function (avaliacaoId) {
    const bloco = document.getElementById(`avaliacao-block-${avaliacaoId}`);
    if (bloco) {
        bloco.classList.add("animate__animated", "animate__fadeOutUp");
        setTimeout(() => {
            bloco.remove();
            const lista = document.querySelector(".reviews-list");
            if (lista && lista.querySelectorAll('[id^="avaliacao-block-"]').length === 0) {
                lista.innerHTML = '<div class="text-center py-4 text-muted bg-light rounded-3"><span class="d-block small">⭐ Este utilizador ainda não tem feedback.</span></div>';
            }
        }, 500);
    }
});

profileHubConnection.start()
    .then(() => {
        if (profileUserId > 0) {
            profileHubConnection.invoke("JoinProfileGroup", profileUserId);
        }
    })
    .catch(err => console.error("Erro ao ligar ao SignalR do Perfil", err));