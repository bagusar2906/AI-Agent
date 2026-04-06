import React, { useState, useRef, useEffect } from "react";
import ReactMarkdown from "react-markdown";
import "./Chat.css";

const VALIDATION_URL = "http://localhost:5000/chat/resolve";
const STREAM_URL = "http://localhost:5000/chat/stream";


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
        `${STREAM_URL}?message=${encodeURIComponent(userInput)}`
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
        console.log("Parsed response:", parsed);

        setMessages(prev => {
          const updated = [...prev];
          const lastIndex = updated.length - 1;

          // ✅ Tool success
          if (parsed?.success === true && parsed?.data) {
            updated[lastIndex] = {
              role: "assistant",
              type: "tool",
              content: parsed.data
            };
          }

          // ✅ Missing required fields
          else if (parsed?.success === false && parsed?.missingFields) {
            updated[lastIndex] = {
              role: "assistant",
              type: "validation",
              content: parsed.missingFields
            };
          }

          return updated;
        });

      } catch (err) {
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
    console.log("Rendering message:", msg);
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

      case "validation":
        return (
          <ValidationForm
            fields={msg.content.missingFields}
            originalInput={msg.content.originalUserInput}
          />
        );

      case "error":
        return <div className="error">{msg.content}</div>;

      default:
        return <ReactMarkdown>{msg.content}</ReactMarkdown>;
    }
  };

  const ValidationForm = ({ fields, originalInput }) => {
    const [formData, setFormData] = useState({});

    const handleChange = (field, value) => {
      setFormData(prev => ({
        ...prev,
        [field]: value
      }));
    };

    const handleSubmit = async () => {
      const payload = {
        originalInput,
        ...formData
      };

      // Remove validation message
      setMessages(prev => prev.slice(0, -1));

      // Add thinking bubble
      setMessages(prev => [
        ...prev,
        { role: "assistant", type: "thinking", content: "" }
      ]);

      try {
        const response = await fetch(VALIDATION_URL, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(payload)
        });

        const result = await response.json();

        setMessages(prev => {
          const updated = [...prev];
          updated[updated.length - 1] = {
            role: "assistant",
            type: "tool",
            content: result.data
          };
          return updated;
        });

      } catch {
        setMessages(prev => {
          const updated = [...prev];
          updated[updated.length - 1] = {
            role: "assistant",
            type: "error",
            content: "⚠️ Failed to resolve missing fields."
          };
          return updated;
        });
      }
    };

    return (
      <div className="validation-card">
        <div className="validation-title">
          Missing Required Fields
        </div>

        {fields.map((field, i) => (
          <div key={i} className="form-group">
            <label>{field}</label>
            <input
              type="text"
              onChange={e => handleChange(field, e.target.value)}
            />
          </div>
        ))}

        <button onClick={handleSubmit}>Submit</button>
      </div>
    );
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