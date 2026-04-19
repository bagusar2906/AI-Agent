import React, { useState, useEffect, useRef } from "react";
import Editor, { useMonaco } from "@monaco-editor/react";

const STREAM_URL = "http://localhost:5000/chat/completions"; // streaming
const SUGGEST_URL = "http://localhost:5000/chat/suggest";     // context-aware

function Chat() {
  const [input, setInput] = useState("");
  const [darkMode, setDarkMode] = useState(true);
  const [debugSuggestion, setDebugSuggestion] = useState("");

  const monaco = useMonaco();

  // 🔥 refs for streaming
  const suggestionRef = useRef("");
  const streamingRef = useRef(false);
  const abortRef = useRef(null);
  const lastInputRef = useRef("");
  const editorRef = useRef(null);
  const debounceRef = useRef(null);

  // =========================================================
  // 🔹 STREAMING (Ollama / SSE)
  // =========================================================
  const startStreaming = async (text) => {
    if (!editorRef.current) return;

    // prevent duplicate streams
    if (streamingRef.current) return;

    // cancel previous
    if (abortRef.current) {
      abortRef.current.abort();
    }

    const controller = new AbortController();
    abortRef.current = controller;

    suggestionRef.current = "";
    streamingRef.current = true;

    try {
      const res = await fetch(STREAM_URL, {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify({
          messages: [{ role: "user", content: text }]
        }),
        signal: controller.signal
      });

      const reader = res.body.getReader();
      const decoder = new TextDecoder();

      let fullText = "";

      while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        const chunk = decoder.decode(value);
        const lines = chunk.split("\n");

        for (let line of lines) {
          if (!line.startsWith("data: ")) continue;

          const data = line.replace("data: ", "").trim();

          if (data === "[DONE]") {
            streamingRef.current = false;
            return;
          }

          fullText += data;

          // 🔥 trim already typed part
          let suggestion = fullText;
          if (fullText.startsWith(text)) {
            suggestion = fullText.slice(text.length);
          }

          suggestionRef.current = suggestion;
          setDebugSuggestion(suggestion);

          // limit length
          if (suggestion.length > 100) {
            streamingRef.current = false;
            return;
          }

          // refresh Monaco
          setTimeout(() => {
            editorRef.current?.trigger(
              "keyboard",
              "editor.action.inlineSuggest.trigger",
              {}
            );
          }, 0);
        }
      }
    } catch (err) {
      if (err.name !== "AbortError") console.error(err);
      streamingRef.current = false;
    }
  };

  // =========================================================
  // 🔹 FAST CONTEXT-AWARE SUGGESTION
  // =========================================================
  const fetchContextSuggestion = async (text) => {
    try {
      const res = await fetch(SUGGEST_URL, {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify({
          messages: [{ role: "user", content: text }]
        })
      });

      const data = await res.json();

      if (data.suggestion) {
        suggestionRef.current = data.suggestion;
        setDebugSuggestion(data.suggestion);

        editorRef.current?.trigger(
          "keyboard",
          "editor.action.inlineSuggest.trigger",
          {}
        );

        return true;
      }
    } catch (err) {
      console.error(err);
    }

    return false;
  };

  // =========================================================
  // 🔹 MONACO PROVIDER
  // =========================================================
  useEffect(() => {
    if (!monaco) return;

    const provider = monaco.languages.registerInlineCompletionsProvider(
      "javascript",
      {
        provideInlineCompletions: async (model, position) => {
          const text = model.getValueInRange({
            startLineNumber: 1,
            startColumn: 1,
            endLineNumber: position.lineNumber,
            endColumn: position.column
          });

          if (text.length < 2) return { items: [] };

          // debounce trigger
          if (debounceRef.current) {
            clearTimeout(debounceRef.current);
          }

          debounceRef.current = setTimeout(async () => {
            if (text !== lastInputRef.current) {
              lastInputRef.current = text;

              // 🔥 try fast context suggestion first
              const hasContext = await fetchContextSuggestion(text);

              // fallback to streaming
              if (!hasContext) {
                startStreaming(text);
              }
            }
          }, 250);

          const suggestion = suggestionRef.current;

          return {
            items: suggestion
              ? [
                  {
                    insertText: suggestion,
                    range: {
                      startLineNumber: position.lineNumber,
                      startColumn: position.column,
                      endLineNumber: position.lineNumber,
                      endColumn: position.column
                    }
                  }
                ]
              : []
          };
        },

        freeInlineCompletions: () => {},
        disposeInlineCompletions: () => {}
      }
    );

    return () => provider.dispose();
  }, [monaco]);

  // =========================================================
  // 🔹 UI
  // =========================================================
  return (
    <div className={darkMode ? "app dark" : "app light"}>
      <div className="header">
        <h2>NT8 Assistant</h2>
        <button onClick={() => setDarkMode(!darkMode)}>
          {darkMode ? "Light Mode" : "Dark Mode"}
        </button>
      </div>

      {/* Monaco Editor */}
      <div style={{ height: "200px", border: "1px solid #444" }}>
        <Editor
          height="100%"
          defaultLanguage="javascript"
          value={input}
          onMount={(editor) => {
            editorRef.current = editor;
          }}
          onChange={(value) => {
            const newValue = value || "";
            setInput(newValue);

            // cancel streaming
            if (abortRef.current) {
              abortRef.current.abort();
            }

            suggestionRef.current = "";
            streamingRef.current = false;
          }}
          theme={darkMode ? "vs-dark" : "light"}
          options={{
            minimap: { enabled: false },
            fontSize: 14,
            inlineSuggest: { enabled: true },
            quickSuggestions: false,
            suggestOnTriggerCharacters: false,
            wordBasedSuggestions: false
          }}
        />
      </div>

      {/* Debug suggestion */}
      <div style={{ marginTop: 10, color: "#888" }}>
        Suggestion: {debugSuggestion}
      </div>

      <div style={{ marginTop: 10 }}>
        <button
          onClick={() => {
            console.log("Send:", input);
            setInput("");
          }}
        >
          Send
        </button>
      </div>
    </div>
  );
}

export default Chat;