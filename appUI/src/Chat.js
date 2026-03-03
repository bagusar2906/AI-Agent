import React, { useState, useRef, useEffect } from "react";
import ReactMarkdown from "react-markdown";
import "./Chat.css";

function Chat() {
  const [messages, setMessages] = useState(() => {
    //const saved = localStorage.getItem("chatHistory");
    const saved = null; // Disable localStorage for now
    return saved ? JSON.parse(saved) : [];
  });

  const [input, setInput] = useState("");
  const [isTyping, setIsTyping] = useState(false);
  const [darkMode, setDarkMode] = useState(true);
  const chatEndRef = useRef(null);

  useEffect(() => {
    localStorage.setItem("chatHistory", JSON.stringify(messages));
    chatEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const sendMessage = async () => {
    if (!input.trim()) return;

    const userInput = input;
    const userMessage = { role: "user", content: userInput };

    setMessages(prev => [...prev, userMessage]);
    setInput("");

    // Add assistant "thinking" bubble
    setMessages(prev => [
      ...prev,
      { role: "assistant", content: "__THINKING__" }
    ]);

    try {
      const response = await fetch(
        `http://localhost:5000/chat/stream?message=${encodeURIComponent(userInput)}`
      );

      const reader = response.body.getReader();
      const decoder = new TextDecoder();

      let firstChunk = true;

      while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        const decodedChunk = decoder.decode(value);

        setMessages(prev => {
          const updated = [...prev];
          const lastIndex = updated.length - 1;

          if (firstChunk) {
            // Replace Thinking...
            updated[lastIndex].content = decodedChunk;
            firstChunk = false;
          } else {
            updated[lastIndex].content += decodedChunk;
          }

          return updated;
        });
      }
    } catch (error) {
      setMessages(prev => {
        const updated = [...prev];
        updated[updated.length - 1].content =
          "⚠️ Error connecting to server.";
        return updated;
      });
    }
  };

  const renderMessageContent = (content) => {
    // Thinking bubble
    if (content === "__THINKING__") {
      return (
        <div className="thinking-bubble">
          Thinking
          <span className="dot">.</span>
          <span className="dot">.</span>
          <span className="dot">.</span>
        </div>
      );
    }

    try {
      const parsed = JSON.parse(content);

      if (parsed?.success === true && parsed?.data) {
        return (
          <pre className="json-block">
            <code>
              {JSON.stringify(parsed.data, null, 2)}
            </code>
          </pre>
        );
      }
    } catch {}

    return <ReactMarkdown>{content}</ReactMarkdown>;
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
            {renderMessageContent(msg.content)}
          </div>
        ))}

        {isTyping && (
          <div className="typing">
            <span></span><span></span><span></span>
          </div>
        )}

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