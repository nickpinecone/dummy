<script>
    import * as signalR from "@microsoft/signalr";

    let hub = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:8090/signal", {
            transport: signalR.HttpTransportType.ServerSentEvents,
            withCredentials: false,
        })
        .build();

    hub.on("Recording", () => {
        status.innerText = "Слушает";
        image.src = "./listening.jpg";
    });

    hub.on("Thinking", () => {
        status.innerText = "Думает...";
        image.src = "./thinking.jpg";
    });

    hub.on("Ready", () => {
        status.innerText = "Готов!";
        image.src = "./ready.jpg";
    });

    hub.on("Speaking", (result) => {
        status.innerText = "Говорит";
        answer.innerText = result;
        image.src = "./speaking.jpg";
    });

    hub.on("Done", () => {
        status.innerText = "Ждет вопроса";
        answer.innerText = "";
        image.src = "./waiting.jpg";
    });

    hub.start();

    let status;
    let answer;
    let image;
</script>

<main>
    <h1 bind:this={status}></h1>
    <div bind:this={answer}></div>
    <img bind:this={image} src="" alt="" />
</main>

<style>
    div {
        position: relative;
        z-index: 1;
    }

    img {
        width: auto;
        height: 600px;
        position: fixed;
        bottom: 0;
        z-index: 0;
    }
</style>
