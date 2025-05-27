import React, { useState } from "react";
import { UPLOAD_API } from "../api/config";

const FileUpload = () => {
  const [file, setFile] = useState(null);

  const handleFileChange = (e) => {
    setFile(e.target.files[0]);
  };

  const handleUpload = async () => {
    if (!file) return alert("Please select a file!");
    const formData = new FormData();
    formData.append("file", file);

    try {
      const response = await fetch(UPLOAD_API, {
        method: "POST",
        body: formData,
      });
      if (!response.ok) throw new Error("File upload failed!");
      alert("File uploaded successfully!");
    } catch (error) {
      console.error(error);
      alert("Error uploading file.");
    }
  };

  return (
    <div>
      <input type="file" onChange={handleFileChange} />
      <button onClick={handleUpload}>Upload</button>
    </div>
  );
};

export default FileUpload;