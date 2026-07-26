import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import api from '../../api';
import EventForm from '../../components/events/EventForm'; 
import Navbar from '../../components/Navbar';

const CreateEvent = () => {
  const navigate = useNavigate();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [selectedFile, setSelectedFile] = useState(null);

  const handleCreate = async (formData) => {
    try {
      setIsSubmitting(true);
      
      let imageUrl = formData.coverImageUrl;

      if (selectedFile) {
        const data = new FormData();
        data.append("file", selectedFile);
        data.append("upload_preset", import.meta.env.VITE_CLOUDINARY_UPLOAD_PRESET);

        const res = await fetch(`https://api.cloudinary.com/v1_1/${import.meta.env.VITE_CLOUDINARY_CLOUD_NAME}/image/upload`, {
          method: "POST",
          body: data,
        });
        
        const file = await res.json();
        if (file.secure_url) {
          imageUrl = file.secure_url;
        } else {
          throw new Error("Upload slike nije uspeo.");
        }
      }

      const payload = {
        ...formData,
        coverImageUrl: imageUrl,
        latitude: formData.latitude ? parseFloat(formData.latitude) : null,
        longitude: formData.longitude ? parseFloat(formData.longitude) : null,
        tagNames: formData.tagNames || [],
        ticketTypes: (formData.ticketTypes || []).map(t => ({
          name: t.name,
          price: parseFloat(t.price) || 0,
          capacity: parseInt(t.capacity) || 0,
          earlyBirdPrice: t.earlyBirdPrice !== '' && t.earlyBirdPrice !== null ? parseFloat(t.earlyBirdPrice) : null,
          earlyBirdExpiryDate: t.earlyBirdExpiryDate ? t.earlyBirdExpiryDate : null
        }))
      };

      if (formData.isRecurring) {
        await api.post('/events/recurring', payload);
      } else {
        await api.post('/events', payload);
      }
      
      toast.success("Događaj je uspešno kreiran!");
      navigate('/moji-dogadjaji');
    } catch (error) {
      console.error("Greška pri kreiranju:", error);
      
      const errorMessage = error.response?.data?.title || error.response?.data || "Nismo uspeli da kreiramo događaj.";
      toast.error(typeof errorMessage === 'string' ? errorMessage : "Došlo je do greške pri čuvanju podataka.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      
      <div className="container mx-auto p-6 max-w-2xl">
        <h1 className="text-3xl font-bold text-gray-800 mb-6">Kreiraj novi događaj</h1>
        
        <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
          <EventForm 
            onSubmit={handleCreate} 
            isSubmitting={isSubmitting}
            onFileChange={setSelectedFile}
          />
        </div>
      </div>
    </div>
  );
};

export default CreateEvent;