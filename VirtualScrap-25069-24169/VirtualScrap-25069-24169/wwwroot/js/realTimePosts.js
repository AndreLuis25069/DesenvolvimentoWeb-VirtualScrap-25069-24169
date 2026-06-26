//Guardar o id do post que foi carregado no html
const scriptTag = document.querySelector('script[data-post-id]');
const postId = scriptTag ? parseInt(scriptTag.getAttribute('data-post-id'),10) :0 ;

//Carrega o id do utilizador que está logado, caso não esteja logado, o valor será 0 e carrega a condição se o utilizador é admin ou não, caso não esteja logado, o valor será false 
const currentUserId = scriptTag ? parseInt(scriptTag.getAttribute('data-user-id'), 10) : 0;
const isAdmin = scriptTag ? scriptTag.getAttribute('data-is-admin') === "true" : false;
//Criar a ligação ao signalR
const postHubConnection = new signalR.HubConnectionBuilder()
    .withUrl("/signalRHub")
    .build();

//Metodo que vai ser chamado quando houver uma atualização no post
postHubConnection.on("ReceivePostUpdate", function (autorNome, fotoAutor, texto, data, comentarioId) {

    //Lista de comentarios
    const list = document.querySelector(".comments-list");

    //Se ainda não houver comentários, limpar o container
    if (list.innerHTML.includes("💡")) {
        list.innerHTML = "";
    }
    //Guarda a foto do autor do comentário, caso não exista, usar uma foto padrão
    const fotoSrc = fotoAutor && fotoAutor !== "noImage.jpg"
        ? `/images1/${fotoAutor}`
        : '/defaultProfileImage/No-profile-photo.jpg';


    

    //Criar o html do comentário
    const htmlComentario = `
    <div class="d-flex gap-2 align-items-start animate__animated animate__fadeIn mb-2">
        <div class="rounded-circle overflow-hidden bg-dark text-white d-flex align-items-center justify-content-center flex-shrink-0 shadow-sm" style="width: 28px; height: 28px;">
            <img src="${fotoSrc}" class="w-100 h-100 object-fit-cover" alt="Autor" />
        </div>
        <div class="bg-light p-2 rounded-3 w-100" style="border-top-left-radius: 0px !important;">
            <div class="d-flex justify-content-between align-items-center mb-0.5">
                <span class="text-dark fw-bold" style="font-size: 0.78rem;">${autorNome}</span>
                <div class="d-flex align-items-center gap-2">
                    <span class="text-muted" style="font-size: 0.6rem;">🕒 ${data}</span>
                </div>
            </div>
            <p id="texto-${comentarioId}" class="text-secondary m-0" style="font-size: 0.78rem; white-space: pre-line;">${texto}</p>
        </div>
    </div>
`;
         
    

    list.insertAdjacentHTML("afterbegin", htmlComentario);
});

//Segunda conexão para receber likes do post em tempo real

postHubConnection.on("ReceivePostLikeUpdate", function (totalLikes) {
    const contadorLikes = document.querySelector("form[asp-page-handler='Like'] span.fw-bold, .small.text-muted.fw-bold");
    if (contadorLikes) {
        contadorLikes.textContent = totalLikes;
    }
});

//Conexão para Alterar o comentario em tempo real a quando de uma alteração
postHubConnection.on("ReceivePostEdit", function (comentarioId, novoTexto) {
    const paragrafoTexto = document.getElementById(`texto-${comentarioId}`);
    if (paragrafoTexto) {
        paragrafoTexto.textContent = novoTexto;
        paragrafoTexto.classList.add("animate__animated", "animate__flash");
        setTimeout(() => paragrafoTexto.classList.remove("animate__animated", "animate__flash"), 1000);
    }
});


//Conexão para deixar de mostrar um comentário quando foi removido
postHubConnection.on("ReceiveCommentDelete", function (comentarioId) {
    const paragrafoTexto = document.getElementById(`texto-${comentarioId}`)
    if (paragrafoTexto) {
        const blocoComentario = paragrafoTexto.closest(".d-flex.gap-2.align-items-start");
        if (blocoComentario) {
            blocoComentario.classList.add("animate__animated", "animate__fadeOutUp");
            setTimeout(() => {
                blocoComentario.remove();
                const lista = document.querySelector(".comments-list");
                if (lista && lista.children.lenght === 0) {
                    lista.innerHTML = '<div class="text-center py-2 text-muted"><span class="d-block" style="font-size: 0.75rem;">💡 Sê o primeiro a comentar!</span></div>';
                }
            }, 500);
        }
    }
});

//Arrancar o Signal R e entrar no grupo do post
postHubConnection.start()
    .then(() => {
        postHubConnection.invoke("JoinPostGroup", postId)
    })
    .catch(err => console.error("Erro em ligar ao signal R",err));
