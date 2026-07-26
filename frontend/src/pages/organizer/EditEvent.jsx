import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import toast from 'react-hot-toast';
import api from '../../api';
import EventForm from '../../components/events/EventForm'; 
import Navbar from '../../components/Navbar';

const EditEvent = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedFile, setSelectedFile] = useState(null);
  const [eventData, setEventData] = useState(null);

  useEffect(() => {
    const fetchEvent = async () => {
      try {
        const response = await api.get(`/events/${id}`);
        
        const fetchedData = response.data;
        if (fetchedData && !fetchedData.categoryId && fetchedData.category?.id) {
          fetchedData.categoryId = fetchedData.category.id;
        }

        setEventData(fetchedData);
      } catch (error) {
        toast.error("Nismo uspeli da učitamo podatke o događaju.");
        navigate('/moji-dogadjaji');
      } finally {
        setIsLoading(false);
      }
    };
    fetchEvent();
  }, [id, navigate]);

  const handleUpdate = async (formData) => {
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
        id: id,
        coverImageUrl: imageUrl,
        latitude: formData.latitude ? parseFloat(formData.latitude) : null,
        longitude: formData.longitude ? parseFloat(formData.longitude) : null,
        tagNames: formData.tagNames || [],
        ticketTypes: formData.ticketTypes || []
      };

      await api.put(`/events/${id}`, payload);
      
      toast.success("Događaj je uspešno ažuriran!");
      navigate('/moji-dogadjaji');
    } catch (error) {
      console.error("Greška pri ažuriranju:", error);
      const errorMessage = error.response?.data || "Nismo uspeli da ažuriramo događaj.";
      toast.error(typeof errorMessage === 'string' ? errorMessage : "Došlo je do greške.");
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) return <div className="text-center mt-10">Učitavanje...</div>;

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <div className="container mx-auto p-6 max-w-2xl">
        <h1 className="text-3xl font-bold text-gray-800 mb-6">Izmeni događaj</h1>
        
        <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
          <EventForm 
            initialValues={eventData}
            onSubmit={handleUpdate} 
            isSubmitting={isSubmitting}
            onFileChange={setSelectedFile}
          />
        </div>
      </div>
    </div>
  );
};

export default EditEvent;