import { useEffect, useRef, useState } from 'react';
import type { FormEvent, KeyboardEvent  } from 'react';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import './App.css';

type ChatMessage = {
  id: number;
  userName: string;
  text: string;
  createdAtUtc: string;
  sentiment?: string | null;
  sentimentScore?: number | null;
};

const apiBaseUrl = 'https://localhost:7096';

function App() {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [userName, setUserName] = useState('');
  const [text, setText] = useState('');
  const [isConnected, setIsConnected] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  const connectionRef = useRef<HubConnection | null>(null);
  const isStartingConnectionRef = useRef(false);
  const isInitializedRef = useRef(false);
  const messagesEndRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    let isMounted = true;

    const initializeChat = async () => {
      if (isInitializedRef.current) {
        return;
      }

      isInitializedRef.current = true;

      await loadMessages(isMounted);
      await startSignalRConnection();
    };

    initializeChat();

    return () => {
      isMounted = false;

      if (connectionRef.current) {
        connectionRef.current.stop();
        connectionRef.current = null;
      }

      isStartingConnectionRef.current = false;
    };
  }, []);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const loadMessages = async (isMounted = true) => {
    try {
      const response = await fetch(`${apiBaseUrl}/api/messages`);
      if (!response.ok) {
        throw new Error(`Failed to load messages: ${response.status}`);
      }

      const data: ChatMessage[] = await response.json();

      if (isMounted) {
        setMessages(data);
      }
    } catch (error) {
      console.error(error);
    } finally {
      if (isMounted) {
        setIsLoading(false);
      }
    }
  };

  const startSignalRConnection = async () => {
    if (connectionRef.current || isStartingConnectionRef.current) {
      return;
    }

    try {
      isStartingConnectionRef.current = true;

      const connection = new HubConnectionBuilder()
          .withUrl(`${apiBaseUrl}/chatHub`)
          .withAutomaticReconnect()
          .configureLogging(LogLevel.Information)
          .build();

      connection.on('ReceiveMessage', (message: ChatMessage) => {
        setMessages((prev) => {
          const alreadyExists = prev.some((m) => m.id === message.id);
          if (alreadyExists) {
            return prev;
          }

          return [...prev, message];
        });
      });

      connection.onreconnected(() => {
        setIsConnected(true);
      });

      connection.onreconnecting(() => {
        setIsConnected(false);
      });

      connection.onclose(() => {
        setIsConnected(false);
      });

      await connection.start();

      connectionRef.current = connection;
      setIsConnected(true);
    } catch (error) {
      console.error('SignalR connection error:', error);
      setIsConnected(false);
    } finally {
      isStartingConnectionRef.current = false;
    }
  };
  // <-- NEW: винесли логіку відправки в окрему функцію,
  // щоб її використовували і кнопка Send, і клавіша Enter
  const sendMessage = async () => {
    if (!userName.trim() || !text.trim()) {
      return;
    }

    if (!connectionRef.current || connectionRef.current.state !== 'Connected') {
      return;
    }

    try {
      await connectionRef.current.invoke('SendMessage', {
        userName: userName.trim(),
        text: text.trim()
      });

      setText('');
    } catch (error) {
      console.error('Send message error:', error);
    }
  };

  // <-- CHANGED:  submit форми просто викликає sendMessage()
  const handleSendMessage = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    await sendMessage();
  };

  // <-- NEW: обробка Enter у textarea
  // Enter = відправити
  // Shift + Enter = новий рядок
  const handleTextKeyDown = async (e: KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      await sendMessage();
    }
  };

  return (
      <div className="page">
        <div className="chat-card">
          <div className="chat-header">
            <h1>Realtime Chat</h1>
            <span className={isConnected ? 'status online' : 'status offline'}>
            {isConnected ? 'Connected' : 'Disconnected'}
          </span>
          </div>

          <div className="messages-box">
            {isLoading ? (
                <div className="empty-state">Loading messages...</div>
            ) : messages.length === 0 ? (
                <div className="empty-state">No messages yet.</div>
            ) : (
                messages.map((message) => (

                    <div key={message.id} className="message-item">
                      <div className="message-user">{message.userName}</div>
                      <div className="message-text">{message.text}</div>
                      <div className="message-date">
                        {new Date(message.createdAtUtc).toLocaleString()}
                      </div>
                    </div>
                ))

            )}

            <div ref={messagesEndRef} />
          </div>

          <form className="message-form" onSubmit={handleSendMessage}>
            <input
                type="text"
                placeholder="Your name"
                value={userName}
                onChange={(e) => setUserName(e.target.value)}
            />

            <textarea
                placeholder="Type a message"
                value={text}
                onChange={(e) => setText(e.target.value)}
                onKeyDown={handleTextKeyDown} // <-- NEW: додали сюди
                rows={2}
            />

            <button type="submit">Send</button>
          </form>
        </div>
      </div>
  );
}

export default App;