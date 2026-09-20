const form = document.querySelector("form");
let numericMessageId = 0;

form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const messageBoxElement = document.getElementById("messageBox");
    const message = messageBoxElement.value;
    const messageIds = createMessageIds();
    displayMessageToSend(message, messageIds);
    sendMessage(message, messageIds);
    messageBoxElement.value = "";
})

async function sendMessage(message, messageIds) {
    const requestOptions = {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        mode: "cors",
        body: JSON.stringify(message)
    }
    const messageStatusElement = document.querySelector(`[data-hook=${messageIds.messageStatusId}]`);
    
    await fetch("http://localhost:8080/sendmessage", requestOptions)
    .then((response) => {
        if (response.status === 200) {
            messageStatusElement.innerText = "Sent";
            messageStatusElement.setAttribute("class", "senderMessageStatus");
            return;
        }
    
        addFailedMessageLink(messageStatusElement, messageIds);
    })
    .catch((error) => {
        addFailedMessageLink(messageStatusElement, messageIds);
    })

}

function createMessageIds() {
    const messageIdsObject = new MessageIds(`message-${numericMessageId}`, `status-${numericMessageId}`);
    numericMessageId++;
    return messageIdsObject;
}

function displayMessageToSend(message, messageIds) {
    const conversationContainer = document.getElementById("conversationContainer");
    const messageContainerElement = document.createElement("div")
    messageContainerElement.setAttribute("data-hook", messageIds.messageId);
    const messageValueElement = document.createElement("p")
    const initialMessageStatusElement = document.createElement("p");

    messageContainerElement.setAttribute("class", "senderMessageElement");
    initialMessageStatusElement.setAttribute("class", "senderMessageStatus");
    initialMessageStatusElement.setAttribute("data-hook", messageIds.messageStatusId);

    messageValueElement.innerText = message;
    initialMessageStatusElement.innerText = "Sending..."

    messageContainerElement.appendChild(messageValueElement);
    messageContainerElement.appendChild(initialMessageStatusElement);
    conversationContainer.appendChild(messageContainerElement);

    conversationContainer.insertBefore(messageContainerElement, conversationContainer.firstChild);
}

function addFailedMessageLink(messageStatusElement, messageIds) {
    messageStatusElement.innerText = "";
    const messageFailedLink = document.createElement("a");
    messageFailedLink.setAttribute("href", "javascript:;");
    messageFailedLink.setAttribute("onclick", `retrySendMessage(event, \"${messageIds.messageStatusId}\", \"${messageIds.messageId}\")`);
    messageFailedLink.innerText = "Failed, please try again";
    messageFailedLink.setAttribute("class", "senderMessageFailedLink");
    messageStatusElement.appendChild(messageFailedLink);
}

async function retrySendMessage(event, messageStatusId, messageId) {
    const messageStatusElement = document.querySelector(`[data-hook='${messageStatusId}']`)
    messageStatusElement.removeChild(event.target);
    messageStatusElement.innerText = "Retrying...";
    messageStatusElement.setAttribute("class", "senderMessageStatus");

    const messageElement = document.querySelector(`[data-hook='${messageId}']`);
    const messageValueElement = messageElement.children[0];
    const messageValue = messageValueElement.innerText;

    const requestOptions = {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        mode: "cors",
        body: JSON.stringify(messageValue)
    }
    
    await fetch("http://localhost:8080/sendmessage", requestOptions)
    .then((response) => {
        if (response.status === 200) {
            messageStatusElement.innerText = "Sent";
            messageStatusElement.setAttribute("class", "senderMessageStatus");
            return;
        }
    
        addFailedMessageLink(messageStatusElement, new MessageIds(messageId, messageStatusId));
    })
    .catch((error) => {
        addFailedMessageLink(messageStatusElement, new MessageIds(messageId, messageStatusId));
    })

}

class MessageIds {
    constructor (messageId, messageStatusId) {
        this.messageId = messageId;
        this.messageStatusId = messageStatusId;
    }
}