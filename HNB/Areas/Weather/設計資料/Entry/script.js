// Upload functionality
const uploadArea = document.getElementById('uploadArea');
const uploadZone = uploadArea.querySelector('.upload-zone');

// Drag and drop functionality
uploadZone.addEventListener('dragover', (e) => {
  e.preventDefault();
  uploadZone.style.backgroundColor = 'rgba(210, 105, 30, 0.2)';
});

uploadZone.addEventListener('dragleave', (e) => {
  e.preventDefault();
  uploadZone.style.backgroundColor = '';
});

uploadZone.addEventListener('drop', (e) => {
  e.preventDefault();
  uploadZone.style.backgroundColor = '';
  
  const files = e.dataTransfer.files;
  if (files.length > 0) {
    handleFileUpload(files[0]);
  }
});

// Click to upload
uploadZone.addEventListener('click', () => {
  const input = document.createElement('input');
  input.type = 'file';
  input.accept = 'image/*';
  input.onchange = (e) => {
    if (e.target.files.length > 0) {
      handleFileUpload(e.target.files[0]);
    }
  };
  input.click();
});

function handleFileUpload(file) {
  if (file.type.startsWith('image/')) {
    const reader = new FileReader();
    reader.onload = (e) => {
      // Create new image element
      const img = document.createElement('img');
      img.src = e.target.result;
      img.style.width = '100%';
      img.style.height = '100%';
      img.style.objectFit = 'cover';
      
      // Replace upload zone content
      uploadZone.innerHTML = '';
      uploadZone.appendChild(img);
      uploadZone.style.border = 'none';
      
      // Add to clothing carousel
      addToCarousel(e.target.result);
    };
    reader.readAsDataURL(file);
  }
}

function addToCarousel(imageSrc) {
  const clothingItems = document.querySelector('.clothing-items');
  
  const newItem = document.createElement('div');
  newItem.className = 'clothing-item';
  newItem.innerHTML = `
    <button class="remove-btn">&times;</button>
    <img src="${imageSrc}" alt="Uploaded item">
  `;
  
  // Add remove functionality
  const removeBtn = newItem.querySelector('.remove-btn');
  removeBtn.addEventListener('click', () => {
    newItem.remove();
  });
  
  clothingItems.appendChild(newItem);
}

// Remove item functionality for existing items
document.querySelectorAll('.remove-btn').forEach(btn => {
  btn.addEventListener('click', (e) => {
    e.target.closest('.clothing-item').remove();
  });
});

// Generate button functionality
const generateBtn = document.querySelector('.generate-btn');
const weatherField = document.querySelector('.weather-field');

generateBtn.addEventListener('click', () => {
  const weatherInput = weatherField.value.trim();
  
  if (weatherInput) {
    // Simulate outfit generation
    generateBtn.textContent = 'Generating...';
    generateBtn.disabled = true;
    
    setTimeout(() => {
      generateBtn.textContent = 'Generate';
      generateBtn.disabled = false;
      
      // Show success message
      showNotification('Outfit generated based on your preferences!');
    }, 2000);
  } else {
    showNotification('Please enter your weather preference first.');
  }
});

// Weather field enter key support
weatherField.addEventListener('keypress', (e) => {
  if (e.key === 'Enter') {
    generateBtn.click();
  }
});

// Location edit functionality
const editLocationBtn = document.querySelector('.edit-location');
const locationText = document.querySelector('.location-text');

editLocationBtn.addEventListener('click', () => {
  const currentLocation = locationText.textContent;
  const newLocation = prompt('Enter new location:', currentLocation);
  
  if (newLocation && newLocation.trim() !== '') {
    locationText.textContent = newLocation.trim();
    showNotification('Location updated successfully!');
  }
});

// Notification system
function showNotification(message) {
  // Remove existing notification
  const existingNotification = document.querySelector('.notification');
  if (existingNotification) {
    existingNotification.remove();
  }
  
  // Create notification
  const notification = document.createElement('div');
  notification.className = 'notification';
  notification.textContent = message;
  notification.style.cssText = `
    position: fixed;
    top: 20px;
    right: 20px;
    background-color: #D2691E;
    color: white;
    padding: 1rem 1.5rem;
    border-radius: 10px;
    box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
    z-index: 1000;
    animation: slideIn 0.3s ease;
  `;
  
  // Add animation
  const style = document.createElement('style');
  style.textContent = `
    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
    @keyframes slideOut {
      from { transform: translateX(0); opacity: 1; }
      to { transform: translateX(100%); opacity: 0; }
    }
  `;
  document.head.appendChild(style);
  
  document.body.appendChild(notification);
  
  // Auto remove after 3 seconds
  setTimeout(() => {
    notification.style.animation = 'slideOut 0.3s ease';
    setTimeout(() => {
      notification.remove();
    }, 300);
  }, 3000);
}

// Smooth scrolling for carousel
const clothingItems = document.querySelector('.clothing-items');
let isScrolling = false;

clothingItems.addEventListener('wheel', (e) => {
  if (Math.abs(e.deltaY) > Math.abs(e.deltaX)) {
    e.preventDefault();
    clothingItems.scrollLeft += e.deltaY;
  }
});

// Initialize weather suggestions
const weatherSuggestions = [
  'go for a walk in the park',
  'attend a business meeting',
  'have a casual dinner',
  'go to the gym',
  'attend a wedding',
  'go shopping',
  'have a picnic',
  'go to work',
  'attend a party',
  'stay home and relax'
];

weatherField.addEventListener('focus', () => {
  if (!weatherField.value) {
    const randomSuggestion = weatherSuggestions[Math.floor(Math.random() * weatherSuggestions.length)];
    weatherField.placeholder = `Today I want to ${randomSuggestion}...`;
  }
});

weatherField.addEventListener('blur', () => {
  weatherField.placeholder = 'Today I want to ...';
});

// Add loading states and animations
document.addEventListener('DOMContentLoaded', () => {
  // Animate shapes on load
  const shapes = document.querySelectorAll('.shape');
  shapes.forEach((shape, index) => {
    shape.style.opacity = '0';
    shape.style.transform = 'scale(0.8)';
    shape.style.transition = 'all 0.8s ease';
    
    setTimeout(() => {
      shape.style.opacity = shape.classList.contains('shape-1') ? '0.7' : 
                           shape.classList.contains('shape-2') ? '0.6' : '0.8';
      shape.style.transform = 'scale(1)';
    }, index * 200);
  });
  
  // Animate main content
  const mainContent = document.querySelector('.main-content');
  mainContent.style.opacity = '0';
  mainContent.style.transform = 'translateY(20px)';
  mainContent.style.transition = 'all 0.6s ease';
  
  setTimeout(() => {
    mainContent.style.opacity = '1';
    mainContent.style.transform = 'translateY(0)';
  }, 300);
});
