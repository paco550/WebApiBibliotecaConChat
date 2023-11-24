using WebApiBiblioteca.Classes;

namespace WebApiBiblioteca.Hubs
{
    // Definición de la interfaz IChat que especifica los métodos que pueden ser
    // llamados desde el servidor hacia el cliente
    public interface IChat
    {
        // Método para enviar mensajes de chat al cliente
        Task GetMessage(Message message);

        // Método para enviar la lista de usuarios conectados al cliente
        Task GetUsers(List<Connection> connections);
    }
}
