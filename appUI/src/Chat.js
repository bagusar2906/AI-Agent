import React, { useState, useRef, useEffect } from "react";
import ReactMarkdown from "react-markdown";
import "./Chat.css";

function Chat() {
  const [messages, setMessages] = useState(() => {
    const saved = localStorage.getItem("chatHistory");
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

    const userMessage = { role: "user", content: input };
    setMessages(prev => [...prev, userMessage]);
    setInput("");
    setIsTyping(true);

    const response = await fetch(
      `http://localhost:5000/chat/stream?message=${encodeURIComponent(input)}`
    );

    const reader = response.body.getReader();
    const decoder = new TextDecoder();
    let assistantText = "";

    setMessages(prev => [...prev, { role: "assistant", content: "" }]);

    while (true) {
      const { done, value } = await reader.read();
      if (done) break;

      assistantText += decoder.decode(value);
      setMessages(prev => {
        const updated = [...prev];
        updated[updated.length - 1].content = assistantText;
        return updated;
      });
    }

    setIsTyping(false);
  };

  const renderMessageContent = (content) => {
  try {
    const parsed = JSON.parse(content);

    // 🔹 Only render as code snippet if it's a tool result
    if (parsed?.success === true && parsed?.data) {
      return (
        <pre className="json-block">
          <code>
            {JSON.stringify(parsed.data, null, 2)}
          </code>
        </pre>
      );
    }
  } catch {
    // Not JSON → fall back to markdown
  }

  return <ReactMarkdown>{content}</ReactMarkdown>;
};

  return (
    <div className={darkMode ? "app dark" : "app light"}>
      <div className="header">
        <h2>Hybrid AI Agent</h2>
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