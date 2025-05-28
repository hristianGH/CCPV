import { useRef, useState } from 'react';
import './App.css'
import FileUpload from "./components/FileUpload";
import PortfolioDisplay from "./components/PortfolioDisplay";

function App() {
  const portfolioRef = useRef();
  const [userName, setUserName] = useState('User');
  const [showProfile, setShowProfile] = useState(false);
  const [pendingUserName, setPendingUserName] = useState(userName);
  const [showUpload, setShowUpload] = useState(false);

  return (
    <>
      <div className="header">
        <h1>Crypto Portfolio Manager</h1>
        <div className="profile" onClick={() => setShowProfile(true)}>
          <div className="profile-icon" title="Change UserName">{userName[0]?.toUpperCase() || '?'}</div>
          <span>{userName}</span>
        </div>
      </div>
      {showProfile && (
        <div style={{ position: 'fixed', top: 0, right: 0, background: '#23263a', color: '#fff', border: '2px solid #2563eb', borderRadius: 8, padding: 24, zIndex: 2000 }}>
          <h3>Change UserName</h3>
          <input value={pendingUserName} onChange={e => setPendingUserName(e.target.value)} style={{ marginRight: 8 }} />
          <button onClick={() => { setUserName(pendingUserName); setShowProfile(false); }}>Save</button>
          <button style={{ marginLeft: 8 }} onClick={() => setShowProfile(false)}>Cancel</button>
        </div>
      )}
      <div className="bordered">
        <button onClick={() => setShowUpload(true)} style={{ marginBottom: 16 }}>Upload Portfolio</button>
        <FileUpload userName={userName} show={showUpload} onClose={() => setShowUpload(false)} onUploadSuccess={() => { setShowUpload(false); portfolioRef.current?.refresh(); }} />
      </div>
      <div className="bordered">
        <PortfolioDisplay ref={portfolioRef} userName={userName} />
      </div>
    </>
  )
}

export default App
