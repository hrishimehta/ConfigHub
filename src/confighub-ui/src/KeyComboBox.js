import React, { useState, useEffect } from 'react';
import axios from 'axios';

function KeyComboBox({ application, component, onSelectKey }) {
  const [keys, setKeys] = useState([]);
  const [selectedKey, setSelectedKey] = useState('');

  useEffect(() => {
    if (application && component) {
      // Fetch keys based on selected application and component
      axios.get(`/api/Config/component/${component}`)
        .then(response => {
          setKeys(response.data.map(item => item.key));
        })
        .catch(error => {
          console.error('Error fetching keys:', error);
        });
    }
  }, [application, component]);

  const handleKeyChange = (event) => {
    const selectedKey = event.target.value;
    setSelectedKey(selectedKey);
    onSelectKey(selectedKey);
  };

  return (
    <div>
      <label>Key:</label>
      <select value={selectedKey} onChange={handleKeyChange}>
        <option value="">Select a key</option>
        {keys.map(key => (
          <option key={key} value={key}>{key}</option>
        ))}
      </select>
    </div>
  );
}

export default KeyComboBox;
