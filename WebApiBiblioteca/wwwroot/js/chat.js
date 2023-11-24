
// Creación de la conexión al hub SignalR
const conexion = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

// Código que muestra el mensaje en la interfaz de usuario
conexion.on("GetMessage", (message) => {
    // Crear elementos HTML para mostrar el mensaje
    const li = document.createElement("li");
    const avatar = document.createElement("img");
    const mensaje = document.createElement("span");

    // Configurar la imagen de avatar
    avatar.src = message.avatar;
    avatar.alt = "Avatar de " + message.user;
    avatar.width = 50;
    avatar.height = 50;
    avatar.classList.add("img-fluid");
    avatar.style.marginRight = "10px";
    avatar.style.borderRadius = "50%";

    // Configurar el contenido del mensaje con el nombre de usuario y el texto
    mensaje.textContent = message.user + " - " + message.text;

    // Agregar la imagen de avatar y el mensaje al elemento de lista
    li.appendChild(avatar);
    li.appendChild(mensaje);

    // Agregar el elemento de lista a la lista de mensajes en la interfaz de usuario
    document.getElementById("lstMensajes").appendChild(li);
});

// Código para mostrar la lista de usuarios conectados en la interfaz de usuario
conexion.on("GetUsers", (users) => {
    // Imprimir la lista de usuarios en la consola (solo para propósitos de depuración)
    console.log(users)

    // Limpiar la lista de usuarios en la interfaz de usuario
    document.getElementById("lstUsuarios").innerHTML = "";

    // Iterar a través de la lista de usuarios recibida del servidor
    users.forEach(x => {
        // Crear elementos HTML para mostrar la información del usuario
        const avatar = document.createElement("img");
        const usuario = document.createElement("span");

        // Configurar atributos y estilos de la imagen de avatar
        avatar.src = x.avatar;
        avatar.alt = "Avatar de " + x.user;
        avatar.width = 50;
        avatar.height = 50;
        avatar.classList.add("img-fluid");
        avatar.style.marginRight = "10px";
        avatar.style.borderRadius = "50%";

        // Configurar el contenido del elemento de texto del usuario
        usuario.textContent = x.user;

        // Crear un elemento de lista y agregar la imagen y el texto del usuario
        const li = document.createElement("li");
        li.appendChild(avatar);
        li.appendChild(usuario);

        // Imprimir la lista de usuarios en la consola (solo para propósitos de depuración)
        console.log(li)

        // Agregar el elemento de lista a la lista de usuarios en la interfaz de usuario
        document.getElementById("lstUsuarios").appendChild(li);
    })
})

// Evento de conexión
//conexion.start().then(() => {
//    const li = document.createElement("li");
//    li.textContent = "Bienvenido al chat";
//    document.getElementById("lstMensajes").appendChild(li);
//}).catch((error) => {
//    console.error(error);
//});

//manejo de eventos del cliente Se agregan varios event listeners para manejar eventos 
//del usuario, como la introducción de texto de usuario, mensajes, 
//y el clic en los botones de conectar y enviar.
document.getElementById("txtUsuario").addEventListener("input", (event) => {
    document.getElementById("btnConectar").disabled = event.target.value === "";
});

document.getElementById("txtMensaje").addEventListener("input", (event) => {
    document.getElementById("btnEnviar").disabled = event.target.value === "";
});

// Código para conectar y desconectar el cliente
document.getElementById("btnConectar").addEventListener("click", (event) => {
    // Verificar si la conexión está desconectada
    if (conexion.state === signalR.HubConnectionState.Disconnected) {
        // Iniciar la conexión con el servidor SignalR
        conexion.start().then(() => {
            // Mostrar mensaje de conexión en la interfaz de usuario
            const li = document.createElement("li");
            li.textContent = "Conectado con el servidor en tiempo real";
            document.getElementById("lstMensajes").appendChild(li);

            // Actualizar la interfaz de usuario y deshabilitar campos
            document.getElementById("btnConectar").textContent = "Desconectar";
            document.getElementById("txtUsuario").disabled = true;
            document.getElementById("txtAvatar").disabled = true;
            document.getElementById("txtMensaje").disabled = false;
            document.getElementById("btnEnviar").disabled = false;

            // Obtener información de usuario y construir un mensaje vacío
            const usuario = document.getElementById("txtUsuario").value;
            const avatar = document.getElementById("txtAvatar").value;
            const message = {
                user: usuario,
                avatar: avatar,
                text: ""
            }

            // Enviar un mensaje al servidor indicando que el usuario se ha conectado
            conexion.invoke("SendMessage", message).catch(function (error) {
                console.error(error);
            });

        }).catch(function (error) {
            console.error(error);
        });
    }
    // Verificar si la conexión está conectada
    else if (conexion.state === signalR.HubConnectionState.Connected) {
        // Detener la conexión con el servidor SignalR
        conexion.stop();

        // Limpiar las listas de usuarios y mensajes en la interfaz de usuario
        document.getElementById("lstUsuarios").innerHTML = "";
        document.getElementById("lstMensajes").innerHTML = "";

        // Mostrar mensaje de salida en la interfaz de usuario
        const li = document.createElement("li");
        li.textContent = "Has salido del chat";
        document.getElementById("lstMensajes").appendChild(li);

        // Actualizar la interfaz de usuario y habilitar campos
        document.getElementById("btnConectar").textContent = "Conectar";
        document.getElementById("txtUsuario").disabled = false;
        document.getElementById("txtAvatar").disabled = false;
        document.getElementById("txtMensaje").disabled = true;
        document.getElementById("btnEnviar").disabled = true;
    }
});

//Código para enviar un mensaje al hub Cuando el usuario hace clic en el botón 
//"Enviar", se recopila la información del usuario y el mensaje, y se invoca 
//el método del hub llamado "SendMessage" para enviar el mensaje al servidor.
document.getElementById("btnEnviar").addEventListener("click", (event) => {
    const usuario = document.getElementById("txtUsuario").value;
    const texto = document.getElementById("txtMensaje").value;
    const avatar = document.getElementById("txtAvatar").value;
    const data = {
        user: usuario,
        text: texto,
        avatar: avatar
    }

    // invoke nos va a comunicar con el hub y el evento para pasarle el mensaje
    conexion.invoke("SendMessage", data).catch((error) => {
        console.error(error);
    });
    document.getElementById("txtMensaje").value = "";
    document.getElementById("btnEnviar").disabled = true;
    event.preventDefault(); // Para evitar el submit
})