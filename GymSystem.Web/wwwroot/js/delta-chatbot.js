// Delta AI Floating Chatbot
document.addEventListener("DOMContentLoaded", function () {
  const toggle = document.getElementById("deltaChatToggle");
  const popup  = document.getElementById("deltaChatPopup");
  const close  = document.getElementById("deltaChatClose");
  const input  = document.getElementById("deltaChatInput");
  const send   = document.getElementById("deltaChatSend");
  const body   = document.getElementById("deltaChatBody");

  if (!toggle || !popup) return;

  function openChat() {
    popup.classList.add("open");
    toggle.classList.add("hidden");
    input.focus();
  }

  function closeChat() {
    popup.classList.remove("open");
    toggle.classList.remove("hidden");
  }

  toggle.addEventListener("click", openChat);
  close.addEventListener("click", closeChat);

  // Auto-resize textarea
  input.addEventListener("input", function () {
    this.style.height = "auto";
    this.style.height = Math.min(this.scrollHeight, 80) + "px";
  });

  // Send on Enter (Shift+Enter for newline)
  input.addEventListener("keydown", function (e) {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      sendMessage();
    }
  });

  send.addEventListener("click", sendMessage);

  function appendMessage(text, role) {
    var msg = document.createElement("div");
    msg.className = "delta-chat-msg " + role;
    msg.textContent = text;
    body.appendChild(msg);
    body.scrollTop = body.scrollHeight;
    return msg;
  }

  async function sendMessage() {
    var message = input.value.trim();
    if (!message) return;

    appendMessage(message, "user");
    input.value = "";
    input.style.height = "auto";
    send.disabled = true;

    var loadingMsg = appendMessage("DELTA is thinking...", "bot loading");

    try {
      var response = await fetch("https://localhost:7183/api/LLM/chat", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ prompt: message })
      });

      var data = await response.json();
      loadingMsg.remove();

      if (!response.ok) {
        appendMessage(data.error || "Something went wrong while contacting DELTA.", "bot error");
      } else {
        appendMessage(data.response || "Sorry, I couldn't get a response.", "bot");
      }
    } catch (error) {
      loadingMsg.remove();
      appendMessage("Something went wrong while contacting DELTA.", "bot error");
      console.error("Chatbot error:", error);
    } finally {
      send.disabled = false;
      input.focus();
    }
  }
});
