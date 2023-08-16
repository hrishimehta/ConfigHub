import React, { useState } from 'react';
import axios from 'axios';

function ConfigItemForm({ application, component, key }) {
  const [value, setValue] = useState('');

  const handleSubmit = (event) => {
    event.preventDefault();

    // Create a new config item object
    const configItem = {
      application,
      component,
      key,
      value,
    };

    // Send a POST request to add the config item
    axios.post('https://localhost:44315/api/Config', configItem)
      .then(response => {
        console.log('Config item added:', response.data);
        // Clear the form after successful submission
        setValue('');
      })
      .catch(error => {
        console.error('Error adding config item:', error);
      });
  };

  return (
    <div>
      <h2>Add/Update Config Item</h2>
      <form onSubmit={handleSubmit}>
        <label>Value:</label>
        <input
          type="text"
          value={value}
          onChange={event => setValue(event.target.value)}
        />
        <button type="submit">Add/Update</button>
      </form>
    </div>
  );
}

export default ConfigItemForm;
