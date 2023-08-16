import React, { useState, useEffect } from 'react';
import axios from 'axios';
import backendBaseUrl from './apiConfig'; // Import the configured base URL

function ApplicationComboBox({ onSelectApplication }) {
  const [appInfo, setAppInfo] = useState([]);

  useEffect(() => {
    axios.get(`${backendBaseUrl}api/Config/appinfo`)
      .then(response => {
        setAppInfo(response.data);
      })
      .catch(error => {
        console.error('Error fetching application info:', error);
      });
  }, []);

  return (
    <div>
      <label>Application:</label>
      <select
        value={selectedApplication}
        onChange={(e) => onSelectApplication(e.target.value)}
      >
        <option value="">Select an application</option>
        {appInfo.map(info => (
          <option key={info.application} value={info.application}>
            {info.application}
          </option>
        ))}
      </select>
    </div>
  );
}

export default ApplicationComboBox;
