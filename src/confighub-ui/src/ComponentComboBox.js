import React, { useState, useEffect } from 'react';
import axios from 'axios';
import backendBaseUrl from './apiConfig'; // Import the configured base URL

function ComponentComboBox({ application, onSelectComponent }) {
  const [appInfo, setAppInfo] = useState([]);
  const [selectedComponent, setSelectedComponent] = useState('');

  useEffect(() => {
    // Fetch application info
    axios.get(`${backendBaseUrl}api/Config/appinfo`)
      .then(response => {
        setAppInfo(response.data);
      })
      .catch(error => {
        console.error('Error fetching application info:', error);
      });
  }, []);

  const handleComponentChange = (event) => {
    const selectedComp = event.target.value;
    setSelectedComponent(selectedComp);
    onSelectComponent(selectedComp);
  };

  return (
    <div>
      <label>Component:</label>
      <select value={selectedComponent} onChange={handleComponentChange}>
        <option value="">Select a component</option>
        {appInfo
          .find(info => info.application === application)
          ?.components.map(comp => (
            <option key={comp} value={comp}>
              {comp}
            </option>
          ))}
      </select>
    </div>
  );
}

export default ComponentComboBox;
