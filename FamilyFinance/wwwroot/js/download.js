window.downloadFile = (filename, contentType, base64Data) => {
  const link = document.createElement('a');
  link.download = filename;
  link.href = `data:${contentType};base64,${base64Data}`;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
};

// Trigger file input click
window.triggerFileInput = (inputId) => {
  document.getElementById(inputId)?.click();
};

// Read file as text
window.readFileAsText = async (inputId) => {
  const input = document.getElementById(inputId);
  if (!input || !input.files || input.files.length === 0) {
    return null;
  }
  
  const file = input.files[0];
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = (e) => resolve(e.target.result);
    reader.onerror = (e) => reject(e);
    reader.readAsText(file);
  });
};

