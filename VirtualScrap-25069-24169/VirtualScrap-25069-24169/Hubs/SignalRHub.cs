using Microsoft.AspNetCore.SignalR;
namespace VirtualScrap_25069_24169.Hubs
{
    public class SignalRHub : Hub
    {
        //Metodo para se juntar a um grupo que está a ver o anuncio nesse devido momento.
        public async Task JoinPostGroup(int postId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Post_{postId}");
        }

        //Junta os utilizadores que estão a ver o perfil em simultaneo, em um grupo
        public async Task JoinProfileGroup(int profileUserId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Profile_{profileUserId}");
        }

        //Metodo que adiciona um novo comentario a um post
        public async Task EnviarComentarioPost(string postId, string autorNome, string fotoAutor, string texto, string data)
        {
            await Clients.Group($"Post_{postId}").SendAsync("ReceberComentarioPost", autorNome, fotoAutor, texto, data);
        }

        //Metodo que atualiza a contagem de likes/favoritos de um post em tempo real
        public async Task AtualizarLikesPost(string postId, int totalLikes)
        {
            await Clients.Group($"Post_{postId}").SendAsync("ReceberLikesPost", totalLikes);
        }

        //Metodo que envia uma nova avaliação de um utilizador para o perfil do mesmo em tempo real
        public async Task EnviarAvaliacaoUtilizador(string profileUserId, string autorNome, string fotoAutor, int estrelas, string texto, string data)
        {
            await Clients.Group($"Profile_{profileUserId}").SendAsync("ReceberAvaliacaoUtilizador", autorNome, fotoAutor, estrelas, texto, data);
        }
    }
}
