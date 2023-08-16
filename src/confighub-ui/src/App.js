import React, { useState, useEffect } from 'react';
import axios from 'axios';
import './App.css';
import EditModal from './EditModal'; // Adjust the path as needed

function App() {
  const backendUrl = 'https://localhost:44315'; // Replace with your backend URL
  const axiosInstance = axios.create({
    baseURL: backendUrl,
  });

  const [appInfos, setAppInfos] = useState([]);
  const [selectedApp, setSelectedApp] = useState('');
  const [selectedComponent, setSelectedComponent] = useState('');
  const [configItems, setConfigItems] = useState([]);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [selectedConfigItem, setSelectedConfigItem] = useState(null);

  useEffect(() => {
    fetchAppInfos();
  }, []);

  useEffect(() => {
    if (appInfos.length > 0) {
      setSelectedApp(appInfos[0].ApplicationName);
      setSelectedComponent(appInfos[0].Components[0]);
    }
  }, [appInfos]);

  useEffect(() => {
    if (selectedComponent) {
      fetchConfigItems();
    }
  }, [selectedComponent]);

  const fetchAppInfos = async () => {
    try {
      const response = await axiosInstance.get('/api/config/appinfo');
      setAppInfos(response.data);
    } catch (error) {
      console.error('Error fetching app info:', error);
    }
  };

  const fetchConfigItems = async () => {
    try {
      axiosInstance.defaults.headers.common['X-ApplicationId'] = selectedApp;

      const response = await axiosInstance.get(`/api/config/component/${selectedComponent}`);
      setConfigItems(response.data);
    } catch (error) {
      console.error('Error fetching config items:', error);
    }
  };

  const openEditModal = (configItem) => {
    setSelectedConfigItem(configItem);
    setIsEditModalOpen(true);
  };

  const closeEditModal = () => {
    setSelectedConfigItem(null);
    setIsEditModalOpen(false);
  };

  const handleSaveConfigItem = async (editedConfigItem) => {
    try {
      await axiosInstance.put(`/api/config/${editedConfigItem.Component}/${editedConfigItem.Key}`, editedConfigItem);
      fetchConfigItems();
      closeEditModal();
    } catch (error) {
      console.error('Error saving config item:', error);
    }
  };

  const handleAppSelect = (event) => {
    setSelectedApp(event.target.value);
  };

  const handleComponentSelect = (event) => {
    setSelectedComponent(event.target.value);
  };

  return (
    <div className="container">
      <h1>Configuration Management</h1>
      <div className="select-container">
        <label className="select-label">Select Application:</label>
        <select className="select-input" value={selectedApp} onChange={handleAppSelect}>
          {appInfos.map((appInfo, index) => (
            <option key={index} value={appInfo.ApplicationName}>
              {appInfo.ApplicationName}
            </option>
          ))}
        </select>
      </div>
      {selectedApp && (
        <div className="select-container">
          <label className="select-label">Select Component:</label>
          <select className="select-input" value={selectedComponent} onChange={handleComponentSelect}>
            {appInfos.find((appInfo) => appInfo.ApplicationName === selectedApp)?.Components.map((component, index) => (
              <option key={index} value={component}>
                {component}
              </option>
            ))}
          </select>
        </div>
      )}
      {selectedComponent && (
        <div className="config-section">
          <h2 className="config-heading">Configuration Items for {selectedComponent}</h2>
          <table className="config-table">
            <thead>
              <tr>
                <th>Key</th>
                <th>Value</th>
                <th>Linked Key</th>
                <th>Is Encrypted</th>
                <th>Last Updated Date Time</th>
                <th>Created Date Time</th>
                <th>Edit</th>
              </tr>
            </thead>
            <tbody>
              {configItems.map((configItem, index) => (
                <tr key={index}>
                  <td>{configItem.Key}</td>
                  <td>{configItem.Value}</td>
                  <td>{configItem.LinkedKey}</td>
                  <td>{configItem.IsEncrypted ? 'Yes' : 'No'}</td>
                  <td>{configItem.LastUpdatedDateTime}</td>
                  <td>{configItem.CreatedDateTime}</td>
                  <td>
                    <button onClick={() => openEditModal(configItem)}>Edit</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      <EditModal
        isOpen={isEditModalOpen}
        closeModal={closeEditModal}
        configItem={selectedConfigItem}
        onSave={handleSaveConfigItem}
      />
    </div>
  );
}

export default App;
