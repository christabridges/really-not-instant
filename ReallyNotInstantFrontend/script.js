const form = document.querySelector("form");
let numericMessageId = 0;

form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const messageBoxElement = document.getElementById("messageBox");
    const message = messageBoxElement.value;
    const messageId = createMessageId();
    displayMessageToSend(message, messageId);
    sendMessage(message, messageId);
    messageBoxElement.value = "";
})

async function sendMessage(message, messageId) {
    const requestOptions = {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        mode: "cors",
        body: JSON.stringify(message)
    }

    const messageElement = document.querySelector(`[data-hook='${messageId}']`);
    const messageStatusElement = messageElement.children[1];
    
    await fetch("http://localhost:8080/sendmessage", requestOptions)
    .then((response) => {
        if (response.status === 200) {
            messageStatusElement.innerText = "Sent";
            messageStatusElement.setAttribute("class", "senderMessageStatus");
            return;
        }
    
        addFailedMessageLink(messageStatusElement, messageId);
    })
    .catch((error) => {
        addFailedMessageLink(messageStatusElement, messageId);
    })

}

function createMessageId() {
    const messageId = `message-${numericMessageId}`;
    numericMessageId++;
    return messageId;
}

function displayMessageToSend(message, messageId) {
    const conversationContainer = document.getElementById("conversationContainer");
    const messageContainerElement = document.createElement("div")
    messageContainerElement.setAttribute("data-hook", messageId);
    const messageValueElement = document.createElement("p")
    const initialMessageStatusElement = document.createElement("p");

    messageContainerElement.setAttribute("class", "senderMessageElement");
    initialMessageStatusElement.setAttribute("class", "senderMessageStatus");

    messageValueElement.innerText = message;
    initialMessageStatusElement.innerText = "Sending..."

    messageContainerElement.appendChild(messageValueElement);
    messageContainerElement.appendChild(initialMessageStatusElement);
    conversationContainer.appendChild(messageContainerElement);

    conversationContainer.insertBefore(messageContainerElement, conversationContainer.firstChild);
}

function addFailedMessageLink(messageStatusElement, messageId) {
    messageStatusElement.innerText = "";
    const messageFailedLink = document.createElement("a");
    messageFailedLink.setAttribute("href", "javascript:;");
    messageFailedLink.addEventListener("click", () => retrySendMessage(messageId));
    messageFailedLink.innerText = "Failed, please try again";
    messageFailedLink.setAttribute("class", "senderMessageFailedLink");
    messageStatusElement.appendChild(messageFailedLink);
}

async function retrySendMessage(messageId) {
    const messageElement = document.querySelector(`[data-hook='${messageId}']`);
    const messageValueElement = messageElement.children[0];
    const messageValue = messageValueElement.innerText;

    const messageStatusElement = messageElement.children[1];
    messageStatusElement.removeChild(event.target);
    messageStatusElement.innerText = "Retrying...";
    messageStatusElement.setAttribute("class", "senderMessageStatus");

    sendMessage(messageValue, messageId);
}
