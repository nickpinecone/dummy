<script>
    import * as signalR from "@microsoft/signalr";
    import { onMount } from "svelte";

    let hub = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:8085/signal", {
            transport: signalR.HttpTransportType.ServerSentEvents,
            withCredentials: false,
        })
        .build();

    const State = {
        Record: 1,
        Generate: 2,
        Ready: 3,
        Speak: 4,
        Pause: 5,
        Wait: 6,
    };

    let state = State.Wait;
    let status;
    let answerElement;
    let answer;
    let image;

    onMount(() => {
        wait();
    });

    function wait() {
        state = State.Wait;
        status.innerText = "Ждет вопроса";
        answerElement.innerText = "";
        image.src = "./wait.jpg";
    }

    async function post(url) {
        await fetch("http://localhost:8085" + url, {
            method: "Post",
        });
    }

    async function handleKeyPress(event) {
        if (state == State.Speak && event.code == "KeyQ") {
            await post("/stop");
            wait();
        }
        if (event.code != "Space") {
            return;
        }

        if (state == State.Wait) {
            await post("/record");
            state = State.Record;

            status.innerText = "Слушает";
            image.src = "./record.jpg";
        } else if (state == State.Record) {
            post("/generate");
            state = State.Generate;

            status.innerText = "Думает...";
            image.src = "./generate.jpg";
        } else if (state == State.Ready) {
            await post("/speak");
            state = State.Speak;

            status.innerText = "Говорит";
            answerElement.innerText = answer;
            image.src = "./speak.jpg";
        } else if (state == State.Speak) {
            await post("/pause");
            state = State.Pause;
        } else if (state == State.Pause) {
            await post("/speak");
            state = State.Speak;
        }
    }

    hub.on("Ready", (result) => {
        state = State.Ready;
        status.innerText = "Готов!";
        image.src = "./ready.jpg";
        answer = result;
    });

    hub.on("Wait", () => {
        wait();
    });

    hub.start();
</script>

<svelte:document on:keypress={handleKeyPress} />

<main>
    <h1 bind:this={status}></h1>
    <div bind:this={answerElement}></div>
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
