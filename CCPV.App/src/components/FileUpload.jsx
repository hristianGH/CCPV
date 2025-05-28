import React, { useState } from "react";
import { UPLOAD_API } from "../api/config";

const FileUpload = ({ userName, onUploadSuccess, show, onClose }) => {
  const [file, setFile] = useState(null);
  const [portfolioName, setPortfolioName] = useState("");
  const [uploading, setUploading] = useState(false);

  const handleFileChange = (e) => {
    setFile(e.target.files[0]);
  };

  const handleNameChange = (e) => {
    setPortfolioName(e.target.value);
  };

  const handleUpload = async () => {
    if (!portfolioName) return alert("Please enter a portfolio name!");
    if (!file) return alert("Please select a file!");
    const formData = new FormData();
    formData.append("file", file);
    setUploading(true);
    try {
      const response = await fetch(
        `${UPLOAD_API}?portfolioName=${encodeURIComponent(portfolioName)}`,
        {
          method: "POST",
          headers: {
            UserName: userName || "User",
          },
          body: formData,
        }
      );
      if (!response.ok) throw new Error("File upload failed!");
      alert("File uploaded successfully!");
      setFile(null);
      setPortfolioName("");
      if (onUploadSuccess) onUploadSuccess();
      if (onClose) onClose();
    } catch (error) {
      console.error(error);
      alert("Error uploading file.");
    } finally {
      setUploading(false);
    }
  };

  if (!show) return null;

  return (
    <div
      style={{
        position: "fixed",
        top: 0,
        left: 0,
        width: "100vw",
        height: "100vh",
        background: "rgba(24,26,32,0.85)",
        zIndex: 3000,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
      }}
    >
      <div
        style={{
          background: "#23263a",
          color: "#fff",
          borderRadius: 12,
          padding: 32,
          minWidth: 320,
          boxShadow: "0 4px 32px #0008",
          border: "2px solid #2563eb",
          position: "relative",
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
        }}
      >
        <button
          onClick={onClose}
          style={{
            position: "absolute",
            top: 12,
            right: 12,
            background: "none",
            color: "#fff",
            border: "none",
            fontSize: 22,
            cursor: "pointer",
          }}
        >
          &times;
        </button>
        <h2 style={{ marginBottom: 16 }}>Upload Portfolio</h2>
        <input
          type="text"
          placeholder="Portfolio Name"
          value={portfolioName}
          onChange={handleNameChange}
          style={{ marginBottom: 16, width: "100%" }}
          required
        />
        <input
          type="file"
          onChange={handleFileChange}
          style={{ marginBottom: 16, width: "100%" }}
        />
        <div
          style={{
            width: "100%",
            display: "flex",
            justifyContent: "center",
          }}
        >
          <button
            onClick={handleUpload}
            disabled={uploading || !portfolioName}
            style={{ width: "60%" }}
          >
            {uploading ? "Uploading..." : "Upload"}
          </button>
        </div>
      </div>
    </div>
  );
};

export default FileUpload;