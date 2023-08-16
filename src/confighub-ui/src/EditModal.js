import React, { useState, useEffect } from 'react';
import './EditModal.css'; // You can adjust the path as needed

function EditModal({ isOpen, closeModal, configItem, onSave }) {
  const [editedConfigItem, setEditedConfigItem] = useState({ ...configItem });

  useEffect(() => {
    setEditedConfigItem({ ...configItem });
  }, [configItem]);

  const handleInputChange = (event) => {
    const { name, value } = event.target;
    setEditedConfigItem((prevState) => ({
      ...prevState,
      [name]: value,
    }));
  };

  const handleSave = async () => {
    try {
      await onSave(editedConfigItem);
      closeModal();
    } catch (error) {
      console.error('Error saving config item:', error);
    }
  };

  return (
    <div className={`edit-modal ${isOpen ? 'open' : ''}`}>
      <div className="modal-content">
        <h2 className="modal-heading">Edit Configuration</h2>
        <div className="input-group">
          <label htmlFor="Key">Key:</label>
          <input
            type="text"
            id="Key"
            name="Key"
            value={editedConfigItem.Key}
            readOnly
            className="input-field"
          />
        </div>
        <div className="input-group">
          <label htmlFor="Value">Value:</label>
          <input
            type="text"
            id="Value"
            name="Value"
            value={editedConfigItem.Value}
            onChange={handleInputChange}
            className="input-field"
          />
        </div>
        <div className="input-group">
          <label htmlFor="LinkedKey">Linked Key:</label>
          <input
            type="text"
            id="LinkedKey"
            name="LinkedKey"
            value={editedConfigItem.LinkedKey}
            onChange={handleInputChange}
            className="input-field"
          />
        </div>
        <div className="readonly-group">
          <label>Application Name:</label>
          <span>{editedConfigItem.ApplicationName}</span>
        </div>
        <div className="readonly-group">
          <label>Component:</label>
          <span>{editedConfigItem.Component}</span>
        </div>
        <div className="readonly-group">
          <label>Last Updated By:</label>
          <span>{editedConfigItem.LastUpdatedBy}</span>
        </div>
        <div className="button-group">
          <button className="save-button" onClick={handleSave}>Save</button>
          <button className="cancel-button" onClick={closeModal}>Close</button>
        </div>
      </div>
    </div>
  );
}

export default EditModal;
