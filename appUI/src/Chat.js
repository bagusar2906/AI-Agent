import React, { useState, useRef, useEffect } from "react";
import ReactMarkdown from "react-markdown";
import "./Chat.css";

function Chat() {
  const [messages, setMessages] = useState([]);
  const [input, setInput] = useState("");
  const [darkMode, setDarkMode] = useState(true);
  const chatEndRef = useRef(null);

  useEffect(() => {
    chatEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const sendMessage = async () => {
    if (!input.trim()) return;

    const userInput = input;

    // Add user message
    setMessages(prev => [
      ...prev,
      { role: "user", type: "chat", content: userInput }
    ]);

    setInput("");

    // Add thinking bubble
    setMessages(prev => [
      ...prev,
      { role: "assistant", type: "thinking", content: "" }
    ]);

    try {
      const response = await fetch(
        `http://localhost:5000/chat/stream?message=${encodeURIComponent(userInput)}`
      );

      const reader = response.body.getReader();
      const decoder = new TextDecoder();

      let fullResponse = "";

      while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        const decodedChunk = decoder.decode(value);
        fullResponse += decodedChunk;

        setMessages(prev => {
          const updated = [...prev];
          const lastIndex = updated.length - 1;
          const last = updated[lastIndex];

          // If the last message is the initial "thinking" bubble, replace it on first chunk
          if (!last || last.type === "thinking") {
            updated[lastIndex] = {
              role: "assistant",
              type: "chat",
              content: decodedChunk
            };
          } else {
            updated[lastIndex].content += decodedChunk;
          }

          return updated;
        });
      }

      // After streaming finished → detect tool JSON
      try {
        const parsed = JSON.parse(fullResponse);

        if (parsed?.success === true && parsed?.data) {
          setMessages(prev => {
            const updated = [...prev];
            updated[updated.length - 1] = {
              role: "assistant",
              type: "tool",
              content: parsed.data
            };
            return updated;
          });
        }
      } catch {
        // Not JSON → keep chat message
      }

    } catch (err) {
      setMessages(prev => {
        const updated = [...prev];
        updated[updated.length - 1] = {
          role: "assistant",
          type: "error",
          content: "⚠️ Failed to connect to server."
        };
        return updated;
      });
    }
  };

  const renderMessage = (msg) => {
    switch (msg.type) {
      case "thinking":
        return (
          <div className="thinking">
            Thinking
            <span className="dot">.</span>
            <span className="dot">.</span>
            <span className="dot">.</span>
          </div>
        );

      case "tool":
        return (
          <div className="tool-card">
            {/* <div className="tool-title">Tool Result</div> */}
            <pre>
              <code>{JSON.stringify(msg.content, null, 2)}</code>
            </pre>
          </div>
        );

      case "error":
        return <div className="error">{msg.content}</div>;

      default:
        return <ReactMarkdown>{msg.content}</ReactMarkdown>;
    }
  };

  return (
    <div className={darkMode ? "app dark" : "app light"}>
      <div className="header">
        <h2>NT8 Assistant</h2>
        <button onClick={() => setDarkMode(!darkMode)}>
          {darkMode ? "Light Mode" : "Dark Mode"}
        </button>
      </div>

      <div className="chat">
        {messages.map((msg, i) => (
          <div key={i} className={`message ${msg.role}`}>
            {renderMessage(msg)}
          </div>
        ))}
        <div ref={chatEndRef} />
      </div>

      <div className="inputArea">
        <input
          value={input}
          onChange={e => setInput(e.target.value)}
          placeholder="Ask something..."
          onKeyDown={e => e.key === "Enter" && sendMessage()}
        />
        <button onClick={sendMessage}>Send</button>
      </div>
    </div>
  );
}

export default Chat;